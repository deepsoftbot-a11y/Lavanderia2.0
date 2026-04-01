import {
  Receipt,
  Package,
  Banknote,
  BarChart2,
  Shirt,
  UserCog,
  LogOut,
  type LucideIcon,
} from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { cn } from '@/shared/utils/cn';
import type { NavItem } from '@/shared/types/navigation';

const ICONS: Record<string, LucideIcon> = {
  Receipt,
  Package,
  Banknote,
  BarChart2,
  Shirt,
  UserCog,
  LogOut,
};

interface NavLinkProps {
  item: NavItem;
  isActive: boolean;
  onClick: () => void;
  isCollapsed?: boolean;
}

export function NavLink({ item, isActive, onClick, isCollapsed = false }: NavLinkProps) {
  const IconComponent = ICONS[item.icon];

  if (isCollapsed) {
    return (
      <Button
        variant="ghost"
        size="icon"
        className={cn(
          'w-full h-10',
          isActive && 'bg-blue-100 text-blue-700'
        )}
        onClick={onClick}
        title={item.label}
      >
        {IconComponent && <IconComponent className="h-4 w-4" />}
      </Button>
    );
  }

  return (
    <Button
      variant="ghost"
      className={cn(
        'w-full justify-start',
        isActive && 'bg-blue-100 text-blue-700 font-medium'
      )}
      onClick={onClick}
    >
      {IconComponent && <IconComponent className="mr-2 h-4 w-4" />}
      {item.label}
    </Button>
  );
}
