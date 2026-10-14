# Voltaje Moda

Ecommerce de ropa construido en **Blazor Web App** (.NET 10), full-stack en C#: un mismo proyecto sirve tanto la lógica de servidor como la interfaz. No hay una separación de repos/deploys entre "frontend" y "backend" — es un solo codebase, y se espera que **todo el equipo trabaje en ambas partes** según lo que necesite la funcionalidad que esté construyendo.

## Alcance actual del proyecto

Funcionalidades ya implementadas y operativas de punta a punta:

- **Catálogo**: productos con variantes (talla/color) y stock por variante, categorías, filtro por categoría, productos en oferta (precio original + precio de oferta).
- **Cuentas**: registro/login (ASP.NET Core Identity), roles `Cliente` y `Administrador`, perfil editable, gestión de direcciones de envío. Registro con nombre, teléfono, correo, contraseña y dirección de envío en un solo paso, más aceptación de términos y condiciones. Login también disponible con **Google** (vincula automáticamente si ya existe una cuenta local con el mismo correo). Interfaz de login/registro con diseño propio (`auth-page`/`auth-card` en `wwwroot/app.css`).
- **Gestión Transaccional (API)**: Carrito de compras persistente con validación de inventario en tiempo real. Checkout protegido por transacciones SQL que descuenta stock, aplica matemática de cupones y registra historial de Kardex automáticamente.
- **Motor de Marketing**: CRUD administrativo para creación de cupones promocionales con fechas de caducidad y endpoint de validación en tiempo real para el cliente.
- **Panel de administración** (`/admin`, solo rol `Administrador`): shell propio con sidebar oscura y diseño independiente del sitio público. Incluye dashboard con KPIs y gráficos (ventas, top productos), gestión del banner/carrusel del inicio (`/admin/banner`, imagen por URL o subida desde el equipo, con enlace opcional al hacer click). CRUD de productos (incluyendo variantes, imagen y marca de "destacado") y categorías, control de inventario con historial de movimientos de stock (`/admin/inventario`). Gestión avanzada de pedidos (OMS) con filtros de búsqueda y máquina de estados logísticos con reversión automática de inventario ante cancelaciones. Gestión de estadísticas con rango de fechas configurable (`/admin/estadisticas`) y gestión de usuarios/roles (`/admin/usuarios`). Con protecciones para no borrar en cascada datos con historial real.
- **Inicio (`/`)**: carrusel configurable desde el admin (con fallback a una imagen genérica si no hay slides activos) y sección "Ofertas destacadas" con los productos que el admin marcó como destacados (no es automático por precio/oferta).
- **Diseño base**: navbar superior, footer con contacto/redes, paleta blanco/negro aplicada globalmente en el sitio público; el panel admin tiene su propia paleta (sidebar oscura + acento coral), ver `wwwroot/admin.css`.

### Backlog / próximos pasos (no implementado todavía)

- Pasarela de pago real (hoy el checkout es simulado).
- Búsqueda de productos.
- Reemplazar los placeholders del footer (redes sociales, teléfono) por los datos reales.
- Rehacer el diseño visual del sitio público (el panel de administración ya tiene diseño propio, ver arriba).
- Pruebas automatizadas (unitarias/integración).

## Stack

- **.NET 10** (Blazor Web App, interactividad Server)
- **Entity Framework Core 10** + **SQL Server Express** en desarrollo / **Azure SQL Database** en producción
- **ASP.NET Core Identity** (registro/login, roles `Cliente` / `Administrador`), con login externo por **Google OAuth**
- **Azure Communication Services** para el envío real de emails (confirmación de cuenta, reseteo de contraseña) en producción
- **Bootstrap** (incluido por la plantilla, vendored en `wwwroot/lib/bootstrap`)
- **Chart.js**, cargado por CDN desde `Components/App.razor`. **Bootstrap Icons**, cargado por CDN solo en el panel de administración (`Components/Layout/AdminLayout.razor`). No hay pipeline de npm/build de JS en el proyecto, por eso se usan así en vez de instalarlos como paquete.

