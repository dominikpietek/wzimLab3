import json
import ollama

#żeby uruchomić ten skrypt, upewnij się, że masz zainstalowaną bibliotekę ollama:
# pip install ollama
#oraz że masz uruchomiony lokalny model gpt-oss:20b za pomocą Ollama.

# 1. Wczytaj dane z pliku JSON
with open("input.json", "r", encoding="utf-8") as file:

    data = json.load(file)

prompt = data.get("detektyw", "")

# 2. Wyślij prompt do lokalnego modelu
response = ollama.chat(
    model="gpt-oss:20b",
    messages=[{"role": "user", "content": prompt}]
)

# 3. Pobierz treść odpowiedzi
answer = response["message"]["content"]

with open("output.json", "w", encoding="utf-8") as file:
    json.dump({"answer": answer}, file, ensure_ascii=False, indent=4)
