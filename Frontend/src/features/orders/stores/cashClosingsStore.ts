import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { CashClosing, CreateCashClosingInput } from '@/features/orders/types/cashClosing';
import {
  createCashClosing as apiCreateCashClosing,
  getTodayPaymentTotals,
} from '@/api/cashClosings';

interface PaymentTotals {
  cashAmount: number;
  cardAmount: number;
  transferAmount: number;
  checkAmount: number;
  totalSales: number;
}

interface CashClosingsState {
  isLoading: boolean;
  isLoadingTotals: boolean;
  error: string | null;
  todayTotals: PaymentTotals | null;
  fetchTodayTotals: () => Promise<void>;
  createCashClosing: (
    input: CreateCashClosingInput
  ) => Promise<CashClosing | null>;
  clearError: () => void;
}

export const useCashClosingsStore = create<CashClosingsState>()(
  immer((set) => ({
    isLoading: false,
    isLoadingTotals: false,
    error: null,
    todayTotals: null,

    fetchTodayTotals: async () => {
      set({ isLoadingTotals: true, error: null });
      try {
        const totals = await getTodayPaymentTotals();
        set({ todayTotals: totals, isLoadingTotals: false });
      } catch (error) {
        set({
          error: (error as Error).message,
          isLoadingTotals: false,
        });
      }
    },

    createCashClosing: async (input: CreateCashClosingInput) => {
      set({ isLoading: true, error: null });
      try {
        const closing = await apiCreateCashClosing(input);
        set({ isLoading: false });
        return closing;
      } catch (error) {
        set({
          error: (error as Error).message,
          isLoading: false,
        });
        return null;
      }
    },

    clearError: () => set({ error: null }),
  }))
);
