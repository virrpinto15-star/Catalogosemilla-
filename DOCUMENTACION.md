# 🌱 Semilla de Vida - Documentación del Proyecto

Bienvenido/a a la documentación técnica y funcional de **Semilla de Vida**, una plataforma web de catálogo y comercio electrónico para dietética y tienda de productos naturales, desarrollada sobre **ASP.NET Core MVC** (.NET 10).

---

## 📌 1. Resumen General del Proyecto

- **Nombre de Marca:** Semilla de Vida (anteriormente CatalogoPapa).
- **Enfoque de Negocio:** Tienda de alimentos naturales, frutos secos, yerbas, condimentos, frascos, snacks, granolas y frutas deshidratadas.
- **Modelo de Ventas:** Catálogo web con carrito de compras en sesión y finalización de pedidos directa vía **WhatsApp**, sin necesidad de pasarelas de pago costosas ni registros obligatorios para el cliente.
- **Stack Tecnológico:**
  - **Lenguaje y Framework:** C# con ASP.NET Core MVC (.NET 10).
  - **Persistencia de Datos:** Entity Framework Core con PostgreSQL (Npgsql / Supabase).
  - **Frontend:** Razor Pages (.cshtml), HTML5, Bootstrap 5, CSS3 personalizado y JavaScript vanilla.
  - **Manejo de Estado:** `ISession` para el carrito de compras en memoria/sesión de usuario.

---

## 🏗️ 2. Arquitectura y Estructura del Código

### 📁 Modelos (`Models/`)
1. **`Producto.cs`**:
   - `Id`: Clave primaria autoincremental.
   - `Nombre`: Nombre del producto.
   - `Descripcion`: Detalle del producto (opcional).
   - `Imagen`: Nombre del archivo de imagen almacenado en `wwwroot/images/`.
   - `Precio`: Decimal formateado a 2 decimales (`decimal(18,2)`).
   - `Stock`: Cantidad disponible en inventario.
   - `CategoriaId` y `Categoria`: Relación foránea con el modelo `Categoria`.
2. **`Categoria.cs`**:
   - `Id`: Clave primaria.
   - `Nombre`: Nombre de la categoría.
   - `Descripcion`: Breve descripción temática.
   - `Productos`: Colección de productos asociados.
3. **`CarritoItem.cs`**:
   - Objeto ligero utilizado para la sesión del carrito.
   - Contiene `ProductoId`, `Nombre`, `Precio`, `Cantidad`, `Imagen` y la propiedad calculada `Subtotal` (`Precio * Cantidad`).

---

### 🎮 Controladores (`Controllers/`)

1. **`HomeController.cs`**:
   - Inyecta `ApplicationDbContext`.
   - Carga tanto las categorías como los productos para desplegar la portada interactiva.
2. **`ProductoController.cs`**:
   - **`Index(categoriaId, buscar)`**: Catálogo público con búsqueda por texto y filtrado por categoría.
   - **`Admin()`**: Panel de control con la grilla de todos los productos, stock, precios y categorías activas.
   - **`Crear()` / `Crear(...)`**: Formulario de alta con subida de imagen a través de `IWebHostEnvironment` guardando archivos en `wwwroot/images/`.
   - **`Editar(...)`**: Modificación de datos existentes (precio, stock, categoría) con vista previa de imagen y reemplazo opcional.
   - **`Eliminar(id)`**: Borrado de producto con confirmación previa.
   - **`CrearCategoria(nombre, descripcion)`**: Endpoint para dar de alta categorías desde el modal web en tiempo de ejecución.
3. **`CarritoController.cs`**:
   - **`Index()`**: Muestra la vista del carrito.
   - **`Agregar(id)`**: Agrega un producto verificando stock disponible en base de datos.
   - **`Aumentar(id)`**: Suma 1 unidad validando el límite de stock.
   - **`Disminuir(id)`**: Resta 1 unidad (si llega a 0, lo elimina).
   - **`Eliminar(id)`**: Quita un producto específico del carrito.
   - **`Vaciar()`**: Limpia completamente la sesión del carrito en un solo clic.

---

### 🎨 Vistas y Diseño (`Views/`)

1. **`Views/Shared/_Layout.cshtml`**:
   - **Barra Superior (Top Bar):** Mensaje comercial con enlace directo a WhatsApp (`351 800-8605`) y aviso de envíos.
   - **Navegación Principal:** Enlaces a Inicio, Catálogo, botón **⚙️ Administrar** y **🛒 Carrito** con **contador dinámico en tiempo real (`badge`)**.
   - **Pie de Página Comercial (Footer):** 4 columnas responsivas con información de Semilla de Vida, enlaces rápidos, contacto/horarios y medios de pago (Efectivo, Transferencia, Mercado Pago).
   - **Botón Flotante de WhatsApp:** Acceso rápido flotante en la esquina inferior derecha con icono SVG oficial y efecto de pulso animado.
   - **Favicon Temático:** Icono `🌱` visible en la pestaña del navegador.
