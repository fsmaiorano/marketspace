#!/bin/bash
set -e

ollama serve &
OLLAMA_PID=$!

echo "[ollama] Waiting for server to be ready..."
until curl -s http://localhost:11434 > /dev/null 2>&1; do
  sleep 1
done
echo "[ollama] Server is ready."

# llama3.2:1b - 1.3GB RAM, fits in 2GB+, modern & capable
echo "[ollama] Pulling llama3.2:1b (1.3GB)..."
ollama pull llama3.2:1b

echo "[ollama] Pulling nomic-embed-text (600MB)..."
ollama pull nomic-embed-text

# Warm-up to load model into memory before first request
echo "[ollama] Warming up models..."
echo "hi" | ollama run llama3.2:1b > /dev/null 2>&1 || true

echo "[ollama] All models ready. Serving on :11434"
wait $OLLAMA_PID
