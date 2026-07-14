import os
import psycopg2
from dotenv import load_dotenv

def create_readonly_user():
    # Load environment variables
    load_dotenv()
    
    # We expect the admin connection string to be in DB_CONNECTION_STRING
    # But for this script, we'll parse it to connect using psycopg2
    admin_conn_str = os.environ.get("DB_CONNECTION_STRING")
    if not admin_conn_str:
        print("Error: DB_CONNECTION_STRING not found in .env")
        return
        
    # Replace the SQLAlchemy prefix if present
    if admin_conn_str.startswith("postgresql+psycopg2://"):
        admin_conn_str = admin_conn_str.replace("postgresql+psycopg2://", "postgresql://")
        
    print(f"Connecting to database to create ai_readonly_user...")
    
    try:
        # Connect to the database
        conn = psycopg2.connect(admin_conn_str)
        conn.autocommit = True
        cursor = conn.cursor()
        
        # SQL Commands to create the read-only user and grant permissions
        commands = [
            # Check if role exists and drop it if we want to recreate (or just ignore if exists)
            "DO $$ BEGIN IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'ai_readonly_user') THEN CREATE ROLE ai_readonly_user WITH LOGIN PASSWORD 'secure_readonly_password123'; END IF; END $$;",
            
            # Grant connect to the database
            "GRANT CONNECT ON DATABASE \"SchoolERPSystem\" TO ai_readonly_user;",
            
            # Grant usage on the public schema
            "GRANT USAGE ON SCHEMA public TO ai_readonly_user;",
            
            # Grant select on all current tables
            "GRANT SELECT ON ALL TABLES IN SCHEMA public TO ai_readonly_user;",
            
            # Grant select on all future tables
            "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO ai_readonly_user;"
        ]
        
        for cmd in commands:
            print(f"Executing: {cmd[:50]}...")
            cursor.execute(cmd)
            
        print("Successfully created ai_readonly_user and granted read-only permissions!")
        
    except Exception as e:
        print(f"Error: {e}")
    finally:
        if 'cursor' in locals():
            cursor.close()
        if 'conn' in locals():
            conn.close()

if __name__ == "__main__":
    create_readonly_user()
