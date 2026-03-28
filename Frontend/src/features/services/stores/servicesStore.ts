import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { Service, CreateServiceInput, UpdateServiceInput } from '@/features/services/types/service';
import * as servicesApi from '@/api/services';

interface ServicesState {
  services: Service[];
  selectedService: Service | null;
  isLoading: boolean;
  error: string | null;

  fetchServices: () => Promise<void>;
  fetchServiceById: (id: number) => Promise<void>;
  createService: (input: CreateServiceInput) => Promise<Service | null>;
  updateService: (id: number, input: UpdateServiceInput) => Promise<Service | null>;
  deleteService: (id: number) => Promise<void>;
  toggleStatus: (id: number) => Promise<Service | null>;
  setSelectedService: (service: Service | null) => void;
  clearError: () => void;
}

export const useServicesStore = create<ServicesState>()(
  immer((set) => ({
    services: [],
    selectedService: null,
    isLoading: false,
    error: null,

    fetchServices: async () => {
      set({ isLoading: true, error: null });
      try {
        const services = await servicesApi.getServices();
        set({ services, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar servicios';
        set({ error: message, isLoading: false });
      }
    },

    fetchServiceById: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const service = await servicesApi.getServiceById(id);
        set({ selectedService: service, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar servicio';
        set({ error: message, isLoading: false });
      }
    },

    createService: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const service = await servicesApi.createService(input);
        set((state) => {
          state.services.push(service);
          state.isLoading = false;
        });
        return service;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear servicio';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    updateService: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const service = await servicesApi.updateService(id, input);
        set((state) => {
          const index = state.services.findIndex((s) => s.id === id);
          if (index !== -1) state.services[index] = service;
          if (state.selectedService?.id === id) state.selectedService = service;
          state.isLoading = false;
        });
        return service;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar servicio';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    deleteService: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await servicesApi.deleteService(id);
        set((state) => {
          state.services = state.services.filter((s) => s.id !== id);
          if (state.selectedService?.id === id) state.selectedService = null;
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar servicio';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    toggleStatus: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const service = await servicesApi.toggleServiceStatus(id);
        set((state) => {
          const index = state.services.findIndex((s) => s.id === id);
          if (index !== -1) state.services[index] = service;
          state.isLoading = false;
        });
        return service;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cambiar estado';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    setSelectedService: (service) => {
      set({ selectedService: service });
    },

    clearError: () => {
      set({ error: null });
    },
  }))
);
