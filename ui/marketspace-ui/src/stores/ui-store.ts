import { create } from "zustand";
import { devtools } from "zustand/middleware";

interface UIState {
  isSidebarOpen: boolean;
  isDarkMode: boolean;
  notification: {
    message: string;
    type: "success" | "error" | "info" | "warning";
  } | null;

  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  toggleDarkMode: () => void;
  setDarkMode: (dark: boolean) => void;
  showNotification: (
    message: string,
    type?: "success" | "error" | "info" | "warning",
  ) => void;
  clearNotification: () => void;
}

export const useUIStore = create<UIState>()(
  devtools((set) => ({
    isSidebarOpen: true,
    isDarkMode: false,
    notification: null,

    toggleSidebar: () =>
      set((state) => ({
        isSidebarOpen: !state.isSidebarOpen,
      })),

    setSidebarOpen: (open) => set({ isSidebarOpen: open }),

    toggleDarkMode: () =>
      set((state) => ({
        isDarkMode: !state.isDarkMode,
      })),

    setDarkMode: (dark) => set({ isDarkMode: dark }),

    showNotification: (message, type = "info") =>
      set({ notification: { message, type } }),

    clearNotification: () => set({ notification: null }),
  })),
);
