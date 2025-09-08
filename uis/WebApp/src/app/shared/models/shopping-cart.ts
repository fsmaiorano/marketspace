import {CatalogItem} from '@app/shared/models/catalog-item';

export interface ShoppingCart {
  username: string;
  items: CatalogItem[];
  totalPrice: number;
}
