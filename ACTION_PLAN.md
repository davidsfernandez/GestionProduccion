# GESTIONPRODUCCIÓN MVP_V2 - PLAN DE ACCIÓN

**Versión:** 1.0  
**Fecha:** Febrero 2026  
**Responsable:** Equipo de Desarrollo  
**Estado:** Planificación en Progreso

---

## ?? CONTEXTO

Basado en la auditoría de arquitectura completada, el MVP de GestionProducción se encuentra en **estado avanzado (~70% funcional)** pero requiere:

1. **Normalización de Base de Datos** (inglés)
2. **Completar funcionalidades de negocio**
3. **Traducción de UI a portugués**
4. **Integración completa de SignalR**
5. **Testing y validación**

---

## ?? OBJETIVOS

### Objetivo Principal
Transformar el MVP actual de **70% ? 95% de funcionalidad** en **2 semanas**, con código de calidad profesional, seguridad garantizada y totalmente documentado.

### Objetivos Secundarios
- ? Código inglés consistente (backend + BD)
- ? UI en portugués (cliente lo requiere)
- ? Funcionalidad completa según requisitos
- ? Comunicación en tiempo real funcional
- ? Base de código limpia y mantenible

---

## ?? FASES DE EJECUCIÓN

### FASE 1: PREPARACIÓN (Día 1)
**Duración:** 1 día  
**Participantes:** 1 Dev Senior

#### Tareas

##### 1.1 - Backup de BD
```bash
# Exportar BD actual
mysqldump -u root -p GestionProduccionDB > backup_before_normalization.sql
```
**Responsable:** David  
**Tiempo:** 15 min  
**Entregable:** Archivo `.sql`

##### 1.2 - Crear rama Git
```bash
git checkout -b feature/db-normalization
git branch -u origin/feature/db-normalization
```
**Responsable:** David  
**Tiempo:** 5 min  
**Entregable:** Rama creada

##### 1.3 - Documentar estado actual
```bash
# Ejecutar tests actuales (si existen)
dotnet test GestionProduccion.sln

# Documentar coverage
# Tomar screenshots del estado actual
```
**Responsable:** David  
**Tiempo:** 30 min  
**Entregable:** Reporte de estado

---

### FASE 2: NORMALIZACIÓN BD (Día 2-3)
**Duración:** 2 días  
**Participantes:** 1-2 Devs

#### Tareas

##### 2.1 - Crear Migration vacía
```bash
cd GestionProduccion
dotnet ef migrations add NormalizeDbToEnglish
```
**Responsable:** David  
**Tiempo:** 10 min  
**Entregable:** Archivo de migration creado

##### 2.2 - Escribir SQL de renombramiento
**Archivos a editar:**
- `GestionProduccion/Migrations/[timestamp]_NormalizeDbToEnglish.cs`

**SQL a ejecutar:**

