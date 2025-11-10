from pydantic import BaseModel, Field
from typing import List, Optional



# Npc Schemas
class NPCChatResponse(BaseModel):
    speech: str
    action: str
    intent: str
    # state_change: Optional[StateChange] = None


class NPCChatRequest(BaseModel):
    sceneContext: str = Field()
    playerText: str = Field()
    npcName: str = Field()
    npcRole: str = Field()


# Scene Schemas
class SceneLoadRequest(BaseModel):
    locationName: str = Field()
    scenePrompt: str = Field()

class SceneItem(BaseModel):
    name: str = Field()
    description: str = Field()


class SceneNPC(BaseModel):
    name: str = Field()
    role: str = Field()


class SceneLoadResponse(BaseModel):
    sceneDescription: str = Field()
    npcs: List[SceneNPC] = Field()
    items: List[SceneItem] = Field()


# SCHEMA_FOR_OLLAMA = {
#     "type": "object",
#     "properties": {
#         "speech": {"type": "string"},
#         "action": {"type": "string"},
#         "intent": {"type": "string"},
#         "state_change": {
#             "type": "object",
#             "properties": {
#                 "npc_morale": {"type": "integer"},
#                 "quest_flag": {"type": "string"}
#             },
#             "additionalProperties": True
#         }
#     },
#     "required": ["speech", "action", "intent"],
#     "additionalProperties": False
# }