## Requisitos previos

- [.NET SDK 10] — mismo instalador en Windows, Linux y macOS.
- **Base de datos** (SQL Server): la forma de tenerla corriendo localmente cambia según tu sistema operativo — ver Base de datos según tu sistema operativo más abajo.
- **Editor + extensiones**:
  - **VS Code** (Windows, Linux, macOS):
    - [C# Dev Kit] (`ms-dotnettools.csdevkit`) — soporte de C#/Razor/Blazor y depurador.
    - [SQL Server (mssql)] (`ms-mssql.mssql`) — opcional, para conectarte y explorar la base de datos desde el propio editor.
  - **Visual Studio 2022** (Windows; también hay una vista previa para Mac, pero el equipo usa mayormente VS Code + Visual Studio en Windows): versión **17.12 o superior** (la que soporta .NET 10), con el workload **"Desarrollo web y de ASP.NET"** marcado en el instalador. El workload **"Almacenamiento y procesamiento de datos"** (SQL Server Data Tools) es opcional.
  - **Docker Desktop** (Linux: Docker Engine) — necesario únicamente si vas a correr SQL Server en contenedor (Linux y macOS, ver abajo).

### Base de datos según tu sistema operativo

- **Windows**: SQL Server Express local (instalación "Basic"), con la instancia por defecto `SQLEXPRESS`. Si tu instancia se llama distinto, ajusta la cadena de conexión en `appsettings.json`. Es la opción que usa hoy el resto del equipo.
- **Linux**: no existe SQL Server Express nativo fuera de contenedor — corré la imagen oficial de Microsoft con Docker.
- **macOS**:
  - **Intel**: la misma imagen y comando que en Linux funciona sin cambios.
  - **Apple Silicon (M1/M2/M3)**: la imagen estándar de SQL Server no tiene build ARM64 nativo. Usá **Azure SQL Edge** en su lugar (sí soporta ARM64).
- **Nota común para Linux/macOS**: quien use Docker debe ajustar la cadena de conexión en `appsettings.Development.json` (o vía `dotnet user-secrets`) para usar autenticación SQL en vez de la integrada de Windows.

## Cómo levantar el proyecto

- Trabaja siempre a partir de la rama `desarrollo`.
- `dotnet ef database update` crea la base de datos `VoltajeModaDb` y aplica todas las migraciones existentes.
- Al arrancar **en desarrollo local únicamente**, la app siembra automáticamente datos de prueba (categorías, productos con variantes, una oferta) y roles (`Cliente`, `Administrador`), incluyendo un usuario administrador de prueba:
  - **Email:** `admin@voltajemoda.com`
  - **Contraseña:** `Admin1234!`
  
  Este usuario **no existe en producción** (el seeding de datos demo y del admin de prueba está condicionado a `IsDevelopment()` en `Program.cs`). En producción, el primer administrador se asigna manualmente en la base de datos — ver la sección "Permisos" más abajo.

Si no tienes la herramienta `dotnet-ef` instalada:

```powershell
dotnet tool install --global dotnet-ef
```

## Estructura del proyecto

```
Models/           Entidades de dominio (Producto, Categoria, Carrito, Pedido, MovimientoStock, etc.) — el "backend" de datos
Data/             ApplicationDbContext, ApplicationUser, sembradores de datos
Migrations/       Migraciones de EF Core
DTOs/             Objetos de Transferencia de Datos estructurados por dominios (Consumidor, Marketing, Transaccional)
Controllers/      Endpoints de la API REST para la gestión de direcciones, carritos, pedidos y cupones.
Components/
  Pages/          Páginas públicas (Home, Productos, Carrito, Checkout, MisPedidos...)
  Pages/Admin/    Panel de administración (protegido por rol "Administrador"): dashboard, productos, categorías, inventario, pedidos, estadísticas, usuarios
  Account/        Login/Registro/gestión de cuenta (scaffolding de Identity)
  Layout/         MainLayout, NavMenu, Footer, AdminLayout
  Shared/         Componentes reutilizables sin ruta propia (ej. ProductoCard)
```

Además del sitio Blazor, el proyecto expone una **API REST** bajo `/api/...` pensada para consumidores y transacciones directas, integrando validación mediante JWT y políticas de autorización por roles.

## Convenciones a tener en cuenta

- **Soft Delete (Borrado Lógico):** Las direcciones de envío y los cupones promocionales no se eliminan físicamente de la base de datos para no romper la integridad histórica. Se utiliza una bandera booleana `Activo = false`.
- **Inmutabilidad Transaccional:** Al realizar un pedido, los datos críticos como la dirección de envío, ciudad y precio unitario se congelan como texto plano en las tablas transaccionales, evitando que futuras ediciones en el catálogo o en la libreta del cliente alteren la factura.
- **Manejo de Transacciones SQL:** Todo el proceso de generación de pedidos (creación de orden, detalles, descuento de stock y registro en Kardex) se ejecuta bajo un `BeginTransactionAsync`. Si el stock se agota a mitad de la compra, toda la operación hace rollback.
- **Todo el texto visible al usuario debe estar en español** — títulos de página, botones, labels, placeholders, mensajes de error/éxito, todo.
- **Cuidado con `ON DELETE CASCADE`:** borrar una `Categoria` con productos, o un `Producto`/`ProductoVariante` con pedidos asociados, borraría en cascada historial real. 
- **Precio de un producto:** usa siempre `producto.PrecioEfectivo` (no `producto.Precio` directamente) para cualquier cálculo de dinero real (carrito, checkout, totales) — respeta automáticamente si el producto está en oferta.
- **Colores:** paleta blanco/negro aplicada globalmente en `wwwroot/app.css` (sobreescribe `.btn-primary`, `.text-primary`, `.bg-primary` de Bootstrap). Usa clases `btn-dark` / `btn-outline-dark` en vistas nuevas en vez de `btn-primary`, para consistencia visual.

## Autenticación

- **Registro por correo**: pide nombre completo, correo, teléfono, contraseña y una dirección de envío en el mismo formulario, además de aceptar los Términos y Condiciones. Requiere confirmar el correo antes de poder iniciar sesión (`RequireConfirmedAccount = true`).
- **Login con Google**: opcional — solo aparece si están configuradas las credenciales. Si el correo de la cuenta de Google ya tiene una cuenta local registrada por contraseña, se **vincula automáticamente** a esa cuenta en vez de fallar por correo duplicado (`Components/Account/Pages/ExternalLogin.razor`). 
- **Política de contraseña**: mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo. Bloqueo de cuenta tras 5 intentos fallidos por 10 minutos.
- **Rate limiting**: máximo 10 solicitudes por minuto por IP en cualquier ruta `/Account/*`, para mitigar fuerza bruta/credential stuffing.

## Despliegue

La app corre en producción sobre **Azure App Service** (Linux, .NET 10) + **Azure SQL Database**, con **Azure Communication Services** para el envío real de emails.

- El despliegue es **automático**: cada push a `main` dispara un workflow de GitHub Actions (`.github/workflows/main_voltajemoda.yml`) que compila y publica la app al App Service llamado `voltajemoda`.
- **Migraciones en producción**: el workflow de GitHub Actions hoy solo compila y publica — no corre `dotnet ef database update` contra la base de producción. 
- Las credenciales (connection string de SQL, connection string de Azure Communication Services) se configuran como variables de entorno/cadenas de conexión directamente en el App Service — **nunca** en `appsettings.json` ni en el repo.

## Permisos

- La app tiene dos roles de Identity: `Cliente` y `Administrador` (sembrados en `Data/IdentitySeeder.cs`). El panel `/admin` completo exige `Administrador`.
- **Primer administrador en producción**: siempre requiere un paso manual — registrate normalmente en el sitio y asignate el rol `Administrador` por SQL directo contra la base de producción. 
- **A partir de ahí**, otorgar o quitar el rol `Administrador` a otras cuentas se hace desde el propio panel, en **`/admin/usuarios`**.
