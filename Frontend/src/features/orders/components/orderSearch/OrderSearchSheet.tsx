import { useState, useCallback, useEffect, useRef } from 'react';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from '@/shared/components/ui/sheet';
import { ScrollArea } from '@/shared/components/ui/scroll-area';
import { OrderSearchInput } from './OrderSearchInput';
import { OrderSearchResultsList } from './OrderSearchResultsList';
import { OrderDetailView } from './OrderDetailView';
import { useOrdersStore } from '@/features/orders/stores/ordersStore';
import type { OrderSummary } from '@/features/orders/types/order';

interface OrderSearchSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialOrderId?: number;
}

export function OrderSearchSheet({ open, onOpenChange, initialOrderId }: OrderSearchSheetProps) {
  const [view, setView] = useState<'search' | 'detail'>('search');
  const [hasSearched, setHasSearched] = useState(false);
  const loadedOrderIdRef = useRef<number | undefined>(undefined);

  const {
    searchResults,
    isSearching,
    searchError,
    selectedOrder,
    searchOrders,
    setSelectedOrder,
    fetchOrderById,
    clearSearchResults,
  } = useOrdersStore();

  useEffect(() => {
    if (!open) {
      setView('search');
      setHasSearched(false);
      setSelectedOrder(null);
      clearSearchResults();
      loadedOrderIdRef.current = undefined;
    }
  }, [open, setSelectedOrder, clearSearchResults]);

  useEffect(() => {
    if (open && initialOrderId !== undefined && loadedOrderIdRef.current !== initialOrderId) {
      loadedOrderIdRef.current = initialOrderId;
      fetchOrderById(initialOrderId).then(() => setView('detail'));
    }
  }, [open, initialOrderId, fetchOrderById]);

  const handleSearch = useCallback((query: string) => {
    setHasSearched(true);
    searchOrders({ query });
  }, [searchOrders]);

  const handleClearSearch = useCallback(() => {
    setHasSearched(false);
    clearSearchResults();
  }, [clearSearchResults]);

  const handleSelectOrder = useCallback(async (order: OrderSummary) => {
    await fetchOrderById(order.id);
    setView('detail');
  }, [fetchOrderById]);

  const handleBack = useCallback(() => {
    setView('search');
    setSelectedOrder(null);
  }, [setSelectedOrder]);

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="w-full sm:max-w-[540px] p-0 gap-0 flex flex-col">

        {view === 'search' ? (
          <>
            {/* Header — patrón system.md */}
            <SheetHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
              <SheetTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
                Buscar Orden
              </SheetTitle>
              <SheetDescription className="text-xs text-zinc-400">
                Busca por folio, nombre o teléfono
              </SheetDescription>
            </SheetHeader>

            {/* Input de búsqueda */}
            <div className="px-6 py-3 border-b border-zinc-100">
              <OrderSearchInput
                onSearch={handleSearch}
                onClear={handleClearSearch}
                isSearching={isSearching}
              />
            </div>

            {/* Resultados */}
            <ScrollArea className="flex-1">
              <OrderSearchResultsList
                results={searchResults}
                isSearching={isSearching}
                searchError={searchError}
                hasSearched={hasSearched}
                onSelectOrder={handleSelectOrder}
              />
            </ScrollArea>
          </>
        ) : selectedOrder ? (
          <OrderDetailView
            order={selectedOrder}
            onBack={handleBack}
          />
        ) : null}

      </SheetContent>
    </Sheet>
  );
}
