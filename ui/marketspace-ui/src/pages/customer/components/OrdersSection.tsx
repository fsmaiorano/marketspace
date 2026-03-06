import { Button } from "@/components/ui/button";
import type { Order } from "@/services/order-service";
import { RefreshCw } from "lucide-react";

interface OrdersSectionProps {
  orders: Order[];
  loading: boolean;
  onRefresh: () => void;
}

export function OrdersSection({ orders, loading, onRefresh }: OrdersSectionProps) {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">My Orders</h2>
        <Button variant="outline" size="sm" onClick={onRefresh} disabled={loading}>
          <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
        </Button>
      </div>

      {loading ? (
        <div className="space-y-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="animate-pulse space-y-2 rounded-xl border bg-card p-4">
              <div className="h-4 w-1/3 rounded bg-muted" />
              <div className="h-3 w-1/2 rounded bg-muted" />
            </div>
          ))}
        </div>
      ) : orders.length === 0 ? (
        <div className="py-16 text-center text-muted-foreground">No orders found.</div>
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
              <div key={order.id} className="space-y-3 rounded-xl border bg-card p-4">
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
                    <span className="text-sm font-bold">${order.totalAmount?.toFixed(2)}</span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

