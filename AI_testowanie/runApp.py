import os
import subprocess
import sys

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

cmd = [sys.executable, "-m", "fastapi", "dev", "app/main.py"]

env = os.environ.copy()
env["PYTHONPATH"] = BASE_DIR

subprocess.run(cmd, cwd=BASE_DIR, env=env)
