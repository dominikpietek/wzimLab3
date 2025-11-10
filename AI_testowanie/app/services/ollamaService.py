import json
from typing import Any, Dict, Type
import requests
from pydantic import BaseModel
from app.config import settings
# from app.schema import NPCChatResponse


# schema: Dict[str, Any]
def buildPayload(systemPrompt: str, userPrompt: str) -> Dict[str, Any]:
    return {
        "model": settings.MODEL,
        "messages": [
            {"role": "system", "content": systemPrompt},
            {"role": "user", "content": userPrompt}
        ],
        "stream": False,
        "options": {"temperature": 0.3},
        "format": "json"  # schema
    }


def generateStructuredOutput(
    systemPrompt: str,
    userPrompt: str,
    responseModel: Type[BaseModel]
) -> Dict[str, Any]:

    schemaPrompt = f"""
    {systemPrompt}

    {responseModel.model_json_schema()}
    """

    payload = buildPayload(schemaPrompt, userPrompt)

    try:
        response = requests.post(
            settings.OLLAMA_URL, json=payload, timeout=50000)
        response.raise_for_status()

        data = response.json()
        contentStr = data.get("message", {}).get("content", "")

        if not contentStr:
            raise ValueError("Ollama has returned empty [] 'message.content'")

        parsedJson = json.loads(contentStr)

        validatedOutput = responseModel.model_validate(parsedJson)

        return validatedOutput.model_dump()

    except Exception as e:
        print(f"Error in Ollama Service: {e}")
        return {"error": str(e)}
    
