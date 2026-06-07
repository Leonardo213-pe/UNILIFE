# UniLife

Sistema web desarrollado con ASP.NET Core MVC para la gestión de la vida universitaria: cursos, eventos, lugares y perfil de usuario — con API REST y documentación Swagger integrada.

---

## Información del proyecto

**URL del sistema**
https://unilife-shum.onrender.com/

**Repositorio**
https://github.com/Leonardo213-pe/UNILIFE.git

---

## Tecnologías

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core + SQLite
- ASP.NET Identity (autenticación y roles)
- Razor Views
- Bootstrap 5 + Bootstrap Icons + CSS personalizado (glassmorphism)
- JWT Bearer Authentication (API REST)
- Swagger / OpenAPI (Swashbuckle)
- CORS habilitado para consumidores externos
- Redis (Render KeyValue) — producción
- Docker + Render

---

## Roles del sistema

| Rol | Descripción |
|-----|-------------|
| **Coordinador** | Acceso total: gestiona cursos, eventos, lugares y usuarios |
| **Docente** | Ve sus propios cursos, crea módulos y actividades |
| **Alumno** | Ve cursos en los que está inscrito y eventos de su carrera |

---

## Funcionalidades

### Autenticación y seguridad
- Inicio de sesión con cookie (MVC) y JWT Bearer (API)
- Bloqueo de cuenta tras 5 intentos fallidos (10 minutos)
- Validación de contraseña actual antes de cambiarla
- Sesión invalidada automáticamente al reiniciar la aplicación (security stamp)
- Archivos de cursos almacenados fuera de wwwroot, servidos solo a usuarios autenticados
- Validación de tipo y tamaño de archivos subidos (máx. 20 MB)

### Coordinador
- CRUD completo de cursos, eventos y lugares
- Gestión de usuarios (crear, editar, eliminar, asignar roles)
- Dashboard con estadísticas generales
- Acceso a todos los cursos y eventos

### Docente
- Dashboard con sus cursos asignados
- Creación de módulos y actividades dentro de cada curso
- Subida de archivos por módulo (PDF, DOCX, XLSX, imágenes, videos)
- Vista de alumnos inscritos por curso

### Alumno
- Dashboard con cursos inscritos y próximos eventos de su carrera
- Acceso a módulos y actividades de sus cursos
- Eventos filtrados: generales + los de su carrera específica
- Gestión de tareas personales

### Perfil de usuario (todos los roles)
- Edición de nombre, apellido y email
- Cambio de contraseña (requiere contraseña actual)
- Foto de perfil (subida de imagen)
- Toast de confirmación al guardar cambios

### Diseño e interfaz
- Tema claro / oscuro con persistencia en localStorage
- Sidebar con Bootstrap Icons
- Tarjetas con efecto glassmorphism
- Notificaciones badge en la barra lateral
- Sistema de toasts globales para feedback de acciones
- Grid de módulos responsive con imágenes por tipo de actividad
- Tarjetas de cursos clickeables (card completa lleva al detalle)

---

## API REST

La API usa autenticación JWT Bearer. Documentación interactiva disponible en `/swagger`.

### Autenticación

| Método | Ruta | Acceso | Descripción |
|--------|------|--------|-------------|
| POST | `/api/auth/login` | Público | Obtiene token JWT |
| GET | `/api/auth/me` | Bearer | Datos del usuario autenticado |

### Usuarios

| Método | Ruta | Acceso | Descripción |
|--------|------|--------|-------------|
| GET | `/api/usuarios` | Coordinador | Lista todos los usuarios (filtro `?rol=`) |
| GET | `/api/usuarios/me` | Bearer | Perfil del usuario autenticado |
| PUT | `/api/usuarios/me` | Bearer | Actualiza nombre, apellido y email |

### Cursos

| Método | Ruta | Acceso | Descripción |
|--------|------|--------|-------------|
| GET | `/api/cursos` | Bearer | Cursos filtrados por rol (Alumno: inscritos; Docente: propios; Coordinador: todos) |
| GET | `/api/cursos/{id}` | Bearer | Detalle de curso con módulos |
| POST | `/api/cursos` | Coordinador | Crear curso |
| PUT | `/api/cursos/{id}` | Coordinador | Editar curso |
| DELETE | `/api/cursos/{id}` | Coordinador | Eliminar curso |
| GET | `/api/cursos/{id}/modulos` | Bearer | Módulos con actividades |

### Eventos

| Método | Ruta | Acceso | Descripción |
|--------|------|--------|-------------|
| GET | `/api/eventos` | Bearer | Eventos filtrados por rol y carrera (`?soloProximos=true`) |
| GET | `/api/eventos/{id}` | Bearer | Detalle de evento |
| POST | `/api/eventos` | Coordinador | Crear evento |
| PUT | `/api/eventos/{id}` | Coordinador | Editar evento |
| DELETE | `/api/eventos/{id}` | Coordinador | Eliminar evento |

### Cómo usar la API

1. Hacer POST a `/api/auth/login` con email y password
2. Copiar el `token` de la respuesta
3. En Swagger: botón **Authorize** → ingresar `Bearer <token>`
4. Todos los endpoints protegidos quedan desbloqueados

---

## Credenciales de prueba

| Rol | Email | Contraseña |
|-----|-------|------------|
| Coordinador | coordinador@unilife.com | Admin123* |
| Docente | juan.perez@unilife.com | Docente123* |
| Alumno (Sistemas) | maria.garcia@unilife.com | Alumno123* |
| Alumno (Derecho) | diego.flores@unilife.com | Alumno123* |

---

## Arquitectura

El sistema sigue el patrón MVC + API REST en el mismo proyecto:

```
Controllers/
  ├── MVC (cookie auth): AccountController, HomeController, CursosController, EventosController, LugaresController, CoordinadorController
  └── Api/ (JWT auth): AuthApiController, CursosApiController, EventosApiController, UsuariosApiController

Models/
  ├── Entidades: Curso, Evento, Lugar, Modulo, HorarioCurso, CursoAlumno, CarreraRegistrada
  ├── ViewModels: DashboardViewModel, CursoFormViewModel, EditarPerfilViewModel, ...
  └── Api/: ApiDtos (records para request/response)

Views/  (Razor)
ViewComponents/  (NotifBadgeViewComponent)
Data/  (ApplicationDbContext, SeedData)
Migrations/
wwwroot/  (CSS, JS, imágenes públicas)
PrivateUploads/  (archivos de módulos — fuera de wwwroot)
```

---

## Variables de entorno (Render)

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__DefaultConnection=Data Source=app.db
Redis__ConnectionString=red-d719v6t7vvec73e4c7ig:6379
Jwt__Key=<clave-secreta-produccion>
Jwt__Issuer=UniLife
Jwt__Audience=UniLife-API
Jwt__ExpiresHours=8
```

---

## Pruebas realizadas

- Autenticación por cookie (MVC) y JWT Bearer (API)
- Control de acceso por roles en MVC y API
- Bloqueo por intentos fallidos de login
- Invalidación de sesión al reiniciar la app
- CRUD completo de cursos, eventos y lugares
- Creación de módulos y actividades con subida de archivos
- Filtrado de eventos por carrera del alumno
- Vista de cursos inscritos para alumnos
- Foto de perfil y edición de datos
- Tema oscuro/claro
- Swagger con autenticación Bearer funcional
- Despliegue en producción (Render)
