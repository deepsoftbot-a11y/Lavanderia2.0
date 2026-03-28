import { Menu } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { useUIStore } from '@/shared/stores/uiStore';
import { NAVIGATION_CONFIG } from '@/shared/config/navigation.config';

export function Header() {
  const openSidebar = useUIStore((state) => state.openSidebar);

  return (
    <header className="sticky top-0 z-40 flex h-14 items-center gap-4 border-b border-zinc-100 bg-white px-4 md:hidden">
      <Button
        variant="ghost"
        size="icon"
        onClick={openSidebar}
        aria-label="Abrir menú de navegación"
      >
        <Menu className="h-5 w-5" />
      </Button>
      <span className="text-sm font-semibold text-zinc-900 tracking-tight">{NAVIGATION_CONFIG.appName}</span>
    </header>
  );
}
