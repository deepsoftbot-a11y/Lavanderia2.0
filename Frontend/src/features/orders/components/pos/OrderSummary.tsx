interface OrderSummaryProps {
  itemCount: number;
  subtotal: number;
  totalDiscount: number;
  total: number;
}

export function OrderSummary({
  itemCount,
  subtotal,
  totalDiscount,
  total,
}: OrderSummaryProps) {
  return (
    <div>
      <div className="flex justify-between py-2 border-b border-zinc-100">
        <span className="text-xs text-zinc-500">Items ({itemCount})</span>
        <span className="text-xs font-mono tabular-nums text-zinc-700">
          ${subtotal.toFixed(2)}
        </span>
      </div>

      {totalDiscount > 0 && (
        <div className="flex justify-between py-2 border-b border-zinc-100">
          <span className="text-xs text-emerald-600">Descuento</span>
          <span className="text-xs font-mono tabular-nums text-emerald-600">
            -${totalDiscount.toFixed(2)}
          </span>
        </div>
      )}

      <div className="flex justify-between pt-3">
        <span className="text-xs font-semibold text-zinc-900">Total</span>
        <span className="font-mono font-bold tabular-nums text-zinc-900">
          ${total.toFixed(2)}
        </span>
      </div>
    </div>
  );
}
