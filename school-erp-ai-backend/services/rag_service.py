import os
from langchain_community.document_loaders import TextLoader, PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_community.vectorstores import Chroma
from langchain_ollama import OllamaEmbeddings, ChatOllama
from langchain_core.prompts import ChatPromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
import requests
from langchain_core.messages import AIMessage

class RAGService:
    def __init__(self):
        self.persist_directory = "./data/chroma_db"
        
        ollama_base_url = os.environ.get("OLLAMA_BASE_URL", "http://localhost:11434")
        self.model_name = os.environ.get("OLLAMA_MODEL", "llama3")
        
        self.embeddings = OllamaEmbeddings(
            base_url=ollama_base_url,
            model=self.model_name
        )
        
        self.api_key = os.environ.get("OPENROUTER_API_KEY", "")
        self.chat_model_name = os.environ.get("OLLAMA_MODEL", "meta-llama/llama-3-8b-instruct:free")
        
        def call_openrouter(prompt_value):
            if not self.api_key or self.api_key == "sk-or-v1-put-your-key-here":
                raise Exception("Please put your OpenRouter API key in the .env file")
                
            messages = [{"role": m.type if m.type != "human" else "user", "content": m.content} for m in prompt_value.to_messages()]
            
            response = requests.post(
                url="https://openrouter.ai/api/v1/chat/completions",
                headers={"Authorization": f"Bearer {self.api_key}"},
                json={
                    "model": self.chat_model_name,
                    "messages": messages,
                    "temperature": 0
                }
            )
            if response.status_code == 200:
                return AIMessage(content=response.json()["choices"][0]["message"]["content"])
            else:
                raise Exception(f"OpenRouter Error: {response.text}")

        self.llm = RunnableLambda(call_openrouter)
        
        self.vectorstore = Chroma(
            persist_directory=self.persist_directory,
            embedding_function=self.embeddings
        )
        self.retriever = self.vectorstore.as_retriever(search_kwargs={"k": 3})
        
        template = """Answer the question strictly based on the following context. If you cannot answer based on the context, say "I don't have that information in my FAQ documents."
        
Context: {context}

Question: {question}

Answer:"""
        self.prompt = ChatPromptTemplate.from_template(template)
        
    def ingest_document(self, filepath: str):
        if filepath.endswith('.pdf'):
            loader = PyPDFLoader(filepath)
        else:
            loader = TextLoader(filepath)
            
        docs = loader.load()
        
        text_splitter = RecursiveCharacterTextSplitter(chunk_size=1000, chunk_overlap=200)
        splits = text_splitter.split_documents(docs)
        
        self.vectorstore.add_documents(splits)
        # In newer versions of Chroma, persistence is automatic, but we can call it if needed depending on version.

    def ask(self, query: str) -> str:
        def format_docs(docs):
            return "\n\n".join(doc.page_content for doc in docs)

        rag_chain = (
            {"context": self.retriever | format_docs, "question": RunnablePassthrough()}
            | self.prompt
            | self.llm
            | StrOutputParser()
        )
        
        return rag_chain.invoke(query)
