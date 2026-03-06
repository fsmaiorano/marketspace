import { Button } from "@/components/ui/button";
import { LogOut, Package, ShoppingCart } from "lucide-react";

type View = "catalog" | "orders";

interface CustomerHeaderProps {
  view: View;
  basketItemCount: number;
  greeting?: string | null;
  onViewChange: (view: View) => void;
  onBasketOpen: () => void;
  onLogout: () => void;
}

export function CustomerHeader({
  view,
  basketItemCount,
  greeting,
  onViewChange,
  onBasketOpen,
  onLogout,
}: CustomerHeaderProps) {
  return (
    <header className="sticky top-0 z-30 border-b bg-card">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-3">
        <div>
          <h1 className="text-xl font-bold">MarketSpace</h1>
          {greeting && <p className="text-xs text-muted-foreground">Hello, {greeting}</p>}
        </div>

        <nav className="flex items-center gap-2">
          <Button
            variant={view === "catalog" ? "default" : "outline"}
            size="sm"
            onClick={() => onViewChange("catalog")}
          >
            <Package className="mr-1 h-4 w-4" />
            Catalog
          </Button>
          <Button
            variant={view === "orders" ? "default" : "outline"}
            size="sm"
            onClick={() => onViewChange("orders")}
          >
            My Orders
          </Button>
          <Button variant="outline" size="sm" className="relative" onClick={onBasketOpen}>
            <ShoppingCart className="h-4 w-4" />
            {basketItemCount > 0 && (
              <span className="absolute -right-1.5 -top-1.5 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-primary-foreground">
                {basketItemCount}
              </span>
            )}
          </Button>
          <Button variant="ghost" size="sm" onClick={onLogout}>
            <LogOut className="h-4 w-4" />
          </Button>
        </nav>
      </div>
    </header>
  );
}

