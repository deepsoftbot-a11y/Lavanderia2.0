import { useState, useEffect } from 'react';
import { Search, X } from 'lucide-react';
import { Input } from '@/shared/components/ui/input';
import { Button } from '@/shared/components/ui/button';
import { useDebounce } from '@/shared/hooks/useDebounce';

interface OrderSearchInputProps {
  onSearch: (query: string) => void;
  onClear: () => void;
  isSearching: boolean;
}

export function OrderSearchInput({ onSearch, onClear, isSearching }: OrderSearchInputProps) {
  const [query, setQuery] = useState('');
  const debouncedQuery = useDebounce(query, 300);

  useEffect(() => {
    if (debouncedQuery.trim().length >= 2) {
      onSearch(debouncedQuery.trim());
    } else if (debouncedQuery.trim().length === 0) {
      onClear();
    }
  }, [debouncedQuery, onSearch, onClear]);

  const handleClear = () => {
    setQuery('');
    onClear();
  };

  return (
    <div className="relative">
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
      <Input
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Buscar por folio, nombre o teléfono..."
        className="pl-9 pr-9 h-9 text-sm"
        autoFocus
      />
      {query && !isSearching && (
        <Button
          variant="ghost"
          size="sm"
          className="absolute right-1 top-1/2 -translate-y-1/2 h-6 w-6 p-0 text-zinc-400 hover:text-zinc-600"
          onClick={handleClear}
        >
          <X className="h-3.5 w-3.5" />
        </Button>
      )}
      {isSearching && (
        <div className="absolute right-3 top-1/2 -translate-y-1/2">
          <div className="h-3.5 w-3.5 animate-spin rounded-full border-2 border-zinc-200 border-t-zinc-600" />
        </div>
      )}
    </div>
  );
}
