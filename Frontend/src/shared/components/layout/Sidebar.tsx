import { useMemo } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { LogOut, ChevronLeft, Banknote } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { DeepSoftLogo } from '@/shared/components/ui/deepsoft-logo';
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
  const openCashClosing = useUIStore((state) => state.openCashClosing);

  const filteredNavItems = useMemo(
    () => NAVIGATION_CONFIG.items.filter((item: NavItem) => permissions.includes(item.requiredPermission)),
    [permissions]
  );

  const operationItems = filteredNavItems.filter((item) => item.group === 'operation');
  const adminItems = filteredNavItems.filter((item) => item.group === 'admin');

  const handleNavigation = (path: string) => {
    navigate(path);
    onNavigate?.();
  };

  const handleLogout = async () => {
    onNavigate?.();
    await logout();
  };

  const sectionLabel = (label: string) => (
    !isCollapsed && (
      <p className="px-2 pt-3 pb-1 text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
        {label}
      </p>
    )
  );

  return (
    <div className="flex h-full flex-col">
      {/* Header: logo + collapse button */}
      <div className={cn('border-b border-blue-100 bg-indigo-50 p-4 flex items-center', isCollapsed ? 'justify-center' : 'justify-between')}>
        {!isCollapsed && (
          <div className="flex items-center gap-2">
            <DeepSoftLogo width={28} height={28} />
            <h2 className="text-sm font-semibold text-indigo-900 tracking-tight">{NAVIGATION_CONFIG.appName}</h2>
          </div>
        )}
        {isCollapsed && <DeepSoftLogo width={28} height={28} />}
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

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto p-2">
        {/* Grupo: Operación diaria */}
        {operationItems.length > 0 && (
          <div>
            {sectionLabel('Operación')}
            <ul className="space-y-1">
              {operationItems.map((item) => (
                <li key={item.path}>
                  <NavLink
                    item={item}
                    isActive={location.pathname === item.path}
                    onClick={() => handleNavigation(item.path)}
                    isCollapsed={isCollapsed}
                  />
                </li>
              ))}
              {/* Corte de Caja — acción, no ruta */}
              <li>
                <Button
                  variant="ghost"
                  className={cn('w-full', isCollapsed ? 'justify-center px-2' : 'justify-start')}
                  onClick={() => { openCashClosing(); onNavigate?.(); }}
                  title="Corte de Caja"
                >
                  <Banknote className={cn('h-4 w-4', !isCollapsed && 'mr-2')} />
                  {!isCollapsed && 'Corte de Caja'}
                </Button>
              </li>
            </ul>
          </div>
        )}

        {/* Separador entre grupos */}
        {operationItems.length > 0 && adminItems.length > 0 && (
          <div className={cn('border-t border-zinc-100', !isCollapsed ? 'mt-1' : 'mt-2 mb-1')} />
        )}

        {/* Grupo: Administración */}
        {adminItems.length > 0 && (
          <div>
            {sectionLabel('Administración')}
            <ul className="space-y-1">
              {adminItems.map((item) => (
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
          </div>
        )}
      </nav>

      {/* Bottom: UserInfo + Cerrar Sesión */}
      <div className="border-t border-blue-100 p-2 space-y-1">
        {!isCollapsed && (
          <div className="px-2 py-2 border-b border-zinc-100 mb-1">
            <UserInfo user={user} />
          </div>
        )}
        {isCollapsed && (
          <div className="flex justify-center py-1 border-b border-zinc-100 mb-1">
            <UserInfo user={user} compact />
          </div>
        )}
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
