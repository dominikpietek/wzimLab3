from pathlib import Path
from fastapi import APIRouter, HTTPException
from app.schema import SceneLoadRequest, SceneLoadResponse
from app.services.ollamaService import generateStructuredOutput
from app.config import settings

sceneRouter = APIRouter(prefix="/scene")

@sceneRouter.post("/load", response_model=SceneLoadResponse)
def load_scene(data: SceneLoadRequest):
    print("Load Scene Called")

    try:
        scenario_id = data.scenarioID 
        base_path = settings.PARENT_DIR / "assets" / scenario_id
        
        scenario_lore_path = base_path / "scenarioLore.txt"
        system_prompt_path = base_path / "systemPrompt.txt"

        SCENARIO_LORE = scenario_lore_path.read_text(encoding="utf-8")
        SCENARIO_PROMPT = system_prompt_path.read_text(encoding="utf-8")
        
    except FileNotFoundError:
        raise HTTPException(status_code=404, detail=f"Scenariusz '{data.scenarioID}' nie istnieje lub brak plików scenarioLore.txt/systemPrompt.txt.")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

    systemPrompt = f"""
    Jesteś kreatywnym Mistrzem Gry. Twoim zadaniem jest wygenerowanie sceny do gry wideo po polsku.
    Scena musi być zgodna z globalną fabułą.
    
    {SCENARIO_PROMPT}

    GLOBALNA FABUŁA (SZCZEGÓŁY):
    <lore>
    {SCENARIO_LORE}
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

    print("Test:", response)
    if "error" in response:
        raise HTTPException(status_code=502, detail=response)

    return response