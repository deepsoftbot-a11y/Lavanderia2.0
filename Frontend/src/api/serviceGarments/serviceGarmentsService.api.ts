import api from '@/api/axiosConfig';
import type {
  ServiceGarment,
  CreateServiceGarmentInput,
  UpdateServiceGarmentInput,
} from '@/features/services/types/serviceGarment';

export async function getServiceGarments(serviceId?: number): Promise<ServiceGarment[]> {
  try {
    const params: Record<string, unknown> = {};
    if (serviceId !== undefined) params.serviceId = serviceId;

    const response = await api.get<ServiceGarment[]>('/service-garments', { params });
    return response.data;
  } catch (error) {
    console.error('Get service garments API error:', error);
    throw new Error('Error al obtener precios de prenda desde el servidor');
  }
}

export async function getServiceGarmentById(id: number): Promise<ServiceGarment> {
  try {
    const response = await api.get<ServiceGarment>(`/service-garments/${id}`);
    return response.data;
  } catch (error) {
    console.error('Get service garment by ID API error:', error);
    throw new Error('Error al obtener el precio de prenda desde el servidor');
  }
}

export async function createServiceGarment(
  input: CreateServiceGarmentInput
): Promise<ServiceGarment> {
  try {
    const response = await api.post<ServiceGarment>('/service-garments', input);
    return response.data;
  } catch (error) {
    console.error('Create service garment API error:', error);
    throw new Error('Error al crear el precio de prenda en el servidor');
  }
}

export async function updateServiceGarment(
  id: number,
  input: UpdateServiceGarmentInput
): Promise<ServiceGarment> {
  try {
    const response = await api.put<ServiceGarment>(`/service-garments/${id}`, input);
    return response.data;
  } catch (error) {
    console.error('Update service garment API error:', error);
    throw new Error('Error al actualizar el precio de prenda en el servidor');
  }
}

export async function deleteServiceGarment(id: number): Promise<void> {
  try {
    await api.delete(`/service-garments/${id}`);
  } catch (error) {
    console.error('Delete service garment API error:', error);
    throw new Error('Error al eliminar el precio de prenda del servidor');
  }
}

export async function toggleServiceGarmentStatus(id: number): Promise<ServiceGarment> {
  try {
    const response = await api.patch<ServiceGarment>(`/service-garments/${id}/status`);
    return response.data;
  } catch (error) {
    console.error('Toggle service garment status API error:', error);
    throw new Error('Error al cambiar el estado del precio de prenda');
  }
}
