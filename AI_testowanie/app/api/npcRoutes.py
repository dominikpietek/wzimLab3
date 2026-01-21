import json
from pathlib import Path
from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse

from app.services.gameState import gameState
from app.schema import NPCChatRequest, NPCChatResponse, VerdictRequest, VerdictResponse
from app.services.aiService import generateStream, generateStructuredOutput


npcRouter = APIRouter(prefix="/npc")
eventContext = """
W tym miejscu doszło do morderstwa. Znaleziono ciało. Wszyscy są zszokowani lub zdenerwowani obecnością policji.
System: [Przypomnienie: Reaguj emocjonalnie na morderstwo. Jeśli jesteś winny - kłam!]
System: [Prypomnienie: (Żadnych opisów myśli, żadnych opisów czynności, żadnego trzeciego osoba)(BARDZO WAŻNE)].
"""


@npcRouter.post("/chat", response_model=NPCChatResponse)
def chatWithNpc(data: NPCChatRequest):

    if not gameState.isSceneLoaded():
        raise HTTPException(
            status_code=400,
            detail="Błąd: Scena nie została załadowana. Użyj /scene/load."
        )
    transcript = gameState.getSceneTranscript()
    gameState.addMessage(speaker="Player", text=data.userText)

    currentNpcDesc = ""
    otherPeopleList = ""
    foundNpc = False

    for npc in gameState.currentNpcs:
        if npc.name == data.npcName:
            currentNpcDesc = f"Rola: {npc.role}. Opis: {npc.description}"
            foundNpc = True
        else:
            otherPeopleList += f"Imie: {npc.name}. Rola: {npc.role}. Opis: {npc.description}\n"

    if not foundNpc:
        print(f"[WARNING] NPC '{data.npcName}' nie znaleziono w scenie!")
        currentNpcDesc = "Rola: Nieznana. Opis: Brak danych."

    if not otherPeopleList:
        otherPeopleList = "Nikogo innego tu nie ma."

    itemsList = ""
    for item in gameState.currentItems:
        itemsList += f"- {item.name}: {item.description} (Wiedza/Wskazówka: {item.hints})\n"

    if not itemsList:
        itemsList = "Brak przedmiotów."

    systemPrompt = f"""
        # ROLA I TOŻSAMOŚĆ
        Nazywasz się {data.npcName}.
        Twoja historia: {currentNpcDesc}

        # WIEDZA (TWOJE WSPOMNIENIA)
        Co się stało: {eventContext}
        Lokalizacja: {gameState.currentSceneDescription}
        Twoja opinia: Znasz ofiarę. Wiesz, że nie żyje. Masz własne zdanie na ten temat.

        # ZASADY DYNAMIKI DIALOGU (BARDZO WAŻNE - ŻEBYŚ NIE BYŁ ROBOTEM)
        1. **ODPOWIADAJ NA KONKRETNE PYTANIE:**
        - Jeśli Detektyw pyta "Co się stało?" -> Opowiedz ogólnie o morderstwie.
        - Jeśli Detektyw pyta "Co z nim?" / "Jak zginął?" -> Podaj szczegóły (np. "Spłonął", "Dostał nożem" - zależnie od tego co wiesz/wymyślisz).
        - **NIE POWTARZAJ TEGO SAMEGO:** Nie mów ciągle tej samej regułki. Jeśli już powiedziałeś, że Stefan nie żyje, a gracz pyta dalej, dodaj nowe szczegóły.

        2. **STYL MOWY:**
        - Mów własnymi słowami! Nie recytuj encyklopedii.
        - Używaj krótkich zdań, pauz, pytań retorycznych.

        # TWOJA STRATEGIA (WINNY / NIEWINNY)
        - **Jeśli jesteś NIEWINNY:** Jesteś pomocny i przerażony. Chcesz gadać, spekulować, plotkować.
        - **Jeśli jesteś MORDERCĄ:** Udajesz pomocnego, ale Twoje odpowiedzi są wymijające. Możesz oskarżać innych.

        # INSTRUKCJA FORMATU (TYLKO MOWA)
        Jesteś generatorem dialogu.
        1. **Zero narracji:** Żadnych "powiedziałam", żadnych opisów czynności.
        2. **Czysty tekst:** Tylko to, co postać mówi na głos.
        3. **Płeć:** Pilnuj końcówek ({data.npcName}).

        # INTERAKCJA
        Rozmówca: Detektyw.
        Inni ludzie: {otherPeopleList if otherPeopleList else "Brak"}

        # OUTPUT (JSON)
        {{
        "speech": "Tutaj wpisz Twoją unikalną odpowiedź na ostatnie pytanie Detektywa."
        }}
        """

    userPrompt = f"Gracz pyta: \"{data.userText}\". Odpowiedz jako {data.npcName}."

    response = generateStructuredOutput(
        systemPrompt,
        userPrompt,
        NPCChatResponse
    )

    if "error" in response:
        raise HTTPException(status_code=502, detail=response)

    gameState.addMessage(speaker=data.npcName, text=response['speech'])
    return response


