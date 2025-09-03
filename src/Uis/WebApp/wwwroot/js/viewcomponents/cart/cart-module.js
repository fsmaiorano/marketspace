(function () {
    'use strict';

    const CartModule = {
        init: function () {
            console.log('CartModule initialized');
            CartModule.updateCartCount();
        },

        getCartItemCount: function () {
            debugger;
            const cart = JSON.parse(localStorage.getItem('cart')) || [];
            // return cart.reduce((total, item) => total + item.quantity, 0);
            return 3;
        },

        updateCartCount: function () {
            debugger;
            const count = CartModule.getCartItemCount();
            const cartCountElement = document.getElementById('cart-count');
            
            if (cartCountElement) {
                if (count > 0) {
                    cartCountElement.textContent = count;
                    cartCountElement.classList.remove('header__cart-count--hidden');
                } else {
                    cartCountElement.classList.add('header__cart-count--hidden');
                }
            }
        },

        addToCart: function (productId, quantity = 1) {
            try {
                debugger;
                const cart = JSON.parse(localStorage.getItem('cart')) || [];
                const existingItem = cart.find(item => item.productId === productId);

                if (existingItem) {
                    existingItem.quantity += quantity;
                } else {
                    cart.push({ productId, quantity });
                }

                localStorage.setItem('cart', JSON.stringify(cart));
                this.updateCartCount();
                return true;
            } catch (error) {
                console.error('Error adding to cart:', error);
                return false;
            }
        }
    };

    window.CartModule = CartModule;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', CartModule.init);
    } else {
        CartModule.init();
    }
})();

