import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { Order, OrderSummary, OrderFilters, OrderSearchFilters } from '@/features/orders/types/order';
import * as ordersApi from '@/api/orders';

interface OrdersState {
  orders: Order[];
  selectedOrder: Order | null;
  isLoading: boolean;
  error: string | null;

  // Search state
  searchResults: OrderSummary[];
  isSearching: boolean;
  searchError: string | null;

  // Actions
  fetchOrders: (filters?: OrderFilters) => Promise<void>;
  fetchOrderById: (id: number) => Promise<void>;
  createOrder: (input: any) => Promise<Order | null>;
  updateOrder: (id: number, input: any) => Promise<Order | null>;
  deleteOrder: (id: number) => Promise<void>;
  setSelectedOrder: (order: Order | null) => void;
  clearError: () => void;

  // Search actions
  searchOrders: (filters: OrderSearchFilters) => Promise<void>;
  updateOrderStatus: (orderId: number, statusId: number) => Promise<Order | null>;
  refreshSelectedOrder: () => Promise<void>;
  clearSearchResults: () => void;
}

export const useOrdersStore = create<OrdersState>()(
  immer((set, get) => ({
    orders: [],
    selectedOrder: null,
    isLoading: false,
    error: null,

    searchResults: [],
    isSearching: false,
    searchError: null,

    fetchOrders: async (filters) => {
      set({ isLoading: true, error: null });
      try {
        const orders = await ordersApi.getOrders(filters);
        set({ orders, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar órdenes';
        set({ error: message, isLoading: false });
      }
    },

    fetchOrderById: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.getOrderById(id);
        set({ selectedOrder: order, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar orden';
        set({ error: message, isLoading: false });
      }
    },

    createOrder: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.createOrder(input);
        set((state) => {
          state.orders.unshift(order);
          state.isLoading = false;
        });
        return order;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear orden';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    updateOrder: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.updateOrder(id, input);
        set((state) => {
          const index = state.orders.findIndex((o) => o.id === id);
          if (index !== -1) {
            state.orders[index] = order;
          }
          if (state.selectedOrder?.id === id) {
            state.selectedOrder = order;
          }
          state.isLoading = false;
        });
        return order;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar orden';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    deleteOrder: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await ordersApi.deleteOrder(id);
        set((state) => {
          state.orders = state.orders.filter((o) => o.id !== id);
          if (state.selectedOrder?.id === id) {
            state.selectedOrder = null;
          }
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar orden';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    setSelectedOrder: (order) => {
      set({ selectedOrder: order });
    },

    clearError: () => {
      set({ error: null });
    },

    searchOrders: async (filters) => {
      set({ isSearching: true, searchError: null });
      try {
        const results: OrderSummary[] = await ordersApi.searchOrders(filters);
        set({ searchResults: results, isSearching: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al buscar órdenes';
        set({ searchError: message, isSearching: false });
      }
    },

    updateOrderStatus: async (orderId, statusId) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.updateOrderStatus(orderId, { orderStatusId: statusId });
        set((state) => {
          const index = state.orders.findIndex((o) => o.id === orderId);
          if (index !== -1) state.orders[index] = order;
          const searchIndex = state.searchResults.findIndex((o) => o.id === orderId);
          if (searchIndex !== -1) state.searchResults[searchIndex] = order;
          state.isLoading = false;
        });
        return order;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar estado';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    refreshSelectedOrder: async () => {
      const selectedOrder = get().selectedOrder;
      if (!selectedOrder) return;
      try {
        const order = await ordersApi.getOrderById(selectedOrder.id);
        set((state) => {
          state.selectedOrder = order;
          const searchIndex = state.searchResults.findIndex((o) => o.id === order.id);
          if (searchIndex !== -1) state.searchResults[searchIndex] = order;
        });
      } catch (error) {
        console.error('Error refreshing selected order:', error);
      }
    },

    clearSearchResults: () => {
      set({ searchResults: [], searchError: null });
    },
  }))
);