```sql
-- ============================================
-- NORMALIZACIÓN A INGLÉS
-- ============================================

-- 1. RENOMBRAR TABLAS
ALTER TABLE Usuarios RENAME TO Users;
ALTER TABLE OrdensProducao RENAME TO ProductionOrders;
ALTER TABLE HistoricoProducoes RENAME TO ProductionHistories;

-- 2. RENOMBRAR COLUMNAS: Users
ALTER TABLE Users CHANGE Nome Name VARCHAR(150);
ALTER TABLE Users CHANGE HashPassword PasswordHash VARCHAR(255);
ALTER TABLE Users CHANGE Perfil Role VARCHAR(50);
ALTER TABLE Users CHANGE Ativo IsActive TINYINT(1);

-- 3. RENOMBRAR COLUMNAS: ProductionOrders
ALTER TABLE ProductionOrders CHANGE CodigoUnico UniqueCode VARCHAR(50) UNIQUE;
ALTER TABLE ProductionOrders CHANGE DescricaoProduto ProductDescription VARCHAR(500);
ALTER TABLE ProductionOrders CHANGE Quantidade Quantity INT;
ALTER TABLE ProductionOrders CHANGE QuantidadeRealizada QuantityCompleted INT;
ALTER TABLE ProductionOrders CHANGE EtapaAtual CurrentStage VARCHAR(50);
ALTER TABLE ProductionOrders CHANGE StatusAtual CurrentStatus VARCHAR(50);
ALTER TABLE ProductionOrders CHANGE DataCriacao CreationDate DATETIME DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE ProductionOrders CHANGE DataEstimadaEntrega EstimatedDeliveryDate DATETIME;
ALTER TABLE ProductionOrders CHANGE DataConclusao CompletionDate DATETIME;
ALTER TABLE ProductionOrders CHANGE UsuarioId UserId INT;
ALTER TABLE ProductionOrders CHANGE ObservacaoDelegacao DelegationNote VARCHAR(500);

-- 4. RENOMBRAR COLUMNAS: ProductionHistories
ALTER TABLE ProductionHistories CHANGE OrdemProducaoId ProductionOrderId INT;
ALTER TABLE ProductionHistories CHANGE EtapaAnterior PreviousStage VARCHAR(50);
ALTER TABLE ProductionHistories CHANGE EtapaNova NewStage VARCHAR(50);
ALTER TABLE ProductionHistories CHANGE StatusAnterior PreviousStatus VARCHAR(50);
ALTER TABLE ProductionHistories CHANGE StatusNovo NewStatus VARCHAR(50);
ALTER TABLE ProductionHistories CHANGE UsuarioId UserId INT;
ALTER TABLE ProductionHistories CHANGE DataModificacao ModificationDate DATETIME DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE ProductionHistories CHANGE Observacao Note VARCHAR(500);

-- 5. RECREAR ÍNDICES Y FOREIGN KEYS (si es necesario)
-- Los indices se recrearán automáticamente
```

**Responsable:** David  
**Tiempo:** 2-3 horas (incluye testing)  
**Entregable:** Migration ejecutada, BD normalizada

##### 2.3 - Simplificar AppDbContext
**Archivos a editar:**
- `GestionProduccion/Data/AppDbContext.cs`

**Cambios:**

```csharp
// ANTES (complejo - líneas 29-53):
modelBuilder.Entity<User>().ToTable("Usuarios");
modelBuilder.Entity<User>().Property(u => u.Name).HasColumnName("Nome");
modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("HashPassword");
// ... más mapeos ...

// DESPUÉS (limpio - solo lo esencial):
// Eliminar todos los ToTable() y HasColumnName() que son iguales a convención
// Mantener solo excepciones (si las hay)

// Posiblemente:
// modelBuilder.Entity<User>().ToTable("Users");  // ya que EF Core lo haría "Users" por defecto
// Pero como tenemos override, mejor dejar explícito para claridad
```

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** AppDbContext simplificado

##### 2.4 - Testing de migration
```bash
# Crear BD nueva para testing
dotnet ef database drop -f --no-build
dotnet ef migrations script 0 latest | mysql -u root -p

# Verificar estructura
SHOW TABLES;
DESC Users;
DESC ProductionOrders;
DESC ProductionHistories;

# Verificar datos
SELECT * FROM Users;
SELECT * FROM ProductionOrders;

# Rollback test
dotnet ef database update [number_of_previous_migration]
dotnet ef database update latest
```

**Responsable:** David  
**Tiempo:** 1 hora  
**Entregable:** Tests exitosos, documentación

---

### FASE 3: COMPLETAR SERVICIOS (Día 4-5)
**Duración:** 2 días  
**Participantes:** 1-2 Devs

#### Tareas

##### 3.1 - Auditar IProductionOrderService
**Archivos a revisar:**
- `GestionProduccion/Services/IProductionOrderService.cs`
- `GestionProduccion/Services/ProductionOrderService.cs`

**Checklist:**

```csharp
public interface IProductionOrderService
{
    // ? CREATE
    Task<ProductionOrderDTO> CreateProductionOrderAsync(CreateProductionOrderDTO request, int createdByUserId);
    
    // ? READ
    Task<ProductionOrderDTO?> GetProductionOrderByIdAsync(int id);
    Task<List<ProductionOrderDTO>> ListProductionOrdersAsync(FilterProductionOrderDTO? filter);
    
    // ? ASSIGN TASK
    Task<bool> AssignTaskAsync(int orderId, int userId);
    
    // ? WORKFLOW: ADVANCE STAGE
    Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId);
    
    // ? UPDATE STATUS
    Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId);
    
    // ? DASHBOARD
    Task<DashboardDTO> GetDashboardAsync();
    
    // ? HISTORY
    Task<List<ProductionHistoryDTO>> GetHistoryByProductionOrderIdAsync(int orderId);
}
```

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** Interface completa y validada

