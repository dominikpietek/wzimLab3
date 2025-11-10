# 🧠 Gra Detektywistyczna — API z LLM NPC

## ⚙️ 1. Wymagania i konfiguracja Ollama

Projekt wymaga zainstalowanego Ollama oraz modelu **gpt-oss:20b**.

### 🔹 Instalacja Ollama:
Pobierz i zainstaluj Ollama zgodnie z instrukcjami: https://ollama.com

### 🔹 Pobranie modelu GPT:
```bash
ollama pull gpt-oss:20b
```

### 🔧 Stwórz model gry:
Projekt wymaga specjalnie przygotowanego modelu, który zawiera systemowe instrukcje używane przez NPC.  
Definicja modelu znajduje się w pliku **`Modelfile`** w katalogu głównym projektu.
```bash
ollama create game-npc-model -f Modelfile
```

### 🔹 Uruchomienie serwera Ollama:
Przed uruchomieniem aplikacji FastAPI, należy oddzielnie uruchomić serwer Ollama:
```bash
ollama serve
```
Upewnij się, że serwer działa pod adresem domyślnym `http://127.0.0.1:11434`.

### ✅ Weryfikacja:
Upewnij się, że serwer Ollama działa poprawnie:
```bash
ollama list
```
Powinieneś zobaczyć na liście model `game-npc-model:latest`.

---

## 🚀 Instalacja i Uruchomienie

### 1️⃣ Zainstaluj zależności:
```bash
pip install -r requirements.txt
```

### 2️⃣ Uruchom serwer FastAPI:
Użyj przygotowanego skryptu:
```bash
python runApp.py
```

Serwer uruchomi się pod adresem:  
👉 `http://127.0.0.1:8000`

Interaktywna dokumentacja API (Swagger UI):  
🌐 `http://127.0.0.1:8000/docs`

---

## 📜 Punkty Końcowe API

| Endpoint       | Metoda | Opis                                                       |
|----------------|--------|------------------------------------------------------------|
| `/npc/chat`    | POST   | Generuje odpowiedź od NPC (mowa, akcja, intencja).         |
| `/scene/load`  | POST   | Generuje nową scenę (opis, NPC, przedmioty).               |
| `/health`      | GET    | Sprawdza stan serwera. |