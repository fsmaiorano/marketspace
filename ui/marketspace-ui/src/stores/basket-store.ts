import { create } from "zustand";
import { devtools, persist } from "zustand/middleware";

interface Product {
  id: string;
  name: string;
  price: number;
  quantity: number;
}

interface BasketState {
  items: Product[];
  total: number;
  isOpen: boolean;

  addItem: (product: Product) => void;
  removeItem: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearBasket: () => void;
  toggleBasket: () => void;
  setOpen: (open: boolean) => void;
}

const calculateTotal = (items: Product[]) => {
  return items.reduce((sum, item) => sum + item.price * item.quantity, 0);
};

export const useBasketStore = create<BasketState>()(
  devtools(
    persist(
      (set) => ({
        items: [],
        total: 0,
        isOpen: false,

        addItem: (product) =>
          set((state) => {
            const existing = state.items.find((item) => item.id === product.id);
            let newItems: Product[];

            if (existing) {
              newItems = state.items.map((item) =>
                item.id === product.id
                  ? { ...item, quantity: item.quantity + product.quantity }
                  : item,
              );
            } else {
              newItems = [...state.items, product];
            }

            return {
              items: newItems,
              total: calculateTotal(newItems),
            };
          }),

        removeItem: (productId) =>
          set((state) => {
            const newItems = state.items.filter(
              (item) => item.id !== productId,
            );
            return {
              items: newItems,
              total: calculateTotal(newItems),
            };
          }),

        updateQuantity: (productId, quantity) =>
          set((state) => {
            const newItems = state.items.map((item) =>
              item.id === productId ? { ...item, quantity } : item,
            );
            return {
              items: newItems,
              total: calculateTotal(newItems),
            };
          }),

        clearBasket: () =>
          set({
            items: [],
            total: 0,
          }),

        toggleBasket: () =>
          set((state) => ({
            isOpen: !state.isOpen,
          })),

        setOpen: (open) => set({ isOpen: open }),
      }),
      {
        name: "basket-store",
      },
    ),
  ),
);