##### 3.2 - Implementar métodos faltantes
**Métodos a completar:**

```csharp
// 1. AssignTaskAsync
public async Task<bool> AssignTaskAsync(int orderId, int userId)
{
    var order = await _context.ProductionOrders.FindAsync(orderId);
    if (order == null) return false;
    
    var user = await _context.Users.FindAsync(userId);
    if (user == null) return false;
    
    // Validar que usuario sea Tailor o Workshop
    if (user.Role != UserRole.Tailor && user.Role != UserRole.Workshop)
        return false;
    
    // Registrar cambio en historial
    await _context.ProductionHistories.AddAsync(new ProductionHistory
    {
        ProductionOrderId = orderId,
        PreviousStage = order.CurrentStage,
        NewStage = order.CurrentStage,
        PreviousStatus = order.CurrentStatus,
        NewStatus = order.CurrentStatus,
        UserId = _httpContextAccessor.HttpContext?.User.GetUserId() ?? 0,
        Note = $"Assigned to {user.Name}"
    });
    
    order.UserId = userId;
    await _context.SaveChangesAsync();
    return true;
}

// 2. AdvanceStageAsync
public async Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId)
{
    var order = await _context.ProductionOrders.FindAsync(orderId);
    if (order == null) return false;
    
    // No retroceder
    var nextStage = GetNextStage(order.CurrentStage);
    if (nextStage == null) return false; // Ya está en última etapa
    
    var previousStage = order.CurrentStage;
    
    // Cambiar etapa y resetear status
    order.CurrentStage = nextStage.Value;
    order.CurrentStatus = ProductionStatus.InProduction;
    
    // Si es la última etapa, marcar como finalizado
    if (nextStage.Value == ProductionStage.Packaging)
    {
        order.CurrentStatus = ProductionStatus.Finished;
        order.CompletionDate = DateTime.UtcNow;
    }
    
    // Registrar en historial
    await _context.ProductionHistories.AddAsync(new ProductionHistory
    {
        ProductionOrderId = orderId,
        PreviousStage = previousStage,
        NewStage = order.CurrentStage,
        PreviousStatus = ProductionStatus.InProduction,
        NewStatus = order.CurrentStatus,
        UserId = modifiedByUserId,
        Note = $"Advanced from {previousStage} to {order.CurrentStage}"
    });
    
    await _context.SaveChangesAsync();
    
    // Notificar vía SignalR
    await _productionHub.Clients.All.SendAsync("ReceiveUpdate", orderId, order.CurrentStage.ToString(), order.CurrentStatus.ToString());
    
    return true;
}

// 3. UpdateStatusAsync
public async Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId)
{
    var order = await _context.ProductionOrders.FindAsync(orderId);
    if (order == null) return false;
    
    var previousStatus = order.CurrentStatus;
    order.CurrentStatus = newStatus;
    
    await _context.ProductionHistories.AddAsync(new ProductionHistory
    {
        ProductionOrderId = orderId,
        PreviousStage = order.CurrentStage,
        NewStage = order.CurrentStage,
        PreviousStatus = previousStatus,
        NewStatus = newStatus,
        UserId = modifiedByUserId,
        Note = note
    });
    
    await _context.SaveChangesAsync();
    
    // Notificar vía SignalR
    await _productionHub.Clients.All.SendAsync("ReceiveUpdate", orderId, order.CurrentStage.ToString(), newStatus.ToString());
    
    return true;
}

// 4. GetDashboardAsync
public async Task<DashboardDTO> GetDashboardAsync()
{
    var orders = await _context.ProductionOrders
        .Include(o => o.History)
        .ToListAsync();
    
    var userWorkload = await _context.ProductionOrders
        .Where(o => o.UserId != null)
        .GroupBy(o => new { o.UserId, o.AssignedUser!.Name })
        .Select(g => new UserWorkloadDTO
        {
            UserId = g.Key.UserId ?? 0,
            UserName = g.Key.Name,
            TotalOrders = g.Count(),
            PendingOrders = g.Count(o => o.CurrentStatus != ProductionStatus.Finished)
        })
        .ToListAsync();
    
    return new DashboardDTO
    {
        OrdersByStage = orders
            .GroupBy(o => o.CurrentStage)
            .ToDictionary(g => g.Key.ToString(), g => g.Count()),
        
        PausedOrders = orders
            .Where(o => o.CurrentStatus == ProductionStatus.Paused)
            .Select(o => new ProductionOrderDTO { /* mapping */ })
            .ToList(),
        
        UserWorkload = userWorkload,
        
        CompletionRate = orders.Any() 
            ? (decimal)orders.Count(o => o.CurrentStatus == ProductionStatus.Finished) / orders.Count() * 100
            : 0,
        
        AverageStageTime = CalculateAverageStageTime(orders)
    };
}
```

