import api from '@/api/axiosConfig';
import type {
  GarmentType,
  CreateGarmentTypeInput,
  UpdateGarmentTypeInput,
} from '@/features/services/types/garmentType';

export async function getGarmentTypes(): Promise<GarmentType[]> {
  try {
    const response = await api.get<GarmentType[]>('/garment-types');
    return response.data;
  } catch (error) {
    console.error('Get garment types API error:', error);
    throw new Error('Error al obtener tipos de prenda desde el servidor');
  }
}

export async function getGarmentTypeById(id: number): Promise<GarmentType> {
  try {
    const response = await api.get<GarmentType>(`/garment-types/${id}`);
    return response.data;
  } catch (error) {
    console.error('Get garment type by ID API error:', error);
    throw new Error('Error al obtener el tipo de prenda desde el servidor');
  }
}

export async function createGarmentType(input: CreateGarmentTypeInput): Promise<GarmentType> {
  try {
    const response = await api.post<GarmentType>('/garment-types', input);
    return response.data;
  } catch (error) {
    console.error('Create garment type API error:', error);
    throw new Error('Error al crear el tipo de prenda en el servidor');
  }
}

export async function updateGarmentType(
  id: number,
  input: UpdateGarmentTypeInput
): Promise<GarmentType> {
  try {
    const response = await api.put<GarmentType>(`/garment-types/${id}`, input);
    return response.data;
  } catch (error) {
    console.error('Update garment type API error:', error);
    throw new Error('Error al actualizar el tipo de prenda en el servidor');
  }
}

export async function deleteGarmentType(id: number): Promise<void> {
  try {
    await api.delete(`/garment-types/${id}`);
  } catch (error) {
    console.error('Delete garment type API error:', error);
    throw new Error('Error al eliminar el tipo de prenda del servidor');
  }
}

export async function toggleGarmentTypeStatus(id: number): Promise<GarmentType> {
  try {
    const response = await api.patch<GarmentType>(`/garment-types/${id}/status`);
    return response.data;
  } catch (error) {
    console.error('Toggle garment type status API error:', error);
    throw new Error('Error al cambiar el estado del tipo de prenda');
  }
}
