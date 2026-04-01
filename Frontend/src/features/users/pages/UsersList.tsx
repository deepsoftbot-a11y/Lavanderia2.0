import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Search, Pencil, Trash2, Power } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/shared/components/ui/tabs';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';

import { useUsersStore } from '@/features/users/stores/usersStore';
import { useRolesStore } from '@/features/users/stores/rolesStore';
import { usePermissionsStore } from '@/features/users/stores/permissionsStore';
import { RolesTab } from '@/features/users/components/RolesTab';
import { PermissionsTab } from '@/features/users/components/PermissionsTab';
import { TABLE_HEADER_CLASS as TH } from '@/shared/utils/constants';
import { StatusBadge } from '@/shared/components/ui/status-badge';
import type { User } from '@/features/users/types/user';

export function UsersList() {
  const navigate = useNavigate();
  const { users, isLoading, error, filters, setFilters, fetchUsers, deleteUser, toggleUserStatus } =
    useUsersStore();
  const { roles, fetchRoles } = useRolesStore();
  const { fetchPermissions } = usePermissionsStore();

  const [searchTerm, setSearchTerm] = useState(filters.search ?? '');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [toggleStatusDialogOpen, setToggleStatusDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);

  useEffect(() => {
    fetchUsers();
    fetchRoles();
    fetchPermissions();
  }, [fetchUsers, fetchRoles, fetchPermissions]);

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setFilters({ search: searchTerm });
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm, setFilters]);

  const handleDelete = async () => {
    if (!selectedUser) return;
    await deleteUser(selectedUser.id);
    setDeleteDialogOpen(false);
    setSelectedUser(null);
  };

  const handleToggleStatus = async () => {
    if (!selectedUser) return;
    await toggleUserStatus(selectedUser.id);
    setToggleStatusDialogOpen(false);
    setSelectedUser(null);
  };

  return (
    <div className="bg-white border border-zinc-200 rounded-lg overflow-hidden">
      {/* Page header */}
      <div className="px-6 py-5 border-b border-zinc-100">
        <h1 className="text-xl font-semibold text-zinc-900 tracking-tight">Usuarios</h1>
        <p className="text-xs text-zinc-400 mt-0.5">Gestiona usuarios, roles y permisos del sistema</p>
      </div>

      <Tabs defaultValue="users">
        <TabsList className="border-b border-zinc-100 rounded-none w-full justify-start bg-transparent h-auto p-0 gap-0">
          <TabsTrigger
            value="users"
            className="rounded-none border-b-2 border-transparent data-[state=active]:border-zinc-900 data-[state=active]:text-zinc-900 text-zinc-500 text-xs font-medium px-4 py-2.5 bg-transparent hover:text-zinc-700 transition-colors"
          >
            Usuarios
          </TabsTrigger>
          <TabsTrigger
            value="roles"
            className="rounded-none border-b-2 border-transparent data-[state=active]:border-zinc-900 data-[state=active]:text-zinc-900 text-zinc-500 text-xs font-medium px-4 py-2.5 bg-transparent hover:text-zinc-700 transition-colors"
          >
            Roles
          </TabsTrigger>
          <TabsTrigger
            value="permissions"
            className="rounded-none border-b-2 border-transparent data-[state=active]:border-zinc-900 data-[state=active]:text-zinc-900 text-zinc-500 text-xs font-medium px-4 py-2.5 bg-transparent hover:text-zinc-700 transition-colors"
          >
            Permisos
          </TabsTrigger>
        </TabsList>

        {/* ── USERS TAB ── */}
        <TabsContent value="users" className="mt-0">
          {/* Filters */}
          <div className="flex flex-col sm:flex-row gap-3 px-6 py-4 border-b border-zinc-100 bg-zinc-50">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-zinc-400" />
              <Input
                placeholder="Buscar por nombre, usuario o email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9 text-sm h-8"
              />
            </div>

            <Select
              value={filters.roleId ? String(filters.roleId) : 'all'}
              onValueChange={(val) =>
                setFilters({ roleId: val === 'all' ? undefined : Number(val) })
              }
            >
              <SelectTrigger className="w-full sm:w-40 h-8 text-xs">
                <SelectValue placeholder="Todos los roles" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos los roles</SelectItem>
                {roles.map((r) => (
                  <SelectItem key={r.id} value={String(r.id)}>
                    {r.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select
              value={filters.isActive !== undefined ? String(filters.isActive) : 'all'}
              onValueChange={(val) =>
                setFilters({ isActive: val === 'all' ? undefined : val === 'true' })
              }
            >
              <SelectTrigger className="w-full sm:w-32 h-8 text-xs">
                <SelectValue placeholder="Estado" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="true">Activos</SelectItem>
                <SelectItem value="false">Inactivos</SelectItem>
              </SelectContent>
            </Select>

            <Button
              size="sm"
              onClick={() => navigate('/users/create')}
              className="text-xs h-8 whitespace-nowrap"
            >
              <Plus className="h-3.5 w-3.5 mr-1.5" />
              Nuevo usuario
            </Button>
          </div>

          {/* Table header */}
          <div className="hidden md:grid grid-cols-[2fr_3fr_2fr_auto_auto_auto] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
            <p className={TH}>Usuario</p>
            <p className={TH}>Nombre / Email</p>
            <p className={TH}>Rol</p>
            <p className={TH}>Estado</p>
            <p className={TH}>Último acceso</p>
            <p className={TH}>Acciones</p>
          </div>

          {error ? (
            <div className="px-6 py-8 text-center">
              <p className="text-sm text-rose-500 mb-3">Error: {error}</p>
              <Button size="sm" variant="outline" onClick={fetchUsers}>
                Reintentar
              </Button>
            </div>
          ) : isLoading ? (
            <div className="flex justify-center py-8">
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-zinc-900" />
            </div>
          ) : users.length === 0 ? (
            <p className="px-6 py-8 text-sm text-zinc-400 text-center">No se encontraron usuarios</p>
          ) : (
            <>
              {/* Desktop rows */}
              <div className="hidden md:block">
                {users.map((user) => (
                  <div
                    key={user.id}
                    className="grid grid-cols-[2fr_3fr_2fr_auto_auto_auto] gap-4 items-center px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
                  >
                    <span className="font-mono text-sm font-medium text-zinc-900">
                      {user.username}
                    </span>

                    <div className="min-w-0">
                      <p className="text-sm font-medium text-zinc-800 truncate">{user.fullName}</p>
                      <p className="text-xs text-zinc-400 truncate">{user.email}</p>
                    </div>

                    <span className="text-xs text-zinc-500">{user.role?.name ?? '—'}</span>

                    <StatusBadge active={user.isActive} />

                    <span className="text-xs text-zinc-400 capitalize">
                      {user.lastLogin
                        ? format(new Date(user.lastLogin), "d 'de' MMM 'de' yyyy", { locale: es })
                        : 'Nunca'}
                    </span>

                    <div className="flex items-center gap-1">
                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-7 w-7 p-0"
                        onClick={() => navigate(`/users/edit/${user.id}`)}
                      >
                        <Pencil className="h-3.5 w-3.5 text-zinc-400" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-7 w-7 p-0"
                        onClick={() => {
                          setSelectedUser(user);
                          setToggleStatusDialogOpen(true);
                        }}
                      >
                        <Power className="h-3.5 w-3.5 text-zinc-400" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-7 w-7 p-0 hover:text-rose-600"
                        onClick={() => {
                          setSelectedUser(user);
                          setDeleteDialogOpen(true);
                        }}
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>

              {/* Mobile rows */}
              <div className="md:hidden">
                {users.map((user) => (
                  <div
                    key={user.id}
                    className="px-6 py-4 border-b border-zinc-100"
                  >
                    <div className="flex items-start justify-between gap-3 mb-2">
                      <div className="min-w-0">
                        <p className="text-sm font-medium text-zinc-900">{user.fullName}</p>
                        <p className="text-xs text-zinc-400 font-mono">@{user.username}</p>
                      </div>
                      <StatusBadge active={user.isActive} className="shrink-0" />
                    </div>
                    <p className="text-xs text-zinc-500 mb-1">{user.email}</p>
                    <p className="text-xs text-zinc-400">{user.role?.name ?? 'Sin rol'}</p>
                    <div className="flex gap-2 mt-3 pt-3 border-t border-zinc-100">
                      <Button
                        size="sm"
                        className="flex-1 text-xs h-7"
                        onClick={() => navigate(`/users/edit/${user.id}`)}
                      >
                        <Pencil className="h-3 w-3 mr-1" />
                        Editar
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        className="h-7"
                        onClick={() => {
                          setSelectedUser(user);
                          setToggleStatusDialogOpen(true);
                        }}
                      >
                        <Power className="h-3 w-3" />
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        className="h-7 hover:text-rose-600 hover:border-rose-200"
                        onClick={() => {
                          setSelectedUser(user);
                          setDeleteDialogOpen(true);
                        }}
                      >
                        <Trash2 className="h-3 w-3" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>

              {/* Footer counter */}
              <div className="px-6 py-3 bg-zinc-50 border-t border-zinc-100">
                <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
                  {users.length} usuario{users.length !== 1 ? 's' : ''}
                </p>
              </div>
            </>
          )}
        </TabsContent>

        {/* ── ROLES TAB ── */}
        <TabsContent value="roles" className="mt-0">
          <RolesTab />
        </TabsContent>

        {/* ── PERMISSIONS TAB ── */}
        <TabsContent value="permissions" className="mt-0">
          <PermissionsTab />
        </TabsContent>
      </Tabs>

      {/* Delete dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar usuario?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. El usuario{' '}
              <span className="font-semibold">{selectedUser?.fullName}</span> será eliminado
              permanentemente del sistema.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-rose-600 hover:bg-rose-700 text-white"
            >
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Toggle status dialog */}
      <AlertDialog open={toggleStatusDialogOpen} onOpenChange={setToggleStatusDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {selectedUser?.isActive ? 'Desactivar' : 'Activar'} usuario
            </AlertDialogTitle>
            <AlertDialogDescription>
              ¿Estás seguro de que deseas{' '}
              {selectedUser?.isActive ? 'desactivar' : 'activar'} al usuario{' '}
              <span className="font-semibold">{selectedUser?.fullName}</span>?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleToggleStatus}>Confirmar</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
