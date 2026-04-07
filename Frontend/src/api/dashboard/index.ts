import api from '@/api/axiosConfig';
import type { DashboardData } from '@/features/dashboard/types/dashboard';

export const getDashboard = async (fechaInicio: string, fechaFin: string): Promise<DashboardData> => {
  const response = await api.get<DashboardData>('/dashboard', {
    params: { fechaInicio, fechaFin },
  });
  return response.data;
};
