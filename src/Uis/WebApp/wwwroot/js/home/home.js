(function () {
    'use strict';

    const HomeModule = {
        currentPage: 1,
        pageSize: 20,
        totalCount: 0,
        isLoading: false,
        hasMoreProducts: false,
        abortController: null,

        init: function (initialData) {
            console.log('HomeModule initialized');

            if (initialData) {
                this.currentPage = (initialData.pageIndex || 1) + 1;
                this.pageSize = initialData.pageSize || 20;
                this.totalCount = initialData.count || 0;
                this.hasMoreProducts = this.calculateHasMore();
                this.setupScrollListener();
            }

            this.bindEvents();
        },

        cleanup: function () {
            console.log('HomeModule cleaned up');
            if (this.abortController) {
                this.abortController.abort();
            }
        },

        calculateHasMore: function () {
            const container = document.getElementById('products-container');
            const currentCount = container?.children?.length || 0;
            return currentCount < this.totalCount;
        },

        setupScrollListener: function () {
            if (!this.hasMoreProducts) {
                this.showNoMoreProducts();
                return;
            }

            let scrollTimeout;
            window.addEventListener('scroll', () => {
                if (scrollTimeout) clearTimeout(scrollTimeout);
                scrollTimeout = setTimeout(() => this.handleScroll(), 100);
            });
        },

        handleScroll: function () {
            const threshold = 200;
            const scrollPosition = window.innerHeight + window.scrollY;
            const documentHeight = document.documentElement.offsetHeight;

            if (scrollPosition >= documentHeight - threshold) {
                this.loadProducts();
            }
        },

        async loadProducts() {
            if (this.isLoading || !this.hasMoreProducts) return;

            this.isLoading = true;
            this.showLoading();

            try {
                if (this.abortController) this.abortController.abort();
                this.abortController = new AbortController();

                const response = await fetch(
                    `/api/products?page=${this.currentPage}&pageSize=${this.pageSize}`,
                    {
                        signal: this.abortController.signal,
                        headers: {'Accept': 'application/json'}
                    }
                );

                if (response.status === 204) {
                    this.hasMoreProducts = false;
                    this.showNoMoreProducts();
                    return;
                }

                if (!response.ok) {
                    console.error(`HTTP error! status: ${response.status}`);
                    this.showError();
                    return;
                }

                const data = await response.json();

                if (data.products && data.products.length > 0) {
                    this.renderProducts(data.products);
                    this.currentPage++;
                    this.updateCount(data.products.length);

                    const container = document.getElementById('products-container');
                    if (container.children.length >= this.totalCount) {
                        this.hasMoreProducts = false;
                        this.showNoMoreProducts();
                    }
                } else {
                    this.hasMoreProducts = false;
                    this.showNoMoreProducts();
                }

            } catch (error) {
                if (error.name === 'AbortError') {
                    console.log('Request was aborted');
                    return;
                }
                console.error('Error loading products:', error);
                this.showError();
            } finally {
                this.isLoading = false;
                this.hideLoading();
            }
        },

        renderProducts: function (products) {
            const container = document.getElementById('products-container');
            const fragment = document.createDocumentFragment();

            products.forEach(product => {
                const productElement = this.createProductElement(product);
                fragment.appendChild(productElement);
            });

            container.appendChild(fragment);

            this.bindEvents();
        },

        createProductElement: function (product) {
            const div = document.createElement('div');
            div.className = 'product';

            div.innerHTML = `
                <div class="product__image-container">
                    ${product.imageUrl ? 
                        `<img src="${this.escapeHtml(product.imageUrl)}" alt="${this.escapeHtml(product.name)}" class="product__image"/>` : 
                        '<div class="product__no-image">No Image</div>'
                    }
                </div>
                <div class="product__content">
                    <h3 class="product__title">${this.escapeHtml(product.name || 'Product')}</h3>
                    <p class="product__description">${this.escapeHtml(product.description || 'Description not available')}</p>
                    <div class="product__categories">
                        ${(product.categories || []).map(cat => `<span class="product__category">${this.escapeHtml(cat)}</span>`).join('')}
                    </div>
                    <div class="product__footer">
                        <div class="product__price">$${(product.price || 0).toFixed(2)}</div>
                        <button class="product__add-button add-to-cart-btn" data-product-id="${product.id}">
                            Add to Cart
                        </button>
                    </div>
                </div>
            `;

            return div;
        },

        updateCount: function (newProductsCount) {
            const countInfo = document.getElementById('current-count');
            if (countInfo) {
                countInfo.textContent = (parseInt(countInfo.textContent) + newProductsCount).toString();
            }
        },

        escapeHtml: function (text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        },

        showLoading: function () {
            const indicator = document.getElementById('loading-indicator');
            if (indicator) indicator.style.display = 'block';
        },

        hideLoading: function () {
            const indicator = document.getElementById('loading-indicator');
            if (indicator) indicator.style.display = 'none';
        },

        showNoMoreProducts: function () {
            const noMore = document.getElementById('no-more-products');
            if (noMore) noMore.style.display = 'block';
        },

        showError: function () {
            const container = document.getElementById('products-container');
            const errorElement = document.createElement('div');
            errorElement.className = 'alert alert-danger text-center my-3';
            errorElement.textContent = 'Error loading products. Please try again later.';
            container.parentElement.appendChild(errorElement);
        },

        bindEvents: function () {
            const addToCartButtons = document.querySelectorAll('.add-to-cart-btn');
            addToCartButtons.forEach(button => {
                button.removeEventListener('click', this.handleAddToCart);
                button.addEventListener('click', this.handleAddToCart.bind(this));
            });
        },

        handleAddToCart: function (event) {
            const button = event.target;
            const productId = button.getAttribute('data-product-id');

            console.log(`Add to Cart clicked for product ID: ${productId}`);

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
                    if (Math.random() > 0.1) {
                        console.log(`Product ${productId} added to cart`);
                        resolve();
                    } else {
                        reject(new Error('Failed to add to cart'));
                    }
                }, 1000);
            });
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            HomeModule.init(window.homeInitialData);
        });
    } else {
        HomeModule.init(window.homeInitialData);
    }

    window.addEventListener('beforeunload', HomeModule.cleanup.bind(HomeModule));
    window.HomeModule = HomeModule;
})();
