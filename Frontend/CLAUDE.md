# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a React application for a laundry service (AppWebLavanderia) built with Vite, React 19, and TypeScript.

## Development Commands

```bash
npm run dev      # Start development server with HMR at http://localhost:5173
npm run build    # Type-check with tsc and build for production
npm run lint     # Run ESLint on the codebase
npm run preview  # Preview production build locally
```

## Tech Stack

- **Build Tool**: Vite 7.x with Fast Refresh
- **Framework**: React 19.2
- **Language**: TypeScript 5.9
- **Linting**: ESLint 9.x with React hooks and React refresh plugins

## Architecture

- **Entry Point**: `src/main.tsx` - Initializes React root with StrictMode
- **Main Component**: `src/App.tsx` - Root application component
- **Build Configuration**: `vite.config.ts` - Uses @vitejs/plugin-react for Fast Refresh
- **TypeScript Config**: Uses project references (`tsconfig.app.json` for app code, `tsconfig.node.json` for config files)

## Key Notes

- The project uses Babel-based Fast Refresh via @vitejs/plugin-react
- React Compiler is not enabled (performance consideration)
- ESLint is configured but not using type-aware rules (can be expanded for production)
- No test framework is currently configured

## Technology Rules

### UI & Styling

- **TailwindCSS**: Use TailwindCSS for all styling (utility-first approach)
  - Configure Tailwind in `tailwind.config.js`
  - Import Tailwind directives in main CSS file
  - Use Tailwind classes directly in JSX, avoid inline styles
  - Follow mobile-first responsive design (sm:, md:, lg:, xl:)

- **Shadcn/ui**: Use Shadcn/ui for UI components
  - Install components via CLI: `npx shadcn@latest add [component]`
  - Components are placed in `src/shared/components/ui/` — import as `@/shared/components/ui/button`
  - Customize components by editing the generated files directly
  - DO NOT install entire component library, only add needed components
  - Follow Shadcn's composition patterns for building complex UIs
  - Shadcn's `cn` utility lives at `src/shared/utils/cn.ts` — always import from `@/shared/utils/cn`
  - There is **no `src/lib/` directory** in this project

### State Management

- **Zustand**: Use Zustand for global state management
  - Create stores inside the feature: `src/features/<feature>/stores/<name>Store.ts`
  - UI-only state lives in `src/shared/stores/uiStore.ts`
  - Keep stores simple and focused (one store per domain/feature)
  - Use `immer` middleware for easier nested state updates
  - Avoid storing derived data; compute it in selectors or components
  - Use local component state (useState) for UI-only state

### HTTP & Data Fetching

- **Axios**: Use Axios for all HTTP requests
  - Create Axios instance in `src/api/axiosConfig.ts` with base configuration
  - Set up interceptors for auth tokens, error handling, and logging
  - Organize API calls in `src/api/` directory by resource (e.g., `src/api/orders.ts`, `src/api/customers.ts`)
  - Use async/await syntax, not .then() chains
  - Handle errors consistently with try/catch blocks
  - Consider adding React Query later if data caching/synchronization becomes complex

### Routing

- **React Router**: Use React Router v6+ for navigation
  - Define routes in a central location (`src/routes/` or `src/App.tsx`)
  - Use declarative routing with `<Routes>` and `<Route>` components
  - Use `useNavigate` hook for programmatic navigation (not `useHistory`)
  - Use `<Link>` or `<NavLink>` components for navigation links
  - Implement protected routes with wrapper components for authentication
  - Use route parameters and query strings appropriately

### Forms & Validation

- **React Hook Form**: Use React Hook Form for all forms
  - Leverage uncontrolled components for better performance
  - Use `register` for simple inputs, `Controller` for custom components
  - Integrate with Shadcn/ui form components
  - Keep form state isolated to form components

- **Zod**: Use Zod for schema validation
  - Define schemas inside the feature: `src/features/<feature>/schemas/<name>.schema.ts`
  - Use `zodResolver` to integrate Zod with React Hook Form
  - Reuse schemas for both client-side validation and TypeScript types
  - Export TypeScript types from schemas: `type User = z.infer<typeof userSchema>`

### Date Handling

- **Date-fns**: Use date-fns for date manipulation and formatting
  - Import only needed functions (tree-shaking friendly)
  - Use consistent date formats across the application
  - For laundry service: handle pickup/delivery dates, service duration, etc.
  - Prefer `date-fns/format` over native `toLocaleDateString()`

### General Technology Guidelines

