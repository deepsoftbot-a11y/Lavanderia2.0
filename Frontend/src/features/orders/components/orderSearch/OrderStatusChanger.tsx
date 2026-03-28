import { useState } from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';

interface OrderStatusOption {
  id: number;
  name: string;
  color?: string;
}

interface OrderStatusChangerProps {
  currentStatusId: number;
  statuses: OrderStatusOption[];
  onStatusChange: (statusId: number) => Promise<boolean>;
  isLoading: boolean;
}

export function OrderStatusChanger({
  currentStatusId,
  statuses,
  onStatusChange,
  isLoading,
}: OrderStatusChangerProps) {
  const [localStatusId, setLocalStatusId] = useState(currentStatusId);

  const handleValueChange = async (value: string) => {
    const newStatusId = parseInt(value);
    if (newStatusId === localStatusId) return;

    const previousId = localStatusId;
    setLocalStatusId(newStatusId);

    const success = await onStatusChange(newStatusId);
    if (!success) {
      setLocalStatusId(previousId);
    }
  };

  return (
    <div className="space-y-1.5">
      <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
        Cambiar estado
      </p>
      <Select
        value={localStatusId.toString()}
        onValueChange={handleValueChange}
        disabled={isLoading}
      >
        <SelectTrigger className="h-8 text-xs">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {statuses.map((status) => (
            <SelectItem key={status.id} value={status.id.toString()}>
              <span className="flex items-center gap-2">
                {status.color && (
                  <span
                    className="w-2 h-2 rounded-full shrink-0"
                    style={{ backgroundColor: status.color }}
                  />
                )}
                <span>{status.name}</span>
              </span>
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
