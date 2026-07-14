import asyncio
import jwt
from fastapi import FastAPI, UploadFile, File, Form, HTTPException, Depends
from fastapi.responses import FileResponse
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from pydantic import BaseModel
from dotenv import load_dotenv

from services.rag_service import RAGService
from services.sql_agent_service import SQLAgentService

load_dotenv()

app = FastAPI(title="School ERP AI Backend", version="1.0")
security = HTTPBearer()

JWT_SECRET = os.environ.get("JWT_SECRET", "SchoolERPManagement_SuperSecretKey_2026_AtLeast32CharsLong")
ALGORITHM = "HS256"

def get_current_user(credentials: HTTPAuthorizationCredentials = Depends(security)):
    token = credentials.credentials
    try:
        payload = jwt.decode(token, JWT_SECRET, algorithms=[ALGORITHM], audience="SchoolERPManagementAngularApp")
        
        # In .NET, the role claim is often stored with this URI key
        role_claim_uri = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        role = payload.get(role_claim_uri) or payload.get("role")
        
        if not role:
            raise HTTPException(status_code=403, detail="Role not found in token")
            
        return {"role": role, "sub": payload.get("sub")}
    except jwt.ExpiredSignatureError:
        raise HTTPException(status_code=401, detail="Token has expired")
    except jwt.InvalidTokenError as e:
        raise HTTPException(status_code=401, detail=f"Invalid token: {str(e)}")

def require_admin(user: dict = Depends(get_current_user)):
    if user.get("role") != "Admin":
        raise HTTPException(status_code=403, detail="Requires Admin privileges")
    return user

rag_service = RAGService()
sql_agent_service = SQLAgentService()

class ChatRequest(BaseModel):
    query: str

class SqlSearchRequest(BaseModel):
    query: str

@app.post("/chat/faq")
async def chat_faq(request: ChatRequest, user: dict = Depends(get_current_user)):
    """Answers a user question based strictly on the uploaded FAQ documents."""
    try:
        # Run blocking LLM call in a separate thread
        answer = await asyncio.to_thread(rag_service.ask, request.query)
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
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
