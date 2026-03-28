// Cambiar entre mock y API real comentando/descomentando

// PRODUCCIÓN: Usar API real
export * from './customersService.api';

// DESARROLLO: Usar mock data
// export * from './customersService.mock';

export { mockCustomers } from './mockData';
