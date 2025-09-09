import {Injectable, computed, signal} from '@angular/core';
import {Catalog} from '@app/shared/models/catalog';
import {MarketSpaceService} from '@app/core/services/marketspace.service';
import {CatalogItem} from '@app/shared/models/catalog-item';

@Injectable({
  providedIn: 'root'
})
export class MarketspaceStoreService {
  constructor(private marketspaceService: MarketSpaceService) {
  }

  private _cartItems = signal<CatalogItem[]>([]);

  cartItems = this._cartItems.asReadonly();

  totalQuantity = computed(() =>
    this._cartItems().reduce((total, item) => total + item.quantity, 0)
  );

  totalPrice = computed(() =>
    this._cartItems().reduce((total, item) => total + (item.price * item.quantity), 0)
  );

  itemCount = computed(() => this._cartItems().length);

  isEmpty = computed(() => this._cartItems().length === 0);

  addToCart(catalog: Catalog, quantity: number = 1): void {
    const currentItems = this._cartItems();
    const existingItemIndex = currentItems.findIndex(item => item.productId === catalog.id);

    if (existingItemIndex >= 0) {
      const updatedItems = [...currentItems];
      updatedItems[existingItemIndex] = {
        ...updatedItems[existingItemIndex],
        quantity: updatedItems[existingItemIndex].quantity + quantity
      };
      this._cartItems.set(updatedItems);
    } else {
      const newItem: CatalogItem = {
        productId: catalog.id,
        productName: catalog.name,
        price: catalog.price,
        quantity: quantity,
      };
      this._cartItems.set([...currentItems, newItem]);
    }
  }

  removeFromCart(catalogId: string): void {
    const filteredItems = this._cartItems().filter(item => item.productId !== catalogId);
    this._cartItems.set(filteredItems);
  }

  updateQuantity(catalogId: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeFromCart(catalogId);
      return;
    }

    const currentItems = this._cartItems();
    const itemIndex = currentItems.findIndex(item => item.productId === catalogId);

    if (itemIndex >= 0) {
      const updatedItems = [...currentItems];
      updatedItems[itemIndex] = {
        ...updatedItems[itemIndex],
        quantity
      };
      this._cartItems.set(updatedItems);
    }
  }

  decreaseQuantity(catalogId: string): void {
    const currentItems = this._cartItems();
    const item = currentItems.find(item => item.productId === catalogId);

    if (item) {
      this.updateQuantity(catalogId, item.quantity - 1);
    }
  }

  increaseQuantity(catalogId: string): void {
    const currentItems = this._cartItems();
    const item = currentItems.find(item => item.productId === catalogId);

    if (item) {
      this.updateQuantity(catalogId, item.quantity + 1);
    }
  }

  clearCart(): void {
    this._cartItems.set([]);
  }

  getCartItems(): CatalogItem[] {
    return this._cartItems();
  }

  getCartItem(catalogId: string): CatalogItem | undefined {
    return this._cartItems().find(item => item.productId === catalogId);
  }

  isInCart(catalogId: string): boolean {
    return this._cartItems().some(item => item.productId === catalogId);
  }

  getItemQuantity(catalogId: string): number {
    const item = this.getCartItem(catalogId);
    return item ? item.quantity : 0;
  }

  getCartTotal() : number {
    return this.totalPrice();
  }
}
