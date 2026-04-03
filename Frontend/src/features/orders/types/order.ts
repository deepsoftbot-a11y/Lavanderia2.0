import type { Customer } from '@/features/customers/types/customer';
import type { User } from '@/features/users/types/user';
import type { Service } from '@/features/services/types/service';
import type { GarmentType } from '@/features/services/types/garmentType';
import type { Discount } from '@/features/services/types/discount';
import type { Payment, PaymentStatus } from './payment';

export interface OrderStatus {
  id: number;
  name: string;
  color?: string;
}

// Modelo ligero para resultados de búsqueda (GET /orders/search)
export interface OrderSummary {
  id: number;
  folioOrden: string;

  // Estado
  orderStatusId: number;
  orderStatus?: OrderStatus; // inyectado en frontend desde mockOrderStatuses

  // Cliente (solo lo visible en la tarjeta de resultado)
  clientId: number;
  client?: {
    name: string;
  };

  // Financiero resumido
  total: number;
  paymentStatus: PaymentStatus;

  // Auditoría
  createdAt: string;
}

export interface Order {
  id: number;
  folioOrden: string; // Folio de la orden (dbo.Ordenes.FolioOrden)
  clientId: number;
  promisedDate: string; // ISO 8601 format
  deliveredDate?: string; // Fecha de entrega real (dbo.Ordenes.FechaEntrega)
  receivedBy: number; // ID del usuario que recibió
  initialStatusId: number; // Estado inicial de prendas
  orderStatusId: number; // Estado de la orden (dbo.Ordenes.EstadoOrdenID)
  notes: string;
  storageLocation: string; // Ubicación de almacenamiento
  items: OrderItem[];

  // Campos calculados
  subtotal: number;
  totalDiscount: number;
  total: number;

  // Campos de pago
  amountPaid: number; // Total pagado hasta ahora
  balance: number; // Saldo pendiente (total - amountPaid)
  paymentStatus: PaymentStatus; // Estado de pago

  // Campos de auditoría
  createdAt: string;
  createdBy: number;
  updatedAt?: string;
  updatedBy?: number;

  // Relaciones pobladas (para UI)
  client?: Customer;
  receivedByUser?: User;
  initialStatus?: { id: number; name: string; color?: string };
  orderStatus?: OrderStatus; // Estado de la orden poblado
  payments?: Payment[]; // Lista de pagos realizados
}

export interface OrderItem {
  id: number;
  serviceId: number;
  serviceGarmentId: number; // Tipo de prenda (edredón, cortina, etc.)
  discountId: number; // Descuento aplicado
  weightKilos: number; // Peso en kilos (0 si no aplica)
  quantity: number; // Cantidad de piezas (0 si es por peso)
  unitPrice: number; // Precio unitario
  notes: string;

  // Campos calculados
  subtotal: number; // unitPrice × quantity o weightKilos
  discountAmount: number;
  total: number; // subtotal - discountAmount

  // Relaciones pobladas (para UI)
  service?: Service;
  garmentType?: GarmentType;
  discount?: Discount;
}

// Datos de pago inicial (sin orderId porque aún no existe)
export interface CreatePaymentData {
  amount: number;
  paymentMethodId: number;
  reference?: string;
  notes?: string;
  paidAt: string;
}

// DTOs para creación/actualización
export interface CreateOrderInput {
  clientId: number;
  promisedDate: string;
  initialStatusId: number;
  notes: string;
  storageLocation: string;
  receivedBy: number; // ID del usuario que recibe la orden
  items: CreateOrderItemInput[];
  initialPayment?: CreatePaymentData; // Pago inicial opcional
}

export interface CreateOrderItemInput {
  serviceId: number;
  serviceGarmentId: number;
  discountAmount: number;
  weightKilos: number;
  quantity: number;
  unitPrice: number;
  notes: string;
}

export interface UpdateOrderInput {
  promisedDate?: string;
  initialStatusId?: number;
  notes?: string;
  storageLocation?: string;
  items?: UpdateOrderItemInput[];
}

export interface UpdateOrderItemInput {
  id?: number;
  serviceId: number;
  serviceGarmentId: number;
  discountAmount: number;
  weightKilos: number;
  quantity: number;
  unitPrice: number;
  notes: string;
}

export interface OrderFilters {
  search?: string;
  clientId?: number;
  startDate?: string;
  endDate?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface OrderSearchFilters {
  query: string; // Búsqueda unificada en folio, nombre cliente o teléfono
}

export interface UpdateOrderStatusInput {
  orderStatusId: number;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface OrderHistoryFilters {
  startDate?: string;       // "2026-01-01"
  endDate?: string;         // "2026-12-31"
  statusIds?: number[];
  paymentStatuses?: PaymentStatus[];
}
