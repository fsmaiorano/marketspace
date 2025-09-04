import { CatalogDto } from './catalogdto';

export class GetCatalogResult {
  pageIndex: number;
  pageSize: number;
  count: number;
  products: CatalogDto[];

  constructor(pageIndex: number, pageSize: number, count: number, products: CatalogDto[]) {
    this.pageIndex = pageIndex;
    this.pageSize = pageSize;
    this.count = count;
    this.products = products;
  }
}
