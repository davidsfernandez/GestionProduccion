# GESTIONPRODUCCIÓN MVP_V2 - COMANDOS ÚTILES

**Última actualización:** Febrero 2026

---

## ?? SETUP Y CONFIGURACIÓN

### Clonar proyecto
```bash
git clone https://github.com/davidsfernandez/GestionProduccion.git
cd GestionProduccion
git checkout MVP_V2
```

### Restaurar dependencias
```bash
# Backend
cd GestionProduccion
dotnet restore

# Frontend
cd ../Client/GestionProduccion.Client
dotnet restore

# Volver a raíz
cd ../..
```

### Crear BD
```bash
# Crear BD desde migration
cd GestionProduccion
dotnet ef database update

# O manual en MySQL
mysql -u root -p < scripts/create_database.sql
```

---

## ?? EJECUTAR PROYECTOS

### Backend (API)
```bash
cd GestionProduccion

# Debug
dotnet run

# Con watch (auto-reload)
dotnet watch run

# URL: https://localhost:5151
# Swagger: https://localhost:5151/swagger
```

### Frontend (Blazor WASM)
```bash
cd Client/GestionProduccion.Client

# Debug
dotnet run

# Con watch
dotnet watch run

# URL: https://localhost:7120
```

### Ejecutar ambos en paralelo
```bash
# Terminal 1:
cd GestionProduccion && dotnet watch run

# Terminal 2 (en otra ventana):
cd Client/GestionProduccion.Client && dotnet watch run
```

---

## ?? MIGRACIONES DE BD (Entity Framework)

### Ver migraciones
```bash
cd GestionProduccion
dotnet ef migrations list
```

### Crear nueva migration
```bash
# Crear migration vacía
dotnet ef migrations add NormalizeDbToEnglish

# Crear con automático (detecta cambios en modelos)
dotnet ef migrations add AddNewFeatureX
```

### Ver SQL generado
```bash
dotnet ef migrations script [migration_name]
```

### Aplicar migraciones
```bash
# Aplicar todas las pendientes
dotnet ef database update

# Aplicar hasta una migration específica
dotnet ef database update [migration_name]

# Revertir a migration anterior
dotnet ef database update [previous_migration_name]
```

### Eliminar migration (antes de aplicarla)
```bash
dotnet ef migrations remove
```

### Ver SQL de cambios
```bash
dotnet ef migrations script 0 latest > migration_full.sql
```

---

## ?? TESTING

### Ejecutar todos los tests
```bash
dotnet test GestionProduccion.sln
```

### Ejecutar tests específicos
```bash
dotnet test --filter "ProductionOrderService"
```

### Con cobertura de código
```bash
dotnet test /p:CollectCoverage=true
```

### Tests con verbose
```bash
dotnet test --verbosity normal
```

---

## ?? SEGURIDAD Y CREDENCIALES

### Usuario admin por defecto (seed data)
```
Email: admin@local.host
Contraseña: admin
Rol: Administrator
```

### Cambiar JWT Secret
**En `appsettings.json`:**
```json
{
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_MAS_LARGA_AQUI_12345678",
    "Issuer": "GestionProduccionApp",
    "Audience": "ApiClient"
  }
}
```

### Conexión a BD
**En `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=GestionProduccionDB;user=root;password=TUPASS;Persist Security Info=False;Pooling=False"
  }
}
```

---

## ?? COMPILACIÓN Y BUILD

### Build para desarrollo
```bash
dotnet build GestionProduccion.sln
```

### Build para producción
```bash
dotnet publish -c Release -o ./publish
```

### Build solo el backend
```bash
cd GestionProduccion
dotnet publish -c Release -o ../publish/api
```

### Build solo el frontend
```bash
cd Client/GestionProduccion.Client
dotnet publish -c Release -o ../../publish/client
```

---

## ?? DIAGNÓSTICO Y DEBUG

### Ver errores de BD
```bash
cd GestionProduccion
dotnet ef migrations script 0 latest
```

### Limpiar cache y rebuild completo
```bash
# Windows
rmdir bin /s /q
rmdir obj /s /q
dotnet clean
dotnet build

# Unix/Linux/Mac
rm -rf bin obj
dotnet clean
dotnet build
```

### Ver puerto en uso
```bash
# Windows
netstat -ano | findstr :5151

# Linux/Mac
lsof -i :5151
```

### Matar proceso en puerto
```bash
# Windows
taskkill /PID 1234 /F

# Linux/Mac
kill -9 1234
```

---

## ?? DEBUGGING

### Breakpoints en VSCode
1. Presionar `F5` para iniciar debug
2. Hacer click en línea para poner breakpoint
3. Inspeccionar variables en panel lateral

### Visual Studio
1. Presionar `F5` para iniciar
2. Hacer click en margen izquierdo para breakpoint
3. Panel "Locals" muestra variables

### Logging
```csharp
using Microsoft.Extensions.Logging;

public class ProductionOrderService
{
    private readonly ILogger<ProductionOrderService> _logger;
    
    public ProductionOrderService(ILogger<ProductionOrderService> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId)
    {
        _logger.LogInformation($"Advancing stage for order {orderId}");
        // ... rest of code
    }
}
```

---

## ?? SWAGGER/API DOCUMENTATION

### Acceder a Swagger
1. Ejecutar backend: `dotnet run` en GestionProduccion
2. Ir a: `https://localhost:5151/swagger`
3. Expandir endpoints para ver documentación

### Autenticación en Swagger
1. Click en "Authorize" (botón arriba a la derecha)
2. Insertar JWT token: `Bearer [tu_token_aqui]`
3. Click "Authorize"
4. Ahora puedes probar endpoints autenticados

