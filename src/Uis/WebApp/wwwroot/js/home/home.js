(function () {
    'use strict';

    const HomeModule = {
        activeEventSources: new Map(),

        init: function () {
            // Initialize any startup logic here
            console.log('HomeModule initialized with SSE support');
        },

        catalog: function (operationType, parameters = null) {
            return new Promise((resolve, reject) => {
                const url = '/Home/Catalog';
                const data = {
                    operationType: operationType,
                    parameters: parameters
                };

                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify(data)
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Network response was not ok');
                        }
                        return response.json();
                    })
                    .then(data => {
                        if (data.success && data.operationId) {
                            // Start SSE monitoring for this operation
                            this.startSSEMonitoring(data.operationId, data.sseUrl);
                            resolve(data);
                        } else {
                            reject(new Error(data.message || 'Operation failed'));
                        }
                    })
                    .catch(error => {
                        console.error('There was a problem with the fetch operation:', error);
                        reject(error);
                    });
            });
        },

        startSSEMonitoring: function (operationId, sseUrl) {
            // Close any existing EventSource for this operation
            if (this.activeEventSources.has(operationId)) {
                this.activeEventSources.get(operationId).close();
            }

            const eventSource = new EventSource(sseUrl);
            this.activeEventSources.set(operationId, eventSource);

            eventSource.addEventListener('status', (event) => {
                try {
                    const data = JSON.parse(event.data);
                    this.onStatusUpdate(operationId, data);
                } catch (error) {
                    console.error('Error parsing SSE status data:', error);
                }
            });

            eventSource.addEventListener('complete', (event) => {
                try {
                    const data = JSON.parse(event.data);
                    this.onOperationComplete(operationId, data);
                } catch (error) {
                    console.error('Error parsing SSE complete data:', error);
                }
                
                eventSource.close();
                this.activeEventSources.delete(operationId);
            });

            eventSource.addEventListener('error', (event) => {
                console.error('SSE connection error for operation:', operationId);
                this.onOperationError(operationId, 'Connection error');
                
                eventSource.close();
                this.activeEventSources.delete(operationId);
            });

            // Set up timeout to prevent hanging connections
            setTimeout(() => {
                if (this.activeEventSources.has(operationId)) {
                    console.warn('SSE timeout for operation:', operationId);
                    eventSource.close();
                    this.activeEventSources.delete(operationId);
                }
            }, 60000); // 1 minute timeout
        },

        onStatusUpdate: function (operationId, data) {
            console.log(`Operation ${operationId} update:`, data);
            
            // Update UI elements based on operation status
            this.updateProgressUI(operationId, data);
            
            // Trigger custom event for external listeners
            document.dispatchEvent(new CustomEvent('catalogStatusUpdate', {
                detail: { operationId, data }
            }));
        },

        onOperationComplete: function (operationId, data) {
            console.log(`Operation ${operationId} completed:`, data);
            
            // Update UI to show completion
            this.updateProgressUI(operationId, data);
            
            // Trigger completion event
            document.dispatchEvent(new CustomEvent('catalogComplete', {
                detail: { operationId, data }
            }));
        },

        onOperationError: function (operationId, error) {
            console.error(`Operation ${operationId} error:`, error);
            
            // Trigger error event
            document.dispatchEvent(new CustomEvent('catalogError', {
                detail: { operationId, error }
            }));
        },

        updateProgressUI: function (operationId, data) {
            // Find progress elements by operation ID
            const container = document.querySelector(`[data-operation-id="${operationId}"]`);
            if (!container) return;

            // Update progress bar if exists
            const progressBar = container.querySelector('.progress-bar');
            if (progressBar) {
                progressBar.style.width = `${data.Progress}%`;
                progressBar.textContent = `${data.Progress}%`;
            }

            // Update status text if exists
            const statusElement = container.querySelector('.status-text');
            if (statusElement) {
                statusElement.textContent = data.Status;
            }

            // Update message if exists
            const messageElement = container.querySelector('.message-text');
            if (messageElement && data.Message) {
                messageElement.textContent = data.Message;
            }

            // Add/remove CSS classes based on status
            container.classList.remove('status-started', 'status-processing', 'status-completed', 'status-failed');
            container.classList.add(`status-${data.Status.toLowerCase()}`);
        },

        // Helper method to stop monitoring a specific operation
        stopMonitoring: function (operationId) {
            if (this.activeEventSources.has(operationId)) {
                this.activeEventSources.get(operationId).close();
                this.activeEventSources.delete(operationId);
            }
        },

        // Cleanup all active SSE connections
        cleanup: function () {
            this.activeEventSources.forEach((eventSource, operationId) => {
                eventSource.close();
            });
            this.activeEventSources.clear();
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            HomeModule.init();
        });
    } else {
        HomeModule.init();
    }

    // Auto-cleanup on page unload
    window.addEventListener('beforeunload', () => {
        HomeModule.cleanup();
    });

    window.HomeModule = HomeModule;
})(window, document);


