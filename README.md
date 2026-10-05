# Grupo-N-10---SistemaDeTiendaDeRopaOnline
Francisco Aguirre y Rocio Milanese

TP_GestionDeTiendaDeRopa_Aguirre_Milanese
## Descripción actual del sistema

El sistema permite gestionar de manera integral una tienda de ropa mediante una aplicación web conectada a una API. La solución administra el catálogo, los usuarios, el inventario, las ventas y los pedidos, y brinda diferentes operaciones según el rol del usuario: **Administrador**, **Empleado** o **Cliente**.

## Funcionalidades principales

- **Autenticación y cuentas:** registro e inicio de sesión, cierre de sesión, cambio de contraseña y recuperación de contraseña.
- **Gestión de productos:** alta, modificación y baja de productos, incluyendo datos de categoría, subcategoría, color, talle, precio, stock e imagen.
- **Catálogo:** consulta de productos con filtrado por categoría y búsqueda por nombre, color o talle.
- **Gestión administrativa:** administración de clientes, empleados, categorías, subcategorías, proveedores, sucursales, stock y configuración general de la tienda.
- **Ventas y pedidos:** carrito de compras persistente para clientes, checkout con datos de envío y medios de pago, registro de pedidos y consulta de su detalle y estado.
- **Operaciones de empleados:** punto de venta para registrar ventas, consulta de pedidos y acceso a facturas.
- **Administración de ventas:** consulta de pedidos y facturas, generación de reportes de ventas y seguimiento de la información comercial.
- **Comunicación:** consulta y envío de notificaciones y difusión de mensajes a los clientes.

## Características no funcionales

- Arquitectura modular con el frontend separado de la API.
- Persistencia de la información en una base de datos SQL Server mediante Entity Framework Core.
- API organizada mediante endpoints, servicios y repositorios, con documentación y exploración a través de OpenAPI/Scalar.
- Mantenimiento independiente de los componentes frontend y backend.
- Interfaz web adaptable a distintos tamaños de pantalla.
- Validaciones de datos, protección de formularios y mensajes de error para prevenir y comunicar operaciones inválidas.
- Persistencia del carrito por usuario mediante sesión y almacenamiento asociado en la API.

2025

[Documentación del proyecto V1 2025](https://docs.google.com/document/d/1Hg90QJY8MIWQN4GuAqqQPCtX6AdywDkUDn_oyyYh8wM/edit?usp=sharing)

[Diagrama](https://drive.google.com/file/d/1OfqwCS4RUwtU5fbeIk9GIqeO6L0xpjtV/view?usp=sharing)

[Conexión a la base de datos](https://docs.google.com/document/d/1AE00ClpscH9iMDRs8t36MpSUsDx8rnw8nVWZDbpD3e4/edit?usp=sharing)

[Programa](https://github.com/PapiFran00/TiendaDeRopa.git)

[Mockups](https://www.figma.com/design/3lo8xvdMKEt96XcnObwNzj/F-R?node-id=72-866&t=aRVZnNKx6dlAImhs-0)

[Diagrama de Clases](https://app.diagrams.net/?splash=0#G1R6Ce9crivHCQzwNS7HZIpege1CisbcTd#%7B%22pageId%22%3A%22xZMBi8gwqBBRkt4viOOO%22%7D)

[Caso de uso Carrito](https://docs.google.com/document/d/1l3y7ztHxBA2SKEsYM5Xys4hGtPr9Vh9hjDoVxyneNWg/edit?tab=t.0#heading=h.e9xbr04ggw3i)

[Codigo](https://github.com/roci0milanese/Filnal-Practica---F-R)

---------------------------------------------------------------------------------------------------------------------------------------
2026

[Documentación del proyecto V2 2026](https://canva.link/gupcqsc66n1xp67)

[Documentación de Endpoints de API](https://docs.google.com/document/d/1nY_3ClcNVSr5X0UoFQVAcXR8z_F3VfA1OR9ROWO5bpo/edit?usp=sharing)

[Codigo 2026 (APIS) Visual Code](https://github.com/PracticasProfesionalizantes-II-2026/Grupo-N-10---SistemaDeTiendaDeRopaOnline/tree/Rama-Prueba/Codigo)
