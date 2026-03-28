import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Pencil, Trash2, Loader2, FolderOpen } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Textarea } from '@/shared/components/ui/textarea';
import { Checkbox } from '@/shared/components/ui/checkbox';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';

import { useCategoriesStore } from '@/features/services/stores/categoriesStore';
import { useToast } from '@/shared/hooks/use-toast';
import {
  createCategorySchema,
  updateCategorySchema,
} from '@/features/services/schemas/category.schema';
import type { Category } from '@/features/services/types/category';

interface CategoryFormContentProps {
  category?: Category;
  onSubmit: (data: Record<string, unknown>) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
}

function CategoryFormContent({
  category,
  onSubmit,
  onCancel,
  isSubmitting,
}: CategoryFormContentProps) {
  const isEdit = !!category;
  const schema = isEdit ? updateCategorySchema : createCategorySchema;

  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues: isEdit
      ? {
          name: category.name,
          description: category.description ?? '',
          isActive: category.isActive,
        }
      : {
          name: '',
          description: '',
        },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col max-h-[82vh]">
      <div className="overflow-y-auto flex-1 px-6 py-4 space-y-4">
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Nombre *</Label>
          <Input {...register('name')} placeholder="Ej: Lavandería, Tintorería, Planchado..." />
          {errors.name && (
            <p className="text-xs text-rose-500">{errors.name.message as string}</p>
          )}
        </div>

        <div className="space-y-1">
          <Label className="text-xs text-zinc-500 font-medium">Descripción</Label>
          <Textarea
            {...register('description')}
            placeholder="Descripción opcional..."
            rows={2}
          />
        </div>

        {isEdit && (
          <div className="flex items-center gap-2 pt-1">
            <Controller
              name="isActive"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="cat-isActive"
                  checked={field.value ?? true}
                  onCheckedChange={field.onChange}
                />
              )}
            />
            <Label
              htmlFor="cat-isActive"
              className="text-xs text-zinc-500 font-medium cursor-pointer"
            >
              Categoría activa
            </Label>
          </div>
        )}
      </div>

      <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button
          type="submit"
          className="bg-zinc-900 hover:bg-zinc-800 text-white"
          disabled={isSubmitting}
        >
          {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          Guardar
        </Button>
      </div>
    </form>
  );
}

const TH = 'text-[10px] font-semibold tracking-widest uppercase text-zinc-400';

export function CategoriesSection() {
  const {
    categories,
    isLoading,
    createCategory,
    updateCategory,
    deleteCategory,
  } = useCategoriesStore();
  const { toast } = useToast();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleOpenCreate = () => {
    setSelectedCategory(null);
    setDialogOpen(true);
  };

  const handleOpenEdit = (category: Category) => {
    setSelectedCategory(category);
    setDialogOpen(true);
  };

  const handleSubmit = async (data: Record<string, unknown>) => {
    setIsSubmitting(true);
    try {
      if (selectedCategory) {
        await updateCategory(selectedCategory.id, data as any);
        toast({ title: 'Categoría actualizada correctamente' });
      } else {
        await createCategory(data as any);
        toast({ title: 'Categoría creada correctamente' });
      }
      setDialogOpen(false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'No se pudo guardar la categoría';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedCategory) return;
    try {
      await deleteCategory(selectedCategory.id);
      toast({ title: 'Categoría eliminada' });
      setDeleteDialogOpen(false);
      setSelectedCategory(null);
    } catch {
      toast({
        title: 'Error',
        description: 'No se pudo eliminar la categoría',
        variant: 'destructive',
      });
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
        <h2 className="text-sm font-semibold text-zinc-900 tracking-tight">
          Categorías
          <span className="ml-2 font-mono font-normal text-xs text-zinc-400">{categories.length}</span>
        </h2>
        <Button
          onClick={handleOpenCreate}
          size="sm"
          className="bg-zinc-900 hover:bg-zinc-800 text-white"
        >
          <Plus className="h-4 w-4 mr-1" />
          Nueva Categoría
        </Button>
      </div>

      {isLoading && categories.length === 0 ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-zinc-400" />
        </div>
      ) : categories.length === 0 ? (
        <div className="py-16 flex flex-col items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
            <FolderOpen className="h-4 w-4 text-zinc-300" />
          </div>
          <div className="text-center">
            <p className="text-sm font-medium text-zinc-400">Sin categorías</p>
            <p className="text-xs text-zinc-300 mt-0.5">Organiza los servicios en categorías</p>
          </div>
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow className="bg-zinc-50 hover:bg-zinc-50">
              <TableHead className={TH}>Nombre</TableHead>
              <TableHead className={TH}>Descripción</TableHead>
              <TableHead className={TH}>Estado</TableHead>
              <TableHead className="w-20" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {categories.map((category) => (
              <TableRow
                key={category.id}
                className="border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
              >
                <TableCell className="text-sm font-medium text-zinc-900">
                  {category.name}
                </TableCell>
                <TableCell className="text-sm text-zinc-500">
                  {category.description ?? '—'}
                </TableCell>
                <TableCell>
                  <span
                    className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                      category.isActive
                        ? 'bg-emerald-50 text-emerald-700'
                        : 'bg-zinc-100 text-zinc-400'
                    }`}
                  >
                    <span className={`w-1.5 h-1.5 rounded-full shrink-0 ${category.isActive ? 'bg-emerald-500' : 'bg-zinc-400'}`} />
                    {category.isActive ? 'Activo' : 'Inactivo'}
                  </span>
                </TableCell>
                <TableCell className="text-right">
                  <div className="flex justify-end gap-1">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleOpenEdit(category)}
                    >
                      <Pencil className="h-4 w-4 text-zinc-400" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        setSelectedCategory(category);
                        setDeleteDialogOpen(true);
                      }}
                    >
                      <Trash2 className="h-4 w-4 text-rose-500" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
          <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
            <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
              {selectedCategory ? 'Editar Categoría' : 'Nueva Categoría'}
            </DialogTitle>
          </DialogHeader>
          <CategoryFormContent
            category={selectedCategory ?? undefined}
            onSubmit={handleSubmit}
            onCancel={() => setDialogOpen(false)}
            isSubmitting={isSubmitting}
          />
        </DialogContent>
      </Dialog>

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar categoría?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. La categoría "{selectedCategory?.name}" será
              eliminada permanentemente.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-rose-600 hover:bg-rose-700 text-white"
            >
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
