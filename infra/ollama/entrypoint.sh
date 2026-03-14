#!/bin/bash
set -e

ollama serve &
OLLAMA_PID=$!

echo "[ollama] Waiting for server to be ready..."
until curl -s http://localhost:11434 > /dev/null 2>&1; do
  sleep 1
done
echo "[ollama] Server is ready."

echo "[ollama] Pulling mistral..."
ollama pull mistral

echo "[ollama] Pulling nomic-embed-text..."
ollama pull nomic-embed-text

echo "[ollama] All models ready. Serving on :11434"
wait $OLLAMA_PID
