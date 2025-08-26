(function () {
    'use strict';

    const HomeModule = {
        init: function () {
            console.log('HomeModule initialized - using simple SSE client');
        },

        // Simple catalog operations using the generic SSE client directly
        loadCatalog: function(options = {}) {
            const parameters = {
                page: options.page || 1,
                pageSize: options.pageSize || 50,
                includeOutOfStock: options.includeOutOfStock !== false,
                category: options.category
            };

            return window.SSEClient.startOperation('api/catalog', 'catalog', parameters, {
                onStatusUpdate: this.onStatusUpdate,
                onComplete: this.onOperationComplete,
                onError: this.onOperationError
            });
        },

        searchProducts: function(query, options = {}) {
            const parameters = {
                query: query,
                maxResults: options.maxResults || 50,
                sortBy: options.sortBy || 'relevance',
                category: options.category
            };

            return window.SSEClient.startOperation('api/catalog', 'search', parameters, {
                onStatusUpdate: this.onStatusUpdate,
                onComplete: this.onOperationComplete,
                onError: this.onOperationError
            });
        },

        filterProducts: function(filters = {}) {
            return window.SSEClient.startOperation('api/catalog', 'filter', filters, {
                onStatusUpdate: this.onStatusUpdate,
                onComplete: this.onOperationComplete,
                onError: this.onOperationError
            });
        },

        // Event handlers for UI updates
        onStatusUpdate: function (operationId, data) {
            console.log(`Operation ${operationId} update:`, data);
            HomeModule.updateProgressUI(operationId, data);
            
            // Trigger custom event for demo page
            document.dispatchEvent(new CustomEvent('catalogStatusUpdate', {
                detail: { operationId, data }
            }));
        },

        onOperationComplete: function (operationId, data) {
            console.log(`Operation ${operationId} completed:`, data);
            HomeModule.updateProgressUI(operationId, data);
            
            // Trigger completion event for demo page
            document.dispatchEvent(new CustomEvent('catalogComplete', {
                detail: { operationId, data }
            }));
        },

        onOperationError: function (operationId, error) {
            console.error(`Operation ${operationId} error:`, error);
            
            // Trigger error event for demo page
            document.dispatchEvent(new CustomEvent('catalogError', {
                detail: { operationId, error }
            }));
        },

        // Simple UI update function
        updateProgressUI: function (operationId, data) {
            const container = document.querySelector(`[data-operation-id="${operationId}"]`);
            if (!container) return;

            // Update progress bar
            const progressBar = container.querySelector('.progress-bar');
            if (progressBar) {
                progressBar.style.width = `${data.Progress}%`;
                progressBar.textContent = `${data.Progress}%`;
            }

            // Update status text
            const statusElement = container.querySelector('.status-text');
            if (statusElement) {
                statusElement.textContent = data.Status;
            }

            // Update message
            const messageElement = container.querySelector('.message-text');
            if (messageElement && data.Message) {
                messageElement.textContent = data.Message;
            }

            // Update CSS classes
            container.classList.remove('status-started', 'status-processing', 'status-completed', 'status-failed');
            container.classList.add(`status-${data.Status.toLowerCase()}`);
        },

        // Simple cleanup
        cleanup: function () {
            window.SSEClient.cleanup();
        },

        // Simple stop monitoring
        stopMonitoring: function (operationId) {
            window.SSEClient.stopMonitoring(operationId);
        }
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', HomeModule.init.bind(HomeModule));
    } else {
        HomeModule.init();
    }

    // Auto-cleanup on page unload
    window.addEventListener('beforeunload', HomeModule.cleanup.bind(HomeModule));

    // Expose module globally
    window.HomeModule = HomeModule;
})();
