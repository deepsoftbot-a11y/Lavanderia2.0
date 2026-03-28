import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import {
  createCustomerSchema,
  type CreateCustomerFormData,
} from '@/features/customers/schemas/customer.schema';

interface QuickCustomerFormProps {
  open: boolean;
  isLoading: boolean;
  onClose: () => void;
  onSubmit: (data: CreateCustomerFormData) => void;
}

export function QuickCustomerForm({
  open,
  isLoading,
  onClose,
  onSubmit,
}: QuickCustomerFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateCustomerFormData>({
    resolver: zodResolver(createCustomerSchema),
  });

  const handleFormSubmit = (data: CreateCustomerFormData) => {
    onSubmit(data);
    reset();
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-md p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            Crear Nuevo Cliente
          </DialogTitle>
          <p className="text-xs text-zinc-400 mt-0.5">
            Registra un nuevo cliente para continuar con la venta
          </p>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)}>
          <div className="flex flex-col max-h-[82vh]">
            <div className="overflow-y-auto flex-1">
              {/* Datos obligatorios */}
              <div className="px-6 py-4 border-b border-zinc-100">
                <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-3">
                  Datos de contacto
                </p>
                <div className="space-y-3">
                  <div className="space-y-1">
                    <Label htmlFor="name" className="text-xs text-zinc-500 font-medium">
                      Nombre completo *
                    </Label>
                    <Input
                      id="name"
                      {...register('name')}
                      placeholder="Juan Pérez García"
                      disabled={isLoading}
                    />
                    {errors.name && (
                      <p className="text-xs text-rose-600">{errors.name.message}</p>
                    )}
                  </div>

                  <div className="space-y-1">
                    <Label htmlFor="phone" className="text-xs text-zinc-500 font-medium">
                      Teléfono *
                    </Label>
                    <Input
                      id="phone"
                      {...register('phone')}
                      placeholder="5551234567"
                      disabled={isLoading}
                      className="font-mono tabular-nums"
                    />
                    {errors.phone && (
                      <p className="text-xs text-rose-600">{errors.phone.message}</p>
                    )}
                  </div>
                </div>
              </div>

              {/* Datos opcionales */}
              <div className="px-6 py-4">
                <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-3">
                  Datos adicionales
                </p>
                <div className="space-y-3">
                  <div className="space-y-1">
                    <Label htmlFor="email" className="text-xs text-zinc-500 font-medium">
                      Email
                    </Label>
                    <Input
                      id="email"
                      type="email"
                      {...register('email')}
                      placeholder="juan.perez@email.com"
                      disabled={isLoading}
                    />
                    {errors.email && (
                      <p className="text-xs text-rose-600">{errors.email.message}</p>
                    )}
                  </div>

                  <div className="space-y-1">
                    <Label htmlFor="address" className="text-xs text-zinc-500 font-medium">
                      Dirección
                    </Label>
                    <Input
                      id="address"
                      {...register('address')}
                      placeholder="Av. Principal #123, Col. Centro"
                      disabled={isLoading}
                    />
                    {errors.address && (
                      <p className="text-xs text-rose-600">{errors.address.message}</p>
                    )}
                  </div>
                </div>
              </div>
            </div>

            <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
              <Button type="button" variant="outline" onClick={handleClose} disabled={isLoading}>
                Cancelar
              </Button>
              <Button
                type="submit"
                disabled={isLoading}
                className="bg-zinc-900 hover:bg-zinc-800 text-white"
              >
                {isLoading ? 'Creando...' : 'Crear cliente'}
              </Button>
            </div>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
