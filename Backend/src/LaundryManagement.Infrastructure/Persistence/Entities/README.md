# Entidades de Infraestructura (EF Core)

## ⚠️ Nota Importante

Las clases en esta carpeta **NO son entidades de dominio**. Son **entidades de infraestructura** generadas por Entity Framework Core para el mapeo objeto-relacional (ORM).

## 📂 Ubicación vs Namespace

- **Ubicación física:** `LaundryManagement.Domain/Entities/`
- **Namespace:** `LaundryManagement.Infrastructure.Persistence.Entities`

Esta aparente contradicción es **intencional** y necesaria porque:

1. **EF Core scaffolding:** Cuando se generan entidades desde la base de datos, EF las coloca aquí por defecto
2. **Arquitectura de capas:** Domain NO debe depender de Infrastructure
3. **El namespace correcto:** Indica su verdadera naturaleza como entidades de persistencia

## 🏗️ Características de estas Entidades

- ✅ Generadas automáticamente (scaffolding desde BD)
- ✅ Partial classes (permiten extensión)
- ✅ Propiedades públicas con setters públicos (requerido por EF Core)
- ✅ Navigation properties virtuales (lazy loading)
- ❌ **SIN** lógica de negocio
- ❌ **SIN** validaciones
- ❌ **SIN** encapsulación

## 🎯 Uso Correcto

### ❌ NO usar directamente en Application o Domain

```csharp
// MAL - Uso directo en lógica de negocio
public async Task CrearOrden()
{
    var orden = new Ordene();
    orden.Subtotal = -100; // ⚠️ Sin validaciones!
    _context.Ordenes.Add(orden);
}
```

### ✅ SÍ usar a través de Agregados de Dominio

```csharp
// BIEN - Usar agregado Order
public async Task CrearOrden()
{
    var order = Order.Create(...); // ← Validaciones automáticas
    order.AddLineItem(...);        // ← Lógica de negocio
    await _repository.AddAsync(order);
}
```

## 📋 Entidades Disponibles

### Orden Management
- `Ordene` - Órdenes principales
- `OrdenesDetalle` - Líneas de orden
- `OrdenesDescuento` - Descuentos aplicados
- `HistorialEstadosOrden` - Historial de cambios de estado
- `EstadosOrden` - Catálogo de estados

### Payment Management
- `Pago` - Pagos
- `PagosDetalle` - Detalle de métodos de pago
- `MetodosPago` - Catálogo de métodos

### Service Catalog
- `Servicio` - Servicios
- `ServiciosPrenda` - Servicios por tipo de prenda
- `Categoria` - Categorías de servicio
- `TiposPrendum` - Tipos de prendas
- `Combo` - Combos de servicios
- `CombosDetalle` - Detalle de combos
- `Descuento` - Catálogo de descuentos

### Customer Management
- `Cliente` - Clientes

### User & Security
- `Usuario` - Usuarios
- `Role` - Roles
- `UsuariosRole` - Asignación usuario-rol
- `Permiso` - Permisos
- `RolesPermiso` - Asignación rol-permiso

### Cash Register
- `CortesCaja` - Cortes de caja
- `CortesCajaDetalle` - Detalle de cortes

### Reporting & Audit
- `ConfiguracionReporte` - Configuración de reportes
- `HistorialReporte` - Historial de reportes
- `AuditoriaGeneral` - Auditoría general

## 🔄 Relación con Agregados de Dominio

Los agregados de dominio (ubicados en `Aggregates/`) **envuelven** estas entidades:

```
Order (Agregado de Dominio)
  ↓ envuelve
Ordene (Entidad de Infraestructura)
  ↓ persiste con
EF Core
  ↓ guarda en
Base de Datos
```

## 📚 Más Información

Ver la documentación del agregado Order en:
`LaundryManagement.Domain/Aggregates/Orders/Order.cs`

---

**Última actualización:** 2026-01-04
