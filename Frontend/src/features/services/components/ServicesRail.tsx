import { cn } from '@/shared/utils/cn';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';

export type ServiceSection =
  | 'services'
  | 'categories'
  | 'garmentTypes'
  | 'prices'
  | 'discounts';

interface ServicesRailProps {
  active: ServiceSection;
  onChange: (section: ServiceSection) => void;
}

const GROUPS: { label: string; items: { key: ServiceSection; label: string }[] }[] = [
  {
    label: 'Catálogo',
    items: [
      { key: 'services', label: 'Servicios' },
      { key: 'categories', label: 'Categorías' },
    ],
  },
  {
    label: 'Prendas',
    items: [
      { key: 'garmentTypes', label: 'Tipos de Prenda' },
      { key: 'prices', label: 'Precios' },
    ],
  },
  {
    label: 'Comercial',
    items: [{ key: 'discounts', label: 'Descuentos' }],
  },
];

const ALL_ITEMS = GROUPS.flatMap((g) => g.items);

const SECTION_LABELS: Record<ServiceSection, string> = {
  services: 'Servicios',
  categories: 'Categorías',
  garmentTypes: 'Tipos de Prenda',
  prices: 'Precios',
  discounts: 'Descuentos',
};

export function ServicesRail({ active, onChange }: ServicesRailProps) {
  return (
    <>
      {/* Desktop — rail lateral */}
      <nav className="hidden lg:flex flex-col py-4">
        {GROUPS.map((group, i) => (
          <div key={group.label} className={i > 0 ? 'mt-6' : ''}>
            <p className="px-4 mb-1 text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
              {group.label}
            </p>
            {group.items.map((item) => (
              <button
                key={item.key}
                onClick={() => onChange(item.key)}
                className={cn(
                  'w-full text-left px-4 py-2 text-sm transition-colors border-l-2',
                  active === item.key
                    ? 'border-zinc-900 bg-zinc-50 text-zinc-900 font-medium'
                    : 'border-transparent text-zinc-500 hover:text-zinc-800 hover:bg-zinc-50'
                )}
              >
                {item.label}
              </button>
            ))}
          </div>
        ))}
      </nav>

      {/* Mobile — select */}
      <div className="lg:hidden px-4 py-3 border-b border-zinc-100">
        <Select value={active} onValueChange={(v) => onChange(v as ServiceSection)}>
          <SelectTrigger className="w-full">
            <SelectValue>
              {SECTION_LABELS[active]}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            {ALL_ITEMS.map((item) => (
              <SelectItem key={item.key} value={item.key}>
                {item.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </>
  );
}
