import { create } from 'zustand';
import { format } from 'date-fns';
import { getDashboard } from '@/api/dashboard';
import type { DashboardKPIs, DashboardCharts } from '../types/dashboard';

interface DashboardState {
  kpis: DashboardKPIs | null;
  charts: DashboardCharts | null;
  fechaInicio: Date;
  fechaFin: Date;
  isLoading: boolean;
  error: string | null;
  setFechaRange: (inicio: Date, fin: Date) => void;
  fetchDashboard: () => Promise<void>;
}

const getDefaultRange = () => {
  const today = new Date();
  const firstOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
  return { fechaInicio: firstOfMonth, fechaFin: today };
};

export const useDashboardStore = create<DashboardState>((set, get) => ({
  ...getDefaultRange(),
  kpis: null,
  charts: null,
  isLoading: false,
  error: null,

  setFechaRange: (inicio, fin) => {
    set({ fechaInicio: inicio, fechaFin: fin });
    get().fetchDashboard();
  },

  fetchDashboard: async () => {
    set({ isLoading: true, error: null });
    try {
      const { fechaInicio, fechaFin } = get();
      const data = await getDashboard(
        format(fechaInicio, 'yyyy-MM-dd'),
        format(fechaFin, 'yyyy-MM-dd'),
      );
      set({ kpis: data.kpis, charts: data.charts, isLoading: false });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Error al cargar dashboard';
      set({ error: message, isLoading: false });
    }
  },
}));