- **Prefer TypeScript**: Always use TypeScript, avoid `any` types
- **No prop-types**: Use TypeScript interfaces instead of PropTypes
- **Async/await**: Use async/await over promises with .then()
- **ES6+ features**: Use modern JavaScript features (destructuring, spread, arrow functions)
- **Functional components**: Use function components only, no class components
- **React Hooks**: Use hooks for all stateful logic
- **Code splitting**: Use React.lazy() and Suspense for route-based code splitting when needed

## Folder Structure Rules

The project follows a **feature-based (vertical slices)** architecture. Each feature is self-contained.

### Root Structure

```
src/
├── api/              # Centralized API layer (mock/real switcher pattern)
├── assets/           # Static assets (images, fonts, icons)
├── features/         # Feature modules (vertical slices)
├── layouts/          # MainLayout.tsx (top-level layout, unchanged)
├── routes/           # router.tsx, ProtectedRoute, PublicRoute
├── shared/           # Truly shared code (used by 2+ features)
├── App.tsx
└── main.tsx
```

### Features Structure

Each feature contains all its own components, pages, stores, schemas, and types:

```
src/features/
├── auth/
│   ├── components/   # LoginForm.tsx
│   ├── pages/        # LoginPage.tsx
│   ├── schemas/      # auth.schema.ts
│   ├── stores/       # authStore.ts
│   └── types/        # auth.ts
│
├── orders/
│   ├── components/
│   │   ├── pos/          # ServiceCatalog, CartItems, etc.
│   │   ├── orderSearch/  # OrderSearchSheet, etc.
│   │   ├── payments/     # PaymentForm, PaymentSummaryCard
│   │   └── cashClosing/  # CashClosingModal
│   ├── pages/        # OrdersList.tsx, NewOrder.tsx
│   ├── schemas/      # order.schema, payment.schema, cashClosing.schema, paymentMethod.schema
│   ├── stores/       # ordersStore, cartStore, cashClosingsStore, paymentMethodsStore
│   └── types/        # order, payment, cashClosing, cashWithdrawal, paymentMethod
│
├── customers/
│   ├── schemas/      # customer.schema.ts
│   ├── stores/       # customersStore.ts
│   └── types/        # customer.ts, category.ts
│
├── services/
│   ├── components/   # ServicesTab, GarmentsTab, PricesTab, DiscountsTab
│   ├── pages/        # ServicesList.tsx
│   ├── schemas/      # service.schema, serviceGarment.schema, discount.schema, garmentStatus.schema
│   ├── stores/       # servicesStore, serviceGarmentsStore, servicePricesStore, discountsStore, garmentStatusesStore
│   └── types/        # service, serviceGarment, servicePrice, discount, garmentStatus
│
├── users/
│   ├── components/   # UserForm.tsx
│   ├── pages/        # UsersList, CreateUser, EditUser
│   ├── schemas/      # user.schema.ts
│   ├── stores/       # usersStore.ts
│   └── types/        # user.ts
│
└── dashboard/
    └── pages/        # Dashboard.tsx
```

### Shared Structure

Code that is truly shared across 2+ features (NOT feature-specific logic):

```
src/shared/
├── components/
│   ├── ui/           # Shadcn/ui components — install via npx shadcn@latest add [component]
│   ├── layout/       # Header, Sidebar, MobileSidebar, NavLink, UserInfo
│   └── common/       # AuthProvider, PermissionGuard, NumericInput
├── config/           # navigation.config, permissions.config, roles.config, auth.config
├── hooks/            # useDebounce, usePermissions, use-toast
├── stores/           # uiStore.ts
├── types/            # navigation.ts, permission.ts
└── utils/            # cn, constants, formatters, permissions
```

### API Structure

API calls organized by resource. **Stays centralized** — not moved into features.

```
src/api/
├── axiosConfig.ts           # Axios instance + interceptors
├── auth/index.ts            # Auth API (mock/real switcher)
├── orders/index.ts          # Orders API
├── customers/index.ts       # Customers API
├── services/index.ts        # Services API
├── servicePrices/index.ts   # Service prices API
├── serviceGarments/index.ts # Service garments API
├── discounts/index.ts       # Discounts API
├── payments/index.ts        # Payments API
├── cashClosings/index.ts    # Cash closings API
├── paymentMethods/index.ts  # Payment methods API
└── users/index.ts           # Users API
```

**API Guidelines:**
- Each `index.ts` is the mock/real switcher — keep these barrel files
- Import from `@/api/orders`, `@/api/customers`, etc.
- Features import from `@/api/*` (centralised), not from each other's API layer

### Inter-Feature Import Rules

Features CAN import stores/types from other features. What is NOT allowed: `shared/` importing from features.

