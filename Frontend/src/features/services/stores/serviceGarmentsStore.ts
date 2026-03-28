import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type {
  ServiceGarment,
  CreateServiceGarmentInput,
  UpdateServiceGarmentInput,
} from '@/features/services/types/serviceGarment';
import * as serviceGarmentsApi from '@/api/serviceGarments';

interface ServiceGarmentsState {
  serviceGarments: ServiceGarment[];
  isLoading: boolean;
  error: string | null;

  fetchServiceGarments: (serviceId?: number) => Promise<void>;
  createServiceGarment: (input: CreateServiceGarmentInput) => Promise<ServiceGarment | null>;
  updateServiceGarment: (id: number, input: UpdateServiceGarmentInput) => Promise<ServiceGarment | null>;
  deleteServiceGarment: (id: number) => Promise<void>;
  toggleStatus: (id: number) => Promise<ServiceGarment | null>;
  clearError: () => void;
}

export const useServiceGarmentsStore = create<ServiceGarmentsState>()(
  immer((set) => ({
    serviceGarments: [],
    isLoading: false,
    error: null,

    fetchServiceGarments: async (serviceId) => {
      set({ isLoading: true, error: null });
      try {
        const serviceGarments = await serviceGarmentsApi.getServiceGarments(serviceId);
        set({ serviceGarments, isLoading: false });
      } catch (error) {
        const message =
          error instanceof Error ? error.message : 'Error al cargar precios de prenda';
        set({ error: message, isLoading: false });
      }
    },

    createServiceGarment: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const serviceGarment = await serviceGarmentsApi.createServiceGarment(input);
        set((state) => {
          state.serviceGarments.push(serviceGarment);
          state.isLoading = false;
        });
        return serviceGarment;
      } catch (error) {
        const message =
          error instanceof Error ? error.message : 'Error al crear precio de prenda';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    updateServiceGarment: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const serviceGarment = await serviceGarmentsApi.updateServiceGarment(id, input);
        set((state) => {
          const index = state.serviceGarments.findIndex((g) => g.id === id);
          if (index !== -1) state.serviceGarments[index] = serviceGarment;
          state.isLoading = false;
        });
        return serviceGarment;
      } catch (error) {
        const message =
          error instanceof Error ? error.message : 'Error al actualizar precio de prenda';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    deleteServiceGarment: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await serviceGarmentsApi.deleteServiceGarment(id);
        set((state) => {
          state.serviceGarments = state.serviceGarments.filter((g) => g.id !== id);
          state.isLoading = false;
        });
      } catch (error) {
        const message =
          error instanceof Error ? error.message : 'Error al eliminar precio de prenda';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    toggleStatus: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const serviceGarment = await serviceGarmentsApi.toggleServiceGarmentStatus(id);
        set((state) => {
          const index = state.serviceGarments.findIndex((g) => g.id === id);
          if (index !== -1) state.serviceGarments[index] = serviceGarment;
          state.isLoading = false;
        });
        return serviceGarment;
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
