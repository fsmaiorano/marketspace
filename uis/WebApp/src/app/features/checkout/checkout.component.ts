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

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  imports: [MaterialModule],
  styleUrls: ['./checkout.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutComponent implements OnInit {
  private cartStore = inject(MarketspaceStoreService);

  cart = signal<ShoppingCart>({
    username: '',
    items: [],
    totalPrice: 0,
  });

  cartItems = computed(() => this.cart().items);
  totalPrice = computed(() => this.cart().totalPrice);
  isEmpty = computed(() => this.cart().items.length === 0);

  ngOnInit() {
    this.loadCartData();
  }

  private loadCartData() {
    const items = this.cartStore.getCartItems();
    const total = this.cartStore.getCartTotal();
    const username = 'fsmaiorano';

    this.cart.set({
      username: username,
      items: items,
      totalPrice: total,
    });
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

  continueShopping() {
    console.log('Continue shopping');
  }

  trackByProductId(index: number, item: CatalogItem): string {
    return item.productId ?? '0';
  }
}
