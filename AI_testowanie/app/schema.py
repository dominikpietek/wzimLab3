from pydantic import BaseModel, Field
from typing import List, Optional


# Npc Schemas


class NPCChatRequest(BaseModel):
    sceneDescription: str = Field()
    userText: str = Field()
    npcName: str = Field()


class NPCChatResponse(BaseModel):
    speech: str = Field()
    action: str = Field()
    intent: str = Field()
    # state_change: Optional[StateChange] = None

# Scene Schemas


class SceneLoadRequest(BaseModel):
    name: str = Field()
    description: str = Field()
    npcs: List[SceneNPC] = Field()
    items: List[SceneItem] = Field()


class SceneLoadResponse(BaseModel):
    extendedDescription: str = Field()


class SceneItem(BaseModel):
    name: str = Field()
    description: str = Field()
    hints: str = Field(
        description="Opis stanu przedmiotu, który może wpłynąć na fabułę, np. zamek został przecięty, drzwi wybite")


class SceneNPC(BaseModel):
    name: str = Field()
    role: str = Field()
    description: str = Field()
