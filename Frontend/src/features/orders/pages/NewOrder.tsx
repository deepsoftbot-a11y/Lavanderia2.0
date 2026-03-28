import { useEffect, useState } from 'react';
import { Calendar, MapPin, DollarSign, ShoppingCart, Package, CreditCard, User, Search } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
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
import { useToast } from '@/shared/hooks/use-toast';
import type { Service } from '@/features/services/types/service';

export function NewOrder() {
  const { toast } = useToast();
  const { user } = useAuthStore();

  const [selectedService, setSelectedService] = useState<Service | null>(null);
  const [showAddServiceDialog, setShowAddServiceDialog] = useState(false);
  const [showQuickCustomerForm, setShowQuickCustomerForm] = useState(false);
  const [cashClosingOpen, setCashClosingOpen] = useState(false);
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

  return (
    <div className="h-[calc(100vh-4rem)] flex flex-col p-2 gap-2 bg-zinc-50">
      {/* Compact Header */}
      <div className="flex items-center justify-between px-3 py-2 bg-white rounded-lg border border-zinc-200 shrink-0">
        <div className="flex items-center gap-2.5">
          <div className="flex items-center justify-center w-7 h-7 rounded bg-zinc-100">
            <ShoppingCart className="h-3.5 w-3.5 text-zinc-500" />
          </div>
          <div>
            <h1 className="text-base font-semibold text-zinc-900 leading-none">
              Punto de Venta
            </h1>
            <p className="text-xs text-zinc-400 mt-0.5">
              Nueva orden
            </p>
          </div>
        </div>
        <div className="flex gap-1.5">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setOrderSearchOpen(true)}
            className="h-7 text-xs px-2.5"
          >
            <Search className="h-3 w-3 mr-1" />
            Buscar Orden
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setCashClosingOpen(true)}
            className="h-7 text-xs px-2.5"
          >
            <DollarSign className="h-3 w-3 mr-1" />
            Corte
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleClearCart}
            disabled={items.length === 0}
            className="h-7 text-xs px-2.5"
          >
            Limpiar
          </Button>
        </div>
      </div>

      {/* Main POS Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-[1fr_380px] gap-2 flex-1 min-h-0">
        {/* Left Side: Customer Info + Services */}
        <div className="flex flex-col gap-2 min-h-0">
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
                {/* Customer Selector */}
                <div className="space-y-1">
                  <Label className="text-xs text-zinc-500 font-medium">Cliente *</Label>
                  <CustomerSelector
                    customers={customers}
                    selectedCustomer={customer}
                    onSelect={setCustomer}
                    onCreateNew={() => setShowQuickCustomerForm(true)}
                  />
                </div>

                {/* Delivery Details */}
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
                    <Input
                      id="storageLocation"
                      value={storageLocation}
                      onChange={(e) => setStorageLocation(e.target.value)}
                      placeholder="Ej: A-1"
                      className="h-8 text-xs"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Services Catalog Section */}
          <div className="flex-1 flex flex-col overflow-hidden bg-white border border-zinc-200 rounded-lg">
            <div className="flex items-center gap-2 px-3 py-2.5 border-b border-zinc-100 shrink-0">
              <div className="flex items-center justify-center w-6 h-6 rounded bg-zinc-100">
                <Package className="h-3.5 w-3.5 text-zinc-500" />
              </div>
              <p className="text-sm font-semibold text-zinc-900">Catálogo de Servicios</p>
            </div>
            <div className="p-3 pt-2 flex-1 overflow-auto">
              <ServiceCatalog services={services} onSelectService={handleSelectService} />
            </div>
          </div>
        </div>

        {/* Right Side: Cart + Payment + Total */}
        <div className="flex flex-col gap-2 min-h-0">
          {/* Cart Items Section */}
          <div className="flex-1 flex flex-col overflow-hidden bg-white border border-zinc-200 rounded-lg">
            <div className="flex items-center justify-between px-3 py-2.5 border-b border-zinc-100 shrink-0">
              <div className="flex items-center gap-2">
                <div className="flex items-center justify-center w-6 h-6 rounded bg-zinc-100">
                  <ShoppingCart className="h-3.5 w-3.5 text-zinc-500" />
                </div>
                <p className="text-sm font-semibold text-zinc-900">Carrito</p>
              </div>
              <span className="flex items-center justify-center h-5 min-w-5 rounded-full bg-zinc-100 px-1.5 text-xs font-semibold text-zinc-500">
                {getItemCount()}
              </span>
            </div>
            <div className="p-3 pt-2 flex-1 overflow-auto">
              {items.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full text-center py-8">
                  <div className="flex items-center justify-center w-12 h-12 rounded-full bg-zinc-50 border border-zinc-100 mb-3">
                    <ShoppingCart className="h-5 w-5 text-zinc-300" />
                  </div>
                  <p className="text-xs font-medium text-zinc-400">
                    No hay servicios agregados
                  </p>
                  <p className="text-xs text-zinc-300 mt-0.5">
                    Selecciona servicios del catálogo
                  </p>
                </div>
              ) : (
                <CartItemsList
                  items={items}
                  onUpdateQuantity={updateItemQuantity}
                  onRemove={removeItem}
                />
              )}
            </div>
          </div>

          {/* Payment Section */}
          <div className="shrink-0 bg-white border border-zinc-200 rounded-lg overflow-hidden">
            <div className="flex items-center justify-between px-3 py-2.5 border-b border-zinc-100">
              <div className="flex items-center gap-2">
                <div className="flex items-center justify-center w-6 h-6 rounded bg-zinc-100">
                  <CreditCard className="h-3.5 w-3.5 text-zinc-500" />
                </div>
                <p className="text-sm font-semibold text-zinc-900">Método de Pago</p>
              </div>
              {paymentData && (
                <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-50 border border-emerald-100 text-[10px] font-semibold text-emerald-700">
                  <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 shrink-0" />
                  Listo
                </span>
              )}
            </div>
            <div className="p-3 pt-2">
              <PaymentForm
                key={paymentFormKey}
                maxAmount={getTotal()}
                defaultAmount={getTotal()}
                onSubmit={(data) => setPaymentData(data)}
                isLoading={false}
                showCancelButton={false}
              />
            </div>
          </div>

          {/* Summary + Action */}
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
              className="w-full h-10 text-sm font-semibold bg-zinc-900 hover:bg-zinc-800 text-white"
              onClick={handleOpenConfirm}
              disabled={!isValid() || isLoadingOrder}
            >
              Registrar Venta
            </Button>
          </div>
        </div>
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

      <CashClosingModal open={cashClosingOpen} onOpenChange={setCashClosingOpen} />

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