**Responsable:** David  
**Tiempo:** 3-4 horas  
**Entregable:** Métodos implementados y testeados

##### 3.3 - Crear DTOs faltantes
**Archivos a crear:**
- `GestionProduccion/Application/DTOs/DashboardDTO.cs`
- `GestionProduccion/Application/DTOs/ProductionHistoryDTO.cs`
- `GestionProduccion/Application/DTOs/FilterProductionOrderDTO.cs`

**Contenido DashboardDTO:**

```csharp
public class DashboardDTO
{
    public Dictionary<string, int> OrdersByStage { get; set; } = new();
    
    public List<ProductionOrderDTO> PausedOrders { get; set; } = new();
    
    public List<UserWorkloadDTO> UserWorkload { get; set; } = new();
    
    public decimal CompletionRate { get; set; }
    
    public Dictionary<string, double> AverageStageTime { get; set; } = new();
}

public class UserWorkloadDTO
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
}
```

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** DTOs creados y mapeados

---

### FASE 4: COMPLETAR CONTROLLERS (Día 6)
**Duración:** 1 día  
**Participantes:** 1-2 Devs

#### Tareas

##### 4.1 - Auditar ProductionOrdersController
**Archivo a revisar:**
- `GestionProduccion/Controllers/ProductionOrdersController.cs`

**Endpoints esperados:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductionOrdersController : ControllerBase
{
    // GET /api/productionorders
    [HttpGet]
    public async Task<IActionResult> GetProductionOrders([FromQuery] FilterProductionOrderDTO? filter)
    {
        var orders = await _service.ListProductionOrdersAsync(filter);
        return Ok(orders);
    }
    
    // GET /api/productionorders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductionOrder(int id)
    {
        var order = await _service.GetProductionOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }
    
    // POST /api/productionorders
    [HttpPost]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> CreateProductionOrder(CreateProductionOrderDTO request)
    {
        var userId = User.GetUserId();
        var order = await _service.CreateProductionOrderAsync(request, userId);
        return CreatedAtAction(nameof(GetProductionOrder), new { id = order.Id }, order);
    }
    
    // PUT /api/productionorders/{id}/stage
    [HttpPut("{id}/stage")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> AdvanceStage(int id)
    {
        var userId = User.GetUserId();
        var success = await _service.AdvanceStageAsync(id, userId);
        if (!success) return BadRequest("Cannot advance stage");
        return Ok();
    }
    
    // PUT /api/productionorders/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var userId = User.GetUserId();
        var success = await _service.UpdateStatusAsync(id, request.NewStatus, request.Note, userId);
        if (!success) return BadRequest("Cannot update status");
        return Ok();
    }
    
    // PUT /api/productionorders/{id}/assign/{userId}
    [HttpPut("{id}/assign/{userId}")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> AssignTask(int id, int userId)
    {
        var success = await _service.AssignTaskAsync(id, userId);
        if (!success) return BadRequest("Cannot assign task");
        return Ok();
    }
    
    // GET /api/productionorders/dashboard
    [HttpGet("dashboard")]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _service.GetDashboardAsync();
        return Ok(dashboard);
    }
    
    // GET /api/productionorders/{id}/history
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var history = await _service.GetHistoryByProductionOrderIdAsync(id);
        return Ok(history);
    }
}
```

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** Controller completo

---

### FASE 5: TRADUCCIÓN UI (Día 7)
**Duración:** 1 día  
**Participantes:** 1-2 Devs

#### Tareas

##### 5.1 - Crear archivo de recursos portugués

**Archivo a crear:**
- `Client/GestionProduccion.Client/Resources/Portuguese.cs`

**Contenido:**

```csharp
namespace GestionProduccion.Client.Resources;

