import { apiClient } from "@/lib/api";

export interface BasketItem {
  productId: string;
  productName: string;
  price: number;
  quantity: number;
}

export interface Basket {
  username: string;
  items: BasketItem[];
  totalPrice: number;
}

export interface CheckoutRequest {
  username: string;
  customerId?: string;
  firstName?: string;
  lastName?: string;
  emailAddress?: string;
  addressLine?: string;
  country?: string;
  state?: string;
  zipCode?: string;
  cardName?: string;
  cardNumber?: string;
  expiration?: string;
  cvv?: string;
  paymentMethod?: number;
}

interface BffResult<T> {
  isSuccess: boolean;
  error?: string;
  data?: T;
}

interface BffShoppingCartDto {
  username?: string;
  items?: BffBasketItemDto[];
  totalPrice?: number;
}

interface BffBasketItemDto {
  productId: string;
  productName: string;
  price: number;
  quantity: number;
}

interface BffGetBasketResponse {
  shoppingCart?: BffShoppingCartDto;
}

interface BffCreateBasketResponse {
  shoppingCart?: BffShoppingCartDto;
}

const mapCart = (cart: BffShoppingCartDto, fallbackUsername: string): Basket => ({
  username: cart.username ?? fallbackUsername,
  items: (cart.items ?? []).map((i) => ({
    productId: i.productId,
    productName: i.productName,
    price: i.price,
    quantity: i.quantity,
  })),
  totalPrice: cart.totalPrice ?? (cart.items ?? []).reduce((s, i) => s + i.price * i.quantity, 0),
});

export const getBasket = async (username: string): Promise<Basket> => {
  const response = await apiClient.get<BffResult<BffGetBasketResponse>>(
    `/api/basket/${username}`,
  );
  const cart = response.data?.data?.shoppingCart;
  if (cart) return mapCart(cart, username);
  return { username, items: [], totalPrice: 0 };
};

export const createOrUpdateBasket = async (basket: Basket): Promise<Basket> => {
  const response = await apiClient.post<BffResult<BffCreateBasketResponse>>(
    "/api/basket",
    {
      username: basket.username,
      items: basket.items.map((i) => ({
        productId: i.productId,
        productName: i.productName,
        price: i.price,
        quantity: i.quantity,
      })),
    },
  );
  const cart = response.data?.data?.shoppingCart;
  if (cart) return mapCart(cart, basket.username);
  return basket;
};

export const deleteBasket = async (username: string): Promise<void> => {
  await apiClient.delete(`/api/basket/${username}`);
};

export const checkoutBasket = async (request: CheckoutRequest): Promise<void> => {
  await apiClient.post<BffResult<unknown>>("/api/basket/checkout", {
    username: request.username,
    customerId: request.customerId ?? "",
    firstName: request.firstName ?? "",
    lastName: request.lastName ?? "",
    emailAddress: request.emailAddress ?? "",
    addressLine: request.addressLine ?? "",
    country: request.country ?? "",
    state: request.state ?? "",
    zipCode: request.zipCode ?? "",
    cardName: request.cardName ?? "",
    cardNumber: request.cardNumber ?? "",
    expiration: request.expiration ?? "",
    cvv: request.cvv ?? "",
    paymentMethod: request.paymentMethod ?? 0,
  });
};
