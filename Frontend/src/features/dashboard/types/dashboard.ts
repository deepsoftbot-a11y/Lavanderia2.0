export interface IngresoPorMetodo {
  metodo: string;
  total: number;
}

export interface OrdenesPendientesPagar {
  cantidad: number;
  total: number;
}

export interface ClienteTop {
  nombre: string;
  ordenes: number;
}

export interface DashboardKPIs {
  ingresosTotales: number;
  ticketPromedio: number;
  totalDescuentos: number;
  ingresosPorMetodo: IngresoPorMetodo[];
  ordenesAtrasadas: number;
  ordenesPendientesPagar: OrdenesPendientesPagar;
  clientesNuevos: number;
  clienteTop: ClienteTop | null;
  totalCorteCaja: number;
  diferencias: number;
  transacciones: number;
}

export interface IngresoPorDia {
  fecha: string;
  ingresos: number;
  ordenes: number;
}

export interface OrdenesPorEstado {
  estado: string;
  cantidad: number;
}

export interface IngresoPorServicio {
  servicio: string;
  total: number;
}

export interface IngresoPorCategoria {
  categoria: string;
  total: number;
}

export interface ComparativaSemanal {
  semanaActual: number;
  semanaAnterior: number;
}

export interface DashboardCharts {
  ingresosPorDia: IngresoPorDia[];
  ordenesPorEstado: OrdenesPorEstado[];
  ingresosPorServicio: IngresoPorServicio[];
  ingresosPorCategoria: IngresoPorCategoria[];
  comparativaSemanal: ComparativaSemanal;
}

export interface DashboardData {
  kpis: DashboardKPIs;
  charts: DashboardCharts;
}
