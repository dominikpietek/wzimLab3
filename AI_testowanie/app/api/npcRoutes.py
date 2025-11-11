from pathlib import Path
from fastapi import APIRouter, HTTPException
from app.schema import NPCChatRequest, NPCChatResponse
from app.services.ollamaService import generateStructuredOutput
from app.config import settings 

npcRouter = APIRouter(prefix="/npc")

try:
    NPCPERSONA = (settings.PARENT_DIR / "assets" /
                  "npcPersona.txt").read_text(encoding="utf-8")
except Exception as e:
    print(f"KRYTYCZNY BŁĄD: Nie można wczytać globalnych reguł npcPersona.txt: {e}")
    raise HTTPException(status_code=500, detail=f"Brak pliku npcPersona.txt: {e}")


@npcRouter.post("/chat", response_model=NPCChatResponse)
def chatWithNpc(data: NPCChatRequest):
    
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

    system_prompt = f"""
      {NPCPERSONA}

      {SCENARIO_PROMPT}

      GLOBALNA FABUŁA (SZCZEGÓŁY):
      <lore>
      {SCENARIO_LORE}
      </lore>

      BIEŻĄCA SCENA:
      <scene>
      {data.sceneContext}
      </scene>

      TWOJA POSTAĆ:
      Imię: {data.npcName}
      Rola: {data.npcRole}
     """

    user_prompt = f"Gracz mówi do Ciebie: \"{data.playerText}\""

    response = generateStructuredOutput(
        system_prompt,
        user_prompt,
        NPCChatResponse
    )
    
    print("Test:", response)
    if "error" in response:
        raise HTTPException(status_code=502, detail=response)
    return response