import { Checkbox } from '@/shared/components/ui/checkbox';
import { Label } from '@/shared/components/ui/label';
import { CurrencyInput } from '@/shared/components/ui/currency-input';
import type { GarmentType } from '@/features/services/types/garmentType';
import type { GarmentPriceInput } from '@/features/services/types/service';

interface GarmentPricesSubsectionProps {
  garmentTypes: GarmentType[];
  value: GarmentPriceInput[];
  onChange: (prices: GarmentPriceInput[]) => void;
}

export function GarmentPricesSubsection({
  garmentTypes,
  value,
  onChange,
}: GarmentPricesSubsectionProps) {
  const getEntry = (id: number) => value.find((p) => p.garmentTypeId === id);
  const isChecked = (id: number) => !!getEntry(id);

  const handleCheck = (garmentTypeId: number, checked: boolean) => {
    if (checked) {
      onChange([...value, { garmentTypeId, unitPrice: 0 }]);
    } else {
      onChange(value.filter((p) => p.garmentTypeId !== garmentTypeId));
    }
  };

  const handlePrice = (garmentTypeId: number, unitPrice: number | '') => {
    onChange(
      value.map((p) =>
        p.garmentTypeId === garmentTypeId
          ? { ...p, unitPrice: typeof unitPrice === 'number' ? unitPrice : 0 }
          : p
      )
    );
  };

  const activeTypes = garmentTypes.filter((gt) => gt.isActive);

  if (activeTypes.length === 0) {
    return (
      <p className="text-xs text-zinc-400 italic py-2">
        No hay tipos de prenda activos. Créalos desde la sección "Tipos de Prenda".
      </p>
    );
  }

  return (
    <div className="space-y-0.5 max-h-56 overflow-y-auto pr-1">
      {activeTypes.map((gt) => {
        const checked = isChecked(gt.id);
        const entry = getEntry(gt.id);
        return (
          <div
            key={gt.id}
            className="flex items-center gap-3 rounded-lg px-2 py-1.5 hover:bg-zinc-50 transition-colors"
          >
            <Checkbox
              id={`gp-${gt.id}`}
              checked={checked}
              onCheckedChange={(v) => handleCheck(gt.id, !!v)}
            />
            <Label
              htmlFor={`gp-${gt.id}`}
              className="flex-1 text-sm text-zinc-700 cursor-pointer select-none"
            >
              {gt.name}
            </Label>
            <div className="w-32 shrink-0">
              <CurrencyInput
                value={checked ? (entry?.unitPrice ?? '') : ''}
                onChange={(v) => handlePrice(gt.id, v)}
                disabled={!checked}
                placeholder="—"
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}
