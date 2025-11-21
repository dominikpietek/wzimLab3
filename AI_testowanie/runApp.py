import os
import subprocess
import sys
import webbrowser
import time
from pathlib import Path

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
UI_DIR = os.path.join(BASE_DIR, "UI_DEV")
HTML_FILE = os.path.join(UI_DIR, "index.html")


def main():

    mode_input = ""
    while mode_input.lower() not in ["1", "2"]:
        print("Wybierz tryb uruchomienia:")
        print("  1) Test Mode (z interfejsem webowym UI)")
        print("  2) Production Mode (tylko serwer API)")
        mode_input = input("Wpisz 1 lub 2: ")

    is_test_mode = mode_input == "1"

    env = os.environ.copy()
    env["PYTHONPATH"] = BASE_DIR

    processes = []

    try:
        print("🤖 Startuję Ollamę...")
        ollama_process = subprocess.Popen(
            ["ollama", "serve"],
            cwd=BASE_DIR,
            shell=True
        )
        processes.append(ollama_process)

        print("🚀 Startuję serwer FastAPI...")

        fastapi_cmd = [sys.executable, "-m", "fastapi", "dev", "app/main.py"]
        fastapi_process = subprocess.Popen(
            fastapi_cmd,
            cwd=BASE_DIR,
            env=env
        )
        processes.append(fastapi_process)

        print("⏳ Czekam, aż wszystko się uruchomi...")
        time.sleep(3)

        if is_test_mode:
            if os.path.exists(HTML_FILE):
                webbrowser.open('file://' + os.path.abspath(HTML_FILE))
                print(f"🌐 Otwieram interfejs w przeglądarce: {HTML_FILE}")
            else:
                print(
                    f"⚠️ Nie ma pliku: {HTML_FILE}. Upewnij się, że 'index.html' jest w folderze UI_DEV.")

        mode_name = "TEST" if is_test_mode else "PRODUCTION"
        print(
            f"\n✅ Uruchomiono w trybie {mode_name}. Naciśnij Ctrl+C, żeby zatrzymać.\n")

        fastapi_process.wait()

    except KeyboardInterrupt:
        print("\n🛑 Zatrzymuję serwisy...")
    finally:
        for p in processes:
            if p.poll() is None:
                p.terminate()
        print("👋 Koniec pracy!")


if __name__ == "__main__":
    main()
