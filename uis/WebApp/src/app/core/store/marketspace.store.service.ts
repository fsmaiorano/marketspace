import { Injectable, computed, signal } from '@angular/core';
import { Catalog } from '@app/shared/models/catalog';

export interface CartItem {
  catalog: Catalog;
  quantity: number;
  addedAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class MarketspaceStoreService {
  private _cartItems = signal<CartItem[]>([]);

  cartItems = this._cartItems.asReadonly();

  totalQuantity = computed(() =>
    this._cartItems().reduce((total, item) => total + item.quantity, 0)
  );

  totalPrice = computed(() =>
    this._cartItems().reduce((total, item) => total + (item.catalog.price * item.quantity), 0)
  );

  itemCount = computed(() => this._cartItems().length);

  isEmpty = computed(() => this._cartItems().length === 0);

  constructor() {}

  addToCart(catalog: Catalog, quantity: number = 1): void {
    const currentItems = this._cartItems();
    const existingItemIndex = currentItems.findIndex(item => item.catalog.id === catalog.id);

    if (existingItemIndex >= 0) {
      const updatedItems = [...currentItems];
      updatedItems[existingItemIndex] = {
        ...updatedItems[existingItemIndex],
        quantity: updatedItems[existingItemIndex].quantity + quantity
      };
      this._cartItems.set(updatedItems);
    } else {
      const newItem: CartItem = {
        catalog,
        quantity,
        addedAt: new Date()
      };
      this._cartItems.set([...currentItems, newItem]);
    }
  }

  removeFromCart(catalogId: string): void {
    const filteredItems = this._cartItems().filter(item => item.catalog.id !== catalogId);
    this._cartItems.set(filteredItems);
  }

  updateQuantity(catalogId: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeFromCart(catalogId);
      return;
    }

    const currentItems = this._cartItems();
    const itemIndex = currentItems.findIndex(item => item.catalog.id === catalogId);

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
    const item = currentItems.find(item => item.catalog.id === catalogId);

    if (item) {
      this.updateQuantity(catalogId, item.quantity - 1);
    }
  }

  increaseQuantity(catalogId: string): void {
    const currentItems = this._cartItems();
    const item = currentItems.find(item => item.catalog.id === catalogId);

    if (item) {
      this.updateQuantity(catalogId, item.quantity + 1);
    }
  }

  clearCart(): void {
    this._cartItems.set([]);
  }

  getCartItem(catalogId: string): CartItem | undefined {
    return this._cartItems().find(item => item.catalog.id === catalogId);
  }

  isInCart(catalogId: string): boolean {
    return this._cartItems().some(item => item.catalog.id === catalogId);
  }

  getItemQuantity(catalogId: string): number {
    const item = this.getCartItem(catalogId);
    return item ? item.quantity : 0;
  }
}
