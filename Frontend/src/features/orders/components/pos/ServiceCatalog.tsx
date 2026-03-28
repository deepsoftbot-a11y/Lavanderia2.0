import { useState, useMemo } from 'react';
import { Search } from 'lucide-react';
import { Input } from '@/shared/components/ui/input';
import { Tabs, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
import { ServiceCard } from './ServiceCard';
import type { Service } from '@/features/services/types/service';


interface ServiceCatalogProps {
  services: Service[];
  onSelectService: (service: Service) => void;
}

export function ServiceCatalog({ services, onSelectService }: ServiceCatalogProps) {
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  const categories = useMemo(() => {
    const seen = new Set<number>();
    return services
      .filter((s) => s.isActive && s.category)
      .reduce<Array<{ id: number; name: string }>>((acc, s) => {
        if (!seen.has(s.categoryId)) {
          seen.add(s.categoryId);
          acc.push({ id: s.categoryId, name: s.category.name });
        }
        return acc;
      }, []);
  }, [services]);

  const filteredServices = useMemo(() => {
    const searchLower = search.toLowerCase();

    return services.filter((s) => {
      if (!s.isActive) return false;

      if (search && !s.name.toLowerCase().includes(searchLower) && !s.description?.toLowerCase().includes(searchLower)) {
        return false;
      }

      if (selectedCategory !== 'all') {
        return s.categoryId === Number(selectedCategory);
      }

      return true;
    });
  }, [services, search, selectedCategory]);

  return (
    <div className="space-y-4">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Buscar servicio..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="pl-9"
        />
      </div>

      <Tabs value={selectedCategory} onValueChange={setSelectedCategory}>
        <TabsList className="flex flex-wrap h-auto w-full gap-1">
          <TabsTrigger value="all" className="text-xs md:text-sm">
            Todos
          </TabsTrigger>
          {categories.map((cat) => (
            <TabsTrigger key={cat.id} value={String(cat.id)} className="text-xs md:text-sm">
              {cat.name}
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>

      <div className="mt-4">
        {filteredServices.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-muted-foreground">No se encontraron servicios</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-2 xl:grid-cols-3 gap-3">
            {filteredServices.map((service) => (
              <ServiceCard
                key={service.id}
                service={service}
                onSelect={onSelectService}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
