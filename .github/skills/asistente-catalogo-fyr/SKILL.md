---
name: asistente-catalogo-fyr
description: "Use when creating or modifying FYR catalog and product API features, including Minimal API endpoints, product DTOs, EF Core repository queries, filters, stock, offers, and HTTP examples."
argument-hint: "Describe the catalog or product API change to implement"
user-invocable: true
---

# Asistente de Catálogo FYR

## Purpose

Guide repeatable implementation of catalog features in the FYR clothing-store application. The workflow keeps API endpoints, DTOs, repositories, validation, and documentation consistent.

## When to use

- Add or modify product catalog endpoints.
- Add product search or filters.
- Change product, stock, offer, category, size, color, or gender behavior.
- Generate DTOs or EF Core repository queries for products.
- Validate a catalog API change before presenting it.

## Procedure

1. Inspect the product entity, `ProductoEndpoints`, `IProductoRepository`, `ProductoRepository`, related DTOs, and `Program.cs`.
2. Define request and response shapes before writing the query.
3. Keep HTTP binding and input validation in the endpoint module.
4. Keep filtering, projection, ordering, and pagination in the product repository.
5. Use `AsNoTracking()` for reads and return only the fields required by the existing response contract.
6. Preserve current CRUD routes and add a distinct route for new search behavior.
7. Add an example to `Codigo/FYR-API/FYR-API.http` and document the usage.
8. Build `Codigo/FYR-API/FYR-API.csproj` and report failures explicitly.

## Search implementation rules

- Text search should be case-insensitive through the database collation and cover name, brand, description, colors, sizes, and gender.
- Optional filters must be ignored when absent.
- Reject negative prices, invalid page numbers, page sizes outside `1..100`, and a minimum price greater than the maximum.
- Default catalog searches to active products; expose an explicit option for administrative searches.
- Use deterministic ordering (`Nombre`, then `Id`) before pagination.
- Return pagination metadata so clients can distinguish an empty page from an invalid request.

## Expected output

Report:

- The endpoint and query parameters.
- Files changed.
- Validation and build result.
- Any assumptions or limitations.
