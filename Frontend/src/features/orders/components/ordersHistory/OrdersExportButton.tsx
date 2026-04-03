import { useState } from 'react';
import { Download, FileSpreadsheet, FileText, Loader2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { useToast } from '@/shared/hooks/use-toast';
import { exportOrders } from '@/api/orders';
import type { OrderHistoryFilters } from '@/features/orders/types/order';

interface OrdersExportButtonProps {
  activeFilters: OrderHistoryFilters;
  disabled?: boolean;
}

export function OrdersExportButton({ activeFilters, disabled }: OrdersExportButtonProps) {
  const [isExporting, setIsExporting] = useState(false);
  const { toast } = useToast();

  async function handleExport(format: 'xlsx' | 'pdf') {
    setIsExporting(true);
    try {
      await exportOrders(format, activeFilters);
    } catch {
      toast({
        variant: 'destructive',
        title: 'No se pudo generar el reporte',
        description: 'Intenta de nuevo o contacta soporte si el problema persiste.',
      });
    } finally {
      setIsExporting(false);
    }
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className="h-8 text-xs"
          disabled={disabled || isExporting}
        >
          {isExporting ? (
            <Loader2 className="h-3.5 w-3.5 mr-1.5 animate-spin" />
          ) : (
            <Download className="h-3.5 w-3.5 mr-1.5" />
          )}
          Exportar
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem
          disabled={isExporting}
          onClick={() => handleExport('xlsx')}
          className="text-xs cursor-pointer"
        >
          <FileSpreadsheet className="h-3.5 w-3.5 mr-2 text-green-600" />
          Exportar Excel
        </DropdownMenuItem>
        <DropdownMenuItem
          disabled={isExporting}
          onClick={() => handleExport('pdf')}
          className="text-xs cursor-pointer"
        >
          <FileText className="h-3.5 w-3.5 mr-2 text-red-500" />
          Exportar PDF
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
