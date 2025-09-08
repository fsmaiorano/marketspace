export interface Catalog {
  id: string;
  name: string;
  categories: string[];
  description: string;
  imageUrl: string;
  price: number;
  merchantId: string;
  createdAt: string;
  updatedAt?: string;
}
