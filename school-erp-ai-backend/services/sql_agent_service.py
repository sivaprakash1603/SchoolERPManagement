import os
import uuid
import pandas as pd
from langchain_community.utilities import SQLDatabase
import requests
import json
from langchain_core.messages import AIMessage
import sqlalchemy

class SQLAgentService:
    def __init__(self):
        db_connection = os.environ.get("DB_CONNECTION_STRING")
        if not db_connection:
            raise ValueError("DB_CONNECTION_STRING environment variable not set")
            
        self.db = SQLDatabase.from_uri(db_connection)
        self.engine = sqlalchemy.create_engine(db_connection)
        
        ollama_base_url = os.environ.get("OLLAMA_BASE_URL", "http://localhost:11434")
        self.model_name = os.environ.get("OLLAMA_MODEL", "meta-llama/llama-3-8b-instruct:free")
        self.api_key = os.environ.get("OPENROUTER_API_KEY", "")
        
        # We use a custom LLM invoke wrapper for OpenRouter to avoid external dependencies
        class OpenRouterLLM:
            def __init__(self, api_key, model):
                self.api_key = api_key
                self.model = model
                
            def invoke(self, prompt: str):
                if not self.api_key or self.api_key == "sk-or-v1-put-your-key-here":
                    raise Exception("Please put your OpenRouter API key in the .env file")
                
                response = requests.post(
                    url="https://openrouter.ai/api/v1/chat/completions",
                    headers={"Authorization": f"Bearer {self.api_key}"},
                    json={
                        "model": self.model,
                        "messages": [{"role": "user", "content": prompt}],
                        "temperature": 0
                    }
                )
                if response.status_code == 200:
                    content = response.json()["choices"][0]["message"]["content"]
                    # For compatibility if needed, return an AIMessage
                    return AIMessage(content=content)
                else:
                    raise Exception(f"OpenRouter Error: {response.text}")

        self.llm = OpenRouterLLM(self.api_key, self.model_name)
        
        # We use create_sql_agent to translate natural language to SQL and get the answer.
        # But we also need the actual table data to export.
        # So we can instruct the LLM to output ONLY the SQL query, 
        # or we can write a custom chain that just generates the SQL.
        
        # A simpler robust approach for exporting data:
        # 1. Ask LLM to translate query to SQL
        # 2. Run the SQL with pandas and export
        # We can just fetch the schema manually or from self.db and use a standard prompt.

    def query_and_export(self, question: str) -> str:
        # Generate the SQL query based on the user's question
        # We tell the model to return ONLY the valid SQL.
        schema = self.db.get_table_info()
        prompt = f"""You are a PostgreSQL expert. Given an input question, create a syntactically correct PostgreSQL query to run.
Return ONLY the raw SQL query, no markdown formatting, no explanations, no wrapping in ```sql ... ```.

CRITICAL RULES:
1. ONLY use valid PostgreSQL syntax (e.g., use `CURRENT_DATE - INTERVAL '1 month'` instead of MySQL's `DATE_SUB()`).
2. ONLY use tables and columns that exist in the Schema below. Do not hallucinate table names.

EXAMPLES:
Question: What is the total fee collected last month?
SQL Query: SELECT SUM(amountpaid) FROM feepayments WHERE paymentdate >= CURRENT_DATE - INTERVAL '1 month';

Question: How many students are there?
SQL Query: SELECT COUNT(id) FROM students;

Schema:
{schema}

Question: {question}"""
        
        response = self.llm.invoke(prompt)
        import re
        
        # Try to extract SQL if the model wrapped it in markdown blocks
        match = re.search(r"```sql\s*(.*?)\s*```", response.content, re.DOTALL | re.IGNORECASE)
        if match:
            sql_query = match.group(1).strip()
        else:
            # Fallback: model might have just returned the query
            sql_query = response.content.strip()
            if sql_query.startswith("```"):
                sql_query = sql_query[3:].strip()
            if sql_query.endswith("```"):
                sql_query = sql_query[:-3].strip()
        
        try:
            # Execute the query and load into a pandas DataFrame
            df = pd.read_sql_query(sql_query, self.engine)
            
            if df.empty:
                return None
            
            # Export to Excel
            os.makedirs("./exports", exist_ok=True)
            export_filename = f"./exports/export_{uuid.uuid4().hex}.xlsx"
            df.to_excel(export_filename, index=False)
            
            return export_filename
        except Exception as e:
            print(f"Error executing SQL: {sql_query}")
            raise e
