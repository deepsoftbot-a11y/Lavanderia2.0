import axios from 'axios';
import api from '@/api/axiosConfig';
import type {
  Customer,
  CustomerFilters,
  CreateCustomerInput,
  UpdateCustomerInput,
} from '@/features/customers/types/customer';

/**
 * GET /clientes - Obtener todos los clientes con filtros opcionales
 */
export async function getCustomers(filters: CustomerFilters = {}): Promise<Customer[]> {
  try {
    const response = await api.get<Customer[]>('/clientes', { params: filters });
    return response.data;
  } catch (error) {
    console.error('Error al obtener clientes:', error);
    throw new Error('Error al obtener clientes desde el servidor');
  }
}

/**
 * GET /clientes/:id - Obtener un cliente por ID
 */
export async function getCustomerById(id: number): Promise<Customer> {
  try {
    const response = await api.get<Customer>(`/clientes/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error al obtener cliente ${id}:`, error);
    throw new Error('Error al obtener el cliente desde el servidor');
  }
}

/**
 * POST /clientes - Crear nuevo cliente
 */
export async function createCustomer(input: CreateCustomerInput): Promise<Customer> {
  try {
    const response = await api.post<Customer>('/clientes', input);
    return response.data;
  } catch (error) {
    console.error('Error al crear cliente:', error);

    // Manejo de errores específicos
    if (axios.isAxiosError(error) && error.response?.status === 409) {
      throw new Error('Ya existe un cliente con ese número de teléfono');
    }

    throw new Error('Error al crear el cliente en el servidor');
  }
}

/**
 * PUT /clientes/:id - Actualizar cliente existente
 */
export async function updateCustomer(
  id: number,
  input: UpdateCustomerInput
): Promise<Customer> {
  try {
    const response = await api.put<Customer>(`/clientes/${id}`, input);
    return response.data;
  } catch (error) {
    console.error(`Error al actualizar cliente ${id}:`, error);

    if (axios.isAxiosError(error) && error.response?.status === 409) {
      throw new Error('Ya existe un cliente con ese número de teléfono');
    }

    throw new Error('Error al actualizar el cliente en el servidor');
  }
}

/**
 * DELETE /clientes/:id - Eliminar cliente
 */
export async function deleteCustomer(id: number): Promise<void> {
  try {
    await api.delete(`/clientes/${id}`);
  } catch (error) {
    console.error(`Error al eliminar cliente ${id}:`, error);

    if (axios.isAxiosError(error) && error.response?.status === 409) {
      throw new Error('No se puede eliminar un cliente con órdenes registradas');
    }

    throw new Error('Error al eliminar el cliente del servidor');
  }
}

/**
 * PATCH /clientes/:id/estado - Cambiar estado activo/inactivo
 */
export async function toggleCustomerStatus(id: number): Promise<Customer> {
  try {
    const response = await api.patch<Customer>(`/clientes/${id}/estado`);
    return response.data;
  } catch (error) {
    console.error(`Error al cambiar estado del cliente ${id}:`, error);
    throw new Error('Error al cambiar el estado del cliente');
  }
}

/**
 * Helper function para actualizar estadísticas del cliente
 * DEPRECATED: En modo API, esto debe ser manejado por el backend
 */
export function updateCustomerStats(_customerId: number, _orderTotal: number): void {
  console.warn('updateCustomerStats está deprecated en modo API - el backend maneja las estadísticas');
  // No hace nada en modo API
}
