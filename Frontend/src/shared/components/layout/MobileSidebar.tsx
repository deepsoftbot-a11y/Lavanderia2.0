import { Sheet, SheetContent } from '@/shared/components/ui/sheet';
import { Sidebar } from './Sidebar';
import { useUIStore } from '@/shared/stores/uiStore';

export function MobileSidebar() {
  const isOpen = useUIStore((state) => state.isSidebarOpen);
  const closeSidebar = useUIStore((state) => state.closeSidebar);

  return (
    <Sheet open={isOpen} onOpenChange={(open) => !open && closeSidebar()}>
      <SheetContent side="left" className="w-[280px] p-0">
        <Sidebar onNavigate={closeSidebar} />
      </SheetContent>
    </Sheet>
  );
}
