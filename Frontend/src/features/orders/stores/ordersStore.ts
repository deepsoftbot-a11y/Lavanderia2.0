import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type {
  Order,
  OrderSummary,
  OrderHistoryFilters,
  PagedResult,
  OrderSearchFilters,
} from '@/features/orders/types/order';
import * as ordersApi from '@/api/orders';

interface OrdersPagination {
  page: number;
  pageSize: number;
  totalCount: number;
}

interface OrdersState {
  orders: OrderSummary[];
  selectedOrder: Order | null;
  isLoading: boolean;
  error: string | null;

  pagination: OrdersPagination;
  activeFilters: OrderHistoryFilters;

  // Search state
  searchResults: OrderSummary[];
  isSearching: boolean;
  searchError: string | null;

  // Actions
  fetchOrders: (filters?: OrderHistoryFilters, page?: number) => Promise<void>;
  fetchOrderById: (id: number) => Promise<void>;
  createOrder: (input: any) => Promise<Order | null>;
  updateOrder: (id: number, input: any) => Promise<Order | null>;
  deleteOrder: (id: number) => Promise<void>;
  setSelectedOrder: (order: Order | null) => void;
  clearError: () => void;
  clearFilters: () => void;

  // Search actions
  searchOrders: (filters: OrderSearchFilters) => Promise<void>;
  updateOrderStatus: (orderId: number, statusId: number) => Promise<Order | null>;
  refreshSelectedOrder: () => Promise<void>;
  clearSearchResults: () => void;
}

const DEFAULT_PAGINATION: OrdersPagination = { page: 1, pageSize: 20, totalCount: 0 };

export const useOrdersStore = create<OrdersState>()(
  immer((set, get) => ({
    orders: [],
    selectedOrder: null,
    isLoading: false,
    error: null,

    pagination: DEFAULT_PAGINATION,
    activeFilters: {},

    searchResults: [],
    isSearching: false,
    searchError: null,

    fetchOrders: async (filters = {}, page = 1) => {
      set({ isLoading: true, error: null });
      try {
        const result: PagedResult<OrderSummary> = await ordersApi.getOrders(filters, page);
        set({
          orders: result.data,
          pagination: { page: result.page, pageSize: result.pageSize, totalCount: result.totalCount },
          activeFilters: filters,
          isLoading: false,
        });
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
        // Refrescar el listado para incluir la nueva orden como summary
        const { activeFilters } = get();
        const result: PagedResult<OrderSummary> = await ordersApi.getOrders(activeFilters, 1);
        set({
          orders: result.data,
          pagination: { page: result.page, pageSize: result.pageSize, totalCount: result.totalCount },
          isLoading: false,
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
            state.orders[index] = {
              id: order.id,
              folioOrden: order.folioOrden,
              orderStatusId: order.orderStatusId,
              orderStatus: order.orderStatus,
              clientId: order.clientId,
              client: order.client ? { name: order.client.name } : undefined,
              total: order.total,
              paymentStatus: order.paymentStatus,
              createdAt: order.createdAt,
            };
          }
          if (state.selectedOrder?.id === id) state.selectedOrder = order;
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
          if (state.selectedOrder?.id === id) state.selectedOrder = null;
          state.pagination.totalCount = Math.max(0, state.pagination.totalCount - 1);
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar orden';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    setSelectedOrder: (order) => set({ selectedOrder: order }),
    clearError: () => set({ error: null }),
    clearFilters: () => {
      const { fetchOrders } = get();
      fetchOrders({}, 1);
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
        const summary: OrderSummary = {
          id: order.id,
          folioOrden: order.folioOrden,
          orderStatusId: order.orderStatusId,
          orderStatus: order.orderStatus,
          clientId: order.clientId,
          client: order.client ? { name: order.client.name } : undefined,
          total: order.total,
          paymentStatus: order.paymentStatus,
          createdAt: order.createdAt,
        };
        set((state) => {
          const index = state.orders.findIndex((o) => o.id === orderId);
          if (index !== -1) state.orders[index] = summary;
          const searchIndex = state.searchResults.findIndex((o) => o.id === orderId);
          if (searchIndex !== -1) state.searchResults[searchIndex] = summary;
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

    clearSearchResults: () => set({ searchResults: [], searchError: null }),
  }))
);
