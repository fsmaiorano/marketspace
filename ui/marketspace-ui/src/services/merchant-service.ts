import { apiClient } from "@/lib/api";
import { envConfig } from "@/lib/env-config";

export interface MerchantProfile {
  id: string;
  name: string;
  description: string;
  email: string;
  phoneNumber: string;
  address: string;
  createdAt: string;
  updatedAt?: string;
}

export interface MerchantProduct {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  categories: string[];
  merchantId: string;
  stock: number;
  createdAt: string;
  updatedAt?: string;
}

export interface MerchantProductsResult {
  items: MerchantProduct[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}

export interface StockChangedEvent {
  catalogId: string;
  productName: string;
  newStock: number;
  changedAt: string;
}

export interface MerchantDashboardSummary {
  totalOrders: number;
  totalUnitsSold: number;
  totalRevenue: number;
  processingOrders: number;
  completedOrders: number;
}

export interface MerchantDashboardOrderItem {
  catalogId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface MerchantDashboardOrder {
  orderId: string;
  customerEmail: string;
  status: string;
  merchantTotalAmount: number;
  createdAt: string;
  items: MerchantDashboardOrderItem[];
}

export interface MerchantDashboardProductSales {
  catalogId: string;
  productName: string;
  unitsSold: number;
  orderCount: number;
  currentStock: number;
}

export interface MerchantDashboardOverview {
  summary: MerchantDashboardSummary;
  recentOrders: MerchantDashboardOrder[];
  productSales: MerchantDashboardProductSales[];
  generatedAt: string;
}

interface BffResult<T> {
  isSuccess: boolean;
  error?: string;
  data?: T;
}

interface BffProductsResponse {
  pageIndex: number;
  pageSize: number;
  count: number;
  products: MerchantProduct[];
}

export const getMerchantMe = async (): Promise<MerchantProfile> => {
  const response = await apiClient.get<BffResult<MerchantProfile>>("/api/merchant/me");
  if (!response.data?.data) throw new Error("Merchant profile not found");
  return response.data.data;
};

export const getMerchantProducts = async (
  merchantId: string,
  pageIndex = 1,
  pageSize = 50,
): Promise<MerchantProductsResult> => {
  const response = await apiClient.get<BffResult<BffProductsResponse>>(
    `/api/catalog/merchant/${merchantId}?PageIndex=${pageIndex}&PageSize=${pageSize}`,
  );
  const inner = response.data?.data;
  return {
    items: inner?.products ?? [],
    totalCount: inner?.count ?? 0,
    pageIndex: inner?.pageIndex ?? pageIndex,
    pageSize: inner?.pageSize ?? pageSize,
  };
};

export const updateStock = async (
  catalogId: string,
  delta: number,
): Promise<{ newStock: number }> => {
  const response = await apiClient.patch<BffResult<{ newStock: number }>>(
    `/api/catalog/${catalogId}/stock`,
    { delta },
  );
  return response.data?.data ?? { newStock: 0 };
};

export const getMerchantDashboardOverview =
  async (): Promise<MerchantDashboardOverview> => {
    const response = await apiClient.get<BffResult<MerchantDashboardOverview>>(
      "/api/merchant-dashboard/overview",
    );

    if (!response.data?.data) {
      throw new Error("Merchant dashboard overview not found");
    }

    return response.data.data;
  };

export const createMerchantEventSource = (): EventSource => {
  const token = localStorage.getItem("token");
  const url = `${envConfig.bffApiUrl}/api/merchant-dashboard/stream`;
  // EventSource doesn't support custom headers — use URL param as fallback
  return new EventSource(`${url}?access_token=${token}`);
};
