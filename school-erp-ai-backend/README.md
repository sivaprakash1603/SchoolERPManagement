# School ERP AI Backend

This is a Python FastAPI microservice that provides AI features to the School ERP system.

## Setup Instructions

### 1. Database Setup (Read-Only User)
For the Text-to-SQL admin feature to be secure, it must run using a read-only database user. 
Run the `create_readonly_user.sql` script against your PostgreSQL instance to create the `ai_readonly_user`.

```bash
# Example if using psql locally:
psql -U your_admin_user -d SchoolERPSystem -f create_readonly_user.sql
```
Once created, uncomment the connection string in the `.env` file that uses `ai_readonly_user`.

### 2. Python Environment Setup
Install the dependencies using Python 3.9+:

```bash
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

### 3. Run the API Server
Start the FastAPI server on port 8000:

```bash
uvicorn main:app --reload
```

## API Endpoints

Once running, you can access the interactive Swagger UI at: `http://localhost:8000/docs`

### 1. `POST /chat/upload`
Upload FAQ documents (PDF or TXT) to the ChromaDB vector database. A sample is provided at `data/sample_faq.txt`.

### 2. `POST /chat/faq`
Chat with the RAG bot.
**Payload:**
```json
{
  "query": "How do I reset my password?"
}
```

### 3. `POST /admin/search`
Admin Text-to-SQL search. Executes natural language queries against the database and returns an Excel (`.xlsx`) export.
**Payload:**
```json
{
  "query": "List all students and their class names"
}
```

## Notes
- This uses **Ollama** running locally. Ensure you have Ollama installed and running with `llama3` (or your chosen model in `.env`).
- Ensure the database is accessible via the connection string in the `.env` file.
