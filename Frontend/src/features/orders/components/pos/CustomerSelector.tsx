import { useState, useMemo, useCallback } from 'react';
import { Check, ChevronsUpDown, UserPlus } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/shared/components/ui/command';
import { Popover, PopoverContent, PopoverTrigger } from '@/shared/components/ui/popover';
import { cn } from '@/shared/utils/cn';
import type { Customer } from '@/features/customers/types/customer';

interface CustomerSelectorProps {
  customers: Customer[];
  selectedCustomer: Customer | null;
  onSelect: (customer: Customer) => void;
  onCreateNew: () => void;
}

export function CustomerSelector({
  customers,
  selectedCustomer,
  onSelect,
  onCreateNew,
}: CustomerSelectorProps) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');

  const handleCreateNew = useCallback(() => {
    setOpen(false);
    onCreateNew();
  }, [onCreateNew]);

  const filteredCustomers = useMemo(() => {
    if (!search) return customers.filter((c) => c.isActive);

    const searchLower = search.toLowerCase();
    return customers.filter(
      (c) =>
        c.isActive &&
        (c.name.toLowerCase().includes(searchLower) ||
          c.phone.includes(search) ||
          c.email?.toLowerCase().includes(searchLower))
    );
  }, [customers, search]);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between"
        >
          {selectedCustomer ? (
            <div className="flex flex-col items-start gap-0.5 text-left">
              <span className="font-medium">{selectedCustomer.name}</span>
              <span className="text-xs text-muted-foreground">{selectedCustomer.phone}</span>
            </div>
          ) : (
            <span className="text-muted-foreground">Seleccionar cliente...</span>
          )}
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput
            placeholder="Buscar cliente por nombre, teléfono o email..."
            value={search}
            onValueChange={setSearch}
          />
          <CommandList>
            <CommandEmpty>
              <div className="py-6 text-center">
                <p className="text-sm text-muted-foreground mb-4">
                  No se encontró el cliente
                </p>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleCreateNew}
                >
                  <UserPlus className="h-4 w-4 mr-2" />
                  Crear nuevo cliente
                </Button>
              </div>
            </CommandEmpty>
            <CommandGroup>
              {filteredCustomers.map((customer) => (
                <CommandItem
                  key={customer.id}
                  value={customer.id.toString()}
                  onSelect={() => {
                    onSelect(customer);
                    setOpen(false);
                  }}
                >
                  <Check
                    className={cn(
                      'mr-2 h-4 w-4',
                      selectedCustomer?.id === customer.id ? 'opacity-100' : 'opacity-0'
                    )}
                  />
                  <div className="flex flex-col gap-0.5">
                    <span className="font-medium">{customer.name}</span>
                    <div className="flex gap-2 text-xs text-muted-foreground">
                      <span>{customer.phone}</span>
                      {customer.email && <span>{customer.email}</span>}
                    </div>
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
          {filteredCustomers.length > 0 && (
            <div className="p-2 border-t">
              <Button
                variant="ghost"
                size="sm"
                className="w-full"
                onClick={handleCreateNew}
              >
                <UserPlus className="h-4 w-4 mr-2" />
                Crear nuevo cliente
              </Button>
            </div>
          )}
        </Command>
      </PopoverContent>
    </Popover>
  );
}
