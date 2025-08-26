/**
 * Generic SSE Client for any controller that implements SSEControllerBase
 * Provides reusable SSE functionality across different domains
 */
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
                this.dispatchCustomEvent('sseStatusUpdate', { operationId, data, controllerPath });
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
                this.dispatchCustomEvent('sseComplete', { operationId, data, controllerPath });
                
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
            this.dispatchCustomEvent('sseError', { operationId, error: 'Connection error', controllerPath });
            
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
     * Get operation status via polling (alternative to SSE)
     */
    async getOperationStatus(controllerPath, operationId) {
        try {
            const response = await fetch(`/${controllerPath}/status/${operationId}`);
            return await response.json();
        } catch (error) {
            console.error('Failed to get operation status:', error);
            throw error;
        }
    }

    /**
     * Get all active operations for a controller
     */
    async getActiveOperations(controllerPath) {
        try {
            const response = await fetch(`/${controllerPath}/operations/active`);
            return await response.json();
        } catch (error) {
            console.error('Failed to get active operations:', error);
            throw error;
        }
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
        document.dispatchEvent(new CustomEvent(eventType, { detail }));
    }
}

// Domain-specific helper classes
class CatalogSSEClient extends GenericSSEClient {
    async loadCatalog(parameters = {}) {
        return this.startOperation('api/catalog', 'catalog', parameters);
    }

    async searchProducts(query, options = {}) {
        const parameters = {
            query,
            maxResults: options.maxResults || 50,
            sortBy: options.sortBy || 'relevance',
            category: options.category
        };
        return this.startOperation('api/catalog', 'search', parameters);
    }

    async filterProducts(filters = {}) {
        return this.startOperation('api/catalog', 'filter', filters);
    }
}

class OrderSSEClient extends GenericSSEClient {
    async createOrder(items, shippingAddress) {
        const parameters = {
            items,
            shippingAddress
        };
        return this.startOperation('api/order', 'create-order', parameters);
    }

    async processPayment(orderId, paymentMethodId, amount) {
        const parameters = {
            orderId,
            paymentMethodId,
            amount
        };
        return this.startOperation('api/order', 'process-payment', parameters);
    }

    async fulfillOrder(orderId, shippingAddress) {
        const parameters = {
            orderId,
            shippingAddress
        };
        return this.startOperation('api/order', 'fulfillment', parameters);
    }
}

// Initialize global instances
window.GenericSSEClient = GenericSSEClient;
window.CatalogSSE = new CatalogSSEClient();
window.OrderSSE = new OrderSSEClient();

// Global cleanup on page unload
window.addEventListener('beforeunload', () => {
    window.CatalogSSE.cleanup();
    window.OrderSSE.cleanup();
});
