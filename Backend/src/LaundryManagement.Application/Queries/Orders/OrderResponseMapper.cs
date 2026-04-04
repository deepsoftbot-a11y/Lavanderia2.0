using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Domain.Aggregates.Clients;
using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.Aggregates.ServiceGarments;
using LaundryManagement.Domain.Aggregates.Services;

namespace LaundryManagement.Application.Queries.Orders;

/// <summary>
/// Mapeo de OrderPure → OrderResponseDto.
/// Centralizado aquí para que múltiples handlers puedan reutilizarlo.
/// </summary>
internal static class OrderResponseMapper
{
    internal static OrderResponseDto MapToDto(OrderPure order, decimal amountPaid)
        => Build(order, amountPaid, null, null, null, null);

    internal static OrderResponseDto MapToDto(
        OrderPure order,
        decimal amountPaid,
        ClientPure? client,
        List<OrderPaymentDto>? payments,
        Dictionary<int, ServicePure>? servicesDict = null,
        Dictionary<int, ServiceGarmentPure>? garmentTypesDict = null)
        => Build(order, amountPaid, client, payments, servicesDict, garmentTypesDict);

    private static OrderResponseDto Build(
        OrderPure order,
        decimal amountPaid,
        ClientPure? client,
        List<OrderPaymentDto>? payments,
        Dictionary<int, ServicePure>? servicesDict,
        Dictionary<int, ServiceGarmentPure>? garmentTypesDict)
    {
        var balance = order.Total.Amount - amountPaid;
        var paymentStatus = amountPaid >= order.Total.Amount && order.Total.Amount > 0
            ? "paid"
            : amountPaid > 0 ? "partial" : "pending";

        return new OrderResponseDto
        {
            Id              = order.Id.Value,
            FolioOrden      = order.Folio.Value,
            ClientId        = order.ClientId.Value,
            PromisedDate    = order.PromisedDate.ToString("o"),
            DeliveredDate   = order.DeliveryDate?.ToString("o"),
            ReceivedBy      = order.ReceivedBy,
            InitialStatusId = order.StatusId,
            OrderStatusId   = order.StatusId,
            Notes           = order.Notes ?? string.Empty,
            StorageLocation = order.StorageLocation ?? string.Empty,
            Subtotal        = order.Subtotal.Amount,
            TotalDiscount   = order.TotalDiscount.Amount,
            Total           = order.Total.Amount,
            AmountPaid      = amountPaid,
            Balance         = balance,
            PaymentStatus   = paymentStatus,
            CreatedAt       = order.ReceivedDate.ToString("o"),
            CreatedBy       = order.ReceivedBy,
            Items = order.LineItems.Select(item =>
            {
                ServicePure? service = null;
                servicesDict?.TryGetValue(item.ServiceId, out service);
                ServiceGarmentPure? garmentType = null;
                if (item.ServiceGarmentId.HasValue)
                    garmentTypesDict?.TryGetValue(item.ServiceGarmentId.Value, out garmentType);

                return new OrderItemResponseDto
                {
                    Id               = item.Id,
                    ServiceId        = item.ServiceId,
                    ServiceGarmentId = item.ServiceGarmentId ?? 0,
                    DiscountId       = 0,
                    WeightKilos      = item.WeightKilos ?? 0m,
                    Quantity         = item.Quantity ?? 0,
                    UnitPrice        = item.UnitPrice.Amount,
                    Notes            = item.Notes ?? string.Empty,
                    Subtotal         = item.Subtotal.Amount,
                    DiscountAmount   = item.LineDiscount.Amount,
                    Total            = item.LineTotal.Amount,
                    Service = service == null ? null : new OrderItemServiceDto
                    {
                        Id          = service.Id.Value,
                        CategoryId  = service.CategoryId.Value,
                        Category    = new OrderItemCategoryDto
                        {
                            Id       = service.Category.Id.Value,
                            Name     = service.Category.Name,
                            IsActive = true,
                        },
                        Code        = service.Code.Value,
                        Name        = service.Name,
                        ChargeType  = service.IsPieceBased ? "PorPieza" : "PorPeso",
                        PricePerKg  = service.WeightPricing?.PricePerKilo.Amount,
                        MinWeight   = service.WeightPricing?.MinimumWeight,
                        MaxWeight   = service.WeightPricing?.MaximumWeight,
                        Description = service.Description,
                        EstimatedTime = service.EstimatedHours,
                        IsActive    = service.IsActive,
                    },
                    GarmentType = garmentType == null ? null : new OrderItemGarmentTypeDto
                    {
                        Id          = garmentType.Id.Value,
                        Name        = garmentType.Name,
                        Description = garmentType.Description,
                        IsActive    = garmentType.IsActive,
                    },
                };
            }).ToList(),
            Client = client == null ? null : new OrderClientDto
            {
                Id             = client.Id.Value,
                CustomerNumber = client.CustomerNumber,
                Name           = client.Name,
                Phone          = client.PhoneNumber.Value,
                Email          = client.Email?.Value,
                Address        = client.Address,
                Rfc            = client.Rfc?.Value,
                CreditLimit    = client.CreditLimit.Amount,
                CurrentBalance = client.CurrentBalance.Amount,
                IsActive       = client.IsActive,
                CreatedAt      = client.RegisteredAt.ToString("o"),
                CreatedBy      = client.RegisteredBy,
            },
            Payments = payments ?? new List<OrderPaymentDto>(),
        };
    }
}
