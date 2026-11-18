"""Ouvre le parcours IA Windows avec son service local."""

import argparse
import http.client
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time


ROOT = Path(__file__).resolve().parents[2]
PLAYER_RELATIVE_PATH = Path("ConnectionTest/Builds/Windows-IA/CloudVR-IA.exe")
RAG_PORT = 8765
OLLAMA_PORT = 11434
DEFAULT_MODEL = "qwen3:1.7b"


def find_player(explicit_path):
    if explicit_path:
        candidates = [explicit_path]
    else:
        candidates = [ROOT / PLAYER_RELATIVE_PATH]
        if git := shutil.which("git"):
            worktrees = subprocess.run(
                [git, "-C", str(ROOT), "worktree", "list", "--porcelain", "-z"],
                stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, timeout=4,
            )
            for block in worktrees.stdout.decode("utf-8", errors="replace").split("\0\0"):
                fields = block.split("\0")
                if fields[0].startswith("worktree ") and "branch refs/heads/codex/cloudvr-v2" in fields:
                    candidates.append(Path(fields[0][len("worktree "):]) / PLAYER_RELATIVE_PATH)
    for player in candidates:
        if player.is_file() and player.with_name(player.stem + "_Data").is_dir():
            return player.resolve()
    if explicit_path:
        raise RuntimeError(f"Build incomplet ou absent à l’emplacement {explicit_path}.")
    raise RuntimeError(
        "Build Windows IA introuvable. Construire le player depuis le menu "
        "CloudVR > V2 > Construire le player Windows IA, ou indiquer le fichier avec --player."
    )


def local_get(port, path):
    connection = http.client.HTTPConnection("127.0.0.1", port, timeout=1.5)
    try:
        try:
            connection.connect()
        except (ConnectionRefusedError, TimeoutError):
            return None
        connection.request("GET", path)
        response = connection.getresponse()
        body = response.read(65537)
        if len(body) > 65536:
            raise ValueError("Réponse trop grande")
        return response.status, json.loads(body)
    except (OSError, http.client.HTTPException, ValueError) as error:
        raise RuntimeError(f"Le port local {port} répond de façon inattendue.") from error
    finally:
        connection.close()


def rag_model():
    answer = local_get(RAG_PORT, "/health")
    if answer is None:
        return None
    status, body = answer
    if status == 200 and isinstance(body, dict) and body.get("service") == "cloudvr-rag":
        model = body.get("model")
        if isinstance(model, str) and model:
            return model
    if status == 404:
        legacy = local_get(RAG_PORT, "/bilan/envoi")
        if legacy and legacy[0] == 200 and isinstance(legacy[1], dict):
            result = legacy[1]
            if result.get("fiction") is True and result.get("etape") == "envoi" and "joueur_pc_et_box" in result:
                print("Service CloudVR existant (ancienne version). Vérification du modèle par défaut.")
                return DEFAULT_MODEL
    raise RuntimeError(f"Le port {RAG_PORT} est occupé par un autre service.")


def ollama_models():
    answer = local_get(OLLAMA_PORT, "/api/tags")
    if answer is None:
        return None
    status, body = answer
    if status == 200 and isinstance(body, dict) and isinstance(body.get("models"), list):
        return {entry.get("name") for entry in body["models"] if isinstance(entry, dict)}
    raise RuntimeError(f"Le port {OLLAMA_PORT} ne donne pas accès aux modèles d’Ollama.")


def wait_until_ready(probe, name, log):
    deadline = time.monotonic() + 20
    while time.monotonic() < deadline:
        result = probe()
        if result is not None:
            return result
        time.sleep(.25)
    raise RuntimeError(f"{name} reste indisponible. Consulter {log}.")


def start_background(arguments, log, env=None):
    log.parent.mkdir(parents=True, exist_ok=True)
    with log.open("ab") as output:
        subprocess.Popen(
            arguments, cwd=ROOT, stdin=subprocess.DEVNULL, stdout=output,
            stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NO_WINDOW, env=env,
        )


def prepare_services():
    current_model = rag_model()
    models = ollama_models()
    logs = ROOT / "tmp/cloudvr-ia"
    if models is None:
        ollama = shutil.which("ollama")
        if not ollama and (local := os.environ.get("LOCALAPPDATA")):
            executable = Path(local) / "Programs/Ollama/ollama.exe"
            if executable.is_file():
                ollama = str(executable)
        if not ollama:
            raise RuntimeError("Installer Ollama pour Windows, puis relancer CloudVR.")
        print("Démarrage d’Ollama.")
        log = logs / "ollama.log"
        start_background([ollama, "serve"], log, {**os.environ, "OLLAMA_HOST": f"127.0.0.1:{OLLAMA_PORT}"})
        models = wait_until_ready(ollama_models, "Ollama", log)
    else:
        print("Ollama déjà disponible.")

    model = current_model or DEFAULT_MODEL
    installed_name = model if ":" in model.rsplit("/", 1)[-1] else f"{model}:latest"
    if installed_name not in models:
        raise RuntimeError(f"Modèle absent. Dans un terminal, exécuter « ollama pull {model} », puis relancer.")
    print(f"Modèle disponible · {model}")
    if current_model is None:
        print("Démarrage du service CloudVR.")
        log = logs / "rag.log"
        start_background(
            [sys.executable, "-X", "utf8", "-B", str(Path(__file__).with_name("service.py")), "--model", model], log,
        )
        wait_until_ready(rag_model, "Le service CloudVR", log)
    else:
        print("Service CloudVR déjà disponible.")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--player", type=Path, help="Chemin d’un build Windows IA")
    parser.add_argument("--check", action="store_true", help="Vérifier le build et préparer les services")
    options = parser.parse_args()
    try:
        if os.name != "nt" or sys.version_info < (3, 9):
            raise RuntimeError("Windows et Python 3.9 ou plus récent sont requis.")
        player = find_player(options.player)
        prepare_services()
        if not options.check:
            subprocess.Popen([str(player), "--cloudvr-ai"], cwd=player.parent)
        print("CloudVR IA prêt." if options.check else "CloudVR IA lancé.")
        return 0
    except (RuntimeError, OSError, subprocess.SubprocessError) as error:
        print(f"CloudVR IA · {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
