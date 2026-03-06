import { useCallback, useEffect, useState } from "react";
import { CheckCircle, X } from "lucide-react";
import { logout, getMe } from "@/services/authentication-service";
import { getCatalogList, type CatalogItem } from "@/services/catalog-service";
import {
  checkoutBasket,
  createOrUpdateBasket,
  deleteBasket,
  getBasket,
  type Basket,
  type BasketItem,
} from "@/services/basket-service";
import { getOrdersByCustomerId, type Order } from "@/services/order-service";
import { CatalogSection } from "@/pages/customer/components/CatalogSection";
import { CustomerHeader } from "@/pages/customer/components/CustomerHeader";
import { OrdersSection } from "@/pages/customer/components/OrdersSection";
import { BasketDrawer } from "@/pages/customer/components/BasketDrawer";

type View = "catalog" | "orders";

interface MeResponse {
  userId: string;
  userName: string | null;
  email: string | null;
  firstName: string | null;
  lastName: string | null;
  userType: string;
}

export default function CustomerPage() {
  const [me, setMe] = useState<MeResponse | null>(null);
  const [view, setView] = useState<View>("catalog");
  const [catalogItems, setCatalogItems] = useState<CatalogItem[]>([]);
  const [basket, setBasket] = useState<Basket | null>(null);
  const [orders, setOrders] = useState<Order[]>([]);
  const [basketOpen, setBasketOpen] = useState(false);
  const [loadingCatalog, setLoadingCatalog] = useState(false);
  const [loadingBasket, setLoadingBasket] = useState(false);
  const [loadingOrders, setLoadingOrders] = useState(false);
  const [checkingOut, setCheckingOut] = useState(false);
  const [checkoutSuccess, setCheckoutSuccess] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pageIndex, setPageIndex] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 12;

  const loadMe = useCallback(async (): Promise<MeResponse> => {
    const profile = await getMe();
    setMe(profile);
    return profile;
  }, []);

  const loadCatalog = useCallback(async (page: number) => {
    setLoadingCatalog(true);
    setError(null);
    try {
      const data = await getCatalogList(page, pageSize);
      setCatalogItems(data.items ?? []);
      setTotalCount(data.totalCount ?? 0);
    } catch {
      setError("Failed to load catalog.");
    } finally {
      setLoadingCatalog(false);
    }
  }, []);

  const loadBasket = useCallback(async (username: string) => {
    setLoadingBasket(true);
    try {
      const data = await getBasket(username);
      setBasket(data);
    } catch {
      setBasket({ username, items: [], totalPrice: 0 });
    } finally {
      setLoadingBasket(false);
    }
  }, []);

  const loadOrders = useCallback(async (customerId: string) => {
    setLoadingOrders(true);
    setError(null);
    try {
      const data = await getOrdersByCustomerId(customerId);
      setOrders(data ?? []);
    } catch {
      setError("Failed to load orders.");
      setOrders([]);
    } finally {
      setLoadingOrders(false);
    }
  }, []);

  useEffect(() => {
    const init = async () => {
      try {
        const profile = await loadMe();
        await loadCatalog(1);
        if (profile.userName) {
          await loadBasket(profile.userName);
        }
      } catch {
        setError("Failed to initialize page.");
      }
    };
    init();
  }, [loadMe, loadCatalog, loadBasket]);

  useEffect(() => {
    if (view === "orders" && me?.userId) {
      loadOrders(me.userId);
    }
  }, [view, me, loadOrders]);

  const handleAddToBasket = async (item: CatalogItem) => {
    if (!me?.userName) return;
    setError(null);

    const currentBasket: Basket = basket ?? {
      username: me.userName,
      items: [],
      totalPrice: 0,
    };

    const existing = currentBasket.items.find((i) => i.productId === item.id);
    const newItems: BasketItem[] = existing
      ? currentBasket.items.map((i) =>
          i.productId === item.id ? { ...i, quantity: i.quantity + 1 } : i,
        )
      : [
          ...currentBasket.items,
          {
            productId: item.id,
            productName: item.name,
            price: item.price,
            quantity: 1,
          },
        ];

    const totalPrice = newItems.reduce((sum, i) => sum + i.price * i.quantity, 0);
    const updatedBasket: Basket = {
      username: me.userName,
      items: newItems,
      totalPrice,
    };

    try {
      const saved = await createOrUpdateBasket(updatedBasket);
      setBasket(saved ?? updatedBasket);
      setBasketOpen(true);
    } catch {
      setError("Failed to add item to basket.");
    }
  };

  const handleUpdateQuantity = async (productId: string, delta: number) => {
    if (!basket || !me?.userName) return;
    setError(null);

    const newItems: BasketItem[] = basket.items
      .map((i) =>
        i.productId === productId ? { ...i, quantity: i.quantity + delta } : i,
      )
      .filter((i) => i.quantity > 0);

    const totalPrice = newItems.reduce((sum, i) => sum + i.price * i.quantity, 0);
    const updatedBasket: Basket = {
      username: me.userName,
      items: newItems,
      totalPrice,
    };

    try {
      if (newItems.length === 0) {
        await deleteBasket(me.userName);
        setBasket({ username: me.userName, items: [], totalPrice: 0 });
      } else {
        const saved = await createOrUpdateBasket(updatedBasket);
        setBasket(saved ?? updatedBasket);
      }
    } catch {
      setError("Failed to update basket.");
    }
  };

  const handleRemoveItem = async (productId: string) => {
    const qty = basket?.items.find((i) => i.productId === productId)?.quantity ?? 1;
    await handleUpdateQuantity(productId, -qty);
  };

  const handleCheckout = async () => {
    if (!basket || basket.items.length === 0 || !me) return;
    setError(null);
    setCheckingOut(true);

    try {
      await checkoutBasket({
        username: me.userName ?? me.userId,
      });

      setBasket({ username: me.userName ?? "", items: [], totalPrice: 0 });
      setBasketOpen(false);
      setCheckoutSuccess("Order placed successfully!");
      setTimeout(() => setCheckoutSuccess(null), 6000);
    } catch {
      setError("Checkout failed. Please try again.");
    } finally {
      setCheckingOut(false);
    }
  };

  const handleLogout = () => {
    logout();
    window.location.href = "/";
  };

  const basketItemCount = basket?.items.reduce((s, i) => s + i.quantity, 0) ?? 0;

  return (
    <div className="min-h-screen bg-background">
      <CustomerHeader
        view={view}
        basketItemCount={basketItemCount}
        greeting={me?.firstName ?? me?.userName ?? me?.email}
        onViewChange={setView}
        onBasketOpen={() => setBasketOpen(true)}
        onLogout={handleLogout}
      />

      <main className="mx-auto w-full max-w-6xl space-y-4 px-4 py-6">
        {error && (
          <div className="flex items-center justify-between rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700">
            <span>{error}</span>
            <button onClick={() => setError(null)}>
              <X className="h-4 w-4" />
            </button>
          </div>
        )}

        {checkoutSuccess && (
          <div className="flex items-center gap-2 rounded-md border border-green-300 bg-green-50 px-4 py-3 text-sm text-green-700">
            <CheckCircle className="h-4 w-4" />
            {checkoutSuccess}
          </div>
        )}

        {view === "catalog" ? (
          <CatalogSection
            items={catalogItems}
            totalCount={totalCount}
            pageIndex={pageIndex}
            pageSize={pageSize}
            loading={loadingCatalog}
            onRefresh={loadCatalog}
            onAddToBasket={handleAddToBasket}
            onPageChange={(nextPage) => {
              setPageIndex(nextPage);
              loadCatalog(nextPage);
            }}
          />
        ) : (
          <OrdersSection
            orders={orders}
            loading={loadingOrders}
            onRefresh={() => me && loadOrders(me.userId)}
          />
        )}
      </main>

      <BasketDrawer
        open={basketOpen}
        loading={loadingBasket}
        basket={basket}
        basketItemCount={basketItemCount}
        checkingOut={checkingOut}
        onClose={() => setBasketOpen(false)}
        onUpdateQuantity={handleUpdateQuantity}
        onRemoveItem={handleRemoveItem}
        onCheckout={handleCheckout}
      />
    </div>
  );
}

