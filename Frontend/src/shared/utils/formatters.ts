import { format } from 'date-fns';
import { DATE_FORMAT, DATETIME_FORMAT, TIME_FORMAT } from './constants';

/**
 * Format a date to display format
 */
export function formatDate(date: Date | string | number): string {
  const dateObj = typeof date === 'string' || typeof date === 'number' ? new Date(date) : date;
  return format(dateObj, DATE_FORMAT);
}

/**
 * Format a datetime to display format
 */
export function formatDateTime(date: Date | string | number): string {
  const dateObj = typeof date === 'string' || typeof date === 'number' ? new Date(date) : date;
  return format(dateObj, DATETIME_FORMAT);
}

/**
 * Format a time to display format
 */
export function formatTime(date: Date | string | number): string {
  const dateObj = typeof date === 'string' || typeof date === 'number' ? new Date(date) : date;
  return format(dateObj, TIME_FORMAT);
}

/**
 * Format currency values
 */
export function formatCurrency(amount: number, currency: string = 'USD'): string {
  return new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency,
  }).format(amount);
}

/**
 * Format phone number
 */
export function formatPhoneNumber(phone: string): string {
  const cleaned = phone.replace(/\D/g, '');
  const match = cleaned.match(/^(\d{3})(\d{3})(\d{4})$/);

  if (match) {
    return `(${match[1]}) ${match[2]}-${match[3]}`;
  }

  return phone;
}

/**
 * Capitalize first letter of string
 */
export function capitalize(str: string): string {
  return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
}

/**
 * Format order status for display
 */
export function formatOrderStatus(status: string): string {
  return status
    .split('_')
    .map(word => capitalize(word))
    .join(' ');
}