@npcRouter.post("/chat/stream")
def chatWithNpcStream(data: NPCChatRequest):

    if not gameState.isSceneLoaded():
        raise HTTPException(
            status_code=400,
            detail="Błąd: Scena nie została załadowana. Użyj /scene/load."
        )

    gameState.addMessage(speaker="Player", text=data.userText)
    transcript = gameState.getSceneTranscript()

    currentNpcDesc = ""
    otherPeopleList = ""
    foundNpc = False

    for npc in gameState.currentNpcs:
        if npc.name == data.npcName:
            currentNpcDesc = f"Rola: {npc.role}. Opis: {npc.description}\n"
            foundNpc = True
        else:
            otherPeopleList += f"Imie: {npc.name}. Rola: {npc.role}. Opis: {npc.description}\n"

    if not foundNpc:
        currentNpcDesc = "Rola: Nieznana. Opis: Brak danych."

    if not otherPeopleList:
        otherPeopleList = "Nikogo innego tu nie ma."

    itemsList = ""
    for item in gameState.currentItems:
        itemsList += f"{item.name}: {item.description} (Wiedza/Wskazówka: {item.hints})\n"

    if not itemsList:
        itemsList = "Brak przedmiotów."

    systemPrompt = f"""
        # ROLA I TOŻSAMOŚĆ
        Nazywasz się {data.npcName}.
        Twoja historia: {currentNpcDesc}

        # WIEDZA (TWOJE WSPOMNIENIA)
        Co się stało: {eventContext}
        Lokalizacja: {gameState.currentSceneDescription}
        Twoja opinia: Znasz ofiarę. Wiesz, że nie żyje. Masz własne zdanie na ten temat.

        # ZASADY DYNAMIKI DIALOGU (BARDZO WAŻNE - ŻEBYŚ NIE BYŁ ROBOTEM)
        1. **ODPOWIADAJ NA KONKRETNE PYTANIE:**
        - Jeśli Detektyw pyta "Co się stało?" -> Opowiedz ogólnie o morderstwie.
        - Jeśli Detektyw pyta "Co z nim?" / "Jak zginął?" -> Podaj szczegóły (np. "Spłonął", "Dostał nożem" - zależnie od tego co wiesz/wymyślisz).
        - **NIE POWTARZAJ TEGO SAMEGO:** Nie mów ciągle tej samej regułki. Jeśli już powiedziałeś, że Stefan nie żyje, a gracz pyta dalej, dodaj nowe szczegóły.

        2. **STYL MOWY:**
        - Mów własnymi słowami! Nie recytuj encyklopedii.
        - Używaj krótkich zdań, pauz, pytań retorycznych.

        # TWOJA STRATEGIA (WINNY / NIEWINNY)
        - **Jeśli jesteś NIEWINNY:** Jesteś pomocny i przerażony. Chcesz gadać, spekulować, plotkować.
        - **Jeśli jesteś MORDERCĄ:** Udajesz pomocnego, ale Twoje odpowiedzi są wymijające. Możesz oskarżać innych.

        # INSTRUKCJA FORMATU (TYLKO MOWA)
        Jesteś generatorem dialogu.
        1. **Zero narracji:** Żadnych "powiedziałam", żadnych opisów czynności.
        2. **Czysty tekst:** Tylko to, co postać mówi na głos.
        3. **Płeć:** Pilnuj końcówek ({data.npcName}).

        # INTERAKCJA
        Rozmówca: Detektyw.
        Inni ludzie: {otherPeopleList if otherPeopleList else "Brak"}

        # OUTPUT (JSON)
        {{
        "speech": "Tutaj wpisz Twoją unikalną odpowiedź na ostatnie pytanie Detektywa."
        }}
        """
    userPrompt = f"Gracz pyta: \"{data.userText}\". Odpowiedz jako {data.npcName}."

    def response_generator():
        fullResponseText = ""

        streamIterator = generateStream(systemPrompt, userPrompt)

        for chunk in streamIterator:
            yield chunk

            try:
                if chunk.startswith("data: "):
                    jsonStr = chunk.replace("data: ", "").strip()
                    dataObj = json.loads(jsonStr)

                    if "token" in dataObj:
                        fullResponseText += dataObj["token"]
            except Exception:
                pass

        if fullResponseText:
            gameState.addMessage(speaker=data.npcName, text=fullResponseText)

    return StreamingResponse(
        response_generator(),
        media_type="text/event-stream"
    )


@npcRouter.post("/verdict", response_model=VerdictResponse)
def getGameVerdict(request: VerdictRequest):

    selectedEnding = None

    for ending in request.endings:
        if ending.accusedName.lower() in request.accusedName.lower() or \
                request.accusedName.lower() in ending.accusedName.lower():
            selectedEnding = ending
            break

    if not selectedEnding:
        return {
            "speech": f"Nie znaleziono zakończenia dla osoby: {request.accusedName}. Sprawdź dane.",
            "isPlayerRight": False
        }

    systemPrompt = f"""
    Jesteś sędzią i narratorem finału gry kryminalnej.
    
    DANE ZAKOŃCZENIA:
    Oskarżony: {selectedEnding.accusedName}
    Czy to poprawny sprawca? {'TAK (WYGRANA)' if selectedEnding.isMurderer else 'NIE (PRZEGRANA)'}
    Opis sytuacji: {selectedEnding.description}
    
    ZADANIE:
    Napisz krótkie podsumowanie dla gracza (maksymalnie 3 zdania).
    Opisz konsekwencje wyboru na podstawie "Opisu sytuacji".
    Bądź surowy i klimatyczny.
    
    Odpowiedz JSONem: {{"speech": "Twoje podsumowanie...", "isPlayerRight": {str(selectedEnding.isMurderer).lower()}}}
    """

    userPrompt = "Wydaj werdykt."

    try:
        response = generateStructuredOutput(
            systemPrompt,
            userPrompt,
            VerdictResponse
        )

        response['isPlayerRight'] = selectedEnding.isMurderer
        return response

    except Exception:

        return {
            "speech": selectedEnding.description,
            "isPlayerRight": selectedEnding.isMurderer
        }