```
features/orders   → may import from features/services, features/customers, features/auth
features/services → may import from features/auth
features/users    → may import from features/auth
shared/           → must NOT import from any feature
```

**Import examples (orders feature):**
```typescript
import { useDiscountsStore }     from '@/features/services/stores/discountsStore';
import { useServicesStore }      from '@/features/services/stores/servicesStore';
import { useServicePricesStore } from '@/features/services/stores/servicePricesStore';
import { useCustomersStore }     from '@/features/customers/stores/customersStore';
import { useAuthStore }          from '@/features/auth/stores/authStore';
import type { Service }          from '@/features/services/types/service';
import type { Customer }         from '@/features/customers/types/customer';
```

### Naming Conventions

- **Files**: PascalCase for components/pages (`OrdersList.tsx`), camelCase for utilities/hooks (`useAuth.ts`)
- **Schema files**: Use `*.schema.ts` pattern (e.g., `cashClosing.schema.ts`, NOT `cashClosingSchema.ts`)
- **Components**: PascalCase (`const OrderCard = () => {}`)
- **Functions**: camelCase (`function calculateTotal() {}`)
- **Constants**: UPPER_SNAKE_CASE (`const API_BASE_URL = ''`)
- **Interfaces/Types**: PascalCase (`interface Order {}`)
- **Folders**: **camelCase** for subfolders within features (e.g., `cashClosing/`, `orderSearch/`, `pos/`); lowercase for utility/hook folders

### File Organization Rules

1. **Colocation**: Keep related files inside the feature (components, pages, stores, schemas, types together)
2. **Index files**: Only `src/api/*/index.ts` barrel files are allowed (mock/real switcher pattern)
   - **Not allowed**: feature components, pages, stores, hooks — import directly from the file
   - Bad: `import { CashClosingModal } from '@/features/orders/components/cashClosing'`
   - Good: `import { CashClosingModal } from '@/features/orders/components/cashClosing/CashClosingModal'`
3. **One component per file**: Each component should have its own file (except very small, tightly coupled sub-components)
4. **Consistent imports**: Use absolute imports with `@/` alias (e.g., `@/features/orders/stores/ordersStore`)
5. **No circular dependencies**: Avoid importing from files that import from the current file

## Code and Programming Conventions

### TypeScript Guidelines

**Type Safety:**
- Avoid `any` type at all costs; use `unknown` if type is truly unknown
- Use strict TypeScript settings (strict mode enabled)
- Prefer `interface` for object shapes, `type` for unions/intersections
- Always type function parameters and return types explicitly
- Use generics for reusable type-safe functions

```typescript
// Good
interface Customer {
  id: string;
  name: string;
  email: string;
}

function getCustomer(id: string): Promise<Customer> {
  // implementation
}

// Bad
function getCustomer(id: any): any {
  // implementation
}
```

**Null Safety:**
- Use optional chaining (`?.`) and nullish coalescing (`??`)
- Avoid using `!` (non-null assertion) unless absolutely necessary
- Handle null/undefined cases explicitly

```typescript
// Good
const customerName = customer?.name ?? 'Unknown';

// Bad
const customerName = customer!.name || 'Unknown';
```

### React Component Conventions

**Component Structure:**
- Use functional components exclusively (no class components)
- Define component props interface above the component
- Keep component functions small and focused (under 200 lines)
- Extract complex logic into custom hooks

```typescript
// Component template structure
interface OrderCardProps {
  order: Order;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}

export function OrderCard({ order, onEdit, onDelete }: OrderCardProps) {
  // Hooks at the top
  const [isExpanded, setIsExpanded] = useState(false);

  // Event handlers
  const handleEdit = () => {
    onEdit(order.id);
  };

  // Render
  return (
    // JSX
  );
}
```

**Hooks Rules:**
- Call hooks at the top level of the component (never in conditionals/loops)
- Group related hooks together
- Custom hooks should start with `use` prefix
- Extract complex useEffect logic into custom hooks

**Props:**
- Destructure props in function signature
- Use default parameters for optional props when appropriate
- Avoid prop drilling; use context or state management for deeply nested data
- Pass callbacks with useCallback when necessary to prevent re-renders

```typescript
// Good
interface ButtonProps {
  label: string;
  variant?: 'primary' | 'secondary';
  onClick: () => void;
}

export function Button({ label, variant = 'primary', onClick }: ButtonProps) {
  // implementation
}

// Bad - no destructuring, no types
export function Button(props) {
  return <button onClick={props.onClick}>{props.label}</button>;
}
```

### Import/Export Conventions

**Import Order:**
Organize imports in this specific order with blank lines between groups:

