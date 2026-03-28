export interface Customer {
  id: number;                    // ClienteID
  customerNumber: string;        // NumeroCliente
  name: string;                  // NombreCompleto
  phone: string;                 // Telefono
  email?: string;                // Email (nullable en DB)
  address?: string;              // Direccion (nullable en DB)
  rfc?: string;                  // RFC (nullable en DB)
  creditLimit: number;           // LimiteCredito
  currentBalance: number;        // SaldoActual

  // Auditoría
  createdAt: string;             // FechaRegistro
  createdBy: number;             // RegistradoPor
  updatedAt?: string;            // Opcional (si backend lo incluye)
  isActive: boolean;             // Activo
}

// DTOs
export interface CreateCustomerInput {
  // customerNumber NO se incluye (auto-generado por backend)
  name: string;                  // Requerido
  phone: string;                 // Requerido
  email?: string;                // Opcional
  address?: string;              // Opcional
  rfc?: string;                  // Opcional
  creditLimit?: number;          // Opcional (default 0 en backend)
}

export interface UpdateCustomerInput {
  name?: string;
  phone?: string;
  email?: string;
  address?: string;
  rfc?: string;                  // NUEVO
  creditLimit?: number;          // NUEVO
  currentBalance?: number;       // NUEVO (para ajustes manuales)
  isActive?: boolean;
}

// Filtros para búsqueda
export interface CustomerFilters {
  search?: string;               // Busca en name, phone, email, customerNumber
  isActive?: boolean;
  sortBy?: 'name' | 'createdAt';
  sortOrder?: 'asc' | 'desc';
}
