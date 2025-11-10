from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from api.npcRoutes import npcRouter
from api.sceneRoutes import sceneRouter
from config import settings

app = FastAPI()

app.include_router(npcRouter)
app.include_router(sceneRouter)


app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health")
def health():
    return {"Ok": True, "model": settings.MODEL, "ollama": "local"}
