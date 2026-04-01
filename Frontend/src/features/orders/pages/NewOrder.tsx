import { useEffect, useState } from 'react';
import { Calendar, MapPin, ShoppingCart, Package, User, Search, Trash2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { ClearableInput } from '@/shared/components/ui/field-input';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/shared/components/ui/tabs';
import { ServiceCatalog } from '@/features/orders/components/pos/ServiceCatalog';
import { CartItemsList } from '@/features/orders/components/pos/CartItemsList';
import { OrderSummary } from '@/features/orders/components/pos/OrderSummary';
import { CustomerSelector } from '@/features/orders/components/pos/CustomerSelector';
import { AddServiceDialog } from '@/features/orders/components/pos/AddServiceDialog';
import { ConfirmOrderDialog } from '@/features/orders/components/pos/ConfirmOrderDialog';
import { QuickCustomerForm } from '@/features/orders/components/pos/QuickCustomerForm';
import { CashClosingModal } from '@/features/orders/components/cashClosing/CashClosingModal';
import { OrderSearchSheet } from '@/features/orders/components/orderSearch/OrderSearchSheet';
import { PaymentForm, type PaymentFormData } from '@/features/orders/components/payments/PaymentForm';
import { useCartStore } from '@/features/orders/stores/cartStore';
import { useServicesStore } from '@/features/services/stores/servicesStore';
import { useDiscountsStore } from '@/features/services/stores/discountsStore';
import { useCustomersStore } from '@/features/customers/stores/customersStore';
import { useOrdersStore } from '@/features/orders/stores/ordersStore';
import { usePaymentMethodsStore } from '@/features/orders/stores/paymentMethodsStore';
import { useAuthStore } from '@/features/auth/stores/authStore';
import { useUIStore } from '@/shared/stores/uiStore';
import { useToast } from '@/shared/hooks/use-toast';
import type { Service } from '@/features/services/types/service';

export function NewOrder() {
  const { toast } = useToast();
  const { user } = useAuthStore();

  const cashClosingOpen = useUIStore((state) => state.cashClosingOpen);
  const closeCashClosing = useUIStore((state) => state.closeCashClosing);

  const [selectedService, setSelectedService] = useState<Service | null>(null);
  const [showAddServiceDialog, setShowAddServiceDialog] = useState(false);
  const [showQuickCustomerForm, setShowQuickCustomerForm] = useState(false);
  const [orderSearchOpen, setOrderSearchOpen] = useState(false);
  const [paymentData, setPaymentData] = useState<PaymentFormData | null>(null);
  const [paymentFormKey, setPaymentFormKey] = useState(0);
  const [confirmOrderOpen, setConfirmOrderOpen] = useState(false);
  const [createdOrderId, setCreatedOrderId] = useState<number | undefined>(undefined);

  const {
    customer,
    promisedDate,
    storageLocation,
    initialStatusId,
    notes,
    items,
    setCustomer,
    setPromisedDate,
    setStorageLocation,
    addItem,
    updateItemQuantity,
    removeItem,
    clearCart,
    getSubtotal,
    getTotalDiscount,
    getTotal,
    getItemCount,
    isValid,
    getValidationErrors,
  } = useCartStore();

  const { services, fetchServices } = useServicesStore();
  const { discounts, fetchDiscounts } = useDiscountsStore();
  const { customers, fetchCustomers, createCustomer, isLoading: isLoadingCustomer } =
    useCustomersStore();
  const { createOrder, isLoading: isLoadingOrder } = useOrdersStore();
  const { fetchPaymentMethods } = usePaymentMethodsStore();

  useEffect(() => {
    fetchServices();
    fetchDiscounts();
    fetchCustomers({ isActive: true });
    fetchPaymentMethods();
  }, [
    fetchServices,
    fetchDiscounts,
    fetchCustomers,
    fetchPaymentMethods,
  ]);

  useEffect(() => {
    if (!promisedDate) {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      setPromisedDate(tomorrow.toISOString().split('T')[0]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSelectService = (service: Service) => {
    setSelectedService(service);
    setShowAddServiceDialog(true);
  };

  const handleAddService = (
    service: Service,
    garment: any,
    discount: any | null,
    quantity: number,
    weightKilos: number,
    itemNotes: string,
    specificPrice: number | null
  ) => {
    addItem(service, garment, discount, quantity, weightKilos, itemNotes, specificPrice ?? undefined);

    const description = garment
      ? `${service.name} - ${(garment as any)?.garmentType?.name ?? (garment as any)?.name ?? ''}`
      : `${service.name} - ${weightKilos} kg`;

    toast({
      title: 'Servicio agregado',
      description,
    });
  };

  const handleCreateCustomer = async (data: any) => {
    const customer = await createCustomer(data);

    if (customer) {
      if (!customer.id || typeof customer.id !== 'number') {
        toast({
          title: 'Advertencia',
          description: 'El cliente fue creado pero no tiene un ID válido.',
          variant: 'destructive',
        });
        return;
      }

      setCustomer(customer);
      setShowQuickCustomerForm(false);
      toast({
        title: 'Cliente creado',
        description: `${customer.name} registrado exitosamente`,
      });
    } else {
      toast({
        title: 'Error',
        description: 'No se pudo crear el cliente',
        variant: 'destructive',
      });
    }
  };

  const handleOpenConfirm = () => {
    if (!isValid()) {
      const errors = getValidationErrors();
      toast({
        title: 'Orden incompleta',
        description: errors.join(', '),
        variant: 'destructive',
      });
      return;
    }
    setConfirmOrderOpen(true);
  };

  const handleRegisterOrder = async (confirmedPayment: PaymentFormData | null) => {
    if (!user?.id || user.id <= 0) {
      toast({
        title: 'Error de sesión',
        description: 'No se pudo identificar el usuario.',
        variant: 'destructive',
      });
      return;
    }

    if (!customer?.id) {
      toast({
        title: 'Error de validación',
        description: 'No se pudo identificar el cliente seleccionado',
        variant: 'destructive',
      });
      return;
    }

    try {
      const orderData = {
        clientId: customer.id,
        promisedDate,
        initialStatusId,
        notes,
        storageLocation,
        receivedBy: user.id,
        items: items.map((item) => ({
          serviceId: item.serviceId,
          serviceGarmentId: item.serviceGarmentId,
          discountAmount: item.discountAmount,
          weightKilos: item.weightKilos,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          notes: item.notes,
        })),
        initialPayment: confirmedPayment || undefined,
      };

      const order = await createOrder(orderData);

      if (!order) {
        toast({
          title: 'Error',
          description: 'No se pudo crear la orden',
          variant: 'destructive',
        });
        return;
      }

      const successMessage = confirmedPayment
        ? `Orden #${order.id} creada con pago inicial de $${confirmedPayment.amount.toFixed(2)}`
        : `Orden #${order.id} creada exitosamente`;

      toast({
        title: 'Orden registrada',
        description: successMessage,
      });

      setConfirmOrderOpen(false);
      setCreatedOrderId(order.id);
      setOrderSearchOpen(true);
      clearCart();
      setPaymentData(null);
      setPaymentFormKey((k) => k + 1);
      const nextDay = new Date();
      nextDay.setDate(nextDay.getDate() + 1);
      setPromisedDate(nextDay.toISOString().split('T')[0]);
    } catch (error) {
      toast({
        title: 'Error',
        description: (error as Error).message,
        variant: 'destructive',
      });
    }
  };

  const handleClearCart = () => {
    if (items.length > 0) {
      if (confirm('¿Estás seguro de que deseas limpiar el carrito?')) {
        clearCart();
        toast({
          title: 'Carrito limpiado',
          description: 'Todos los items han sido eliminados',
        });
      }
    }
  };

  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const minDate = tomorrow.toISOString().split('T')[0];

  // Secciones reutilizables para mobile tabs y desktop grid
  const leftColumn = (
    <div className="flex flex-col gap-2 lg:min-h-0">
      {/* Customer Info Section */}
      <div className="shrink-0 bg-white border border-zinc-200 rounded-lg overflow-hidden">
        <div className="flex items-center justify-between px-3 py-2.5 border-b border-zinc-100">
          <div className="flex items-center gap-2">
            <div className="flex items-center justify-center w-6 h-6 rounded bg-zinc-100">
              <User className="h-3.5 w-3.5 text-zinc-500" />
            </div>
            <p className="text-sm font-semibold text-zinc-900">Información del Cliente</p>
          </div>
          {customer && (
            <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-50 border border-emerald-100 text-[10px] font-semibold text-emerald-700">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 shrink-0" />
              Listo
            </span>
          )}
        </div>
        <div className="p-3">
          <div className="space-y-2.5">
            <div className="space-y-1">
              <Label className="text-xs text-zinc-500 font-medium">Cliente *</Label>
              <CustomerSelector
                customers={customers}
                selectedCustomer={customer}
                onSelect={setCustomer}
                onCreateNew={() => setShowQuickCustomerForm(true)}
              />
            </div>
            <div className="grid grid-cols-2 gap-2">
              <div className="space-y-1">
                <Label htmlFor="promisedDate" className="text-xs text-zinc-500 font-medium flex items-center gap-1">
                  <Calendar className="h-3 w-3" />
                  Entrega *
                </Label>
                <Input
                  id="promisedDate"
                  type="date"
                  value={promisedDate}
                  onChange={(e) => setPromisedDate(e.target.value)}
                  min={minDate}
                  className="h-8 text-xs"
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="storageLocation" className="text-xs text-zinc-500 font-medium flex items-center gap-1">
                  <MapPin className="h-3 w-3" />
                  Ubicación
                </Label>
                <ClearableInput
                  id="storageLocation"
                  value={storageLocation}
                  onChange={(e) => setStorageLocation(e.target.value)}
                  placeholder="Ej: A-1"
                  onClear={() => setStorageLocation('')}
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Services Catalog Section */}
      <div className="flex flex-col overflow-hidden bg-white border border-zinc-200 rounded-lg lg:flex-1">
        <div className="flex items-center gap-2 px-3 py-2.5 border-b border-zinc-100 shrink-0">
          <div className="flex items-center justify-center w-6 h-6 rounded bg-zinc-100">
            <Package className="h-3.5 w-3.5 text-zinc-500" />
          </div>
          <p className="text-sm font-semibold text-zinc-900">Catálogo de Servicios</p>
        </div>
        <div className="p-3 pt-2 overflow-auto max-h-[50vh] md:max-h-[480px] lg:max-h-none lg:flex-1">
          <ServiceCatalog services={services} onSelectService={handleSelectService} />
        </div>
      </div>
    </div>
  );

  const cartHeader = (
    <div className="flex items-center justify-between px-3 py-2.5 border-b border-zinc-100 shrink-0">
      <div className="flex items-center gap-2">
        <div className="flex items-center justify-center w-6 h-6 rounded bg-zinc-100">
          <ShoppingCart className="h-3.5 w-3.5 text-zinc-500" />
        </div>
        <p className="text-sm font-semibold text-zinc-900">Carrito</p>
        <span className="flex items-center justify-center h-5 min-w-5 rounded-full bg-zinc-100 px-1.5 text-xs font-semibold text-zinc-500">
          {getItemCount()}
        </span>
      </div>
      <div className="flex gap-1">
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7"
          onClick={() => setOrderSearchOpen(true)}
          title="Buscar orden"
        >
          <Search className="h-3.5 w-3.5" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7"
          onClick={handleClearCart}
          disabled={items.length === 0}
          title="Limpiar carrito"
        >
          <Trash2 className="h-3.5 w-3.5" />
        </Button>
      </div>
    </div>
  );

  const cartItems = (
    <div className="p-3 pt-2 overflow-auto max-h-[40vh] md:max-h-[380px] lg:max-h-none lg:flex-1">
      {items.length === 0 ? (
        <div className="flex flex-col items-center justify-center h-full text-center py-8">
          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-zinc-50 border border-zinc-100 mb-3">
            <ShoppingCart className="h-5 w-5 text-zinc-300" />
          </div>
          <p className="text-xs font-medium text-zinc-400">No hay servicios agregados</p>
          <p className="text-xs text-zinc-300 mt-0.5">Selecciona servicios del catálogo</p>
        </div>
      ) : (
        <CartItemsList
          items={items}
          onUpdateQuantity={updateItemQuantity}
          onRemove={removeItem}
        />
      )}
    </div>
  );

  const paymentRow = (
    <div className="shrink-0 border-t border-zinc-100 px-3 py-2">
      <PaymentForm
        key={paymentFormKey}
        maxAmount={getTotal()}
        defaultAmount={getTotal()}
        onSubmit={(data) => setPaymentData(data)}
        onClear={() => setPaymentData(null)}
        isLoading={false}
        showCancelButton={false}
        compact
      />
    </div>
  );

  const summaryAndAction = (
    <div className="shrink-0 space-y-2">
      <div className="bg-zinc-50 border border-zinc-100 rounded-lg p-3">
        <OrderSummary
          itemCount={getItemCount()}
          subtotal={getSubtotal()}
          totalDiscount={getTotalDiscount()}
          total={getTotal()}
        />
      </div>
      <Button
        className="w-full h-10 text-sm font-semibold"
        onClick={handleOpenConfirm}
        disabled={!isValid() || isLoadingOrder}
      >
        Registrar Venta
      </Button>
    </div>
  );

  const rightColumn = (
    <div className="flex flex-col gap-2 lg:min-h-0">
      {/* Cart + Payment in one card */}
      <div className="flex flex-col overflow-hidden bg-white border border-zinc-200 rounded-lg lg:flex-1">
        {cartHeader}
        {cartItems}
        {paymentRow}
      </div>
      {summaryAndAction}
    </div>
  );

  return (
    <div className="flex flex-col p-2 gap-2 bg-zinc-50 overflow-y-auto lg:h-[calc(100vh-4rem)] lg:overflow-hidden">

      {/* Mobile: barra mínima + tabs */}
      <div className="md:hidden">
        <div className="flex items-center justify-between px-3 py-2 bg-white rounded-lg border border-zinc-200 mb-2">
          <p className="text-sm font-semibold text-zinc-700">Punto de Venta</p>
          <div className="flex gap-1">
            <Button
              variant="ghost"
              size="icon"
              className="h-7 w-7"
              onClick={() => setOrderSearchOpen(true)}
              title="Buscar orden"
            >
              <Search className="h-3.5 w-3.5" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="h-7 w-7"
              onClick={handleClearCart}
              disabled={items.length === 0}
              title="Limpiar carrito"
            >
              <Trash2 className="h-3.5 w-3.5" />
            </Button>
          </div>
        </div>

        <Tabs defaultValue="catalog">
          <TabsList className="w-full mb-2">
            <TabsTrigger value="catalog" className="flex-1 text-sm">Catálogo</TabsTrigger>
            <TabsTrigger value="cart" className="flex-1 text-sm gap-1.5">
              Carrito
              {getItemCount() > 0 && (
                <span className="flex items-center justify-center h-4 min-w-4 rounded-full bg-blue-600 px-1 text-[10px] font-semibold text-white">
                  {getItemCount()}
                </span>
              )}
            </TabsTrigger>
          </TabsList>
          <TabsContent value="catalog" className="mt-0">
            {leftColumn}
          </TabsContent>
          <TabsContent value="cart" className="mt-0">
            <div className="flex flex-col gap-2">
              <div className="flex flex-col overflow-hidden bg-white border border-zinc-200 rounded-lg">
                {cartHeader}
                {cartItems}
                {paymentRow}
              </div>
              {summaryAndAction}
            </div>
          </TabsContent>
        </Tabs>
      </div>

      {/* Desktop: grid de 2 columnas */}
      <div className="hidden md:grid md:grid-cols-[1fr_340px] gap-2 lg:flex-1 lg:min-h-0">
        {leftColumn}
        {rightColumn}
      </div>

      <AddServiceDialog
        open={showAddServiceDialog}
        service={selectedService}
        discounts={discounts}
        onClose={() => {
          setShowAddServiceDialog(false);
          setSelectedService(null);
        }}
        onAdd={handleAddService}
      />

      <QuickCustomerForm
        open={showQuickCustomerForm}
        isLoading={isLoadingCustomer}
        onClose={() => setShowQuickCustomerForm(false)}
        onSubmit={handleCreateCustomer}
      />

      <CashClosingModal open={cashClosingOpen} onOpenChange={(open) => !open && closeCashClosing()} />

      <OrderSearchSheet
        open={orderSearchOpen}
        onOpenChange={(open) => {
          setOrderSearchOpen(open);
          if (!open) setCreatedOrderId(undefined);
        }}
        initialOrderId={createdOrderId}
      />

      <ConfirmOrderDialog
        open={confirmOrderOpen}
        customer={customer}
        itemCount={getItemCount()}
        subtotal={getSubtotal()}
        totalDiscount={getTotalDiscount()}
        total={getTotal()}
        promisedDate={promisedDate}
        storageLocation={storageLocation}
        paymentData={paymentData}
        isLoading={isLoadingOrder}
        onConfirm={handleRegisterOrder}
        onCancel={() => setConfirmOrderOpen(false)}
      />
    </div>
  );
}
