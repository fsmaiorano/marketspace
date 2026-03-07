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
      Username: basket.username,
      Items: basket.items.map((i) => ({
        ProductId: i.productId,
        ProductName: i.productName,
        Price: i.price,
        Quantity: i.quantity,
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
    Username: request.username,
    CustomerId: request.customerId ?? "",
    FirstName: request.firstName ?? "",
    LastName: request.lastName ?? "",
    EmailAddress: request.emailAddress ?? "",
    AddressLine: request.addressLine ?? "",
    Country: request.country ?? "",
    State: request.state ?? "",
    ZipCode: request.zipCode ?? "",
    CardName: request.cardName ?? "",
    CardNumber: request.cardNumber ?? "",
    Expiration: request.expiration ?? "",
    CVV: request.cvv ?? "",
    PaymentMethod: request.paymentMethod ?? 0,
  });
};
