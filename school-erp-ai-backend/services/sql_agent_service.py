import os
import uuid
import pandas as pd
from langchain_community.utilities import SQLDatabase
from langchain_ollama import ChatOllama
from langchain_community.agent_toolkits import create_sql_agent
from langchain.agents.agent_types import AgentType
import sqlalchemy

class SQLAgentService:
    def __init__(self):
        db_connection = os.environ.get("DB_CONNECTION_STRING")
        if not db_connection:
            raise ValueError("DB_CONNECTION_STRING environment variable not set")
            
        self.db = SQLDatabase.from_uri(db_connection)
        self.engine = sqlalchemy.create_engine(db_connection)
        
        ollama_base_url = os.environ.get("OLLAMA_BASE_URL", "http://localhost:11434")
        model_name = os.environ.get("OLLAMA_MODEL", "llama3")
        
        self.llm = ChatOllama(
            base_url=ollama_base_url,
            model=model_name,
            temperature=0
        )
        
        # We use create_sql_agent to translate natural language to SQL and get the answer.
        # But we also need the actual table data to export.
        # So we can instruct the LLM to output ONLY the SQL query, 
        # or we can write a custom chain that just generates the SQL.
        
        # A simpler robust approach for exporting data:
        # 1. Ask LLM to translate query to SQL
        # 2. Run the SQL with pandas and export
        from langchain.chains import create_sql_query_chain
        self.query_chain = create_sql_query_chain(self.llm, self.db)

    def query_and_export(self, question: str) -> str:
        # Generate the SQL query based on the user's question
        # We tell the model to return ONLY the valid SQL.
        prompt = f"{question}. Return ONLY the raw SQL query, no markdown formatting, no explanations."
        
        response = self.query_chain.invoke({"question": prompt})
        
        # Clean up the response in case the model added markdown blocks anyway
        sql_query = response.strip()
        if sql_query.startswith("```sql"):
            sql_query = sql_query[6:]
        if sql_query.startswith("```"):
            sql_query = sql_query[3:]
        if sql_query.endswith("```"):
            sql_query = sql_query[:-3]
        
        sql_query = sql_query.strip()
        
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
