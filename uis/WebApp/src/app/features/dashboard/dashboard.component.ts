import {Component, OnInit, OnDestroy, HostListener} from '@angular/core';
import {BehaviorSubject, Subject} from 'rxjs';
import {takeUntil, switchMap, map, tap} from 'rxjs/operators';
import {Catalog} from '@app/shared/models/catalog';
import {CatalogItem} from '@app/shared/models/catalog-item';
import {CartHandlerResponse} from '@app/shared/models/cart-handler-response';
import {Result} from '@app/shared/models/result';
import {MaterialModule} from '@app/shared/material.module';
import {SharedModule} from '@app/shared/shared.module';
import {MarketSpaceService} from '@app/core/services/marketspace.service';
import {MarketspaceStoreService} from '@app/core/store/marketspace.store.service';

@Component({
  selector: 'app-dashboard',
  imports: [SharedModule, MaterialModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private pageIndex$ = new BehaviorSubject<number>(1);
  private pageSize = 10;
  private totalCount = 0;
  private isLoadingMore = false;

  catalog$ = new BehaviorSubject<Catalog[]>([]);
  isLoading$ = new BehaviorSubject<boolean>(false);
  hasMoreData$ = new BehaviorSubject<boolean>(true);

  constructor(
    private marketspaceService: MarketSpaceService,
    private cartStore: MarketspaceStoreService
  ) {
  }

  ngOnInit() {
    this.loadInitialData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadInitialData() {
    this.isLoading$.next(true);
    this.pageIndex$.next(1);

    this.pageIndex$
      .pipe(
        takeUntil(this.destroy$),
        switchMap((pageIndex) => this.marketspaceService.getCatalog(pageIndex, this.pageSize)),
        tap((result) => {
          if (result.data) {
            this.totalCount = result.data.count;
            this.hasMoreData$.next(
              result.data.products.length === this.pageSize &&
              this.pageIndex$.value * this.pageSize < this.totalCount
            );
          }
        }),
        map((result) => result.data?.products || [])
      )
      .subscribe({
        next: (products) => {
          const currentCatalog = this.catalog$.value;
          if (this.pageIndex$.value === 1) {
            this.catalog$.next(products);
          } else {
            this.catalog$.next([...currentCatalog, ...products]);
          }
          this.isLoading$.next(false);
          this.isLoadingMore = false;
        },
        error: (error) => {
          console.error('Error loading catalog:', error);
          this.isLoading$.next(false);
          this.isLoadingMore = false;
        },
      });
  }

  @HostListener('window:scroll', ['$event'])
  onScroll(event: any) {
    if (this.isLoadingMore || !this.hasMoreData$.value) return;

    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    const windowHeight = window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;

    // Load more when user is within 200px of the bottom
    if (scrollTop + windowHeight >= documentHeight - 200) {
      this.loadMoreData();
    }
  }

  private loadMoreData() {
    if (this.isLoadingMore || !this.hasMoreData$.value) return;

    this.isLoadingMore = true;
    const nextPage = this.pageIndex$.value + 1;
    this.pageIndex$.next(nextPage);
  }

  get catalog(): Catalog[] {
    return this.catalog$.value;
  }

  async onAddToCart(catalog: Catalog): Promise<void> {
    this.cartStore.addToCart(catalog);
    const cart: CatalogItem[] = this.cartStore.cartItems();
    this.marketspaceService.cartHandler({username: "fsmaiorano", items: cart}).subscribe({
      next: (result: Result<CartHandlerResponse>) => {
        if (result.isSuccess) {
          console.log('Cart updated successfully:', result.data);
        } else {
          console.error('Failed to update cart:', result?.error);
        }
      },
      error: (error) => {
        console.error('Error updating cart:', error);
      }
    });
  }
}
