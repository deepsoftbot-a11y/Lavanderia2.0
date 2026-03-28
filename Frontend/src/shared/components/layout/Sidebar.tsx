import { useMemo } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { LogOut, ChevronLeft } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { NavLink } from './NavLink';
import { UserInfo } from './UserInfo';
import { useAuthStore } from '@/features/auth/stores/authStore';
import { useUIStore } from '@/shared/stores/uiStore';
import { NAVIGATION_CONFIG } from '@/shared/config/navigation.config';
import { cn } from '@/shared/utils/cn';
import type { NavItem } from '@/shared/types/navigation';

interface SidebarProps {
  onNavigate?: () => void;
  isCollapsed?: boolean;
}

export function Sidebar({ onNavigate, isCollapsed = false }: SidebarProps) {
  const navigate = useNavigate();
  const location = useLocation();

  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);
  const permissions = useAuthStore((state) => state.permissions);
  const toggleSidebarCollapse = useUIStore((state) => state.toggleSidebarCollapse);

  const filteredNavItems = useMemo(
    () => NAVIGATION_CONFIG.items.filter((item: NavItem) => permissions.includes(item.requiredPermission)),
    [permissions]
  );

  const handleNavigation = (path: string) => {
    navigate(path);
    onNavigate?.();
  };

  const handleLogout = async () => {
    onNavigate?.();
    await logout();
  };

  return (
    <div className="flex h-full flex-col">
      <div className={cn('border-b border-zinc-100 p-4 flex items-center', isCollapsed ? 'justify-center' : 'justify-between')}>
        {!isCollapsed && (
          <h2 className="text-sm font-semibold text-zinc-900 tracking-tight">{NAVIGATION_CONFIG.appName}</h2>
        )}
        <Button
          variant="ghost"
          size="icon"
          onClick={toggleSidebarCollapse}
          className={cn('h-8 w-8 hidden md:flex', isCollapsed && 'mx-auto')}
          aria-label={isCollapsed ? 'Expandir sidebar' : 'Contraer sidebar'}
        >
          <ChevronLeft className={cn('h-4 w-4 transition-transform', isCollapsed && 'rotate-180')} />
        </Button>
      </div>

      {!isCollapsed && (
        <div className="border-b border-zinc-100 p-4">
          <UserInfo user={user} />
        </div>
      )}

      <nav className="flex-1 p-2">
        <ul className="space-y-1">
          {filteredNavItems.map((item) => (
            <li key={item.path}>
              <NavLink
                item={item}
                isActive={location.pathname === item.path}
                onClick={() => handleNavigation(item.path)}
                isCollapsed={isCollapsed}
              />
            </li>
          ))}
        </ul>
      </nav>

      <div className="border-t border-zinc-100 p-2">
        <Button
          variant="ghost"
          className={cn(
            'w-full text-rose-600 hover:text-rose-600 hover:bg-rose-50',
            isCollapsed ? 'justify-center px-2' : 'justify-start'
          )}
          onClick={handleLogout}
        >
          <LogOut className={cn('h-4 w-4', !isCollapsed && 'mr-2')} />
          {!isCollapsed && 'Cerrar Sesión'}
        </Button>
      </div>
    </div>
  );
}
