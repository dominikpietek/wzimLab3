from pathlib import Path
from fastapi import APIRouter, HTTPException

from app.schema import NPCChatRequest, NPCChatResponse
from app.services.ollamaService import generateStructuredOutput
from config import settings


try:

    SCENARIO = (settings.PARENT_DIR / "assets" /
                "scenarioLore.txt").read_text(encoding="utf-8")
    NPCPERSONA = (settings.PARENT_DIR / "assets" /
                  "npcPersona.txt").read_text(encoding="utf-8")
except Exception as e:
    print(f"Error in npc Router:\n${e}")
    raise HTTPException(status_code=502, detail=e)

npcRouter = APIRouter(prefix="/npc")


@npcRouter.post("/chat", response_model=NPCChatResponse)
def chatWithNpc(data: NPCChatRequest):

    system_prompt = f"""
      {NPCPERSONA}

      GLOBALNA FABUŁA:
      <lore>
      {SCENARIO}
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
    print("Тест:", response)

    if "error" in response:
        raise HTTPException(status_code=502, detail=response)

    return response
