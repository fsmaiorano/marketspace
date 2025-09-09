import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Catalog } from '@app/shared/models/catalog';
import { CatalogItem } from '@app/shared/models/catalog-item';
import { CartHandlerResponse } from '@app/shared/models/cart-handler-response';
import { Result } from '@app/shared/models/result';
import { MarketSpaceService } from '@app/core/services/marketspace.service';
import { MarketspaceStoreService } from '@app/core/store/marketspace.store.service';
import { CardComponent } from '@app/shared/components/card/card.component';
import {SharedModule} from '@app/shared/shared.module';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(window:scroll)': 'onScroll($event)',
  },
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private marketspaceService = inject(MarketSpaceService);
  private cartStore = inject(MarketspaceStoreService);

  private pageIndex = signal<number>(1);
  private pageSize = 10;
  private totalCount = signal<number>(0);

  isLoadingMore = signal<boolean>(false);

  catalog = signal<Catalog[]>([]);
  isLoading = signal<boolean>(false);
  hasMoreData = computed(() => {
    const currentPage = this.pageIndex();
    const total = this.totalCount();

    if (total === 0) return true;

    return currentPage * this.pageSize < total;
  });

  ngOnInit() {
    this.loadInitialData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadInitialData() {
    this.isLoading.set(true);
    this.pageIndex.set(1);
    this.loadCatalogData(1);
  }

  private loadCatalogData(page: number) {
    console.log('loadCatalogData called for page:', page);

    this.marketspaceService
      .getCatalog(page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: Result<any>) => {
          console.log('API response:', result);

          if (result.data) {
            this.totalCount.set(result.data.count);
            const products = result.data.products || [];

            console.log('Products received:', products.length, 'Total count:', result.data.count);

            if (page === 1) {
              this.catalog.set(products);
            } else {
              this.catalog.update((current) => {
                const newCatalog = [...current, ...products];
                console.log('Updated catalog length:', newCatalog.length);
                return newCatalog;
              });
            }
          }
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
        },
        error: (error: any) => {
          console.error('Error loading catalog:', error);
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
        },
      });
  }

  onScroll(event: any) {
    if (this.isLoadingMore() || !this.hasMoreData()) return;

    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    const windowHeight = window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;

    const threshold = 300;
    const isNearBottom = scrollTop + windowHeight >= documentHeight - threshold;

    console.log('Scroll debug:', {
      scrollTop,
      windowHeight,
      documentHeight,
      threshold,
      isNearBottom,
      hasMoreData: this.hasMoreData(),
      isLoadingMore: this.isLoadingMore(),
      currentPage: this.pageIndex(),
      totalCount: this.totalCount(),
      catalogLength: this.catalog().length,
    });

    if (isNearBottom) {
      this.loadMoreData();
    }
  }

  private loadMoreData() {
    console.log('loadMoreData called:', {
      isLoadingMore: this.isLoadingMore(),
      hasMoreData: this.hasMoreData(),
      currentPage: this.pageIndex(),
      totalCount: this.totalCount(),
    });

    if (this.isLoadingMore() || !this.hasMoreData()) return;

    this.isLoadingMore.set(true);
    const nextPage = this.pageIndex() + 1;
    this.pageIndex.set(nextPage);

    console.log('Loading page:', nextPage);
    this.loadCatalogData(nextPage);
  }

  async onAddToCart(catalog: Catalog): Promise<void> {
    this.cartStore.addToCart(catalog);
    const cart: CatalogItem[] = this.cartStore.cartItems();
    this.marketspaceService.cartHandler({ username: 'fsmaiorano', items: cart }).subscribe({
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
