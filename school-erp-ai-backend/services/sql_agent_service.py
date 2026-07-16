import os
import uuid
import pandas as pd
from langchain_community.utilities import SQLDatabase
from langchain_anthropic import ChatAnthropic
from langchain_core.messages import SystemMessage, HumanMessage
import sqlalchemy

class SQLAgentService:
    def __init__(self):
        db_connection = os.environ.get("DB_CONNECTION_STRING")
        if not db_connection:
            raise ValueError("DB_CONNECTION_STRING environment variable not set")
            
        self.db = SQLDatabase.from_uri(db_connection)
        self.engine = sqlalchemy.create_engine(db_connection)
        
        # Fetch Anthropic configuration
        api_key = os.environ.get("ANTHROPIC_API_KEY", "")
        base_url = os.environ.get("ANTHROPIC_BASE_URL", "https://proxy.llm-gateway.ready.presidio.com")
        
        if not api_key:
            print("WARNING: ANTHROPIC_API_KEY is missing from environment variables.")
            
        # We use claude-haiku-4-5-20251001 via the proxy
        self.llm = ChatAnthropic(
            model="claude-haiku-4-5-20251001", 
            anthropic_api_key=api_key,
            base_url=base_url,
            temperature=0
        )

    def query_and_export(self, question: str) -> str:
        # Generate the SQL query based on the user's question
        schema = self.db.get_table_info()
        
        system_prompt = f"""You are a strict PostgreSQL database expert. Given an input question, create a syntactically correct PostgreSQL query to run.
Return ONLY the raw SQL query. Do NOT include markdown blocks, do NOT include explanations, do NOT wrap the query in ```sql ... ```.

CRITICAL RULES:
1. ONLY use valid PostgreSQL syntax (e.g., use `CURRENT_DATE - INTERVAL '1 month'`).
2. ONLY use tables and columns that exist in the Schema below. Do not hallucinate table names.
3. If the user asks for something completely unrelated to the database, return "SELECT 'Invalid Query';"

Schema:
{schema}"""
        
        messages = [
            SystemMessage(content=system_prompt),
            HumanMessage(content=f"Question: {question}\nSQL Query:")
        ]
        
        response = self.llm.invoke(messages)
        
        # Clean the response just in case the model ignored instructions
        sql_query = response.content.strip()
        import re
        match = re.search(r"```sql\s*(.*?)\s*```", sql_query, re.DOTALL | re.IGNORECASE)
        if match:
            sql_query = match.group(1).strip()
        else:
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
