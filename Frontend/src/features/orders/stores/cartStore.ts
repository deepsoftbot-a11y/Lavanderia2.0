import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { Customer } from '@/features/customers/types/customer';
import { CHARGE_TYPE } from '@/features/services/types/service';
import type { Service } from '@/features/services/types/service';
import type { ServiceGarment } from '@/features/services/types/serviceGarment';
import type { Discount } from '@/features/services/types/discount';

export interface CartItem {
  id: string;
  serviceId: number;
  serviceName: string;
  serviceGarmentId: number | null;
  garmentName: string;
  discountId: number;
  discountName: string;
  discountType: string;
  discountValue: number;
  weightKilos: number;
  quantity: number;
  unitPrice: number;
  subtotal: number;
  discountAmount: number;
  total: number;
  notes: string;
}

interface CartState {
  // Datos de la orden
  customer: Customer | null;
  promisedDate: string;
  storageLocation: string;
  initialStatusId: number;
  notes: string;

  // Items del carrito
  items: CartItem[];

  // Actions - Cliente y orden
  setCustomer: (customer: Customer | null) => void;
  setPromisedDate: (date: string) => void;
  setStorageLocation: (location: string) => void;
  setInitialStatus: (statusId: number) => void;
  setNotes: (notes: string) => void;

  // Actions - Items
  addItem: (
    service: Service,
    garment: ServiceGarment | null,
    discount: Discount | null,
    quantity: number,
    weightKilos: number,
    notes?: string,
    customPrice?: number
  ) => void;
  updateItemQuantity: (id: string, quantity: number, weightKilos: number) => void;
  updateItemDiscount: (id: string, discount: Discount) => void;
  updateItemNotes: (id: string, notes: string) => void;
  removeItem: (id: string) => void;
  clearCart: () => void;

  // Getters
  getSubtotal: () => number;
  getTotalDiscount: () => number;
  getTotal: () => number;
  getItemCount: () => number;
  isValid: () => boolean;
  getValidationErrors: () => string[];
}

// Helper para calcular totales de un item
function calculateItemTotals(
  quantity: number,
  weightKilos: number,
  unitPrice: number,
  discountType: string,
  discountValue: number
): { subtotal: number; discountAmount: number; total: number } {
  const subtotal = quantity > 0 ? unitPrice * quantity : unitPrice * weightKilos;

  let discountAmount = 0;
  if (discountType === 'Percentage') {
    discountAmount = (subtotal * discountValue) / 100;
  } else if (discountType === 'FixedAmount') {
    discountAmount = discountValue;
  }

  const total = subtotal - discountAmount;

  return {
    subtotal: Math.round(subtotal * 100) / 100,
    discountAmount: Math.round(discountAmount * 100) / 100,
    total: Math.round(total * 100) / 100,
  };
}

// Generar ID único para items del carrito
let cartItemCounter = 1;
function generateCartItemId(): string {
  return `cart-item-${Date.now()}-${cartItemCounter++}`;
}

export const useCartStore = create<CartState>()(
  immer((set, get) => ({
    // Estado inicial
    customer: null,
    promisedDate: '',
    storageLocation: '',
    initialStatusId: 1,
    notes: '',
    items: [],

    // Actions - Cliente y orden
    setCustomer: (customer) => {
      set({ customer });
    },

    setPromisedDate: (date) => {
      set({ promisedDate: date });
    },

    setStorageLocation: (location) => {
      set({ storageLocation: location });
    },

    setInitialStatus: (statusId) => {
      set({ initialStatusId: statusId });
    },

    setNotes: (notes) => {
      set({ notes });
    },

    // Actions - Items
    addItem: (service, garment, discount, quantity, weightKilos, notes, customPrice) => {
      const unitPrice =
        customPrice ??
        (service.chargeType === CHARGE_TYPE.PorPeso
          ? (service.pricePerKg ?? 0)
          : (garment?.unitPrice ?? 0));

      const discountType = discount?.type ?? 'FixedAmount';
      const discountValue = discount?.value ?? 0;

      const { subtotal, discountAmount, total } = calculateItemTotals(
        quantity,
        weightKilos,
        unitPrice,
        discountType,
        discountValue
      );

      const newItem: CartItem = {
        id: generateCartItemId(),
        serviceId: service.id,
        serviceName: service.name,
        serviceGarmentId: garment?.id ?? null,
        garmentName: garment?.garmentType?.name ?? 'N/A',
        discountId: discount?.id ?? 1,
        discountName: discount?.name ?? 'Sin descuento',
        discountType,
        discountValue,
        weightKilos,
        quantity,
        unitPrice,
        subtotal,
        discountAmount,
        total,
        notes: notes || '',
      };

      set((state) => {
        state.items.push(newItem);
      });
    },

    updateItemQuantity: (id, quantity, weightKilos) => {
      set((state) => {
        const item = state.items.find((i) => i.id === id);
        if (item) {
          item.quantity = quantity;
          item.weightKilos = weightKilos;

          const { subtotal, discountAmount, total } = calculateItemTotals(
            quantity,
            weightKilos,
            item.unitPrice,
            item.discountType,
            item.discountValue
          );

          item.subtotal = subtotal;
          item.discountAmount = discountAmount;
          item.total = total;
        }
      });
    },

    updateItemDiscount: (id, discount) => {
      set((state) => {
        const item = state.items.find((i) => i.id === id);
        if (item) {
          item.discountId = discount.id;
          item.discountName = discount.name;
          item.discountType = discount.type;
          item.discountValue = discount.value;

          const { subtotal, discountAmount, total } = calculateItemTotals(
            item.quantity,
            item.weightKilos,
            item.unitPrice,
            discount.type,
            discount.value
          );

          item.subtotal = subtotal;
          item.discountAmount = discountAmount;
          item.total = total;
        }
      });
    },

    updateItemNotes: (id, notes) => {
      set((state) => {
        const item = state.items.find((i) => i.id === id);
        if (item) {
          item.notes = notes;
        }
      });
    },

    removeItem: (id) => {
      set((state) => {
        state.items = state.items.filter((i) => i.id !== id);
      });
    },

    clearCart: () => {
      set({
        customer: null,
        promisedDate: '',
        storageLocation: '',
        initialStatusId: 1,
        notes: '',
        items: [],
      });
    },

    // Getters
    getSubtotal: () => {
      const items = get().items;
      const subtotal = items.reduce((sum, item) => sum + item.subtotal, 0);
      return Math.round(subtotal * 100) / 100;
    },

    getTotalDiscount: () => {
      const items = get().items;
      const totalDiscount = items.reduce((sum, item) => sum + item.discountAmount, 0);
      return Math.round(totalDiscount * 100) / 100;
    },

    getTotal: () => {
      const items = get().items;
      const total = items.reduce((sum, item) => sum + item.total, 0);
      return Math.round(total * 100) / 100;
    },

    getItemCount: () => {
      return get().items.length;
    },

    isValid: () => {
      const state = get();
      return !!(
        state.customer &&
        state.promisedDate &&
        state.initialStatusId &&
        state.items.length > 0
      );
    },

    getValidationErrors: () => {
      const state = get();
      const errors: string[] = [];

      if (!state.customer) {
        errors.push('Debe seleccionar un cliente');
      }

      if (!state.promisedDate) {
        errors.push('Debe seleccionar una fecha de entrega');
      }

      if (!state.initialStatusId) {
        errors.push('Debe seleccionar el estado inicial de las prendas');
      }

      if (state.items.length === 0) {
        errors.push('Debe agregar al menos un servicio');
      }

      return errors;
    },
  }))
);
