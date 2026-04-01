import { useEffect } from 'react';
import { Outlet } from 'react-router-dom';

import { Header } from '@/shared/components/layout/Header';
import { MobileSidebar } from '@/shared/components/layout/MobileSidebar';
import { Sidebar } from '@/shared/components/layout/Sidebar';
import { useUIStore } from '@/shared/stores/uiStore';
import { cn } from '@/shared/utils/cn';

export function MainLayout() {
  const isSidebarCollapsed = useUIStore((state) => state.isSidebarCollapsed);

  useEffect(() => {
    const handleKeyboard = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
        e.preventDefault();
        useUIStore.getState().toggleSidebarCollapse();
      }
    };

    window.addEventListener('keydown', handleKeyboard);
    return () => window.removeEventListener('keydown', handleKeyboard);
  }, []);

  return (
    <div className="min-h-screen bg-blue-50/40">
      {/* Mobile header with hamburger */}
      <Header />

      {/* Mobile sidebar (Sheet) */}
      <MobileSidebar />

      <div className="flex">
        {/* Desktop sidebar - collapsible */}
        <aside
          className={cn(
            'hidden md:flex md:flex-col md:fixed md:inset-y-0 border-r border-blue-100 bg-white transition-all duration-300',
            isSidebarCollapsed ? 'md:w-16' : 'md:w-64'
          )}
        >
          <Sidebar isCollapsed={isSidebarCollapsed} />
        </aside>

        {/* Main content */}
        <main
          className={cn(
            'flex-1 transition-all duration-300',
            isSidebarCollapsed ? 'md:pl-16' : 'md:pl-64'
          )}
        >
          <div className="p-4 md:p-6">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
