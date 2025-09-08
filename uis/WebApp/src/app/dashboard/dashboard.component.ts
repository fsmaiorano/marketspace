import {Component, OnInit, OnDestroy, HostListener} from '@angular/core';
import {SharedModule} from '../shared/shared.module';
import {MaterialModule} from '../shared/material.module';
import {MarketSpaceService} from '../core/services/marketspace.service';
import {BehaviorSubject, Subject} from 'rxjs';
import {takeUntil, switchMap, map, tap} from 'rxjs/operators';
import {Catalog} from '@app/shared/models/catalog';

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

  constructor(private marketspaceService: MarketSpaceService) {
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
}
