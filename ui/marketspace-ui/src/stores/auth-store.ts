import { create } from "zustand";
import { devtools, persist } from "zustand/middleware";

type UserType = "Customer" | "Merchant";

interface User {
  id?: string;
  email: string;
  userType: UserType;
}

interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  error: string | null;

  setUser: (user: User | null) => void;
  setTokens: (token: string, refreshToken: string) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  logout: () => void;
  clear: () => void;
}

const initialState = {
  user: null,
  token: null,
  refreshToken: null,
  isLoading: false,
  error: null,
};

export const useAuthStore = create<AuthState>()(
  devtools(
    persist(
      (set) => ({
        ...initialState,
        setUser: (user) => set({ user }),
        setTokens: (token, refreshToken) => set({ token, refreshToken }),
        setLoading: (isLoading) => set({ isLoading }),
        setError: (error) => set({ error }),
        logout: () => set(initialState),
        clear: () => set(initialState),
      }),
      {
        name: "auth-store",
        partialize: (state) => ({
          user: state.user,
          refreshToken: state.refreshToken,
        }),
      },
    ),
  ),
);
