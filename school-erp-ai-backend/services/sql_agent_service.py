import os
import re
import uuid
import pandas as pd
import sqlalchemy

from langchain_community.utilities import SQLDatabase
from langchain_anthropic import ChatAnthropic
from langchain_core.messages import SystemMessage, HumanMessage


class SQLAgentService:
    def __init__(self):
        db_connection = os.environ.get("DB_CONNECTION_STRING")

        if not db_connection:
            raise ValueError("DB_CONNECTION_STRING environment variable not set")

        self.db = SQLDatabase.from_uri(db_connection)
        self.engine = sqlalchemy.create_engine(db_connection)

        api_key = os.environ.get("ANTHROPIC_API_KEY")
        base_url = os.environ.get(
            "ANTHROPIC_BASE_URL",
            "https://proxy.llm-gateway.ready.presidio.com",
        )

        self.llm = ChatAnthropic(
            model="claude-haiku-4-5-20251001",
            anthropic_api_key=api_key,
            base_url=base_url,
            temperature=0,
        )

    ###########################################################################
    # STEP 1 - FIND RELEVANT TABLES
    ###########################################################################

    def get_relevant_tables(self, question: str):
        """
        Uses the LLM to determine which tables are needed.
        """

        available_tables = sorted(list(self.db.get_usable_table_names()))

        prompt = f"""
You are a PostgreSQL database expert.

Your job is to identify ONLY the tables required to answer the user's question.

Available Tables

{", ".join(available_tables)}

Rules

1. Return ONLY comma separated table names.
2. Never explain.
3. Never invent table names.
4. If the question is unrelated return EXACTLY

INVALID_QUERY

Example

students,fee_payment,fees
"""

        messages = [
            SystemMessage(content=prompt),
            HumanMessage(content=question),
        ]

        response = self.llm.invoke(messages)

        result = response.content.strip()

        if "INVALID_QUERY" in result.upper():
            raise ValueError(
                "I can only answer questions related to the School ERP database."
            )

        selected = [x.strip() for x in result.split(",") if x.strip()]

        valid_tables = []

        for table in selected:
            if table in available_tables:
                valid_tables.append(table)

        if len(valid_tables) == 0:
            raise ValueError("Unable to determine relevant tables.")

        return valid_tables

    ###########################################################################
    # STEP 2 - GENERATE SQL
    ###########################################################################

    def generate_sql(self, question: str, schema: str):

        prompt = f"""
You are an expert PostgreSQL SQL Generator.

Generate ONLY SQL.

DO NOT explain.
DO NOT use markdown.
DO NOT wrap inside ```.

Rules

1. Generate ONLY PostgreSQL syntax.
2. Use ONLY tables and columns in the schema.
3. Never hallucinate columns.
4. Return INVALID_QUERY if impossible.
5. Generate ONLY SELECT or WITH queries.

Schema

{schema}
"""

        messages = [
            SystemMessage(content=prompt),
            HumanMessage(content=question),
        ]

        response = self.llm.invoke(messages)

        sql = response.content.strip()

        # Remove markdown if Claude accidentally returns it

        match = re.search(
            r"```sql\s*(.*?)\s*```",
            sql,
            re.DOTALL | re.IGNORECASE,
        )

        if match:
            sql = match.group(1).strip()

        sql = sql.replace("```", "").strip()

        if "INVALID_QUERY" in sql.upper():
            raise ValueError(
                "I can only answer questions related to the School ERP database."
            )

        sql_lower = sql.lower()

        if not (
            sql_lower.startswith("select")
            or sql_lower.startswith("with")
        ):
            raise ValueError(
                "Only SELECT queries are permitted."
            )

        return sql

    ###########################################################################
    # STEP 3 - EXECUTE SQL
    ###########################################################################

    def execute_query(self, sql_query: str):

        try:
            df = pd.read_sql_query(sql_query, self.engine)
            return df

        except Exception as ex:

            print(sql_query)

            raise ValueError(
                f"Database execution failed.\n\n{str(ex)}"
            )

    ###########################################################################
    # STEP 4 - EXPORT TO EXCEL
    ###########################################################################

    def export_to_excel(self, dataframe):

        if dataframe.empty:
            return None

        os.makedirs("./exports", exist_ok=True)

        filename = f"./exports/export_{uuid.uuid4().hex}.xlsx"

        dataframe.to_excel(filename, index=False)

        return filename

    ###########################################################################
    # MAIN METHOD
    ###########################################################################

    def query_and_export(self, question: str):

        # -----------------------------
        # Find relevant tables
        # -----------------------------

        relevant_tables = self.get_relevant_tables(question)

        print("\nRelevant Tables")
        print(relevant_tables)

        # -----------------------------
        # Load ONLY those schemas
        # -----------------------------

        schema = self.db.get_table_info(relevant_tables)

        # -----------------------------
        # Generate SQL
        # -----------------------------

        sql_query = self.generate_sql(question, schema)

        print("\nGenerated SQL")
        print(sql_query)

        # -----------------------------
        # Execute
        # -----------------------------

        dataframe = self.execute_query(sql_query)

        # -----------------------------
        # Export
        # -----------------------------

        export_file = self.export_to_excel(dataframe)

        return export_file