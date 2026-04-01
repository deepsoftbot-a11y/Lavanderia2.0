// Application-wide constants

// UI — Tabla header class compartido
export const TABLE_HEADER_CLASS = 'text-[10px] font-semibold tracking-widest uppercase text-zinc-400';

// API Configuration
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:3000/api';
console.log('[Constants] VITE_API_BASE_URL from env:', import.meta.env.VITE_API_BASE_URL);
console.log('[Constants] API_BASE_URL final value:', API_BASE_URL);

// Order Status
export const ORDER_STATUS = {
  PENDING: 'pending',
  IN_PROGRESS: 'in_progress',
  READY: 'ready',
  DELIVERED: 'delivered',
  CANCELLED: 'cancelled',
} as const;

// Service Types
export const SERVICE_TYPES = {
  WASH: 'wash',
  DRY_CLEAN: 'dry_clean',
  IRON: 'iron',
  WASH_AND_IRON: 'wash_and_iron',
} as const;

// Pagination
export const DEFAULT_PAGE_SIZE = 10;
export const MAX_PAGE_SIZE = 100;

// Date Formats
export const DATE_FORMAT = 'dd/MM/yyyy';
export const DATETIME_FORMAT = 'dd/MM/yyyy HH:mm';
export const TIME_FORMAT = 'HH:mm';
