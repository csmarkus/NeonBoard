# NeonBoard UI - Angular Frontend Instructions

This is the Angular 18+ frontend for NeonBoard. It is served from the .NET API in production (via Docker) and from the Vite dev server in local development (orchestrated by .NET Aspire).

## Project Structure

```
src/app/
├── core/          # Singleton services and guards
├── features/      # Lazy-loaded feature areas
│   ├── projects/          # Project list and management
│   └── project-detail/    # Board view, card management, settings
├── layout/        # Global layout components (sidebar, user menu)
└── shared/        # Reusable components (button, input, modal, drawer, etc.)
```

## TypeScript Best Practices

- Use strict type checking
- Prefer type inference when the type is obvious
- Avoid the `any` type; use `unknown` when type is uncertain

## Angular Best Practices

- Always use standalone components over NgModules
- Must NOT set `standalone: true` inside Angular decorators - it's the default in Angular v20+
- Use signals for state management
- Implement lazy loading for feature routes
- Do NOT use the `@HostBinding` and `@HostListener` decorators - put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead
- Use `NgOptimizedImage` for all static images
  - `NgOptimizedImage` does not work for inline base64 images

## Accessibility Requirements

- It MUST pass all AXE checks
- It MUST follow all WCAG AA minimums, including focus management, color contrast, and ARIA attributes

## Components

- Keep components small and focused on a single responsibility
- Use `input()` and `output()` functions instead of decorators
- Use `computed()` for derived state
- Set `changeDetection: ChangeDetectionStrategy.OnPush` in `@Component` decorator
- Prefer inline templates for small components
- Prefer Reactive forms instead of Template-driven ones
- Do NOT use `ngClass`, use `class` bindings instead
- Do NOT use `ngStyle`, use `style` bindings instead
- When using external templates/styles, use paths relative to the component TS file

## State Management

- Use signals for local component state
- Use `computed()` for derived state
- Keep state transformations pure and predictable
- Do NOT use `mutate` on signals, use `update` or `set` instead

## Templates

- Keep templates simple and avoid complex logic
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use the async pipe to handle observables
- Do not assume globals like (`new Date()`) are available
- Do not write arrow functions in templates (they are not supported)

## Services

- Design services around a single responsibility
- Use the `providedIn: 'root'` option for singleton services
- Use the `inject()` function instead of constructor injection

## Tailwind CSS Conventions

### Typography

- **Never use arbitrary pixel values for font sizes** like `text-[14px]` or `text-[16px]`
- Always use Tailwind's built-in text size utilities which use rem for better accessibility and scaling:
  - `text-xs` (0.75rem / 12px)
  - `text-sm` (0.875rem / 14px)
  - `text-base` (1rem / 16px)
  - `text-lg` (1.125rem / 18px)
  - `text-xl` (1.25rem / 20px)
  - `text-2xl` through `text-9xl` for larger sizes
- If a design requires a non-standard size, extend the theme in `tailwind.config.js` rather than using arbitrary values

### General Tailwind Rules

- Prefer semantic utility classes over arbitrary values `[]`
- Arbitrary values are a code smell - if you need them frequently, extend the theme config instead
- Use Tailwind's spacing scale (`p-4`, `m-2`, `gap-3`) rather than arbitrary pixel values

## Important Don'ts

❌ **Don't use localStorage/sessionStorage** - Won't work when the app is served from .NET in production
❌ **Don't use `ngClass` or `ngStyle`** - Use `class` and `style` bindings instead
❌ **Don't use `*ngIf`, `*ngFor`, `*ngSwitch`** - Use native control flow (`@if`, `@for`, `@switch`)
❌ **Don't use constructor injection** - Use `inject()` function instead
❌ **Don't use `@HostBinding` / `@HostListener`** - Use the `host` object in the decorator
❌ **Don't set `standalone: true`** - It's the default and the flag is redundant
❌ **Don't use arbitrary Tailwind values** - Extend the theme config if needed
❌ **Don't write arrow functions in templates** - They are not supported
❌ **Don't use `mutate` on signals** - Use `update` or `set` instead

## Running the UI

In local development, the UI is started automatically by .NET Aspire:

```bash
dotnet run --project src/NeonBoard.AppHost
```

The Vite dev server will be available with hot module replacement. The Angular app proxies API calls to the .NET backend.
