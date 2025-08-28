(function () {
    'use strict';

    const HomeModule = {
        init: function () {
            console.log('HomeModule initialized');
            this.bindEvents();
        },

        cleanup: function () {
            console.log('HomeModule cleaned up');
            document.removeEventListener('DOMContentLoaded', HomeModule.init.bind(HomeModule));
            window.removeEventListener('beforeunload', HomeModule.cleanup.bind(HomeModule));
        },

        bindEvents: function () {
            const addToCartButtons = document.querySelectorAll('.add-to-cart-btn');
            addToCartButtons.forEach(button => {
                button.addEventListener('click', this.handleAddToCart.bind(this));
            });
        },

        handleAddToCart: function (event) {
            const button = event.target;
            const productId = button.getAttribute('data-product-id');
            
            if (!productId) {
                console.error('Product ID not found');
                return;
            }

            const originalText = button.textContent;
            button.textContent = 'Adding...';
            button.disabled = true;

            this.addToCart(productId)
                .then(() => {
                    button.textContent = 'Added!';
                    button.style.backgroundColor = '#28a745';
                    
                    // Reset button after 2 seconds
                    setTimeout(() => {
                        button.textContent = originalText;
                        button.style.backgroundColor = '';
                        button.disabled = false;
                    }, 2000);
                })
                .catch(error => {
                    console.error('Error adding to cart:', error);
                    button.textContent = 'Error';
                    button.style.backgroundColor = '#dc3545';
                    
                    // Reset button after 2 seconds
                    setTimeout(() => {
                        button.textContent = originalText;
                        button.style.backgroundColor = '';
                        button.disabled = false;
                    }, 2000);
                });
        },

        addToCart: function (productId) {
            return new Promise((resolve, reject) => {
                setTimeout(() => {
                    if (Math.random() > 0.1) { // 90% success rate
                        console.log(`Product ${productId} added to cart`);
                        resolve();
                    } else {
                        reject(new Error('Failed to add to cart'));
                    }
                }, 1000);
            });
        },

        getCatalog: function () {
            fetch('/catalog/getProducts')
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('Products:', data);
                })
                .catch(error => {
                    console.error('There was a problem with the fetch operation:', error);
                });
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', HomeModule.init.bind(HomeModule));
    } else {
        HomeModule.init();
    }

    window.addEventListener('beforeunload', HomeModule.cleanup.bind(HomeModule));

    window.HomeModule = HomeModule;
})();
