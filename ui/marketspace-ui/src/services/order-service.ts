import { apiClient } from "@/lib/api";

export interface Order {
  id: string;
  customerId: string;
  orderName: string;
  status: number;
  totalAmount: number;
  createdAt: string;
}

export interface OrderItem {
  productId: string;
  productName: string;
  price: number;
  quantity: number;
}

interface BffResult<T> {
  isSuccess: boolean;
  error?: string;
  data?: T;
}

interface BffGetOrderListResponse {
  orders?: Order[];
}

export const getOrdersByCustomerId = async (customerId: string): Promise<Order[]> => {
  const response = await apiClient.get<BffResult<BffGetOrderListResponse>>(
    `/api/order/customer/${customerId}`,
  );
  return response.data?.data?.orders ?? [];
};
