import api from '@/api/axiosConfig';
import { CHARGE_TYPE } from '@/features/services/types/service';
import type { Service, ChargeType, CreateServiceInput, UpdateServiceInput } from '@/features/services/types/service';

// La API usa 'kg' / 'piece'; el frontend usa 'PorPeso' / 'PorPieza'
type ApiChargeType = 'kg' | 'piece';

interface ServiceApiResponse extends Omit<Service, 'chargeType'> {
  chargeType: ApiChargeType;
}

function toFrontend(apiValue: ApiChargeType): ChargeType {
  return apiValue === 'kg' ? CHARGE_TYPE.PorPeso : CHARGE_TYPE.PorPieza;
}

function toApi(value: ChargeType): ApiChargeType {
  return value === CHARGE_TYPE.PorPeso ? 'kg' : 'piece';
}

function mapService(raw: ServiceApiResponse): Service {
  return { ...raw, chargeType: toFrontend(raw.chargeType) };
}

export async function getServices(): Promise<Service[]> {
  try {
    const response = await api.get<ServiceApiResponse[]>('/services');
    return response.data.map(mapService);
  } catch (error) {
    console.error('Get services API error:', error);
    throw new Error('Error al obtener servicios desde el servidor');
  }
}

export async function getServiceById(id: number): Promise<Service> {
  try {
    const response = await api.get<ServiceApiResponse>(`/services/${id}`);
    return mapService(response.data);
  } catch (error) {
    console.error('Get service by ID API error:', error);
    throw new Error('Error al obtener el servicio desde el servidor');
  }
}

export async function createService(input: CreateServiceInput): Promise<Service> {
  try {
    const payload = input.chargeType
      ? { ...input, chargeType: toApi(input.chargeType) }
      : input;
    const response = await api.post<ServiceApiResponse>('/services', payload);
    return mapService(response.data);
  } catch (error) {
    console.error('Create service API error:', error);
    throw new Error('Error al crear el servicio en el servidor');
  }
}

export async function updateService(id: number, input: UpdateServiceInput): Promise<Service> {
  try {
    const payload = input.chargeType
      ? { ...input, chargeType: toApi(input.chargeType) }
      : input;
    const response = await api.put<ServiceApiResponse>(`/services/${id}`, payload);
    return mapService(response.data);
  } catch (error) {
    console.error('Update service API error:', error);
    throw new Error('Error al actualizar el servicio en el servidor');
  }
}

export async function deleteService(id: number): Promise<void> {
  try {
    await api.delete(`/services/${id}`);
  } catch (error) {
    console.error('Delete service API error:', error);
    throw new Error('Error al eliminar el servicio del servidor');
  }
}

export async function toggleServiceStatus(id: number): Promise<Service> {
  try {
    const response = await api.patch<Service>(`/services/${id}/status`);
    return response.data;
  } catch (error) {
    console.error('Toggle service status API error:', error);
    throw new Error('Error al cambiar el estado del servicio');
  }
}
