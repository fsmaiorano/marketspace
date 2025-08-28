class Home {
    constructor(initialData) {
        this.currentPage = (initialData?.pageIndex || 1) + 1;
        this.pageSize = initialData?.pageSize || 20;
        this.totalCount = initialData?.count || 0;
        this.isLoading = false;
        this.hasMoreProducts = this.calculateHasMore();
        this.abortController = null;
        
        this.container = document.getElementById('products-container');
        this.loadingIndicator = document.getElementById('loading-indicator');
        this.noMoreProducts = document.getElementById('no-more-products');
        this.countInfo = document.getElementById('current-count');
        
        this.init();
    }
    
    calculateHasMore() {
        const currentCount = this.container?.children?.length || 0;
        return currentCount < this.totalCount;
    }
    
    init() {
        if (!this.hasMoreProducts) {
            this.showNoMoreProducts();
            return;
        }
        
        this.setupScrollListener();
    }
    
    setupScrollListener() {
        let scrollTimeout;
        
        window.addEventListener('scroll', () => {
            if (scrollTimeout) {
                clearTimeout(scrollTimeout);
            }
            
            scrollTimeout = setTimeout(() => {
                this.handleScroll();
            }, 100);
        });
    }
    
    handleScroll() {
        const threshold = 200;
        const scrollPosition = window.innerHeight + window.scrollY;
        const documentHeight = document.documentElement.offsetHeight;
        
        if (scrollPosition >= documentHeight - threshold) {
            this.loadProducts();
        }
    }
    
    async loadProducts() {
        if (this.isLoading || !this.hasMoreProducts) {
            return;
        }
        
        this.isLoading = true;
        this.showLoading();
        
        try {
            if (this.abortController) {
                this.abortController.abort();
            }
            
            this.abortController = new AbortController();
            
            const response = await fetch(
                `/api/products?page=${this.currentPage}&pageSize=${this.pageSize}`,
                {
                    signal: this.abortController.signal,
                    headers: { 'Accept': 'application/json' }
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
                
                const currentTotal = this.container.children.length;
                if (currentTotal >= this.totalCount) {
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
    }
    
    renderProducts(products) {
        const fragment = document.createDocumentFragment();
        
        products.forEach(product => {
            const productElement = this.createProductElement(product);
            fragment.appendChild(productElement);
        });
        
        this.container.appendChild(fragment);
    }
    
    createProductElement(product) {
        const div = document.createElement('div');
        div.className = 'product-item';
        
        div.innerHTML = `
            ${product.imageUrl ? 
                `<img src="${this.escapeHtml(product.imageUrl)}" alt="${this.escapeHtml(product.name)}"/>` : 
                '<div>No Image</div>'
            }
            <div>
                <h3>${this.escapeHtml(product.name || 'Produto')}</h3>
                <p>${this.escapeHtml(product.description || 'Descrição não disponível')}</p>
                <div>
                    ${(product.categories || []).map(cat => `<span>${this.escapeHtml(cat)}</span>`).join('')}
                </div>
                <div>$${(product.price || 0).toFixed(2)}</div>
                <button data-product-id="${product.id}">
                    Add to Cart
                </button>
            </div>
        `;
        
        return div;
    }
    
    updateCount(newProductsCount) {
        if (this.countInfo) {
            this.countInfo.textContent = (parseInt(this.countInfo.textContent) + newProductsCount).toString();
        }
    }
    
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
    
    showLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'block';
        }
    }
    
    hideLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'none';
        }
    }
    
    showNoMoreProducts() {
        if (this.noMoreProducts) {
            this.noMoreProducts.style.display = 'block';
        }
    }
    
    showError() {
        const errorElement = document.createElement('div');
        errorElement.className = 'alert alert-danger text-center my-3';
        errorElement.textContent = 'Erro ao carregar produtos. Tente recarregar a página.';
        this.container.parentElement.appendChild(errorElement);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (window.homeInitialData) {
        new Home(window.homeInitialData);
    }
});

