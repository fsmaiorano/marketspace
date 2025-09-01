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
                    `/api/Product/partial?page=${this.currentPage}&pageSize=${this.pageSize}`,
                    {
                        signal: this.abortController.signal,
                        headers: {'Accept': 'text/html'}
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

                const htmlContent = await response.text();

                if (htmlContent && htmlContent.trim().length > 0) {
                    this.renderProductsFromHtml(htmlContent);
                    this.currentPage++;

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

        renderProductsFromHtml: function (htmlContent) {
            const container = document.getElementById('products-container');
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = htmlContent;

            const newProducts = tempDiv.querySelectorAll('.product');

            while (tempDiv.firstChild) {
                container.appendChild(tempDiv.firstChild);
            }

            this.updateCount(newProducts.length);

            this.bindEvents();
        },

        updateCount: function (newProductsCount) {
            const countInfo = document.getElementById('current-count');
            if (countInfo) {
                countInfo.textContent = (parseInt(countInfo.textContent) + newProductsCount).toString();
            }
        },

        // escapeHtml: function (text) {
        //     const div = document.createElement('div');
        //     div.textContent = text;
        //     return div.innerHTML;
        // },

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

            fetch(`/api/Cart/CartHandler?productId=${encodeURIComponent(productId)}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    debugger;
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
