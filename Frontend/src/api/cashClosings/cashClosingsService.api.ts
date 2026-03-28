import api from '@/api/axiosConfig';
import type { CashClosing, CreateCashClosingInput } from '@/features/orders/types/cashClosing';

interface VentaDiariaResponse {
  totalEfectivo: number;
  totalTarjeta: number;
  totalTransferencia: number;
  totalOtros: number;
  totalPagado: number;
}

/**
 * Obtiene el ID del cajero desde localStorage
 */
function getCajeroId(): number {
  const userJson = localStorage.getItem('auth_user');
  return userJson ? Number.parseInt(JSON.parse(userJson).id, 10) : 0;
}

/**
 * GET /api/cortes-caja/totales-dia
 * Obtiene totales de ventas del día para un cajero específico.
 */
export async function getTodayPaymentTotals(): Promise<{
  cashAmount: number;
  cardAmount: number;
  transferAmount: number;
  checkAmount: number;
  totalSales: number;
}> {
  try {
    const today = new Date().toLocaleDateString('en-CA'); // YYYY-MM-DD en zona horaria local
    const cajeroId = getCajeroId();

    const response = await api.get<VentaDiariaResponse>('/cortes-caja/totales-dia', {
      params: { fecha: today, cajeroId },
    });
    return {
      cashAmount: response.data.totalEfectivo,
      cardAmount: response.data.totalTarjeta,
      transferAmount: response.data.totalTransferencia,
      checkAmount: response.data.totalOtros,
      totalSales: response.data.totalPagado,
    };
  } catch (error) {
    console.error('Error al obtener totales del día:', error);
    throw new Error('Error al obtener totales desde el servidor');
  }
}

/**
 * POST /api/cortes-caja
 * Crea un corte de caja con el ajuste incluido en la misma llamada.
 */
export async function createCashClosing(input: CreateCashClosingInput): Promise<CashClosing> {
  try {
    const totals = await getTodayPaymentTotals();
    const cajeroId = getCajeroId();
    const now = new Date();
    const startOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate());

    // Calcular total declarado
    const totalDeclarado = input.actualCashAmount + totals.cardAmount + totals.transferAmount + totals.checkAmount;

    const response = await api.post<{ corteID: number }>('/cortes-caja', {
      cajeroID: cajeroId,
      fechaInicio: startOfDay.toISOString(),
      fechaFin: now.toISOString(),
      fechaCorte: now.toISOString(),
      turnoDescripcion: '',
      // Totales esperados (del sistema)
      totalEsperadoEfectivo: totals.cashAmount,
      totalEsperadoTarjeta: totals.cardAmount,
      totalEsperadoTransferencia: totals.transferAmount,
      totalEsperadoOtros: totals.checkAmount,
      totalEsperado: totals.totalSales,
      // Totales declarados (por el cajero)
      totalDeclaradoEfectivo: input.actualCashAmount,
      totalDeclaradoTarjeta: totals.cardAmount,
      totalDeclaradoTransferencia: totals.transferAmount,
      totalDeclaradoOtros: totals.checkAmount,
      totalDeclarado,
      // Ajuste
      montoAjuste: input.adjustment,
      motivoAjuste: input.notes ?? null,
      observaciones: input.notes ?? null,
      fondoInicial: input.fondoInicial,
    });

    const cashDiscrepancy = input.actualCashAmount - input.fondoInicial + input.adjustment - totals.cashAmount;

    return {
      id: response.data.corteID,
      closingDate: now.toISOString(),
      cashAmount: totals.cashAmount,
      cardAmount: totals.cardAmount,
      transferAmount: totals.transferAmount,
      checkAmount: totals.checkAmount,
      fondoInicial: input.fondoInicial,
      expectedCashAmount: totals.cashAmount,
      actualCashAmount: input.actualCashAmount,
      cashWithdrawal: input.cashWithdrawal,
      adjustment: input.adjustment,
      cashDiscrepancy,
      totalSales: totals.totalSales,
      notes: input.notes,
      createdBy: cajeroId,
      createdAt: now.toISOString(),
    };
  } catch (error) {
    console.error('Error al crear corte de caja:', error);
    throw error instanceof Error ? error : new Error('Error al crear el corte en el servidor');
  }
}
