import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { MaterialModule } from '@app/shared/material.module';
import { CatalogItem } from '@app/shared/models/catalog-item';
import { ShoppingCart } from '@app/shared/models/shopping-cart';
import { MarketspaceStoreService } from '@app/core/store/marketspace.store.service';
import { Router } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { MarketSpaceService } from '@app/core/services/marketspace.service';
import { Result } from '@app/shared/models/result';
import { CartHandlerResponse } from '@app/shared/models/cart-handler-response';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  imports: [MaterialModule, DecimalPipe],
  styleUrls: ['./checkout.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutComponent implements OnInit {
  private router = inject(Router);
  private cartStore = inject(MarketspaceStoreService);
  private marketSpaceService = inject(MarketSpaceService);

  cart = signal<ShoppingCart>({
    username: '',
    items: [],
    totalPrice: 0,
  });

  username = computed(() => this.cart().username);
  cartItems = computed(() => this.cart().items);
  totalPrice = computed(() => this.cart().totalPrice);
  isEmpty = computed(() => this.cart().items.length === 0);

  ngOnInit() {
    this.loadCartData();
  }

  private loadCartData() {
    const items = this.cartStore.getCartItems();
    const total = this.cartStore.getCartTotal();
    const username = this.cartStore.getUsername();

    this.cart.set({
      username: username(),
      items: items,
      totalPrice: total,
    });

    this.updateCart(username(), items);
  }

  increaseQuantity(item: CatalogItem) {
    this.cartStore.increaseQuantity(item.productId || '');
    this.loadCartData();
  }

  decreaseQuantity(item: CatalogItem) {
    this.cartStore.decreaseQuantity(item.productId || '');
    this.loadCartData();
  }

  removeItem(item: CatalogItem) {
    this.cartStore.removeFromCart(item.productId || '');
    this.loadCartData();
  }

  getItemTotal(item: CatalogItem): number {
    return item.price * item.quantity;
  }

  proceedToPayment() {
    console.log('Proceeding to payment with cart:', this.cart());
  }

  async continueShopping() {
    await this.router.navigate(['/dashboard']);
  }

  trackByProductId(index: number, item: CatalogItem): string {
    return item.productId ?? '0';
  }

  updateCart(username: string, cartItems: CatalogItem[]) {
    this.marketSpaceService.cartHandler({ username: username, items: cartItems }).subscribe({
      next: (result: Result<CartHandlerResponse>) => {
        if (result.isSuccess) {
          console.log('Cart updated successfully:', result.data);
        } else {
          console.error('Failed to update cart:', result?.error);
        }
      },
      error: (error: any) => {
        console.error('Error updating cart:', error);
      },
    });
  }
}