```typescript
// 1. React and core libraries
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

// 2. Third-party libraries
import axios from 'axios';
import { z } from 'zod';
import { format } from 'date-fns';

// 3. Internal imports - absolute paths using @/ alias
import { Button } from '@/shared/components/ui/button';
import { useAuthStore } from '@/features/auth/stores/authStore';
import { getOrders } from '@/api/orders';
import type { Order } from '@/features/orders/types/order';

// 4. Relative imports (for sibling files in same folder)
import { OrderCard } from './OrderCard';

// 5. Styles (if any)
import './styles.css';
```

**Export Conventions:**
- Prefer named exports over default exports
- Export types alongside implementation when relevant
- Use `export` keyword directly (not at bottom of file)

```typescript
// Good - named export
export function OrdersList() {
  // implementation
}

export interface OrdersListProps {
  // props
}

// Avoid - default export
export default function OrdersList() {
  // implementation
}
```

**Import Guidelines:**
- Use absolute imports with `@/` alias (configure in tsconfig.json)
- Import only what you need (destructure imports)
- Never use `import * as` unless absolutely necessary
- Group related imports on same line when it makes sense

```typescript
// Good
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';

// Bad
import * as UI from '@/shared/components/ui';
```

### Error Handling

**Try-Catch Pattern:**
- Wrap async operations in try-catch blocks
- Show user-friendly error messages via toast/alert
- Log errors to console (or error tracking service in production)
- Always handle errors, never leave them unhandled

```typescript
async function createOrder(orderData: CreateOrderInput) {
  try {
    const response = await api.createOrder(orderData);
    toast.success('Order created successfully');
    return response.data;
  } catch (error) {
    console.error('Failed to create order:', error);
    toast.error('Failed to create order. Please try again.');
    throw error; // Re-throw if calling code needs to handle it
  }
}
```

**Error Handling in Components:**
```typescript
function OrdersList() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getOrders();
      setOrders(data);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load orders';
      setError(message);
      console.error('Error fetching orders:', err);
    } finally {
      setLoading(false);
    }
  };

  // Render error state
  if (error) return <ErrorMessage message={error} />;
}
```

### Comments and Documentation

**Minimal Comments Philosophy:**
- Write self-documenting code with clear variable/function names
- Only add comments for complex logic that isn't immediately obvious
- Avoid redundant comments that just repeat what code does
- Use comments to explain "why", not "what"

```typescript
// Bad - obvious comment
// Loop through orders
orders.forEach(order => processOrder(order));

// Good - explains non-obvious business logic
// Apply discount only for orders over $100 placed on weekends
if (order.total > 100 && isWeekend(order.createdAt)) {
  applyDiscount(order);
}

// Good - explains complex algorithm
// Use binary search since orders are sorted by date
const index = binarySearch(orders, targetDate);
```

**When to Comment:**
- Complex business logic or algorithms
- Workarounds for bugs or quirks
- TODO items with context
- Important assumptions or constraints

```typescript
// TODO: Replace with WebSocket connection once backend supports it
const pollForUpdates = () => {
  setInterval(fetchOrders, 5000);
};

// WORKAROUND: API returns null instead of empty array for new customers
const orders = response.data.orders ?? [];
```

### Variable and Function Naming

**Descriptive Names:**
- Use clear, descriptive names that reveal intent
- Avoid abbreviations unless universally understood (e.g., `id`, `url`)
- Boolean variables should be prefixed with `is`, `has`, `should`, `can`
- Event handlers should be prefixed with `handle` or `on`

```typescript
// Good
const isLoadingOrders = true;
const hasPermission = checkPermission(user);
const handleSubmit = () => {};
const onOrderCreated = () => {};

// Bad
const loading = true; // loading what?
const perm = checkPermission(user); // unclear abbreviation
const submit = () => {}; // verb without context
```

**Function Naming:**
- Use verbs for functions (get, set, create, update, delete, fetch, handle)
- Be specific about what the function does
- Keep names concise but descriptive

```typescript
// Good
function calculateOrderTotal(items: OrderItem[]): number {}
function fetchCustomerOrders(customerId: string): Promise<Order[]> {}
function validateEmail(email: string): boolean {}

// Bad
function total(items: OrderItem[]): number {} // unclear
function get(id: string): Promise<Order[]> {} // get what?
function check(email: string): boolean {} // check what?
```

### Code Organization Within Files

