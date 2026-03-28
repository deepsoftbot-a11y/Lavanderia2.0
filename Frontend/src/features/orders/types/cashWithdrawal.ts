export interface CashWithdrawal {
  id: number;
  amount: number;
  reason: string;
  notes?: string;
  withdrawalDate: string; // ISO 8601
  createdBy: number;
  createdAt: string;
}

export interface CreateCashWithdrawalInput {
  amount: number;
  reason: string;
  notes?: string;
}
