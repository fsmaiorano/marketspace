import { useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { envConfig } from "@/lib/env-config";
import type { CatalogItem } from "@/services/catalog-service";
import { Package, Plus, RefreshCw } from "lucide-react";

interface CatalogSectionProps {
  items: CatalogItem[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  loading: boolean;
  onRefresh: (page: number) => void;
  onAddToBasket: (item: CatalogItem) => void;
  onPageChange: (nextPage: number) => void;
}

const toImageSrc = (imageUrl?: string) => {
  const value = imageUrl?.trim();
  if (!value) return "";
  if (value.startsWith("http://") || value.startsWith("https://") || value.startsWith("data:")) {
    return value;
  }
  if (value.startsWith("/")) {
    return `${envConfig.bffApiUrl}${value}`;
  }
  return `${envConfig.bffApiUrl}/${value}`;
};

function CatalogItemCard({
  item,
  onAdd,
}: {
  item: CatalogItem;
  onAdd: (item: CatalogItem) => void;
}) {
  const [imageBroken, setImageBroken] = useState(false);
  const imageSrc = toImageSrc(item.imageUrl);
  const showImage = imageSrc.length > 0 && !imageBroken;
  const outOfStock = item.stock === 0;
  const lowStock = item.stock > 0 && item.stock <= 5;

  return (
    <div className="flex flex-col overflow-hidden rounded-xl border bg-card transition-shadow hover:shadow-md">
      <div className="bg-muted relative h-36 flex items-center justify-center">
        {showImage ? (
          <img
            src={imageSrc}
            alt={item.name}
            className={`h-full w-full object-cover ${outOfStock ? "opacity-50" : ""}`}
            loading="lazy"
            onError={() => setImageBroken(true)}
          />
        ) : (
          <Package className="h-14 w-14 text-muted-foreground/40" />
        )}
        {outOfStock && (
          <span className="absolute top-2 left-2 rounded bg-gray-800/80 px-2 py-0.5 text-xs font-semibold text-white">
            Out of Stock
          </span>
        )}
        {lowStock && (
          <span className="absolute top-2 left-2 rounded bg-amber-500/90 px-2 py-0.5 text-xs font-semibold text-white">
            Low Stock ({item.stock})
          </span>
        )}
      </div>
      <div className="flex flex-1 flex-col gap-1 p-3">
        <h3 className="line-clamp-2 text-sm font-semibold leading-tight">{item.name}</h3>
        {item.description && <p className="line-clamp-2 text-xs text-muted-foreground">{item.description}</p>}
        {item.categories?.length > 0 && (
          <p className="line-clamp-1 text-xs text-muted-foreground">{item.categories.join(", ")}</p>
        )}
        <div className="mt-auto flex items-center justify-between gap-2 pt-2">
          <span className="text-sm font-bold">${item.price?.toFixed(2)}</span>
          <Button
            size="sm"
            className="h-7 px-2 text-xs"
            onClick={() => onAdd(item)}
            disabled={outOfStock}
            title={outOfStock ? "Out of stock" : undefined}
          >
            {outOfStock ? (
              "Out of Stock"
            ) : (
              <>
                <Plus className="mr-1 h-3 w-3" />
                Add
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}

export function CatalogSection({
  items,
  totalCount,
  pageIndex,
  pageSize,
  loading,
  onRefresh,
  onAddToBasket,
  onPageChange,
}: CatalogSectionProps) {
  const [searchTerm, setSearchTerm] = useState("");

  const filteredItems = useMemo(
    () =>
      items.filter(
        (item) =>
          item.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          item.description?.toLowerCase().includes(searchTerm.toLowerCase()),
      ),
    [items, searchTerm],
  );

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <>
      <div className="flex items-center gap-3">
        <Input
          placeholder="Search products..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="max-w-sm"
        />
        <Button variant="outline" size="sm" onClick={() => onRefresh(pageIndex)} disabled={loading}>
          <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
        </Button>
      </div>

      {loading ? (
        <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="animate-pulse space-y-3 rounded-xl border bg-card p-4">
              <div className="h-4 w-3/4 rounded bg-muted" />
              <div className="h-3 w-full rounded bg-muted" />
              <div className="h-3 w-1/2 rounded bg-muted" />
              <div className="h-8 rounded bg-muted" />
            </div>
          ))}
        </div>
      ) : filteredItems.length === 0 ? (
        <div className="py-16 text-center text-muted-foreground">No products found.</div>
      ) : (
        <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
          {filteredItems.map((item) => (
            <CatalogItemCard key={item.id} item={item} onAdd={onAddToBasket} />
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 pt-4">
          <Button
            variant="outline"
            size="sm"
            disabled={pageIndex === 1 || loading}
            onClick={() => onPageChange(pageIndex - 1)}
          >
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {pageIndex} of {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={pageIndex >= totalPages || loading}
            onClick={() => onPageChange(pageIndex + 1)}
          >
            Next
          </Button>
        </div>
      )}
    </>
  );
}

