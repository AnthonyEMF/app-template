# Plantilla de Proyecto Web (Backend + Frontend)
Diseñada para ser clonada como base de nuevos proyectos, eliminando la configuración repetitiva desde cero.

## Tecnologías:

| Capa | Tecnología | Versión |
|---|---|---|
| **Orquestador** | .NET Aspire | 9.x |
| **Backend** | ASP.NET Core Web API | .NET 9.0 |
| **ORM** | Entity Framework Core | 9.x |
| **Base de datos** | PostgreSQL | 16 |
| **DB Admin** | pgAdmin 4 | latest |
| **Frontend** | React + TypeScript | React 18 / TS 5.x |
| **Build tool** | Vite | 5.x |
| **HTTP Client** | Axios | 1.x |
| **Server query** | TanStack Query | 5.x |
| **Web server** | Nginx | alpine |
| **Contenedores** | Docker + Docker Compose | 27.x / v2 |
| **Runtime Node** | Node.js | 20 LTS |

## Requisitos:

```bash
# .NET SDK 9  →  https://dotnet.microsoft.com/download/dotnet/9.0
dotnet --version          # >= 9.0.0

# Workload de .NET Aspire
dotnet workload install aspire
dotnet workload list      # debe aparecer "aspire"

# Node.js 20 LTS  →  https://nodejs.org
node --version            # >= 20.0.0
npm --version             # >= 10.0.0

# Docker Desktop  →  https://www.docker.com/products/docker-desktop
docker --version          # >= 27.0.0
docker compose version    # >= 2.0.0

# Entity Framework CLI (gestión de migraciones)
dotnet tool install --global dotnet-ef
dotnet ef --version       # >= 9.0.0
```

## Estructura:

```
AppTemplate/
├── src/
│   ├── AppHost/              # Orquestador .NET Aspire
│   ├── ServiceDefaults/      # Config compartida (telemetría, health checks)
│   ├── API/                  # ASP.NET Core Web API + Entity Framework
│   │   ├── Controllers/
│   │   ├── Database/         # DbContext
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Dockerfile
│   └── Web/                  # React + TypeScript (Vite)
│       ├── src/
│       ├── Dockerfile
│       └── nginx.conf
├── docker-compose.yml        
├── .env                      # Variables de entorno 
└── Application.slnx          # Solución principal
```

## Modos de ejecución:
### 1. Desarrollo con .NET Aspire:

Aspire levanta **todos los servicios automáticamente** (PostgreSQL, pgAdmin, API y React) con hot reload y telemetría integrada.

```bash
cd src/AppHost
dotnet run
```

### 2. Docker Compose (producción / staging):

```bash
# Construir imágenes e iniciar todos los servicios
docker compose up --build -d

# Verificar que todos los contenedores estén corriendo
docker compose ps

# Detener los servicios
docker compose down
```

## Migraciones:

Las migraciones se aplican **automáticamente** al iniciar la API. Los siguientes comandos son para gestión manual en desarrollo.

```bash
# Crear una nueva migración
dotnet ef migrations add NombreMigracion --project src/API

# Aplicar migraciones manualmente
dotnet ef database update --project src/API

# Revertir la última migración
dotnet ef migrations remove --project src/API

# Ver historial de migraciones
dotnet ef migrations list --project src/API
```

## Variables de Entorno:

Copia `.env.template` a `.env` y ajusta los valores.

```env
# PostgreSQL
POSTGRES_USER=admin
POSTGRES_PASSWORD=1234
POSTGRES_DB=appdb

# pgAdmin
PGADMIN_EMAIL=admin@admin.com
PGADMIN_PASSWORD=admin123

# Frontend (URL del API expuesta al navegador)
VITE_API_URL=http://localhost:5000
```

## Comandos:

### Docker Compose:

```bash
# Iniciar servicios en segundo plano
docker compose up -d

# Reconstruir imágenes (tras cambios en código)
docker compose up --build -d

# Ver logs en tiempo real (todos los servicios)
docker compose logs -f

# Ver logs de un servicio específico
docker compose logs -f api
docker compose logs -f web
docker compose logs -f postgres

# Reiniciar un servicio
docker compose restart api

# Detener y eliminar contenedores (conserva volúmenes/DB)
docker compose down

# Reset completo — elimina contenedores Y volúmenes (borra la DB)
docker compose down -v

# Estado de los contenedores
docker compose ps
```

### .NET / API:

```bash
# Ejecutar solo la API en desarrollo
cd src/API && dotnet run

# Restaurar paquetes NuGet
dotnet restore

# Compilar la solución completa
dotnet build

# Publicar la API para producción
dotnet publish src/API/API.csproj -c Release -o ./publish
```

### Node.js / Frontend:

```bash
# Instalar dependencias
cd src/Web && npm install

# Modo desarrollo (hot reload)
npm run dev

# Build de producción
npm run build

# Vista previa del build
npm run preview

# Verificar tipos TypeScript
npm run type-check
```

### Aspire:

```bash
# Instalar / actualizar workload
dotnet workload install aspire
dotnet workload update

# Iniciar el orquestador (levanta todos los servicios)
cd src/AppHost && dotnet run
```

## Notas:

- **ServiceDefaults** agrega automáticamente a todos los servicios: OpenTelemetry, health checks (`/health`, `/alive`) y service discovery entre proyectos.
- **pgAdmin** llega pre-configurado con la conexión al PostgreSQL local vía `AppHost/pgadmin/servers.json`.
- El **frontend** usa **Nginx** en Docker (producción) y **Vite dev server** con Aspire (desarrollo).
- Las **migraciones** de EF Core se aplican automáticamente en el arranque de la API.
