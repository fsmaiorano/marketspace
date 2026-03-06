import { useEffect, useState, useCallback } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { logout, getMe } from "@/services/authentication-service";
import {
  getCatalogList,
  type CatalogItem,
} from "@/services/catalog-service";
import {
  getBasket,
  createOrUpdateBasket,
  deleteBasket,
  checkoutBasket,
  type Basket,
  type BasketItem,
} from "@/services/basket-service";
import { getOrdersByCustomerId, type Order } from "@/services/order-service";
import {
  ShoppingCart,
  Plus,
  Minus,
  Trash2,
  Package,
  LogOut,
  RefreshCw,
  X,
  CheckCircle,
} from "lucide-react";

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
  const [searchTerm, setSearchTerm] = useState("");
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
    let newItems: BasketItem[];

    if (existing) {
      newItems = currentBasket.items.map((i) =>
        i.productId === item.id ? { ...i, quantity: i.quantity + 1 } : i,
      );
    } else {
      newItems = [
        ...currentBasket.items,
        {
          productId: item.id,
          productName: item.name,
          price: item.price,
          quantity: 1,
        },
      ];
    }

    const totalPrice = newItems.reduce((sum, i) => sum + i.price * i.quantity, 0);
    const updatedBasket: Basket = { username: me.userName, items: newItems, totalPrice };

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
      .map((i) => (i.productId === productId ? { ...i, quantity: i.quantity + delta } : i))
      .filter((i) => i.quantity > 0);

    const totalPrice = newItems.reduce((sum, i) => sum + i.price * i.quantity, 0);
    const updatedBasket: Basket = { username: me.userName, items: newItems, totalPrice };

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

  const filteredItems = catalogItems.filter(
    (item) =>
      item.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.description?.toLowerCase().includes(searchTerm.toLowerCase()),
  );

  const basketItemCount = basket?.items.reduce((s, i) => s + i.quantity, 0) ?? 0;
  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="min-h-screen bg-background">
      <header className="border-b bg-card sticky top-0 z-30">
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-3">
          <div>
            <h1 className="text-xl font-bold">MarketSpace</h1>
            {me && (
              <p className="text-xs text-muted-foreground">
                Hello, {me.firstName ?? me.userName ?? me.email}
              </p>
            )}
          </div>

          <nav className="flex items-center gap-2">
            <Button
              variant={view === "catalog" ? "default" : "outline"}
              size="sm"
              onClick={() => setView("catalog")}
            >
              <Package className="mr-1 h-4 w-4" />
              Catalog
            </Button>
            <Button
              variant={view === "orders" ? "default" : "outline"}
              size="sm"
              onClick={() => setView("orders")}
            >
              My Orders
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="relative"
              onClick={() => setBasketOpen(true)}
            >
              <ShoppingCart className="h-4 w-4" />
              {basketItemCount > 0 && (
                <span className="absolute -top-1.5 -right-1.5 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[10px] text-primary-foreground font-bold">
                  {basketItemCount}
                </span>
              )}
            </Button>
            <Button variant="ghost" size="sm" onClick={handleLogout}>
              <LogOut className="h-4 w-4" />
            </Button>
          </nav>
        </div>
      </header>

      <main className="mx-auto w-full max-w-6xl px-4 py-6 space-y-4">
        {error && (
          <div className="rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 flex items-center justify-between">
            <span>{error}</span>
            <button onClick={() => setError(null)}>
              <X className="h-4 w-4" />
            </button>
          </div>
        )}

        {checkoutSuccess && (
          <div className="rounded-md border border-green-300 bg-green-50 px-4 py-3 text-sm text-green-700 flex items-center gap-2">
            <CheckCircle className="h-4 w-4" />
            {checkoutSuccess}
          </div>
        )}

        {view === "catalog" && (
          <>
            <div className="flex items-center gap-3">
              <Input
                placeholder="Search products..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="max-w-sm"
              />
              <Button
                variant="outline"
                size="sm"
                onClick={() => loadCatalog(pageIndex)}
                disabled={loadingCatalog}
              >
                <RefreshCw className={`h-4 w-4 ${loadingCatalog ? "animate-spin" : ""}`} />
              </Button>
            </div>

            {loadingCatalog ? (
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {Array.from({ length: 8 }).map((_, i) => (
                  <div key={i} className="rounded-xl border bg-card p-4 animate-pulse space-y-3">
                    <div className="h-4 bg-muted rounded w-3/4" />
                    <div className="h-3 bg-muted rounded w-full" />
                    <div className="h-3 bg-muted rounded w-1/2" />
                    <div className="h-8 bg-muted rounded" />
                  </div>
                ))}
              </div>
            ) : filteredItems.length === 0 ? (
              <div className="text-center py-16 text-muted-foreground">No products found.</div>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {filteredItems.map((item) => (
                  <div
                    key={item.id}
                    className="rounded-xl border bg-card flex flex-col overflow-hidden hover:shadow-md transition-shadow"
                  >
                    <div className="bg-muted h-36 flex items-center justify-center">
                      <Package className="h-14 w-14 text-muted-foreground/40" />
                    </div>
                    <div className="p-3 flex flex-col gap-1 flex-1">
                      <h3 className="font-semibold text-sm leading-tight line-clamp-2">
                        {item.name}
                      </h3>
                      {item.description && (
                        <p className="text-xs text-muted-foreground line-clamp-2">
                          {item.description}
                        </p>
                      )}
                      {item.categories?.length > 0 && (
                        <p className="text-xs text-muted-foreground line-clamp-1">
                          {item.categories.join(", ")}
                        </p>
                      )}
                      <div className="mt-auto pt-2 flex items-center justify-between gap-2">
                        <span className="font-bold text-sm">${item.price?.toFixed(2)}</span>
                        <Button
                          size="sm"
                          className="h-7 text-xs px-2"
                          onClick={() => handleAddToBasket(item)}
                        >
                          <Plus className="h-3 w-3 mr-1" />
                          Add
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {totalPages > 1 && (
              <div className="flex items-center justify-center gap-2 pt-4">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={pageIndex === 1 || loadingCatalog}
                  onClick={() => {
                    const prev = pageIndex - 1;
                    setPageIndex(prev);
                    loadCatalog(prev);
                  }}
                >
                  Previous
                </Button>
                <span className="text-sm text-muted-foreground">
                  Page {pageIndex} of {totalPages}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={pageIndex >= totalPages || loadingCatalog}
                  onClick={() => {
                    const next = pageIndex + 1;
                    setPageIndex(next);
                    loadCatalog(next);
                  }}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}

        {view === "orders" && (
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold">My Orders</h2>
              <Button
                variant="outline"
                size="sm"
                onClick={() => me && loadOrders(me.userId)}
                disabled={loadingOrders}
              >
                <RefreshCw className={`h-4 w-4 ${loadingOrders ? "animate-spin" : ""}`} />
              </Button>
            </div>

            {loadingOrders ? (
              <div className="space-y-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <div key={i} className="rounded-xl border bg-card p-4 animate-pulse space-y-2">
                    <div className="h-4 bg-muted rounded w-1/3" />
                    <div className="h-3 bg-muted rounded w-1/2" />
                  </div>
                ))}
              </div>
            ) : orders.length === 0 ? (
              <div className="text-center py-16 text-muted-foreground">No orders found.</div>
            ) : (
              <div className="space-y-3">
                {orders.map((order) => {
                  const statusLabel =
                    order.status === 0
                      ? "Pending"
                      : order.status === 1
                        ? "Processing"
                        : order.status === 2
                          ? "Shipped"
                          : order.status === 3
                            ? "Delivered"
                            : "Cancelled";
                  const statusClass =
                    order.status === 3
                      ? "bg-green-100 text-green-700"
                      : order.status === 4
                        ? "bg-red-100 text-red-700"
                        : order.status === 2
                          ? "bg-blue-100 text-blue-700"
                          : "bg-yellow-100 text-yellow-700";
                  return (
                  <div key={order.id} className="rounded-xl border bg-card p-4 space-y-3">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <p className="text-sm font-semibold">
                          {order.orderName || `Order #${order.id.split("-")[0].toUpperCase()}`}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {order.createdAt
                            ? new Date(order.createdAt).toLocaleDateString(undefined, {
                                year: "numeric",
                                month: "short",
                                day: "numeric",
                                hour: "2-digit",
                                minute: "2-digit",
                              })
                            : ""}
                        </p>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${statusClass}`}>
                          {statusLabel}
                        </span>
                        <span className="font-bold text-sm">
                          ${order.totalAmount?.toFixed(2)}
                        </span>
                      </div>
                    </div>
                  </div>
                  );
                })}
              </div>
            )}
          </div>
        )}
      </main>

      {basketOpen && (
        <div className="fixed inset-0 z-50 flex">
          <div className="flex-1 bg-black/40" onClick={() => setBasketOpen(false)} />
          <div className="w-full max-w-sm bg-background border-l shadow-xl flex flex-col h-full">
            <div className="flex items-center justify-between px-4 py-3 border-b">
              <h2 className="font-semibold text-lg flex items-center gap-2">
                <ShoppingCart className="h-5 w-5" />
                Basket
                {basketItemCount > 0 && (
                  <span className="text-sm text-muted-foreground">
                    ({basketItemCount} items)
                  </span>
                )}
              </h2>
              <button className="p-1 rounded hover:bg-muted" onClick={() => setBasketOpen(false)}>
                <X className="h-5 w-5" />
              </button>
            </div>

            <div className="flex-1 overflow-y-auto px-4 py-3 space-y-3">
              {loadingBasket ? (
                <div className="space-y-3">
                  {Array.from({ length: 2 }).map((_, i) => (
                    <div key={i} className="animate-pulse h-16 bg-muted rounded-lg" />
                  ))}
                </div>
              ) : !basket || basket.items.length === 0 ? (
                <div className="text-center py-16 text-muted-foreground">
                  Your basket is empty.
                </div>
              ) : (
                basket.items.map((item) => (
                  <div
                    key={item.productId}
                    className="flex items-start gap-3 rounded-lg border bg-card p-3"
                  >
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium leading-tight line-clamp-2">
                        {item.productName}
                      </p>
                      <p className="text-sm font-semibold mt-1">
                        ${(item.price * item.quantity).toFixed(2)}
                      </p>
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        className="h-6 w-6 rounded border flex items-center justify-center hover:bg-muted"
                        onClick={() => handleUpdateQuantity(item.productId, -1)}
                      >
                        <Minus className="h-3 w-3" />
                      </button>
                      <span className="text-sm w-6 text-center">{item.quantity}</span>
                      <button
                        className="h-6 w-6 rounded border flex items-center justify-center hover:bg-muted"
                        onClick={() => handleUpdateQuantity(item.productId, 1)}
                      >
                        <Plus className="h-3 w-3" />
                      </button>
                      <button
                        className="h-6 w-6 rounded border flex items-center justify-center hover:bg-muted text-red-500 ml-1"
                        onClick={() => handleRemoveItem(item.productId)}
                      >
                        <Trash2 className="h-3 w-3" />
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>

            {basket && basket.items.length > 0 && (
              <div className="border-t px-4 py-4 space-y-3">
                <div className="flex items-center justify-between">
                  <span className="font-semibold">Total</span>
                  <span className="font-bold text-lg">
                    ${basket.totalPrice?.toFixed(2) ?? "0.00"}
                  </span>
                </div>
                <Button className="w-full" disabled={checkingOut} onClick={handleCheckout}>
                  {checkingOut ? "Placing order..." : "Checkout"}
                </Button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}