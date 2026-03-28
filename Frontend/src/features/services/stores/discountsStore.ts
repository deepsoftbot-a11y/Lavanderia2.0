import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { Discount, CreateDiscountInput, UpdateDiscountInput } from '@/features/services/types/discount';
import * as discountsApi from '@/api/discounts';

interface DiscountsState {
  discounts: Discount[];
  isLoading: boolean;
  error: string | null;

  fetchDiscounts: () => Promise<void>;
  createDiscount: (input: CreateDiscountInput) => Promise<Discount | null>;
  updateDiscount: (id: number, input: UpdateDiscountInput) => Promise<Discount | null>;
  deleteDiscount: (id: number) => Promise<void>;
  toggleStatus: (id: number) => Promise<Discount | null>;
  clearError: () => void;
}

export const useDiscountsStore = create<DiscountsState>()(
  immer((set) => ({
    discounts: [],
    isLoading: false,
    error: null,

    fetchDiscounts: async () => {
      set({ isLoading: true, error: null });
      try {
        const discounts = await discountsApi.getDiscounts();
        set({ discounts, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar descuentos';
        set({ error: message, isLoading: false });
      }
    },

    createDiscount: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const discount = await discountsApi.createDiscount(input);
        set((state) => {
          state.discounts.push(discount);
          state.isLoading = false;
        });
        return discount;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear descuento';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    updateDiscount: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const discount = await discountsApi.updateDiscount(id, input);
        set((state) => {
          const index = state.discounts.findIndex((d) => d.id === id);
          if (index !== -1) state.discounts[index] = discount;
          state.isLoading = false;
        });
        return discount;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar descuento';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    deleteDiscount: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await discountsApi.deleteDiscount(id);
        set((state) => {
          state.discounts = state.discounts.filter((d) => d.id !== id);
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar descuento';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    toggleStatus: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const discount = await discountsApi.toggleDiscountStatus(id);
        set((state) => {
          const index = state.discounts.findIndex((d) => d.id === id);
          if (index !== -1) state.discounts[index] = discount;
          state.isLoading = false;
        });
        return discount;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cambiar estado';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    clearError: () => {
      set({ error: null });
    },
  }))
);
