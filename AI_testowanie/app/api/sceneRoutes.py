from pathlib import Path
from fastapi import APIRouter, HTTPException

from app.schema import SceneLoadRequest, SceneLoadResponse
from app.services.ollamaService import generateStructuredOutput
from config import settings

try:

    SCENARIO = (settings.PARENT_DIR / "assets" /
                "scenarioLore.txt").read_text(encoding="utf-8")
except Exception as e:
    print(f"Error in Scene Router:\n${e}")

sceneRouter = APIRouter(prefix="/scene")


@sceneRouter.post("/load", response_model=SceneLoadResponse)
def load_scene(data: SceneLoadRequest):
    print("Load Scene Called")
    systemPrompt = f"""
    Jesteś kreatywnym Mistrzem Gry. Twoim zadaniem jest wygenerowanie sceny do gry wideo po polsku.
    Scena musi być zgodna z globalną fabułą.
    
    GLOBALNA FABUŁA:
    <lore>
    {SCENARIO}
    </lore>
    """

    userPrompt = f"""
    Wygeneruj scenę dla lokalizacji: "{data.locationName}".
    Gracz właśnie wchodzi.
    Kontekst sceny: "{data.scenePrompt}".
    
    Wygeneruj porywający opis, 2-3 postacie NPC i 1-2 interaktywne przedmioty.
    """

    response = generateStructuredOutput(
        systemPrompt,
        userPrompt,
        SceneLoadResponse
    )

    print("Тест:", response)
    if "error" in response:
        raise HTTPException(status_code=502, detail=response)

    return response