### Obtener JWT Token
```bash
# Llamada sin autenticación
curl -X POST https://localhost:5151/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local.host","password":"admin"}'

# Respuesta:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "expiresIn": 3600
# }
```

---

## ?? GIT WORKFLOW

### Crear rama para nueva feature
```bash
git checkout -b feature/nueva-funcionalidad
```

### Ver estado
```bash
git status
git log --oneline
```

### Hacer commit
```bash
git add .
git commit -m "feat: descripción del cambio"
```

### Push a remoto
```bash
git push origin feature/nueva-funcionalidad
```

### Crear Pull Request
```bash
# Desde GitHub UI o:
gh pr create --title "Descripción" --body "Detalles"
```

### Sincronizar con main
```bash
git fetch origin
git rebase origin/main
```

---

## ?? URLS Y ENDPOINTS LOCALES

### Backend API
- URL: `https://localhost:5151`
- Swagger: `https://localhost:5151/swagger`
- Health Check: `https://localhost:5151/health`

### Frontend Blazor
- URL: `https://localhost:7120`
- Hot Reload: Automático con `dotnet watch run`

### SignalR Hub
- WebSocket: `wss://localhost:5151/productionHub`
- URL amistosa desde Blazor: `/productionHub`

### Base de Datos
- Host: `localhost:3306`
- Usuario: `root`
- Password: `Cualquiera1` (por defecto en development)
- Database: `GestionProduccionDB`

---

## ?? LOGS Y MONITOREO

### Ver logs en consola
```bash
# Durante ejecución, los logs aparecen en consola
# Nivel: Information (default)

# Para más detalles:
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Archivo de logs
```csharp
// En Program.cs
builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.AddFile("logs/app-{Date}.log");
});
```

### Query logs de EF Core
```csharp
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
options.UseLoggerFactory(loggerFactory);
```

---

## ?? DOCKER (Opcional)

### Crear imagen Docker del backend
```bash
cd GestionProduccion
docker build -t gestionproduccion-api:latest .
```

### Ejecutar en contenedor
```bash
docker run -p 5151:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__Key="..." \
  gestionproduccion-api:latest
```

### Docker Compose (si existe)
```bash
docker-compose up -d
```

---

## ?? DEPLOYMENT (Producción)

### Preparar para producción
```bash
# 1. Actualizar configuración
nano appsettings.Production.json

# 2. Build release
dotnet publish -c Release -o ./publish

# 3. Ir a carpeta publicada
cd publish

# 4. Ejecutar
dotnet GestionProduccion.dll
```

### En servidor Linux
```bash
# 1. Copiar archivos publicados a servidor
scp -r publish/* user@server:/app/gestionproduccion/

# 2. En servidor:
cd /app/gestionproduccion
dotnet GestionProduccion.dll

# 3. Con supervisor (para persistencia):
# Crear archivo de configuración supervisor
```

### Configurar HTTPS en producción
```bash
# Generar certificado Let's Encrypt
certbot certonly --standalone -d tudominio.com

# En Program.cs
app.UseHttpsRedirection();
```

---

## ?? CHECKLIST PRE-DEPLOYMENT

```bash
# 1. Tests pasan
dotnet test GestionProduccion.sln

# 2. Build release sin errores
dotnet build -c Release

# 3. BD migrations aplicadas
dotnet ef database update

# 4. Logs funcionan
# (verificar archivo de logs se crea)

# 5. JWT secret único en producción
# (cambiar en appsettings.Production.json)

# 6. CORS configurado solo para dominio cliente
# (no wildcard en producción)

# 7. Variables de entorno seteadas
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionString="..."

# 8. Publicar
dotnet publish -c Release
```

---

## ?? TROUBLESHOOTING

### Error: "Cannot connect to database"
```bash
# 1. Verificar MySQL está corriendo
mysql -u root -p -e "SELECT 1;"

# 2. Verificar connection string
cat appsettings.json | grep ConnectionString

# 3. Recrear BD
dotnet ef database drop -f
dotnet ef database update
```

### Error: "Port 5151 already in use"
```bash
# Windows
netstat -ano | findstr :5151
taskkill /PID [PID] /F

# Linux/Mac
lsof -i :5151
kill -9 [PID]
```

### Error en migration: "Pending migrations"
```bash
dotnet ef migrations list
dotnet ef database update
```

### Blazor no carga
```bash
# 1. Verificar servidor backend está corriendo
# 2. Verificar CORS está habilitado
# 3. Limpiar navegador cache
Ctrl+Shift+Del en navegador

# 4. Rebuild
dotnet clean
dotnet build
```

---

## ?? TIPS Y TRICKS

### Hot reload activo
```bash
# Ambos proyectos con auto-reload
dotnet watch run
```

### Ejecutar SQL manual
```bash
mysql -u root -p GestionProduccionDB
mysql> SELECT * FROM Users;
mysql> quit;
```

### Backup de BD
```bash
mysqldump -u root -p GestionProduccionDB > backup_$(date +%Y%m%d_%H%M%S).sql
```

### Restaurar backup
```bash
mysql -u root -p GestionProduccionDB < backup_20260215_143022.sql
```

### Formato de código
```bash
# Si tienes EditorConfig
dotnet tool install -g csharpier
csharpier .
```

---

## ?? REFERENCIAS

- **Documentación .NET:** https://docs.microsoft.com/dotnet
- **Blazor:** https://blazor.net
- **EF Core:** https://docs.microsoft.com/ef/core
- **SignalR:** https://docs.microsoft.com/aspnet/signalr
- **MySQL:** https://dev.mysql.com/doc

---

**Última actualización:** Febrero 2026  
**Versión:** 1.0

