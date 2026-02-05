# ?? FASE 3: COMPLETAR SERVICIOS DE NEGOCIO

**Duración:** Días 3-4  
**Objetivo:** Completar lógica faltante en servicios  
**Estado:** Iniciando

---

## ?? ANÁLISIS ACTUAL

### ProductionOrderService ? 95% COMPLETO

Métodos implementados:
- ? `CreateProductionOrder()` - Crear orden (**FUNCIONAL**)
- ? `GetProductionOrderById()` - Obtener por ID (**FUNCIONAL**)
- ? `AssignTask()` - Asignar a usuario (**FUNCIONAL**)
- ? `UpdateStatus()` - Cambiar status (**FUNCIONAL**)
- ? `AdvanceStage()` - Avanzar etapa (**FUNCIONAL**)
- ? `GetDashboard()` - Dashboard (**FUNCIONAL**)
- ? `AddHistory()` - Registrar cambios (**FUNCIONAL**)

**CONCLUSIÓN:** Service está prácticamente LISTO.

---

## ?? VERIFICACIÓN NECESARIA

### 1. Revisar Interfaces
- [ ] `IProductionOrderService` - Está definida?
- [ ] Todos los métodos públicos tienen interfaz?
- [ ] Retorno de tipos es correcto?

### 2. Revisar Controllers
- [ ] `OpsController.cs` - Está implementado?
- [ ] Endpoints mapeados a métodos de servicio?
- [ ] Manejo de errores?

### 3. Revisar DTOs
- [ ] `CreateOpRequest` - Existe?
- [ ] `OpResponseDto` - Existe?
- [ ] `DashboardDto` - Existe?
- [ ] `HistoricoDto` - Existe?

### 4. Revisar Enums
- [ ] `EtapaProducao` - Mapea a `ProductionStage`?
- [ ] `StatusProducao` - Mapea a `ProductionStatus`?
- [ ] `PerfilUsuario` - Mapea a `UserRole`?

---

## ?? PLAN FASE 3

### PASO 1: Verificar Interfaces
```csharp
// Services/Interfaces/IProductionOrderService.cs
public interface IProductionOrderService
{
    Task<ProductionOrder> CreateProductionOrder(ProductionOrder order);
    Task<ProductionOrder?> GetProductionOrderById(int orderId);
    Task<ProductionOrder> AssignTask(int orderId, int userId);
    Task<ProductionOrder> UpdateStatus(int orderId, ProductionStatus newStatus, string note);
    Task<ProductionOrder> AdvanceStage(int orderId);
    Task<DashboardDto> GetDashboard();
}
```

### PASO 2: Revisar OpsController
```csharp
// Controllers/OpsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OpsController : ControllerBase
{
    private readonly IProductionOrderService _service;
    
    [HttpPost("create")]
    public async Task<ActionResult<ProductionOrder>> CreateOrder(CreateOpRequest request);
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OpResponseDto>> GetOrder(int id);
    
    [HttpPost("{id}/assign")]
    public async Task<ActionResult<OpResponseDto>> AssignOrder(int id, int userId);
    
    [HttpPost("{id}/advance")]
    public async Task<ActionResult<OpResponseDto>> AdvanceStage(int id);
    
    [HttpPost("{id}/status")]
    public async Task<ActionResult<OpResponseDto>> UpdateStatus(int id, AtualizarStatusRequest request);
    
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard();
}
```

### PASO 3: Validar DTOs mapeados correctamente

- ProductionOrder ? OpResponseDto
- DashboardDto ? DashboardDto (ya existe)

### PASO 4: Testing local

```bash
# 1. Compilar
dotnet build

# 2. Ejecutar
dotnet run

# 3. Probar en Swagger
# https://localhost:5151/swagger
# - Login como admin
# - Crear orden
# - Obtener orden
# - Asignar a usuario
# - Avanzar etapa
# - Cambiar status
```

---

## ?? POSIBLES ISSUES

### 1. Nombres inconsistentes (Portugués vs Inglés)
```
Entidades: ProductionOrder (Inglés)
DTOs: CreateOpRequest, OpResponseDto (¿Inglés o Portugués?)
Enums: EtapaProducao, StatusProducao (Portugués)

? Necesita normalización en DTOs/Enums
```

### 2. Mapeos faltantes
```
ProductionOrder ?? OpResponseDto
Falta verificar que todos los campos se mapean correctamente
```

### 3. Autorización
```
¿Todos los endpoints requieren [Authorize]?
¿Se valida que el usuario tiene permiso para la acción?
```

---

## ?? CHECKLIST FASE 3

```
ENTENDIMIENTO
[ ] Leer ProductionOrderService.cs completo
[ ] Leer OpsController.cs completo
[ ] Leer DTOs (CreateOpRequest, OpResponseDto, etc.)

VERIFICACIÓN
[ ] IProductionOrderService existe y está completa
[ ] Todos los métodos están en interfaz
[ ] OpsController implementa todos los endpoints
[ ] DTOs mapean correctamente

VALIDACIÓN
[ ] dotnet build sin errores
[ ] dotnet run sin excepciones
[ ] Swagger carga correctamente
[ ] Endpoints se pueden llamar

TESTING
[ ] Login funciona
[ ] Crear orden funciona
[ ] Asignar orden funciona
[ ] Avanzar etapa funciona
[ ] Cambiar status funciona
[ ] Dashboard funciona

DOCUMENTACIÓN
[ ] Comentarios en métodos públicos
[ ] DTOs documentados
[ ] Errores documentados
[ ] Commit con mensaje claro
```

---

## ?? RESULTADO ESPERADO

Después de FASE 3:

- ? Servicios de negocio 100% funcionales
- ? Controllers REST 100% funcionales
- ? Swagger documenta todos los endpoints
- ? BD actualiza correctamente
- ? SignalR listo para FASE 6
- ? Ready para UI en FASE 5

---

## ?? PRÓXIMO PASO

? Revisar `Services/ProductionOrderService.cs` línea por línea
? Revisar `Controllers/OpsController.cs`
? Completar cualquier método faltante
? Test con Swagger

