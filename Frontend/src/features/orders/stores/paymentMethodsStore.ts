import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import { getPaymentMethods } from '@/api/paymentMethods';
import type { PaymentMethod } from '@/features/orders/types/paymentMethod';

interface PaymentMethodsState {
  paymentMethods: PaymentMethod[];
  isLoading: boolean;
  error: string | null;
}

interface PaymentMethodsActions {
  fetchPaymentMethods: () => Promise<void>;
  getActivePaymentMethods: () => PaymentMethod[];
}

export const usePaymentMethodsStore = create<
  PaymentMethodsState & PaymentMethodsActions
>()(
  immer((set, get) => ({
    paymentMethods: [],
    isLoading: false,
    error: null,

    fetchPaymentMethods: async () => {
      set({ isLoading: true, error: null });
      try {
        const paymentMethods = await getPaymentMethods();
        set({ paymentMethods, isLoading: false });
      } catch (error) {
        set({ error: (error as Error).message, isLoading: false });
      }
    },

    getActivePaymentMethods: () => {
      return get().paymentMethods.filter((pm) => pm.isActive);
    },
  }))
);
