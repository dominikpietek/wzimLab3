

from pathlib import Path


class Settings():
    OLLAMA_URL: str = "http://127.0.0.1:11434/api/chat"
    MODEL: str = "game-npc-model:latest"
    PARENT_DIR = Path(__file__).resolve().parents[0]

settings = Settings()
