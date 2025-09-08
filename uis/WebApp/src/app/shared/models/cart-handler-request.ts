import {CatalogItem} from '@app/shared/models/catalog-item';

export interface CartHandlerRequest {
  username: string;
  items: CatalogItem[];
}