// /**
//  * Home Page JavaScript Module
//  * Following modern JavaScript best practices and ASP.NET MVC conventions
//  */
//
// // Use IIFE to avoid global namespace pollution
// (function(window, document) {
//     'use strict';
//
//     // Home page module
//     const HomeModule = {
//         // Configuration
//         config: {
//             animationSpeed: 300,
//             apiEndpoints: {
//                 stats: '/api/home/stats',
//                 features: '/api/home/features'
//             }
//         },
//
//         // DOM elements cache
//         elements: {},
//
//         // Initialize the module
//         init: function() {
//             this.cacheElements();
//             this.bindEvents();
//             this.loadInitialData();
//             this.initializeAnimations();
//         },
//
//         // Cache DOM elements for better performance
//         cacheElements: function() {
//             this.elements = {
//                 heroSection: document.querySelector('.home-hero'),
//                 ctaButton: document.querySelector('.home-hero__cta'),
//                 featureCards: document.querySelectorAll('.home-features__card'),
//                 statsSection: document.querySelector('.home-stats'),
//                 loadingSpinner: document.querySelector('.home-loading')
//             };
//         },
//
//         // Bind event listeners
//         bindEvents: function() {
//             // CTA button click handler
//             if (this.elements.ctaButton) {
//                 this.elements.ctaButton.addEventListener('click', this.handleCtaClick.bind(this));
//             }
//
//             // Feature card hover effects
//             this.elements.featureCards.forEach(card => {
//                 card.addEventListener('mouseenter', this.handleCardHover.bind(this));
//                 card.addEventListener('mouseleave', this.handleCardLeave.bind(this));
//             });
//
//             // Scroll events for animations
//             window.addEventListener('scroll', this.throttle(this.handleScroll.bind(this), 100));
//
//             // Window resize handler
//             window.addEventListener('resize', this.throttle(this.handleResize.bind(this), 250));
//         },
//
//         // Handle CTA button click
//         handleCtaClick: function(event) {
//             event.preventDefault();
//            
//             // Add loading state
//             const button = event.target;
//             const originalText = button.textContent;
//             button.textContent = 'Loading...';
//             button.classList.add('loading');
//
//             // Simulate API call or navigation
//             setTimeout(() => {
//                 button.textContent = originalText;
//                 button.classList.remove('loading');
//                
//                 // You can replace this with actual navigation
//                 console.log('CTA button clicked - implement your navigation logic here');
//             }, 1000);
//         },
//
//         // Handle feature card hover
//         handleCardHover: function(event) {
//             const card = event.currentTarget;
//             card.style.transform = 'translateY(-8px)';
//         },
//
//         // Handle feature card leave
//         handleCardLeave: function(event) {
//             const card = event.currentTarget;
//             card.style.transform = 'translateY(0)';
//         },
//
//         // Handle scroll events
//         handleScroll: function() {
//             const scrollPosition = window.pageYOffset;
//            
//             // Parallax effect for hero section
//             if (this.elements.heroSection) {
//                 const parallaxSpeed = scrollPosition * 0.5;
//                 this.elements.heroSection.style.transform = `translateY(${parallaxSpeed}px)`;
//             }
//
//             // Reveal animations on scroll
//             this.revealOnScroll();
//         },
//
//         // Handle window resize
//         handleResize: function() {
//             // Update any responsive calculations here
//             console.log('Window resized - implement responsive adjustments if needed');
//         },
//
//         // Load initial data (stats, features, etc.)
//         loadInitialData: function() {
//             this.loadStats();
//             // Add other data loading calls here
//         },
//
//         // Load statistics data
//         loadStats: function() {
//             // Show loading state
//             this.showLoading();
//
//             // Simulate API call - replace with actual fetch call
//             setTimeout(() => {
//                 const mockStats = [
//                     { number: '1,234', label: 'Happy Customers' },
//                     { number: '567', label: 'Products Sold' },
//                     { number: '98%', label: 'Satisfaction Rate' },
//                     { number: '24/7', label: 'Support Available' }
//                 ];
//
//                 this.renderStats(mockStats);
//                 this.hideLoading();
//             }, 1500);
//         },
//
//         // Render statistics
//         renderStats: function(stats) {
//             if (!this.elements.statsSection) return;
//
//             const statsHtml = stats.map(stat => `
//                 <div class="home-stats__item">
//                     <span class="home-stats__number">${stat.number}</span>
//                     <span class="home-stats__label">${stat.label}</span>
//                 </div>
//             `).join('');
//
//             this.elements.statsSection.innerHTML = statsHtml;
//            
//             // Animate numbers counting up
//             this.animateNumbers();
//         },
//
//         // Animate number counting
//         animateNumbers: function() {
//             const numberElements = document.querySelectorAll('.home-stats__number');
//            
//             numberElements.forEach(element => {
//                 const finalNumber = element.textContent.replace(/[^\d]/g, '');
//                 if (finalNumber) {
//                     this.countUp(element, 0, parseInt(finalNumber), 2000);
//                 }
//             });
//         },
//
//         // Count up animation
//         countUp: function(element, start, end, duration) {
//             const increment = end / (duration / 16);
//             let current = start;
//            
//             const timer = setInterval(() => {
//                 current += increment;
//                 if (current >= end) {
//                     current = end;
//                     clearInterval(timer);
//                 }
//                
//                 const originalText = element.textContent;
//                 const nonNumericPart = originalText.replace(/[\d,]/g, '');
//                 element.textContent = Math.floor(current).toLocaleString() + nonNumericPart;
//             }, 16);
//         },
//
//         // Initialize animations
//         initializeAnimations: function() {
//             // Add fade-in animation to feature cards
//             this.elements.featureCards.forEach((card, index) => {
//                 card.style.opacity = '0';
//                 card.style.transform = 'translateY(20px)';
//                
//                 setTimeout(() => {
//                     card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
//                     card.style.opacity = '1';
//                     card.style.transform = 'translateY(0)';
//                 }, index * 200);
//             });
//         },
//
//         // Reveal elements on scroll
//         revealOnScroll: function() {
//             const elements = document.querySelectorAll('[data-reveal]');
//            
//             elements.forEach(element => {
//                 const elementTop = element.getBoundingClientRect().top;
//                 const windowHeight = window.innerHeight;
//                
//                 if (elementTop < windowHeight * 0.8) {
//                     element.classList.add('revealed');
//                 }
//             });
//         },
//
//         // Show loading spinner
//         showLoading: function() {
//             if (this.elements.loadingSpinner) {
//                 this.elements.loadingSpinner.style.display = 'block';
//             }
//         },
//
//         // Hide loading spinner
//         hideLoading: function() {
//             if (this.elements.loadingSpinner) {
//                 this.elements.loadingSpinner.style.display = 'none';
//             }
//         },
//
//         // Utility: Throttle function for performance
//         throttle: function(func, limit) {
//             let inThrottle;
//             return function() {
//                 const args = arguments;
//                 const context = this;
//                 if (!inThrottle) {
//                     func.apply(context, args);
//                     inThrottle = true;
//                     setTimeout(() => inThrottle = false, limit);
//                 }
//             };
//         },
//
//         // Utility: Debounce function
//         debounce: function(func, wait) {
//             let timeout;
//             return function executedFunction(...args) {
//                 const later = () => {
//                     clearTimeout(timeout);
//                     func(...args);
//                 };
//                 clearTimeout(timeout);
//                 timeout = setTimeout(later, wait);
//             };
//         }
//     };
//
//     // Auto-initialize when DOM is ready
//     if (document.readyState === 'loading') {
//         document.addEventListener('DOMContentLoaded', function() {
//             HomeModule.init();
//         });
//     } else {
//         HomeModule.init();
//     }
//
//     // Expose module globally for external access if needed
//     window.HomeModule = HomeModule;
//
// })(window, document);
//
