class GenericSSEClient {
    constructor() {
        this.activeConnections = new Map();
    }

    /**
     * Start an operation and monitor it via SSE
     * @param {string} controllerPath - API path (e.g., 'api/catalog', 'api/order')
     * @param {string} operationType - Type of operation
     * @param {object} parameters - Operation parameters
     * @param {object} callbacks - Event callbacks
     */
    async startOperation(controllerPath, operationType, parameters, callbacks = {}) {
        try {
            // Start the operation
            const response = await fetch(`/${controllerPath}/start-operation`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    operationType,
                    parameters
                })
            });

            const result = await response.json();

            if (result.success) {
                // Start SSE monitoring
                this.connectSSE(controllerPath, result.operationId, callbacks);
                return result;
            } else {
                throw new Error(result.message);
            }
        } catch (error) {
            console.error('Failed to start operation:', error);
            throw error;
        }
    }

    /**
     * Connect to SSE stream for an operation
     */
    connectSSE(controllerPath, operationId, callbacks = {}) {
        const sseUrl = `/${controllerPath}/stream/${operationId}`;
        const eventSource = new EventSource(sseUrl);

        this.activeConnections.set(operationId, eventSource);

        eventSource.addEventListener('status', (event) => {
            try {
                const data = JSON.parse(event.data);
                if (callbacks.onStatusUpdate) {
                    callbacks.onStatusUpdate(operationId, data);
                }
                this.dispatchCustomEvent('sseStatusUpdate', {operationId, data, controllerPath});
            } catch (error) {
                console.error('Error parsing SSE status:', error);
            }
        });

        eventSource.addEventListener('complete', (event) => {
            try {
                const data = JSON.parse(event.data);
                if (callbacks.onComplete) {
                    callbacks.onComplete(operationId, data);
                }
                this.dispatchCustomEvent('sseComplete', {operationId, data, controllerPath});

                eventSource.close();
                this.activeConnections.delete(operationId);
            } catch (error) {
                console.error('Error parsing SSE complete:', error);
            }
        });

        eventSource.addEventListener('error', (event) => {
            console.error('SSE connection error for operation:', operationId);
            if (callbacks.onError) {
                callbacks.onError(operationId, 'Connection error');
            }
            this.dispatchCustomEvent('sseError', {operationId, error: 'Connection error', controllerPath});

            eventSource.close();
            this.activeConnections.delete(operationId);
        });

        // Timeout protection
        setTimeout(() => {
            if (this.activeConnections.has(operationId)) {
                console.warn('SSE timeout for operation:', operationId);
                eventSource.close();
                this.activeConnections.delete(operationId);
            }
        }, 300000); // 5 minutes timeout
    }

    /**
     * Stop monitoring an operation
     */
    stopMonitoring(operationId) {
        if (this.activeConnections.has(operationId)) {
            this.activeConnections.get(operationId).close();
            this.activeConnections.delete(operationId);
        }
    }

    /**
     * Stop all active connections
     */
    cleanup() {
        this.activeConnections.forEach((eventSource) => {
            eventSource.close();
        });
        this.activeConnections.clear();
    }

    /**
     * Dispatch custom events for external listeners
     */
    dispatchCustomEvent(eventType, detail) {
        document.dispatchEvent(new CustomEvent(eventType, {detail}));
    }
}

// Single global instance - no more complex domain-specific classes
window.SSEClient = new GenericSSEClient();

// Global cleanup on page unload
window.addEventListener('beforeunload', () => {
    window.SSEClient.cleanup();
});
