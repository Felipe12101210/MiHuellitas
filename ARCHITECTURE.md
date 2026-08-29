# Arquitectura de MiHuellitas

## Estructura de proyectos

- `MiHuellitas.shared`: modelo de dominio, enums, mock data y servicios compartidos para la capa de negocio y una base común para la web y la app.
- `MiHuellitas.web`: aplicación Blazor para la versión web con páginas, layout, navegación y UI.
- `MiHuellitas.app`: proyecto MAUI preparado para reutilizar la lógica y los modelos definidos en Shared.

## Responsabilidad de cada capa

### Shared
- Modelos reutilizables del dominio: `Pet`, `Foundation`, `Campaign`, `NotificationItem`, `UserProfile`.
- Enums compartidos para estados y tipos de listados.
- Datos mock iniciales para permitir el desarrollo sin backend real.
- `MockCatalogService` como fuente de acceso centralizada a la información visual.

### Web
- Páginas para adopción, perdidos, encontrados, fundaciones, campañas, mapa, notificaciones y perfil.
- Layout principal con navegación global y footer.
- Estilos centralizados en `wwwroot/app.css`.
- Inyección de dependencias para consumir datos desde Shared.

### App
- Se mantiene como implementación nativa MAUI con referencia al proyecto Shared.
- Debe adaptarse a la funcionalidad ya validada en la web y reutilizar modelos y datos cuando sea posible.

## Flujo de datos

La web consume datos mock desde `MiHuellitas.shared` a través de `MockCatalogService`. Esto evita acoplar componentes a listas hardcodeadas y prepara la app para introducir un backend o API más adelante sin romper la UI.

## Navegación

La navegación principal de la web está definida en `Components/Layout/NavMenu.razor` y conecta con páginas dedicadas por sección: inicio, adopciones, perdidos, encontrados, fundaciones, campañas, mapa, notificaciones y perfil.

## Relación Web-App

La idea central es que Shared sea la base común, Web sea la primera implementación completa y App siga el mismo modelo funcional, reutilizando la mayor cantidad posible de modelos y servicios.