/// <summary>
/// Textos de interfaz de usuario en portugués
/// </summary>
public static class Portuguese
{
    // === GENERAL ===
    public const string Save = "Salvar";
    public const string Cancel = "Cancelar";
    public const string Delete = "Deletar";
    public const string Edit = "Editar";
    public const string Create = "Criar";
    public const string Search = "Buscar";
    public const string Filter = "Filtrar";
    public const string Loading = "Carregando...";
    public const string Error = "Erro";
    public const string Success = "Sucesso";
    
    // === PRODUCTION ORDERS ===
    public const string ProductionOrders = "Ordens de Produção";
    public const string CreateProductionOrder = "Criar Ordem de Produção";
    public const string EditProductionOrder = "Editar Ordem de Produção";
    public const string DeleteProductionOrder = "Deletar Ordem de Produção";
    public const string ProductCode = "Código da Ordem";
    public const string ProductDescription = "Descrição do Produto";
    public const string Quantity = "Quantidade";
    public const string CurrentStage = "Etapa Atual";
    public const string CurrentStatus = "Status Atual";
    public const string CreationDate = "Data de Criação";
    public const string EstimatedDeliveryDate = "Data Estimada de Entrega";
    public const string CompletionDate = "Data de Conclusão";
    public const string AssignedTo = "Atribuído a";
    
    // === STAGES ===
    public const string Cutting = "Corte";
    public const string Sewing = "Costura";
    public const string Review = "Revisão";
    public const string Packaging = "Embalagem";
    
    // === STATUS ===
    public const string InProduction = "Em Produção";
    public const string Paused = "Parado";
    public const string Finished = "Finalizado";
    
    // === USERS/ROLES ===
    public const string Users = "Usuários";
    public const string Administrator = "Administrador";
    public const string Leader = "Líder";
    public const string Tailor = "Costurera";
    public const string Workshop = "Oficina";
    
    // === MESSAGES ===
    public const string ConfirmDelete = "Tem certeza que deseja deletar este item?";
    public const string SavedSuccessfully = "Salvo com sucesso";
    public const string DeletedSuccessfully = "Deletado com sucesso";
    public const string ErrorOccurred = "Ocorreu um erro";
    public const string UnauthorizedAccess = "Acesso não autorizado";
    public const string RequiredField = "Campo obrigatório";
}
```

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** Archivo de recursos creado

##### 5.2 - Auditar componentes Blazor
**Archivos a revisar:**
- `Client/GestionProduccion.Client/Pages/ProductionOrders.razor`
- `Client/GestionProduccion.Client/Components/ProductionOrderCard.razor`
- `Client/GestionProduccion.Client/Components/ProductionOrderForm.razor`
- Todos los demás componentes

**Checklist:**
- [ ] Todos los textos usan `Portuguese.` constantes
- [ ] Validaciones en portugués
- [ ] Mensajes de error en portugués
- [ ] Placeholders en portugués
- [ ] Labels en portugués

**Responsable:** David  
**Tiempo:** 2-3 horas  
**Entregable:** Componentes traducidos

---

### FASE 6: SIGNALR INTEGRATION (Día 8)
**Duración:** 1 día  
**Participantes:** 1 Dev

#### Tareas

##### 6.1 - Verificar ProductionHub

**Archivo:**
- `GestionProduccion/Hubs/ProductionHub.cs`

**Debe contener:**

```csharp
public class ProductionHub : Hub
{
    public async Task NotifyUpdateAsync(int productionOrderId, string newStage, string newStatus)
    {
        await Clients.All.SendAsync("ReceiveUpdate", productionOrderId, newStage, newStatus);
    }
    
    public async Task NotifyPauseAsync(int productionOrderId, string reason)
    {
        await Clients.All.SendAsync("ReceivePause", productionOrderId, reason);
    }
    
