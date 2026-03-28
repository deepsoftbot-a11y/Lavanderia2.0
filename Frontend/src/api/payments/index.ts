// API real para pagos
export { createPayment, getPaymentsByOrderId } from './paymentsService.api';

// Exportar funciones del mock que otros mock services necesitan
export { getPayments, addPaymentToArray, getNextPaymentId, calculatePaymentStatus } from './paymentsService.mock';

export * from './mockData';
