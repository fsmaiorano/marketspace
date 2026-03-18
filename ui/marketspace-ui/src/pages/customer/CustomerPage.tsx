import { useCallback, useEffect, useState } from "react";
import { X } from "lucide-react";
import { toast } from "sonner";
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
import { CheckoutModal, type CheckoutFormData } from "@/pages/customer/components/CheckoutModal";
import { AiChatDrawer } from "@/pages/customer/components/AiChatDrawer";

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
  const [aiOpen, setAiOpen] = useState(false);
  const [loadingCatalog, setLoadingCatalog] = useState(false);
  const [loadingBasket, setLoadingBasket] = useState(false);
  const [loadingOrders, setLoadingOrders] = useState(false);
  const [checkingOut, setCheckingOut] = useState(false);
  const [checkoutModalOpen, setCheckoutModalOpen] = useState(false);
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

    if (item.stock <= 0) {
      toast.error(`"${item.name}" is out of stock.`);
      return;
    }

    const currentBasket: Basket = basket ?? {
      username: me.userName,
      items: [],
      totalPrice: 0,
    };

    const existing = currentBasket.items.find((i) => i.productId === item.id);
    const currentQty = existing?.quantity ?? 0;

    if (currentQty + 1 > item.stock) {
      toast.error(`Only ${item.stock} left in stock for "${item.name}".`);
      return;
    }

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

    if (delta > 0) {
      const catalogItem = catalogItems.find((c) => c.id === productId);
      const currentQty = basket.items.find((i) => i.productId === productId)?.quantity ?? 0;
      if (catalogItem && currentQty + delta > catalogItem.stock) {
        toast.error(`Only ${catalogItem.stock} available for "${catalogItem.name}".`);
        return;
      }
    }

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
        try {
          await deleteBasket(me.userName);
        } catch {
          // Ignore errors on delete, just clear the local state
        }
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

  const handleCheckout = () => {
    if (!basket || basket.items.length === 0 || !me) return;

    const stockByProductId = Object.fromEntries(catalogItems.map((c) => [c.id, c.stock]));
    const overStock = basket.items.find(
      (i) => i.quantity > (stockByProductId[i.productId] ?? Infinity),
    );
    if (overStock) {
      const available = stockByProductId[overStock.productId] ?? 0;
      toast.error(
        available === 0
          ? `"${overStock.productName}" is out of stock. Please remove it before checking out.`
          : `Only ${available} of "${overStock.productName}" available. Please adjust your quantity.`,
      );
      return;
    }

    setCheckoutModalOpen(true);
  };

  const handleCheckoutSubmit = async (data: CheckoutFormData) => {
    if (!basket || basket.items.length === 0 || !me) return;
    setError(null);
    setCheckingOut(true);

    try {
      await checkoutBasket({
        username: me.userName ?? me.userId,
        customerId: me.userId,
        firstName: data.firstName,
        lastName: data.lastName,
        emailAddress: data.emailAddress,
        addressLine: data.addressLine,
        country: data.country,
        state: data.state,
        zipCode: data.zipCode,
        cardName: data.cardName,
        cardNumber: data.cardNumber,
        expiration: data.expiration,
        cvv: data.cvv,
        paymentMethod: data.paymentMethod,
      });

      setBasket({ username: me.userName ?? "", items: [], totalPrice: 0 });
      setCheckoutModalOpen(false);
      setBasketOpen(false);
      toast.success("Order placed successfully! 🎉");
    } catch {
      toast.error("Checkout failed. Please try again.");
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
        onAiOpen={() => setAiOpen(true)}
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
        stockByProductId={Object.fromEntries(catalogItems.map((c) => [c.id, c.stock]))}
        onClose={() => setBasketOpen(false)}
        onUpdateQuantity={handleUpdateQuantity}
        onRemoveItem={handleRemoveItem}
        onCheckout={handleCheckout}
      />

      <CheckoutModal
        open={checkoutModalOpen}
        submitting={checkingOut}
        defaultEmail={me?.email ?? undefined}
        onClose={() => setCheckoutModalOpen(false)}
        onSubmit={handleCheckoutSubmit}
      />

      <AiChatDrawer
        open={aiOpen}
        onClose={() => setAiOpen(false)}
      />
    </div>
  );
}