    public async Task NotifyCompletionAsync(int productionOrderId)
    {
        await Clients.All.SendAsync("ReceiveCompletion", productionOrderId);
    }
}
```

**Responsable:** David  
**Tiempo:** 30 min  
**Entregable:** Hub verificado

##### 6.2 - Integrar con ProductionOrderService

**Archivo:**
- `GestionProduccion/Services/ProductionOrderService.cs`

**Cambios:**

```csharp
public class ProductionOrderService : IProductionOrderService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ProductionHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public ProductionOrderService(
        AppDbContext context,
        IHubContext<ProductionHub> hubContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId)
    {
        // ... lógica ...
        
        // Al final, notificar:
        await _hubContext.Clients.All.SendAsync(
            "ReceiveUpdate",
            orderId,
            order.CurrentStage.ToString(),
            order.CurrentStatus.ToString()
        );
        
        return true;
    }
    
    public async Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId)
    {
        // ... lógica ...
        
        // Al final, notificar:
        if (newStatus == ProductionStatus.Paused)
        {
            await _hubContext.Clients.All.SendAsync("ReceivePause", orderId, note);
        }
        else
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceiveUpdate",
                orderId,
                order.CurrentStage.ToString(),
                newStatus.ToString()
            );
        }
        
        return true;
    }
}
```

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** Integración completada

##### 6.3 - Verificar SignalRService en cliente

**Archivo:**
- `Client/GestionProduccion.Client/Services/SignalRService.cs`

**Debe contener listeners:**

```csharp
public class SignalRService
{
    private HubConnection? _hubConnection;
    
    public event Action<int, string, string>? OnUpdateReceived;
    public event Action<int, string>? OnPauseReceived;
    public event Action<int>? OnCompletionReceived;
    
    public async Task StartAsync(string url)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();
        
        _hubConnection.On<int, string, string>("ReceiveUpdate", (id, stage, status) =>
        {
            OnUpdateReceived?.Invoke(id, stage, status);
        });
        
        _hubConnection.On<int, string>("ReceivePause", (id, reason) =>
        {
            OnPauseReceived?.Invoke(id, reason);
        });
        
        _hubConnection.On<int>("ReceiveCompletion", (id) =>
        {
            OnCompletionReceived?.Invoke(id);
        });
        
