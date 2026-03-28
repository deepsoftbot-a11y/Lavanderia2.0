import { Card } from '@/shared/components/ui/card';
import { Separator } from '@/shared/components/ui/separator';
import { cn } from '@/shared/utils/cn';

interface PaymentSummaryCardProps {
  total: number;
  amountPaid?: number;
  className?: string;
}

export function PaymentSummaryCard({
  total,
  amountPaid = 0,
  className,
}: PaymentSummaryCardProps) {
  // Validar que total sea un número válido
  const validTotal = typeof total === 'number' && !Number.isNaN(total) ? total : 0;
  const validAmountPaid = typeof amountPaid === 'number' && !Number.isNaN(amountPaid) ? amountPaid : 0;
  const balance = validTotal - validAmountPaid;

  return (
    <Card className={cn('p-4', className)}>
      <div className="space-y-3">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Total:</span>
          <span className="font-semibold">${total.toFixed(2)}</span>
        </div>

        {amountPaid > 0 && (
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Pagado:</span>
            <span className="text-green-600">${amountPaid.toFixed(2)}</span>
          </div>
        )}

        <Separator />

        <div className="flex justify-between items-center">
          <span className="font-medium">Saldo pendiente:</span>
          <span
            className={cn(
              'font-bold text-lg',
              balance === 0 ? 'text-green-600' : 'text-orange-600'
            )}
          >
            ${balance.toFixed(2)}
          </span>
        </div>
      </div>
    </Card>
  );
}
