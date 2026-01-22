import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch
from fastapi import FastAPI
import sys
import os

sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from app.api.npcRoutes import npcRouter
from app.api.sceneRoutes import sceneRouter
from app.config import settings
from app.services.gameState import gameState

app = FastAPI()
app.include_router(npcRouter)
app.include_router(sceneRouter)

client = TestClient(app)

@pytest.fixture(autouse=True)
def reset_game_state():
    """Resetuje stan gry przed kazdym testem"""
    gameState.currentSceneName = ""
    gameState.currentSceneDescription = ""
    gameState.currentNpcs = []
    gameState.currentItems = []
    gameState.chatHistory = []

@pytest.fixture
def loaded_scene():
    """Ładuje scene"""
    payload = {
        "name": "Biuro",
        "description": "Biuro detektywa",
        "npcs": [{"name": "Partner", "role": "Pomocnik", "description": "Twój wierny druh"}],
        "items": []
    }
    client.post("/scene/load", json=payload)

@pytest.fixture
def mock_ollama():
    """Mockuje wywolania do AI"""
    with patch("app.api.npcRoutes.generateStructuredOutput") as mock_npc:
        yield {"npc": mock_npc}

def test_npc_chat_success(loaded_scene, mock_ollama):
    """
    Sprawdza czy API poprawnie odbiera odpowiedź od AI i ja przekazuje
    """
    original_mock_setting = settings.USE_MOCK
    settings.USE_MOCK = False
    
    try:
        mock_ollama["npc"].return_value = {
            "speech": "Słucham cię uważnie."
        }

        payload = {
            "userText": "Masz chwilę?",
            "npcName": "Partner"
        }

        response = client.post("/npc/chat", json=payload)

        assert response.status_code == 200
        data = response.json()
        assert data["speech"] == "Słucham cię uważnie."
        mock_ollama["npc"].assert_called_once()

    finally:
        settings.USE_MOCK = original_mock_setting

def test_npc_chat_mock_mode(loaded_scene):
    """
    Sprawdza tryb Mock.
    """
    original_mock_setting = settings.USE_MOCK
    settings.USE_MOCK = True 
    
    try:
        payload = {
            "userText": "Cokolwiek",
            "npcName": "Partner"
        }
        
        response = client.post("/npc/chat", json=payload)
        
        assert response.status_code == 200
        data = response.json()
        # aiService zwraca konkretny ciąg znaków w trybie mock
        assert "[MOCK]" in data["speech"]
        assert "NPCChatResponse" in data["speech"]

    finally:
        settings.USE_MOCK = original_mock_setting

def test_scene_load_success():
    """
    Sprawdza czy /scene/load poprawnie zapisuje opis.
    """
    payload = {
        "name": "Zaułek",
        "description": "Mroczny zaułek spowity mgłą",
        "npcs": [],
        "items": []
    }

    response = client.post("/scene/load", json=payload)

    assert response.status_code == 200
    data = response.json()
    assert data["description"] == "Mroczny zaułek spowity mgłą"

def test_npc_chat_validation_error(loaded_scene):
    """Sprawdza błąd walidacji"""
    payload = {
        "userText": "Hej"
    }
    response = client.post("/npc/chat", json=payload)
    assert response.status_code == 422