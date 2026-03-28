import type { CashClosing, CreateCashClosingInput } from '@/features/orders/types/cashClosing';
import { mockCashClosings } from './mockData';
import { getPayments } from '../payments';

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

const cashClosings = [...mockCashClosings];
let nextId = Math.max(...cashClosings.map((c) => c.id), 0) + 1;

// Obtener totales de pagos del día actual por método de pago
export async function getTodayPaymentTotals(): Promise<{
  cashAmount: number;
  cardAmount: number;
  transferAmount: number;
  checkAmount: number;
  totalSales: number;
}> {
  const today = new Date();
  const startOfDay = new Date(today.getFullYear(), today.getMonth(), today.getDate()).toISOString();
  const endOfDay = new Date(
    today.getFullYear(),
    today.getMonth(),
    today.getDate(),
    23,
    59,
    59,
    999
  ).toISOString();

  const payments = await getPayments({
    startDate: startOfDay,
    endDate: endOfDay,
  });

  // IDs de métodos de pago: 1=Efectivo, 2=Tarjeta, 3=Transferencia, 4=Cheque
  const cashAmount = payments
    .filter((p) => p.paymentMethodId === 1)
    .reduce((sum, p) => sum + p.amount, 0);

  const cardAmount = payments
    .filter((p) => p.paymentMethodId === 2)
    .reduce((sum, p) => sum + p.amount, 0);

  const transferAmount = payments
    .filter((p) => p.paymentMethodId === 3)
    .reduce((sum, p) => sum + p.amount, 0);

  const checkAmount = payments
    .filter((p) => p.paymentMethodId === 4)
    .reduce((sum, p) => sum + p.amount, 0);

  const totalSales = cashAmount + cardAmount + transferAmount + checkAmount;

  return {
    cashAmount,
    cardAmount,
    transferAmount,
    checkAmount,
    totalSales,
  };
}

export async function createCashClosing(
  input: CreateCashClosingInput
): Promise<CashClosing> {
  await delay(300);

  const currentUserId = 1; // Mock

  // Validaciones
  if (input.actualCashAmount < 0) {
    throw new Error('El efectivo contado no puede ser negativo');
  }

  // Obtener totales del día automáticamente
  const totals = await getTodayPaymentTotals();

  const expectedCashAmount = totals.cashAmount;
  const cashDiscrepancy = input.actualCashAmount - input.fondoInicial + input.adjustment - expectedCashAmount;

  const newClosing: CashClosing = {
    id: nextId++,
    closingDate: new Date().toISOString(),
    cashAmount: totals.cashAmount,
    cardAmount: totals.cardAmount,
    transferAmount: totals.transferAmount,
    checkAmount: totals.checkAmount,
    fondoInicial: input.fondoInicial,
    expectedCashAmount,
    actualCashAmount: input.actualCashAmount,
    cashWithdrawal: input.cashWithdrawal,
    adjustment: input.adjustment,
    cashDiscrepancy,
    totalSales: totals.totalSales,
    notes: input.notes,
    createdBy: currentUserId,
    createdAt: new Date().toISOString(),
  };

  cashClosings.push(newClosing);

  return newClosing;
}
