export interface CashClosing {
  id: number;
  closingDate: string; // ISO 8601

  // Desglose por método de pago
  cashAmount: number;
  cardAmount: number;
  transferAmount: number;
  checkAmount: number;

  // Conteo de efectivo
  fondoInicial: number; // Dinero con el que se inicia la caja
  expectedCashAmount: number;
  actualCashAmount: number;
  cashWithdrawal: number; // Retiro de efectivo durante el día
  adjustment: number;
  cashDiscrepancy: number; // Calculado: (actual + adjustment) - expected

  // Total
  totalSales: number; // Suma de todos los métodos

  notes?: string;
  createdBy: number;
  createdAt: string;
}

export interface CreateCashClosingInput {
  fondoInicial: number;
  actualCashAmount: number;
  cashWithdrawal: number;
  adjustment: number;
  notes?: string;
}
