import { Button } from "@/components/ui/button";
import type { Basket } from "@/services/basket-service";
import { Minus, Plus, ShoppingCart, Trash2, X } from "lucide-react";

interface BasketDrawerProps {
  open: boolean;
  loading: boolean;
  basket: Basket | null;
  basketItemCount: number;
  checkingOut: boolean;
  onClose: () => void;
  onUpdateQuantity: (productId: string, delta: number) => void;
  onRemoveItem: (productId: string) => void;
  onCheckout: () => void;
}

export function BasketDrawer({
  open,
  loading,
  basket,
  basketItemCount,
  checkingOut,
  onClose,
  onUpdateQuantity,
  onRemoveItem,
  onCheckout,
}: BasketDrawerProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex">
      <div className="flex-1 bg-black/40" onClick={onClose} />
      <div className="flex h-full w-full max-w-sm flex-col border-l bg-background shadow-xl">
        <div className="flex items-center justify-between border-b px-4 py-3">
          <h2 className="flex items-center gap-2 text-lg font-semibold">
            <ShoppingCart className="h-5 w-5" />
            Basket
            {basketItemCount > 0 && (
              <span className="text-sm text-muted-foreground">({basketItemCount} items)</span>
            )}
          </h2>
          <button className="rounded p-1 hover:bg-muted" onClick={onClose}>
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="flex-1 space-y-3 overflow-y-auto px-4 py-3">
          {loading ? (
            <div className="space-y-3">
              {Array.from({ length: 2 }).map((_, i) => (
                <div key={i} className="h-16 animate-pulse rounded-lg bg-muted" />
              ))}
            </div>
          ) : !basket || basket.items.length === 0 ? (
            <div className="py-16 text-center text-muted-foreground">Your basket is empty.</div>
          ) : (
            basket.items.map((item) => (
              <div key={item.productId} className="flex items-start gap-3 rounded-lg border bg-card p-3">
                <div className="min-w-0 flex-1">
                  <p className="line-clamp-2 text-sm font-medium leading-tight">{item.productName}</p>
                  <p className="mt-1 text-sm font-semibold">${(item.price * item.quantity).toFixed(2)}</p>
                </div>
                <div className="flex items-center gap-1">
                  <button
                    className="flex h-6 w-6 items-center justify-center rounded border hover:bg-muted"
                    onClick={() => onUpdateQuantity(item.productId, -1)}
                  >
                    <Minus className="h-3 w-3" />
                  </button>
                  <span className="w-6 text-center text-sm">{item.quantity}</span>
                  <button
                    className="flex h-6 w-6 items-center justify-center rounded border hover:bg-muted"
                    onClick={() => onUpdateQuantity(item.productId, 1)}
                  >
                    <Plus className="h-3 w-3" />
                  </button>
                  <button
                    className="ml-1 flex h-6 w-6 items-center justify-center rounded border text-red-500 hover:bg-muted"
                    onClick={() => onRemoveItem(item.productId)}
                  >
                    <Trash2 className="h-3 w-3" />
                  </button>
                </div>
              </div>
            ))
          )}
        </div>

        {basket && basket.items.length > 0 && (
          <div className="space-y-3 border-t px-4 py-4">
            <div className="flex items-center justify-between">
              <span className="font-semibold">Total</span>
              <span className="text-lg font-bold">${basket.totalPrice?.toFixed(2) ?? "0.00"}</span>
            </div>
            <Button className="w-full" disabled={checkingOut} onClick={onCheckout}>
              {checkingOut ? "Placing order..." : "Checkout"}
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}

