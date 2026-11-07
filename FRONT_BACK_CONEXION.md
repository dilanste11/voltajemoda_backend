# VoltajeModa - Conexión Front ↔ Back

## 1) Arquitectura actual

- Frontend: `voltajemoda/` (Blazor Server)
- Backend: `voltajemoda_backend/` (ASP.NET Core Web API REST)
- Base de datos: SQL Server local, administrada por el backend.
- El frontend ya no debe tocar Entity Framework ni la base de datos directa.

## 2) URLs y puertos

- Backend: `http://localhost:5181/api/`
- Frontend: usa `ApiSettings:BaseUrl` en `voltajemoda/appsettings.json`
- El backend en `voltajemoda_backend/Properties/launchSettings.json` también usa `http://localhost:5181`

## 3) Autenticación

El backend autenticación con JWT usando `Jwt` en `appsettings.json` y `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)`.

Los endpoints protegidos esperan el token en el header Authorization:

```http
Authorization: Bearer <token>
```

Roles reconocidos:

- Cliente
- Empleado
- Administrador

## 4) Configuración del cliente HTTP en el frontend

La configuración actual del frontend es:

```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5181/api/"
}
```

Y se registra así:

```csharp
builder.Services.AddHttpClient<IProductoApiClient, ProductoApiClient>(client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
});
```

## 5) Contrato JSON verificado para catálogo

La respuesta del backend para `GET /api/Productos` coincide con `voltajemoda/HttpServices/Dtos/CatalogoDtos.cs`:

```json
{
  "productos": [
    {
      "productoId": 1,
      "categoriaId": 2,
      "nombreCategoria": "Camisetas",
      "skuBase": "CAM-001",
      "nombre": "Camiseta Premium",
      "descripcion": "...",
      "precioBase": 129000,
      "activo": true
    }
  ],
  "paginaActual": 1,
  "totalPaginas": 3,
  "totalRegistros": 25
}
```

La clase `CatalogoPaginadoDto` del frontend recibe exactamente estos nombres y el `ProductoDetalleDto` también coincide con la respuesta del detalle por ID.

## 6) Endpoints ya existentes y cubiertos

### Catálogo

- `GET /api/Productos`
- `GET /api/Productos/{id}`
- `GET /api/Categorias`
- `GET /api/Variantes/producto/{productoId}`
- `GET /api/ProductoImagenes/producto/{productoId}`

### Cliente / tienda

- `POST /api/Auth/registro`
- `POST /api/Auth/login`
- `GET /api/Carrito`
- `POST /api/Carrito/items`
- `PUT /api/Carrito/items/{itemId}`
- `DELETE /api/Carrito/items/{itemId}`
- `DELETE /api/Carrito`
- `POST /api/Pedidos/checkout`
- `GET /api/Pedidos/metodos-pago`
- `GET /api/Pedidos/mis-pedidos`
- `GET /api/Pedidos/mis-pedidos/{id}`
- `GET /api/Direcciones`
- `POST /api/Direcciones`
- `PUT /api/Direcciones/{id}`
- `DELETE /api/Direcciones/{id}`
- `POST /api/Cupones/validar`
- `GET /api/Perfil`
- `PUT /api/Perfil`
- `PUT /api/Perfil/password`

### Administración / staff

- `GET /api/AdminPedidos`
- `GET /api/AdminPedidos/{id}`
- `PUT /api/AdminPedidos/{id}/estado`
- `GET /api/AuditoriaInventario`
- `GET /api/Usuarios/clientes`
- `GET /api/Usuarios/empleados`
- `PUT /api/Usuarios/clientes/{id}/estado`
- `PUT /api/Usuarios/empleados/{id}/estado`
- `PUT /api/Usuarios/{id}/rol`
- `POST /api/Usuarios/empleados/registro`
- `GET /api/AdminCupones`
- `POST /api/AdminCupones`
- `PUT /api/AdminCupones/{id}`
- `DELETE /api/AdminCupones/{id}`
- `POST /api/Variantes`
- `POST /api/Variantes/{id}/inventario`
- `POST /api/ProductoImagenes`
- `DELETE /api/ProductoImagenes/{id}`

## 7) Endpoints agregados para cubrir el front actual

Se incorporaron los siguientes endpoints para cubrir dashboards, inventario y marketing sin dependen de una base local del frontend:

- `GET /api/Dashboard/resumen`
- `GET /api/Estadisticas/resumen`
- `GET /api/Inventario/resumen`
- `GET /api/Banners`

Estos endpoints se derivan de la base real del backend usando entidades ya existentes (`Productos`, `Variantes`, `Pedidos`, `DetallesPedidos`, etc.).

## 8) Recomendación de migración del frontend

1. Mantener `ApiSettings.BaseUrl` como la única fuente de la URL del backend.
2. Reemplazar de a poco `ApplicationDbContext` por llamadas HTTP a estos endpoints.
3. Dejar la base local del frontend únicamente para autenticación o sesión si fuera necesario.
4. No mezclar `Data/`, `Migrations/` y `Services/` EF con la lógica del backend.
5. Las consumiciones del frontend deben estar centralizadas en `HttpServices/` y DTOs de `HttpServices/Dtos/`.

## 9) Estado actual

La migración parcial ya está correcta en el catálogo y el backend cubre la mayor parte del flujo de e-commerce real. El punto más crítico restante es migrar los componentes que aún usan `ApplicationDbContext` de manera directa en Blazor para que la capa UI quede 100% desacoplada del almacenamiento local.