        await _hubConnection.StartAsync();
    }
}
```

**Responsable:** David  
**Tiempo:** 1 hora  
**Entregable:** Cliente escuchando

---

### FASE 7: TESTING (Día 9-10)
**Duración:** 2 días  
**Participantes:** 1-2 Devs

#### Tareas

##### 7.1 - Unit Tests para ProductionOrderService

**Archivos a crear:**
- `GestionProduccion.Tests/Services/ProductionOrderServiceTests.cs`

**Casos a testear:**

```csharp
[TestClass]
public class ProductionOrderServiceTests
{
    [TestMethod]
    public async Task CreateProductionOrder_WithValidData_ShouldReturnOrder()
    {
        // Arrange
        var service = new ProductionOrderService(...);
        var request = new CreateProductionOrderDTO { ... };
        
        // Act
        var result = await service.CreateProductionOrderAsync(request, 1);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ProductionStage.Cutting, result.CurrentStage);
        Assert.AreEqual(ProductionStatus.InProduction, result.CurrentStatus);
    }
    
    [TestMethod]
    public async Task AdvanceStage_FromCutting_ShouldMoveTSewing()
    {
        // ... similar
    }
    
    [TestMethod]
    public async Task AdvanceStage_FromPackaging_ShouldFail()
    {
        // ... test que no se puede avanzar más
    }
    
    [TestMethod]
    public async Task UpdateStatus_ToPaused_ShouldRecordHistory()
    {
        // ... test que registra en historial
    }
}
```

**Responsable:** David  
**Tiempo:** 3-4 horas  
**Entregable:** Tests greenlight

##### 7.2 - Integration Tests para API

**Archivos a crear:**
- `GestionProduccion.Tests/Controllers/ProductionOrdersControllerTests.cs`

**Casos a testear:**
- POST /api/productionorders (crear)
- GET /api/productionorders (listar)
- GET /api/productionorders/{id} (obtener)
- PUT /api/productionorders/{id}/stage (avanzar)
- PUT /api/productionorders/{id}/status (actualizar status)
- Autorización (401, 403)
- Validaciones (400)

**Responsable:** David  
**Tiempo:** 3-4 horas  
**Entregable:** Tests greenlight

##### 7.3 - Manual Testing Checklist

**Backend:**
- [ ] Todos los endpoints responden correctamente
- [ ] Autenticación funciona
- [ ] Autorización por roles funciona
- [ ] Validaciones rechazan datos inválidos
- [ ] Histórico se registra correctamente
- [ ] CORS habilitado para cliente

**Frontend:**
- [ ] Componentes renderean correctamente
- [ ] Formularios validan
- [ ] Textos en portugués
- [ ] SignalR actualiza sin refresh

**Base de Datos:**
- [ ] Estructura normalizada a inglés
- [ ] Datos preservados
- [ ] Relaciones intactas
- [ ] Índices funcionan

**Responsable:** David  
**Tiempo:** 2 horas  
**Entregable:** Testing matrix completa

---

### FASE 8: DOCUMENTACIÓN Y DEPLOYMENT (Día 11)
**Duración:** 1 día  
**Participantes:** 1 Dev

#### Tareas

##### 8.1 - Actualizar documentación

**Archivos:**
- `README.md` (actualizar)
- `ARCHITECTURE_ANALYSIS.md` (actualizar)
- `ClientRequeriments.txt` (actualizar)

**Contenido:**
- Estado final del MVP
- Cómo ejecutar localmente
- Estructura de carpetas
- Endpoints disponibles
- Variables de entorno
- Requisitos previos

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** Documentación actualizada

##### 8.2 - Preparar para deployment

**Checklist:**
- [ ] appsettings.json para producción
- [ ] Cambiar secrets de desarrollador
- [ ] Habilitar HTTPS
- [ ] CORS configurado solo para dominio cliente
- [ ] Logging configurado
- [ ] Database migrations listas
- [ ] Build optimizado

**Responsable:** David  
**Tiempo:** 1-2 horas  
**Entregable:** Ready para deploy

---

## ?? ENTREGABLES

### Por Fase

| Fase | Entregable | Formato | Fecha |
|------|-----------|---------|-------|
| 1 | Backup + Rama Git | SQL + Git | Día 1 |
| 2 | BD Normalizada | Migration + Code | Día 3 |
| 3 | Servicios Completos | Code + DTO | Día 5 |
| 4 | Controllers Completos | Code | Día 6 |
| 5 | UI Traducida | Code (.razor) | Día 7 |
| 6 | SignalR Integrado | Code | Día 8 |
| 7 | Tests Passing | Code + Report | Día 10 |
| 8 | Documentación | Markdown | Día 11 |

### Final

```
? MVP 95% Funcional
? Código en inglés (backend + BD)
? UI en portugués
? Tests pasando
? Documentación completa
? Ready para deployment
? SignalR funcionando
? BD normalizada
```

---

## ?? MÉTRICAS DE SEGUIMIENTO

### Cobertura de Funcionalidad

```
Meta: 95% ?

Gestión de Usuarios:      100% ?
Órdenes de Producción:    90%  ??
Auditoría:                100% ?
Autenticación:            100% ?
Autorización:             100% ?
SignalR:                  70%  ??
UI:                       60%  ??
Testing:                  40%  ??
```

### Líneas de Código (Estimado)

```
Backend:     ~3,000 LOC
Frontend:    ~4,000 LOC
Tests:       ~2,000 LOC
???????????????????????
Total:       ~9,000 LOC
```

### Timeline

```
Inicio:     Día 1 (Lunes)
Fin:        Día 11 (Viernes)
Duración:   2 semanas (90 horas)
Participantes: 1-2 devs (60-80 horas/persona)
```

---

## ?? RIESGOS Y MITIGACIÓN

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|---|---|---|
| Migración BD fallida | Media | Alto | Backup previo + testing exhaustivo |
| Pérdida de datos | Baja | Crítico | Export SQL antes + rollback strategy |
| Retrasos en testing | Media | Medio | Iniciar tests paralelamente |
| Incompatibilidad con cliente actual | Baja | Alto | Comunicación previa con cliente |

---

## ? CRITERIOS DE ÉXITO

- [ ] Todos los tests pasan (>95% cobertura)
- [ ] Código review aprobado por 2 devs
- [ ] BD migrada sin errores
- [ ] UI 100% en portugués
- [ ] SignalR funcionando en tiempo real
- [ ] Documentación completa y clara
- [ ] Cliente puede ejecutar localmente sin errores
- [ ] Performance >2000 ms en endpoints críticos
- [ ] 0 vulnerabilidades de seguridad conocidas

---

**Plan creado:** Febrero 2026  
**Responsable:** David Fernandez  
**Estado:** Listo para ejecución

