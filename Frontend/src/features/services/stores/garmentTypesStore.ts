import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type {
  GarmentType,
  CreateGarmentTypeInput,
  UpdateGarmentTypeInput,
} from '@/features/services/types/garmentType';
import * as garmentTypesApi from '@/api/garmentTypes';

interface GarmentTypesState {
  garmentTypes: GarmentType[];
  isLoading: boolean;
  error: string | null;

  fetchGarmentTypes: () => Promise<void>;
  createGarmentType: (input: CreateGarmentTypeInput) => Promise<GarmentType | null>;
  updateGarmentType: (id: number, input: UpdateGarmentTypeInput) => Promise<GarmentType | null>;
  deleteGarmentType: (id: number) => Promise<void>;
  toggleStatus: (id: number) => Promise<GarmentType | null>;
  clearError: () => void;
}

export const useGarmentTypesStore = create<GarmentTypesState>()(
  immer((set) => ({
    garmentTypes: [],
    isLoading: false,
    error: null,

    fetchGarmentTypes: async () => {
      set({ isLoading: true, error: null });
      try {
        const garmentTypes = await garmentTypesApi.getGarmentTypes();
        set({ garmentTypes, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar tipos de prenda';
        set({ error: message, isLoading: false });
      }
    },

    createGarmentType: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const garmentType = await garmentTypesApi.createGarmentType(input);
        set((state) => {
          state.garmentTypes.push(garmentType);
          state.isLoading = false;
        });
        return garmentType;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear tipo de prenda';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    updateGarmentType: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const garmentType = await garmentTypesApi.updateGarmentType(id, input);
        set((state) => {
          const index = state.garmentTypes.findIndex((g) => g.id === id);
          if (index !== -1) state.garmentTypes[index] = garmentType;
          state.isLoading = false;
        });
        return garmentType;
      } catch (error) {
        const message =
          error instanceof Error ? error.message : 'Error al actualizar tipo de prenda';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    deleteGarmentType: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await garmentTypesApi.deleteGarmentType(id);
        set((state) => {
          state.garmentTypes = state.garmentTypes.filter((g) => g.id !== id);
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar tipo de prenda';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    toggleStatus: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const garmentType = await garmentTypesApi.toggleGarmentTypeStatus(id);
        set((state) => {
          const index = state.garmentTypes.findIndex((g) => g.id === id);
          if (index !== -1) state.garmentTypes[index] = garmentType;
          state.isLoading = false;
        });
        return garmentType;
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
