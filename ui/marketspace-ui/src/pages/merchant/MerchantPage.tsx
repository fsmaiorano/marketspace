import { useCallback, useEffect, useRef, useState } from "react";
import { LogOut, Package, RefreshCw, TrendingDown, TrendingUp, Wifi, WifiOff } from "lucide-react";
import { toast } from "sonner";
import { logout } from "@/services/authentication-service";
import {
  getMerchantDashboardOverview,
  getMerchantMe,
  getMerchantProducts,
  updateStock,
  createMerchantEventSource,
  type MerchantDashboardOrder,
  type MerchantDashboardOverview,
  type MerchantDashboardProductSales,
  type MerchantProfile,
  type MerchantProduct,
  type StockChangedEvent,
} from "@/services/merchant-service";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

// ─── Header ─────────────────────────────────────────────────────────────────

function MerchantHeader({
  merchant,
  connected,
  onLogout,
}: {
  merchant: MerchantProfile | null;
  connected: boolean;
  onLogout: () => void;
}) {
  return (
    <header className="sticky top-0 z-30 border-b bg-card">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-3">
        <div>
          <h1 className="text-xl font-bold">MarketSpace</h1>
          {merchant && (
            <p className="text-xs text-muted-foreground">
              {merchant.name} — Merchant Dashboard
            </p>
          )}
        </div>
        <div className="flex items-center gap-3">
          <span
            className={`flex items-center gap-1 text-xs font-medium ${connected ? "text-green-600" : "text-muted-foreground"}`}
          >
            {connected ? <Wifi className="h-3.5 w-3.5" /> : <WifiOff className="h-3.5 w-3.5" />}
            {connected ? "Live" : "Offline"}
          </span>
          <Button variant="ghost" size="sm" onClick={onLogout}>
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </header>
  );
}

// ─── Stat Card ───────────────────────────────────────────────────────────────

function StatCard({ label, value, sub }: { label: string; value: string | number; sub?: string }) {
  return (
    <div className="rounded-lg border bg-card p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="mt-1 text-2xl font-bold">{value}</p>
      {sub && <p className="mt-0.5 text-xs text-muted-foreground">{sub}</p>}
    </div>
  );
}

function statusTone(status: string) {
  if (status === "Completed" || status === "Finalized" || status === "Delivered") {
    return "bg-green-100 text-green-700";
  }

  if (status === "Cancelled" || status === "CancelledByCustomer") {
    return "bg-red-100 text-red-700";
  }

  return "bg-yellow-100 text-yellow-700";
}

