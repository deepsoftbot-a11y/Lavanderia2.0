import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { Category, CreateCategoryInput, UpdateCategoryInput } from '@/features/services/types/category';
import * as categoriesApi from '@/api/categories';

interface CategoriesState {
  categories: Category[];
  isLoading: boolean;
  error: string | null;

  fetchCategories: () => Promise<void>;
  createCategory: (input: CreateCategoryInput) => Promise<Category | null>;
  updateCategory: (id: number, input: UpdateCategoryInput) => Promise<Category | null>;
  deleteCategory: (id: number) => Promise<void>;
  toggleStatus: (id: number) => Promise<Category | null>;
  clearError: () => void;
}

export const useCategoriesStore = create<CategoriesState>()(
  immer((set) => ({
    categories: [],
    isLoading: false,
    error: null,

    fetchCategories: async () => {
      set({ isLoading: true, error: null });
      try {
        const categories = await categoriesApi.getCategories();
        set({ categories, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar categorías';
        set({ error: message, isLoading: false });
      }
    },

    createCategory: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const category = await categoriesApi.createCategory(input);
        set((state) => {
          state.categories.push(category);
          state.isLoading = false;
        });
        return category;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear categoría';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    updateCategory: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const category = await categoriesApi.updateCategory(id, input);
        set((state) => {
          const index = state.categories.findIndex((c) => c.id === id);
          if (index !== -1) state.categories[index] = category;
          state.isLoading = false;
        });
        return category;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar categoría';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    deleteCategory: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await categoriesApi.deleteCategory(id);
        set((state) => {
          state.categories = state.categories.filter((c) => c.id !== id);
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar categoría';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    toggleStatus: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const category = await categoriesApi.toggleCategoryStatus(id);
        set((state) => {
          const index = state.categories.findIndex((c) => c.id === id);
          if (index !== -1) state.categories[index] = category;
          state.isLoading = false;
        });
        return category;
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
