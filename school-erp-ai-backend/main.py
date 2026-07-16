import asyncio
import os
import jwt
from fastapi import FastAPI, UploadFile, File, Form, HTTPException, Depends
from fastapi.responses import FileResponse
from fastapi import FastAPI, UploadFile, File, Form, HTTPException, Depends
from fastapi.responses import FileResponse, StreamingResponse
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from dotenv import load_dotenv

from services.rag_service import RAGService
from services.sql_agent_service import SQLAgentService

load_dotenv(override=True)

app = FastAPI(title="School ERP AI Backend", version="1.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
    expose_headers=["*"]
)

security = HTTPBearer()

JWT_SECRET = os.environ.get("JWT_SECRET", "SchoolERPManagement_SuperSecretKey_2026_AtLeast32CharsLong")
ALGORITHM = "HS256"

def get_current_user(credentials: HTTPAuthorizationCredentials = Depends(security)):
    token = credentials.credentials
    try:
        payload = jwt.decode(
            token, 
            JWT_SECRET, 
            algorithms=[ALGORITHM], 
            audience="SchoolERPManagementAngularApp",
            options={"verify_iss": False, "verify_sub": False}
        )
        
        # In .NET, the role claim is often stored with this URI key
        role_claim_uri = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        role = payload.get(role_claim_uri) or payload.get("role")
        
        if not role:
            raise HTTPException(status_code=403, detail="Role not found in token")
            
        return {"role": role, "sub": payload.get("sub")}
    except jwt.ExpiredSignatureError:
        print("Token has expired")
        raise HTTPException(status_code=401, detail="Token has expired")
    except jwt.InvalidTokenError as e:
        print(f"Invalid token: {str(e)}")
        raise HTTPException(status_code=401, detail=f"Invalid token: {str(e)}")
    except Exception as e:
        print(f"Unknown token error: {str(e)}")
        raise HTTPException(status_code=401, detail=f"Unknown token error: {str(e)}")

def require_admin(user: dict = Depends(get_current_user)):
    if user.get("role") != "Admin":
        raise HTTPException(status_code=403, detail="Requires Admin privileges")
    return user

rag_service = RAGService()
sql_agent_service = SQLAgentService()

class ChatRequest(BaseModel):
    query: str
    role: str | None = None

class SqlSearchRequest(BaseModel):
    query: str

@app.post("/chat/faq/stream")
async def chat_faq_stream(request: ChatRequest, user: dict = Depends(get_current_user)):
    try:
        def generate():
            for chunk in rag_service.ask_stream(request.query, request.role):
                yield chunk
        return StreamingResponse(generate(), media_type="text/plain")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/chat/faq")
async def chat_faq(request: ChatRequest, user: dict = Depends(get_current_user)):
    """Answers a user question based strictly on the uploaded FAQ documents."""
    try:
        # Run blocking LLM call in a separate thread
        answer = await asyncio.to_thread(rag_service.ask, request.query, request.role)
        return {"answer": answer}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/chat/upload")
async def upload_faq(file: UploadFile = File(...), user: dict = Depends(require_admin)):
    """Uploads a PDF or text document to the vector database for RAG."""
    try:
        content = await file.read()
        filename = f"./data/{file.filename}"
        with open(filename, "wb") as f:
            f.write(content)
        # Vector ingest can also block
        await asyncio.to_thread(rag_service.ingest_document, filename)
        return {"message": f"Successfully ingested {file.filename}"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/admin/search")
async def admin_sql_search(request: SqlSearchRequest, user: dict = Depends(require_admin)):
    """Executes a natural language query against the database and returns an Excel file export."""
    try:
        # Run blocking DB and LLM calls in a separate thread
        export_filepath = await asyncio.to_thread(sql_agent_service.query_and_export, request.query)
        if not export_filepath or not os.path.exists(export_filepath):
             raise HTTPException(status_code=404, detail="No data found or error generating export.")
        
        return FileResponse(
            path=export_filepath, 
            filename=os.path.basename(export_filepath), 
            media_type="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        )
    except Exception as e:
        error_msg = str(e)
        if "Execution failed on sql" in error_msg:
            # Extract the actual DB syntax error
            error_msg = f"AI generated an invalid query. Detail: {error_msg.split(':', 1)[-1].strip()}"
        raise HTTPException(status_code=400, detail=error_msg)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000)