function OrderCard({ order }: { order: MerchantDashboardOrder }) {
  return (
    <div className="rounded-lg border bg-card p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="font-medium">{order.customerEmail}</p>
          <p className="text-xs text-muted-foreground">
            {new Date(order.createdAt).toLocaleString()}
          </p>
        </div>

        <div className="text-right">
          <span className={`inline-flex rounded-full px-2 py-1 text-xs font-medium ${statusTone(order.status)}`}>
            {order.status}
          </span>
          <p className="mt-2 text-sm font-semibold">${order.merchantTotalAmount.toFixed(2)}</p>
        </div>
      </div>

      <div className="mt-3 space-y-2">
        {order.items.map((item) => (
          <div key={`${order.orderId}-${item.catalogId}`} className="flex items-center justify-between text-sm">
            <div>
              <p className="font-medium">{item.productName}</p>
              <p className="text-xs text-muted-foreground">
                Qty {item.quantity} × ${item.unitPrice.toFixed(2)}
              </p>
            </div>
            <p className="font-semibold">${item.lineTotal.toFixed(2)}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

function ProductSalesCard({
  product,
  currentStock,
}: {
  product: MerchantDashboardProductSales;
  currentStock: number;
}) {
  return (
    <div className="rounded-lg border bg-card p-4">
      <p className="truncate font-medium">{product.productName}</p>
      <div className="mt-3 grid grid-cols-3 gap-2 text-sm">
        <div>
          <p className="text-muted-foreground">Units sold</p>
          <p className="font-semibold">{product.unitsSold}</p>
        </div>
        <div>
          <p className="text-muted-foreground">Orders</p>
          <p className="font-semibold">{product.orderCount}</p>
        </div>
        <div>
          <p className="text-muted-foreground">Stock</p>
          <p className="font-semibold">{currentStock}</p>
        </div>
      </div>
    </div>
  );
}

// ─── Product Row ─────────────────────────────────────────────────────────────

function ProductRow({
  product,
  updating,
  onStock,
}: {
  product: MerchantProduct;
  updating: boolean;
  onStock: (delta: number) => void;
}) {
  const isLow = product.stock <= 5;
  return (
    <div className="flex items-center gap-4 rounded-lg border bg-card p-4 transition-all">
      {product.imageUrl ? (
        <img
          src={product.imageUrl}
          alt={product.name}
          className="h-14 w-14 rounded-md object-cover"
          onError={(e) => {
            (e.target as HTMLImageElement).style.display = "none";
          }}
        />
      ) : (
        <div className="flex h-14 w-14 items-center justify-center rounded-md bg-muted">
          <Package className="h-6 w-6 text-muted-foreground" />
        </div>
      )}

      <div className="min-w-0 flex-1">
        <p className="truncate font-medium">{product.name}</p>
        <p className="text-sm text-muted-foreground">
          {product.categories.join(", ")}
        </p>
        <p className="text-sm font-semibold">${product.price.toFixed(2)}</p>
      </div>

      <div className="flex items-center gap-3">
        <div className="text-right">
          <p
            className={`text-lg font-bold tabular-nums ${isLow ? "text-red-600" : "text-green-600"}`}
          >
            {product.stock}
          </p>
          <p className="text-[10px] text-muted-foreground">in stock</p>
        </div>

        <div className="flex flex-col gap-1">
          <Button
            size="sm"
            variant="outline"
            className="h-8 w-8 p-0"
            disabled={updating}
            onClick={() => onStock(1)}
          >
            <TrendingUp className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="outline"
            className="h-8 w-8 p-0"
            disabled={updating || product.stock <= 1}
            onClick={() => onStock(-1)}
          >
            <TrendingDown className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}

// ─── Main Page ───────────────────────────────────────────────────────────────

export default function MerchantPage() {
  const [merchant, setMerchant] = useState<MerchantProfile | null>(null);
  const [products, setProducts] = useState<MerchantProduct[]>([]);
  const [overview, setOverview] = useState<MerchantDashboardOverview | null>(null);
  const [loading, setLoading] = useState(true);
  const [overviewLoading, setOverviewLoading] = useState(true);
  const [updatingStock, setUpdatingStock] = useState<Record<string, boolean>>({});
  const [sseConnected, setSseConnected] = useState(false);
  const esRef = useRef<EventSource | null>(null);

  const handleLogout = useCallback(() => {
    logout();
    window.location.href = "/";
  }, []);

  const loadDashboard = useCallback(async () => {
    setLoading(true);
    try {
      const profile = await getMerchantMe();
      setMerchant(profile);
      const result = await getMerchantProducts(profile.id);
      setProducts(result.items);
    } catch {
      toast.error("Failed to load dashboard.");
    } finally {
      setLoading(false);
    }
  }, []);

  const loadOverview = useCallback(async (silent = false) => {
    if (!silent) {
      setOverviewLoading(true);
    }

    try {
      const result = await getMerchantDashboardOverview();
      setOverview(result);
    } catch {
      if (!silent) {
        toast.error("Failed to load sales dashboard.");
      }
    } finally {
      if (!silent) {
        setOverviewLoading(false);
      }
    }
  }, []);

  // SSE connection
  useEffect(() => {
    let es: EventSource | null = null;
    let reconnectTimer: ReturnType<typeof setTimeout> | null = null;
    let closed = false;

    const connect = () => {
      if (closed) return;
      es = createMerchantEventSource();
      esRef.current = es;

      es.onopen = () => setSseConnected(true);

      es.onmessage = (e) => {
        try {
          const data = JSON.parse(e.data) as { type?: string } & StockChangedEvent;
          if (data.type === "connected") return;

          setProducts((prev) =>
            prev.map((p) =>
              p.id === data.catalogId ? { ...p, stock: data.newStock } : p,
            ),
          );
        } catch {
          // ignore malformed events
        }
      };

      es.onerror = () => {
        setSseConnected(false);
        es?.close();
        esRef.current = null;
        // Reconnect after 3 s if the component is still mounted
        if (!closed) {
          reconnectTimer = setTimeout(connect, 3000);
        }
      };
    };

    connect();

    return () => {
      closed = true;
      if (reconnectTimer) clearTimeout(reconnectTimer);
      es?.close();
      esRef.current = null;
      setSseConnected(false);
    };
  }, []);

  useEffect(() => {
    loadDashboard();
    loadOverview();
  }, [loadDashboard, loadOverview]);

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      void loadOverview(true);
    }, 5000);

    return () => {
      window.clearInterval(intervalId);
    };
  }, [loadOverview]);

  const handleStockUpdate = async (product: MerchantProduct, delta: number) => {
    setUpdatingStock((s) => ({ ...s, [product.id]: true }));
    try {
      const result = await updateStock(product.id, delta);
      setProducts((prev) =>
        prev.map((p) => (p.id === product.id ? { ...p, stock: result.newStock } : p)),
      );
      void loadOverview(true);
    } catch {
      toast.error(`Failed to update stock for "${product.name}".`);
    } finally {
      setUpdatingStock((s) => ({ ...s, [product.id]: false }));
    }
  };

  // Stats
  const totalProducts = products.length;
  const totalStock = products.reduce((s, p) => s + p.stock, 0);
  const lowStockCount = products.filter((p) => p.stock <= 5).length;
  const stockByProductId = new Map(products.map((product) => [product.id, product.stock]));

  return (
    <div className="min-h-screen bg-background">
      <MerchantHeader
        merchant={merchant}
        connected={sseConnected}
        onLogout={handleLogout}
      />

      <main className="mx-auto w-full max-w-6xl space-y-6 px-4 py-6">
        {/* Stats */}
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-6">
          <StatCard label="Products" value={totalProducts} />
          <StatCard label="Total Stock" value={totalStock} sub="units across all products" />
          <StatCard
            label="Low Stock"
            value={lowStockCount}
            sub={lowStockCount > 0 ? "≤ 5 units — attention needed" : "All good"}
          />
          <StatCard
            label="Sales"
            value={overview?.summary.totalOrders ?? 0}
            sub="orders with your products"
          />
          <StatCard
            label="Units Sold"
            value={overview?.summary.totalUnitsSold ?? 0}
            sub="merchant product quantity sold"
          />
          <StatCard
            label="Revenue"
            value={`$${(overview?.summary.totalRevenue ?? 0).toFixed(2)}`}
            sub="revenue from your products"
          />
        </div>

        <div className="grid gap-6 lg:grid-cols-[1.4fr_1fr]">
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-base font-semibold">Recent Sales</h2>
                <p className="text-xs text-muted-foreground">
                  Payment / order status refreshed every 5 seconds
                </p>
              </div>
              {overview?.generatedAt && (
                <p className="text-xs text-muted-foreground">
                  Updated {new Date(overview.generatedAt).toLocaleTimeString()}
                </p>
              )}
            </div>

            {overviewLoading ? (
              <div className="space-y-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <Skeleton key={i} className="h-32 w-full rounded-lg" />
                ))}
              </div>
            ) : overview?.recentOrders.length ? (
              <div className="space-y-3">
                {overview.recentOrders.map((order) => (
                  <OrderCard key={order.orderId} order={order} />
                ))}
              </div>
            ) : (
              <div className="rounded-lg border border-dashed py-12 text-center text-sm text-muted-foreground">
                No sales yet for this merchant.
              </div>
            )}
          </div>

          <div className="space-y-3">
            <div>
              <h2 className="text-base font-semibold">Best Selling Products</h2>
              <p className="text-xs text-muted-foreground">
                Live stock combines your sales snapshot and current stock
              </p>
            </div>

            {overviewLoading ? (
              <div className="space-y-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <Skeleton key={i} className="h-24 w-full rounded-lg" />
                ))}
              </div>
            ) : overview?.productSales.length ? (
              <div className="space-y-3">
                {overview.productSales.map((product) => (
                  <ProductSalesCard
                    key={product.catalogId}
                    product={product}
                    currentStock={stockByProductId.get(product.catalogId) ?? product.currentStock}
                  />
                ))}
              </div>
            ) : (
              <div className="rounded-lg border border-dashed py-12 text-center text-sm text-muted-foreground">
                Sell a product to see merchant analytics.
              </div>
            )}
          </div>
        </div>

        {/* Products */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h2 className="text-base font-semibold">Your Products</h2>
            <Button
              variant="outline"
              size="sm"
              onClick={loadDashboard}
              disabled={loading}
            >
              <RefreshCw className={`mr-1.5 h-3.5 w-3.5 ${loading ? "animate-spin" : ""}`} />
              Refresh
            </Button>
          </div>

          {loading ? (
            <div className="space-y-3">
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton key={i} className="h-24 w-full rounded-lg" />
              ))}
            </div>
          ) : products.length === 0 ? (
            <div className="flex flex-col items-center gap-2 rounded-lg border border-dashed py-16 text-center text-muted-foreground">
              <Package className="h-10 w-10" />
              <p className="text-sm">No products yet.</p>
            </div>
          ) : (
            <div className="space-y-2">
              {products.map((product) => (
                <ProductRow
                  key={product.id}
                  product={product}
                  updating={!!updatingStock[product.id]}
                  onStock={(delta) => handleStockUpdate(product, delta)}
                />
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