2. **`Views/Home/Index.cshtml`**:
   - **Banner Principal (Hero):** Degradado verde bosque con textura de dietética integrada y accesos directos.
   - **3 Pilares de Confianza:** 100% Natural, Calidad Garantizada y Atención Directa.
   - **Píldoras de Categorías Dinámicas:** Reconocen automáticamente palabras clave para mostrar emojis temáticos (🌰, 🌿, 🧂, 🫙, 🥨, 🥣, 🍓).
   - **Catálogo Integrado:** Productos visibles al desplazarse hacia abajo con botón para agregar al carrito sin salir de la página de inicio.
   - **Guía "¿Cómo comprar en 3 pasos?":** Explicación clara del circuito de compra.
   - **Preguntas Frecuentes (FAQ):** Acordeón interactivo con dudas sobre envíos, pagos y compras mínimas.
3. **`Views/Producto/Index.cshtml`**:
   - Catálogo interactivo con buscador por nombre/descripción.
   - Filtros rápidos tipo píldora por categoría.
   - Tarjetas de productos en grid responsivo con efectos hover.
   - Manejo de stock: etiquetas de stock disponible o distintivo rojo de **"Agotado"** que deshabilita la compra.
4. **`Views/Producto/Admin.cshtml`**:
   - Dashboard de gestión de inventario.
   - Botón modal **"🏷️ Nueva Categoría"** para crear categorías al instante sin tocar código.
   - Botón **"➕ Nuevo Producto"** y botones individuales de edición y eliminación.
5. **`Views/Producto/Crear.cshtml` y `Editar.cshtml`**:
   - Formularios estilizados con selector de categoría, inputs validados y vista previa de imagen en tiempo real antes de guardar.
6. **`Views/Carrito/Index.cshtml`**:
   - Vista detallada del carrito con miniaturas de fotos, controles `+` y `−`, subtotal por producto y total final.
   - Botón de **"Vaciar carrito"** y **"Seguir comprando"**.
   - **Botón "📲 Pedir por WhatsApp":** Genera un mensaje formateado con el resumen exacto de artículos, cantidades y total listo para enviar al número configurado (`3518008605`).

---

### 🎨 Estilos y Recursos (`wwwroot/`)

- **`wwwroot/css/site.css`**:
  - Paleta en gama de verdes orgánicos:
    - Bosque (`#2d6a4f`) y Bosque Oscuro (`#1b4332`).
    - Hoja (`#40916c`) y Menta (`#74c69d`).
    - Salvia y Pastel (`#d8f3dc`).
  - Fondo global temático con imagen de dietética (`fondo-dietetica.jpg`) y filtro cálido en marrones claros/lino (`rgba(244, 237, 228, 0.70)` a `rgba(236, 226, 214, 0.75)`).
  - Efectos de elevación para tarjetas (`.product-card:hover`).
  - Animación `@keyframes pulseWA` para el botón de WhatsApp.
- **`wwwroot/images/fondo-dietetica.jpg`**:
  - Fotografía en alta resolución generada especialmente con cuencos de madera, semillas orgánicas, frutos secos y hierbas aromáticas.

---

## 🏷️ 3. Categorías Actuales del Sistema

1. 🌰 **Frutos secos**
2. 🌿 **Yerbas**
3. 🧂 **Condimentos**
4. 🫙 **Frascos de vidrio**
5. 🥨 **Snacks**
6. 🥣 **Granola**
7. 🍓 **Frutas deshidratadas**

*(Cargadas automáticamente al iniciar en `Program.cs` y en `ApplicationDbContext.cs`).*

---

## 📖 4. Manual de Uso Rápido (Sin Tocar Código)

### ¿Cómo agregar un nuevo producto?
1. Entra a **"⚙️ Administrar"** en el menú superior (o navega a `/Producto/Admin`).
2. Haz clic en **"➕ Nuevo Producto"**.
3. Completa nombre, categoría, precio, stock y descripción.
4. Selecciona la foto desde tu computadora (verás la vista previa al instante).
5. Presiona **"💾 Guardar Producto"**.

### ¿Cómo modificar precios o stock?
1. Ve a **"⚙️ Administrar"**.
2. En la lista de productos, presiona **"✏️ Editar"** al lado del producto que quieras actualizar.
3. Cambia el precio o las unidades de stock.
4. Presiona **"💾 Guardar Cambios"**.

### ¿Cómo crear una nueva categoría?
1. Ve a **"⚙️ Administrar"**.
2. Presiona el botón verde claro **"🏷️ Nueva Categoría"**.
3. Escribe el nombre de la categoría (ej: *Semillas*, *Tés*, *Aceites*) y presiona **"💾 Guardar Categoría"**.
4. La categoría aparecerá de inmediato en el Inicio, en los filtros del Catálogo y en el formulario de nuevos productos.

### ¿Cómo recibe los pedidos el negocio?
1. El cliente entra, selecciona productos y va a su carrito.
2. Al presionar **"📲 Pedir por WhatsApp"**, se abre automáticamente WhatsApp con la lista de compra y el total ya calculados para enviar directo a tu número (`351 800-8605`).
3. Por ese mismo chat acuerdas la forma de pago (transferencia o efectivo) y coordinas la entrega.

---

> [!NOTE]
> Documento generado para **Semilla de Vida** - Octubre 2026. Todos los archivos se encuentran listos y funcionando en el repositorio local.

