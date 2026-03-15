FROM ollama/ollama:latest

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 11434

# llama3.2:1b fits in ~1.3GB RAM; limit parallel to avoid OOM
ENV OLLAMA_NUM_PARALLEL=1
ENV OLLAMA_MINSTABILITY=1

ENTRYPOINT ["/entrypoint.sh"]