**File Structure Order:**
```typescript
// 1. Imports
import { useState } from 'react';

// 2. Types/Interfaces
interface Props {
  // ...
}

// 3. Constants (file-scoped)
const MAX_ITEMS = 100;

// 4. Helper functions (if not extracted to utils)
function calculateDiscount(price: number): number {
  // ...
}

// 5. Main component/function
export function ComponentName({ prop1, prop2 }: Props) {
  // 5a. Hooks
  const [state, setState] = useState();

  // 5b. Derived values
  const total = calculateTotal(items);

  // 5c. Event handlers
  const handleClick = () => {};

  // 5d. Effects
  useEffect(() => {}, []);

  // 5e. Render helpers (optional)
  const renderItem = (item: Item) => <div>{item.name}</div>;

  // 5f. Return/JSX
  return <div>...</div>;
}
```

### React Best Practices

**State Management:**
- Use local state (useState) for UI-only state
- Use Zustand stores for shared global state
- Avoid unnecessary state; derive values when possible
- Keep state as close to where it's used as possible

```typescript
// Good - derived value
const total = items.reduce((sum, item) => sum + item.price, 0);

// Bad - unnecessary state
const [total, setTotal] = useState(0);
useEffect(() => {
  setTotal(items.reduce((sum, item) => sum + item.price, 0));
}, [items]);
```

**Performance:**
- Use React.memo for expensive components that re-render often
- Use useMemo for expensive computations
- Use useCallback for functions passed as props to memoized components
- Avoid inline object/array literals in JSX (causes re-renders)

```typescript
// Good
const memoizedValue = useMemo(() => expensiveCalculation(data), [data]);

const handleClick = useCallback(() => {
  doSomething(id);
}, [id]);

// Bad - creates new function on every render
<Button onClick={() => doSomething(id)} />
```

**Conditional Rendering:**
- Use ternary for simple conditions
- Use `&&` for conditional rendering (but watch for falsy values)
- Extract complex conditions into variables or functions

```typescript
// Good - simple ternary
{isLoading ? <Spinner /> : <Content />}

// Good - conditional with boolean
{hasOrders && <OrdersList orders={orders} />}

// Good - complex condition extracted
const shouldShowEmptyState = !isLoading && orders.length === 0;
{shouldShowEmptyState && <EmptyState />}

// Bad - nested ternaries
{isLoading ? <Spinner /> : hasOrders ? <OrdersList /> : hasError ? <Error /> : <Empty />}
```

### Async/Await Conventions

- Always use async/await over .then() chains
- Handle errors with try-catch
- Use Promise.all() for parallel requests
- Avoid awaiting in loops; use Promise.all() instead

```typescript
// Good - parallel requests
const [orders, customers, services] = await Promise.all([
  getOrders(),
  getCustomers(),
  getServices()
]);

// Bad - sequential when unnecessary
const orders = await getOrders();
const customers = await getCustomers(); // waits for orders first
const services = await getServices(); // waits for customers first

// Good - await in loop when necessary
for (const order of orders) {
  await processOrder(order); // must be sequential
}

// Bad - await in loop when parallel is possible
for (const order of orders) {
  await fetchOrderDetails(order.id); // could be parallel
}

// Good - parallel
await Promise.all(orders.map(order => fetchOrderDetails(order.id)));
```

### Constants and Magic Numbers

- Extract magic numbers into named constants
- Use UPPER_SNAKE_CASE for constants
- Group related constants in const objects or enums

```typescript
// Good
const MAX_ORDER_ITEMS = 50;
const DEFAULT_CURRENCY = 'USD';

const ORDER_STATUS = {
  PENDING: 'pending',
  IN_PROGRESS: 'in_progress',
  COMPLETED: 'completed',
  CANCELLED: 'cancelled'
} as const;

// Bad
if (items.length > 50) {} // what is 50?
if (status === 'pending') {} // string literal
```

### Forms and Validation

- Use React Hook Form for all forms
- Integrate with Zod for validation
- Keep form logic in the component or extract to custom hook
- Show validation errors inline near inputs

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { orderSchema } from '@/features/orders/schemas/order.schema';

function CreateOrderForm() {
  const { register, handleSubmit, formState: { errors } } = useForm({
    resolver: zodResolver(orderSchema)
  });

  const onSubmit = async (data: OrderFormData) => {
    try {
      await createOrder(data);
      toast.success('Order created');
    } catch (error) {
      toast.error('Failed to create order');
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Input {...register('customerName')} />
      {errors.customerName && <span>{errors.customerName.message}</span>}
    </form>
  );
}
```

### Testing Considerations

Although tests are not yet configured, when writing code:
- Write testable code (pure functions, separated concerns)
- Avoid tight coupling between components
- Keep business logic separate from UI logic
- Use dependency injection for easier mocking
