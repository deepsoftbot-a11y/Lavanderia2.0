import type {
  Customer,
  CustomerFilters,
  CreateCustomerInput,
  UpdateCustomerInput,
} from '@/features/customers/types/customer';
import { mockCustomers } from './mockData';

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

let customers = [...mockCustomers];
let nextId = Math.max(...customers.map((c) => c.id)) + 1;

function filterAndSortCustomers(
  customersArray: Customer[],
  filters: CustomerFilters
): Customer[] {
  let filtered = [...customersArray];

  if (filters.search) {
    const searchLower = filters.search.toLowerCase();
    filtered = filtered.filter(
      (customer) =>
        customer.name.toLowerCase().includes(searchLower) ||
        customer.phone.includes(filters.search!) ||
        customer.email?.toLowerCase().includes(searchLower) ||
        customer.customerNumber.toLowerCase().includes(searchLower)
    );
  }

  if (filters.isActive !== undefined) {
    filtered = filtered.filter((customer) => customer.isActive === filters.isActive);
  }

  const sortBy = filters.sortBy || 'name';
  const sortOrder = filters.sortOrder || 'asc';

  filtered.sort((a, b) => {
    let aValue: any = a[sortBy];
    let bValue: any = b[sortBy];

    if (typeof aValue === 'string') {
      aValue = aValue.toLowerCase();
      bValue = bValue.toLowerCase();
    }

    if (aValue < bValue) return sortOrder === 'asc' ? -1 : 1;
    if (aValue > bValue) return sortOrder === 'asc' ? 1 : -1;
    return 0;
  });

  return filtered;
}

export async function getCustomers(filters: CustomerFilters = {}): Promise<Customer[]> {
  await delay(300);
  return filterAndSortCustomers(customers, filters);
}

export async function getCustomerById(id: number): Promise<Customer> {
  await delay(200);
  const customer = customers.find((c) => c.id === id);
  if (!customer) {
    throw new Error('Cliente no encontrado');
  }
  return customer;
}

export async function createCustomer(input: CreateCustomerInput): Promise<Customer> {
  await delay(400);

  const existingCustomer = customers.find((c) => c.phone === input.phone);
  if (existingCustomer) {
    throw new Error('Ya existe un cliente con ese número de teléfono');
  }

  const currentUserId = 1;

  const newCustomer: Customer = {
    id: nextId++,
    customerNumber: `CLI-${String(nextId).padStart(5, '0')}`, // Auto-generado
    name: input.name,
    phone: input.phone,
    email: input.email,
    address: input.address,
    rfc: input.rfc,
    creditLimit: input.creditLimit ?? 0, // Default 0
    currentBalance: 0, // Nuevos clientes inician con saldo 0
    createdAt: new Date().toISOString(),
    createdBy: currentUserId,
    isActive: true,
  };

  customers.push(newCustomer);
  return newCustomer;
}

export async function updateCustomer(id: number, input: UpdateCustomerInput): Promise<Customer> {
  await delay(400);

  const index = customers.findIndex((c) => c.id === id);
  if (index === -1) {
    throw new Error('Cliente no encontrado');
  }

  if (input.phone) {
    const existingCustomer = customers.find((c) => c.id !== id && c.phone === input.phone);
    if (existingCustomer) {
      throw new Error('Ya existe un cliente con ese número de teléfono');
    }
  }

  const updatedCustomer: Customer = {
    ...customers[index],
    ...input,
    updatedAt: new Date().toISOString(),
  };

  customers[index] = updatedCustomer;
  return updatedCustomer;
}

export async function deleteCustomer(id: number): Promise<void> {
  await delay(300);

  const index = customers.findIndex((c) => c.id === id);
  if (index === -1) {
    throw new Error('Cliente no encontrado');
  }

  // En modo mock permitimos eliminar directamente
  // En producción, el backend validará si tiene órdenes asociadas
  customers.splice(index, 1);
}

export async function toggleCustomerStatus(id: number): Promise<Customer> {
  await delay(300);

  const index = customers.findIndex((c) => c.id === id);
  if (index === -1) {
    throw new Error('Cliente no encontrado');
  }

  customers[index] = {
    ...customers[index],
    isActive: !customers[index].isActive,
    updatedAt: new Date().toISOString(),
  };

  return customers[index];
}

// Helper function para actualizar estadísticas del cliente
// DEPRECATED: Las estadísticas ahora se calculan en el backend
export function updateCustomerStats(_customerId: number, _orderTotal: number): void {
  console.warn('updateCustomerStats está deprecated - las estadísticas se calculan en el backend');
  // No hace nada en la nueva implementación
}
