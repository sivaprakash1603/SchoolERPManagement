import os
from langchain_community.document_loaders import TextLoader, PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_community.vectorstores import Chroma
from langchain_huggingface import HuggingFaceEmbeddings
from langchain_anthropic import ChatAnthropic
from langchain_core.prompts import ChatPromptTemplate
from langchain_core.runnables import RunnablePassthrough
from langchain_core.output_parsers import StrOutputParser

class RAGService:
    def __init__(self):
        self.persist_directory = "./data/chroma_db"
        
        self.embeddings = HuggingFaceEmbeddings(
            model_name="all-MiniLM-L6-v2"
        )
        
        # We need an HTTPX client pointing to the proxy
        proxy_url = os.environ.get("ANTHROPIC_BASE_URL", "https://api.anthropic.com")
        self.llm = ChatAnthropic(
            model="claude-haiku-4-5-20251001", 
            temperature=0, 
            max_tokens=1024,
            default_request_timeout=60,
            base_url=proxy_url
        )
        
        self.vectorstore = Chroma(
            persist_directory=self.persist_directory,
            embedding_function=self.embeddings
        )
        self.retriever = self.vectorstore.as_retriever(search_kwargs={"k": 4})
        
        template = """Answer the user's question clearly and politely based on the provided context. If the answer is not in the context, politely state that you do not have that information.

Format your response in Markdown.

User Role: {role}
(If applicable, tailor your response to the user's role. E.g. tell admins they can manage X, but tell students they can only view X).

Context:
{context}

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

    def _build_prompt(self, query: str, role: str):
        docs = self.retriever.invoke(query)
        context = "\n\n".join(doc.page_content for doc in docs)
        return self.prompt.format(context=context, question=query, role=role)

    def ask(self, query: str, role: str = "Guest") -> str:
        prompt_value = self._build_prompt(query, role)
        response = self.llm.invoke(prompt_value)
        return response.content
        
    def ask_stream(self, query: str, role: str = "Guest"):
        prompt_value = self._build_prompt(query, role)
        for chunk in self.llm.stream(prompt_value):
            if chunk.content:
                yield chunk.content
