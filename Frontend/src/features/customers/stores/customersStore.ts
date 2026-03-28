import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { Customer, CustomerFilters } from '@/features/customers/types/customer';
import * as customersApi from '@/api/customers';

interface CustomersState {
  customers: Customer[];
  selectedCustomer: Customer | null;
  isLoading: boolean;
  error: string | null;

  // Actions
  fetchCustomers: (filters?: CustomerFilters) => Promise<void>;
  fetchCustomerById: (id: number) => Promise<void>;
  createCustomer: (input: any) => Promise<Customer | null>;
  updateCustomer: (id: number, input: any) => Promise<Customer | null>;
  deleteCustomer: (id: number) => Promise<void>;
  toggleCustomerStatus: (id: number) => Promise<Customer | null>;
  setSelectedCustomer: (customer: Customer | null) => void;
  clearError: () => void;
}

export const useCustomersStore = create<CustomersState>()(
  immer((set) => ({
    customers: [],
    selectedCustomer: null,
    isLoading: false,
    error: null,

    fetchCustomers: async (filters) => {
      set({ isLoading: true, error: null });
      try {
        const customers = await customersApi.getCustomers(filters);
        set({ customers, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar clientes';
        set({ error: message, isLoading: false });
      }
    },

    fetchCustomerById: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const customer = await customersApi.getCustomerById(id);
        set({ selectedCustomer: customer, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar cliente';
        set({ error: message, isLoading: false });
      }
    },

    createCustomer: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const customer = await customersApi.createCustomer(input);

        // Refrescar la lista completa para asegurar sincronización
        const updatedCustomers = await customersApi.getCustomers({ isActive: true });
        set({ customers: updatedCustomers, isLoading: false });

        return customer;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear cliente';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    updateCustomer: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const customer = await customersApi.updateCustomer(id, input);
        set((state) => {
          const index = state.customers.findIndex((c) => c.id === id);
          if (index !== -1) {
            state.customers[index] = customer;
          }
          if (state.selectedCustomer?.id === id) {
            state.selectedCustomer = customer;
          }
          state.isLoading = false;
        });
        return customer;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar cliente';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    deleteCustomer: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await customersApi.deleteCustomer(id);
        set((state) => {
          state.customers = state.customers.filter((c) => c.id !== id);
          if (state.selectedCustomer?.id === id) {
            state.selectedCustomer = null;
          }
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar cliente';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    toggleCustomerStatus: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const customer = await customersApi.toggleCustomerStatus(id);
        set((state) => {
          const index = state.customers.findIndex((c) => c.id === id);
          if (index !== -1) {
            state.customers[index] = customer;
          }
          state.isLoading = false;
        });
        return customer;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cambiar estado';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    setSelectedCustomer: (customer) => {
      set({ selectedCustomer: customer });
    },

    clearError: () => {
      set({ error: null });
    },
  }))
);
