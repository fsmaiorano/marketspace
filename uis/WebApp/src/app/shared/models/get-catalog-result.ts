import {Catalog} from '@app/shared/models/catalog';

export class GetCatalogResult {
  pageIndex: number;
  pageSize: number;
  count: number;
  products: Catalog[];

  constructor(pageIndex: number, pageSize: number, count: number, products: Catalog[]) {
    this.pageIndex = pageIndex;
    this.pageSize = pageSize;
    this.count = count;
    this.products = products;
  }
}
