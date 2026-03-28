import { useCallback } from 'react';
import { Plus } from 'lucide-react';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { CHARGE_TYPE } from '@/features/services/types/service';
import type { Service } from '@/features/services/types/service';

// Constantes a nivel de módulo — se crean una sola vez
const CATEGORY_MAP: Record<number, { name: string; badge: string }> = {
  1: { name: 'Lavanderia',  badge: 'bg-blue-50   text-blue-700'   },
  2: { name: 'Secado',      badge: 'bg-amber-50  text-amber-700'  },
  3: { name: 'Tintoreria',  badge: 'bg-rose-50   text-rose-700'   },
  4: { name: 'Planchado',   badge: 'bg-violet-50 text-violet-700' },
};

const COLOR_MAP: Record<string, string> = {
  lavanderia:  'bg-blue-50   text-blue-700',
  secado:      'bg-amber-50  text-amber-700',
  planchado:   'bg-violet-50 text-violet-700',
  tintoreria:  'bg-rose-50   text-rose-700',
};

const CHARGE_TYPE_LABEL: Record<string, string> = {
  [CHARGE_TYPE.PorPieza]: 'Por pieza',
  [CHARGE_TYPE.PorPeso]: 'Por kilo',
};

function getCategoryInfo(service: Service): { name: string; badge: string } {
  if (service.category?.name) {
    const normalized = service.category.name
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .trim();
    return {
      name: service.category.name,
      badge: COLOR_MAP[normalized] ?? 'bg-zinc-100 text-zinc-600',
    };
  }

  return CATEGORY_MAP[service.categoryId] ?? { name: 'Sin categoría', badge: 'bg-zinc-100 text-zinc-600' };
}

interface ServiceCardProps {
  service: Service;
  onSelect: (service: Service) => void;
}

export function ServiceCard({ service, onSelect }: ServiceCardProps) {
  const categoryInfo = getCategoryInfo(service);
  const handleSelect = useCallback(() => onSelect(service), [service, onSelect]);

  const priceDisplay = service.chargeType === CHARGE_TYPE.PorPeso
    ? service.pricePerKg
    : null;

  return (
    <Card className="shadow-none transition-colors cursor-pointer group hover:bg-zinc-50 border-zinc-200">
      <CardContent className="p-4">
        <div className="flex flex-col gap-3">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <h3 className="font-semibold text-sm md:text-base line-clamp-1">
                {service.name}
              </h3>
              <p className="text-xs text-muted-foreground line-clamp-2 mt-1">
                {service.description}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2 flex-wrap">
            <Badge className={`${categoryInfo.badge} text-xs font-medium border-0`}>
              {categoryInfo.name}
            </Badge>
            <Badge variant="outline" className="text-xs">
              {CHARGE_TYPE_LABEL[service.chargeType] ?? service.chargeType}
            </Badge>
          </div>

          <div className="flex items-center justify-between">
            <div>
              {priceDisplay !== undefined && priceDisplay !== null ? (
                <>
                  <p className="font-mono tabular-nums text-lg md:text-xl font-bold text-zinc-900">
                    ${priceDisplay}
                  </p>
                  <p className="text-xs text-muted-foreground">por kilo</p>
                </>
              ) : (
                <p className="text-xs text-muted-foreground">Precio por prenda</p>
              )}
            </div>
            <Button
              size="sm"
              onClick={handleSelect}
              className="group-hover:scale-105 transition-transform"
            >
              <Plus className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
