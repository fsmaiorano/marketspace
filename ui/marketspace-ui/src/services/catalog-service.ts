import { apiClient } from "@/lib/api";

export interface CatalogItem {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  categories: string[];
  merchantId: string;
  createdAt: string;
}

export interface CatalogListResult {
  items: CatalogItem[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}

interface BffResult<T> {
  isSuccess: boolean;
  error?: string;
  data?: T;
}

interface BffGetCatalogListResponse {
  pageIndex: number;
  pageSize: number;
  count: number;
  products: CatalogItem[];
}

export const getCatalogList = async (
  pageIndex = 1,
  pageSize = 20,
): Promise<CatalogListResult> => {
  const response = await apiClient.get<BffResult<BffGetCatalogListResponse>>(
    `/api/catalog?PageIndex=${pageIndex}&PageSize=${pageSize}`,
  );
  const inner = response.data?.data;
  return {
    items: inner?.products ?? [],
    totalCount: inner?.count ?? 0,
    pageIndex: inner?.pageIndex ?? pageIndex,
    pageSize: inner?.pageSize ?? pageSize,
  };
};

export const getCatalogById = async (id: string): Promise<CatalogItem> => {
  const response = await apiClient.get<BffResult<CatalogItem>>(`/api/catalog/${id}`);
  return response.data.data!;
};
