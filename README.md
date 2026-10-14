# Voltaje Moda - Backend API

API REST transaccional desarrollada en **ASP.NET Core (.NET 10)** con C#. Este proyecto funciona exclusivamente como el **Backend** del e-commerce Voltaje Moda, proporcionando todos los endpoints y la lógica de negocio para la gestión de productos, carritos, pagos, cupones y administración logística (OMS).

## Alcance del Proyecto

La API cubre los siguientes dominios de negocio:

- **Catálogo e Inventario:** Endpoints para consultar productos, variantes (tallas/colores) y disponibilidad de stock en tiempo real.
- **Gestión Transaccional (Carrito y Checkout):** Persistencia de carrito de compras, validación de inventario en el milisegundo de compra y ejecución de transacciones SQL (Rollback en caso de fallo). Generación de pedidos inmutables.
- **Motor de Marketing:** CRUD para la creación de cupones promocionales con validación de fechas, cálculo de porcentajes y aplicación de descuentos directos en el checkout.
- **Gestión de Pedidos Logística (OMS):** Bandeja protegida para administradores y empleados que permite filtrar órdenes por estado, fechas y montos. Incluye máquina de estados para el seguimiento del pedido.
- **Auditoría y Kardex:** Reversión automática de inventario (las prendas canceladas vuelven al stock) y registro histórico (Kardex) de quién y por qué se alteró el inventario.

## Stack Tecnológico

- **Framework:** ASP.NET Core 10 (Web API)
- **ORM:** Entity Framework Core 10
- **Base de Datos:** SQL Server 
- **Autenticación:** JSON Web Tokens (JWT) con políticas basadas en roles (`Cliente`, `Empleado`, `Administrador`).

## Estructura del Proyecto

El código está organizado siguiendo un patrón enfocado en APIs y controladores ligeros:

- **`Controllers/`**: Controladores que exponen los endpoints REST (ej. `PedidosController`, `CuponesController`, `AdminPedidosController`).
- **`DTOs/`**: Objetos de Transferencia de Datos divididos por dominio (Marketing, Transaccional) para evitar exponer los modelos directamente a las peticiones HTTP.
- **`Middlewares/`**: Interceptores de la tubería HTTP para el manejo de lógica transversal, seguridad o manejo de errores.
- **`Models/`**: Entidades de base de datos (`Pedido`, `Producto`, `Cupones`, etc.) y el contexto principal de configuración de Entity Framework (`VoltajeDbContext.cs`).
- **`Services/`**: Clases de servicios para encapsular lógica de negocio compleja o integraciones.

## Requisitos Previos

- [.NET SDK 10]
- **Base de Datos:** SQL Server (Express, Developer o en contenedor Docker).
- **Herramientas de desarrollo:** Visual Studio 2022 (v17.12+) o VS Code con la extensión C# Dev Kit.

## Cómo levantar el proyecto localmente

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/dilanste11/voltajemoda_backend.git
   cd voltajemoda_backend
   ```

2. **Configurar la base de datos:**
   - Asegúrate de tener SQL Server ejecutándose localmente.
   - Ajusta la cadena de conexión (`ConnectionStrings:DefaultConnection`) dentro del archivo `appsettings.Development.json` o `appsettings.json` según tu instancia de SQL Server.

3. **Restaurar y compilar:**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Ejecutar la API:**
   ```bash
   dotnet run
   ```
   *La API se levantará y podrás explorar todos los endpoints a través de la interfaz de Swagger visitando la URL local que genere la terminal.*

## Convenciones y Reglas de Negocio

- **Inmutabilidad de Pedidos:** Una vez que se ejecuta el checkout, la dirección y ciudad del cliente se guardan como texto fijo en la tabla `Pedidos`. Esto evita que ediciones futuras en la libreta de direcciones del usuario alteren una factura ya emitida.
- **Borrado Lógico (Soft Delete):** Elementos clave como las direcciones de envío o los cupones promocionales utilizan una bandera booleana (`Activo = false`) y no se eliminan de la base de datos para preservar la integridad transaccional e histórica.
- **Transacciones Seguras:** El flujo de checkout utiliza `BeginTransactionAsync()`. Si ocurre algún fallo (ej. otro cliente compró la última unidad un segundo antes), la transacción hace rollback automático sin generar datos huérfanos.
