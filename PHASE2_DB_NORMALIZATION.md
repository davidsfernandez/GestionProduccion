# FASE 2: NORMALIZACIÓN DE BD A INGLÉS

**Duración:** Días 2-3  
**Objetivo:** Cambiar tablas y columnas de BD de portugués a inglés  
**Resultado:** AppDbContext simplificado sin HasColumnName()

---

## ?? PLAN DETALLADO

### PASO 1: Crear Migration vacía

```bash
cd GestionProduccion
dotnet ef migrations add NormalizeDbToEnglish
```

**Resultado:** Se crea archivo:  
`Migrations/[timestamp]_NormalizeDbToEnglish.cs`

---

### PASO 2: Escribir SQL en la Migration

**Editar el archivo migration creado** e insertar el siguiente SQL:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeDbToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // STEP 1: CREAR NUEVAS TABLAS CON NOMBRE INGLÉS
            // ============================================

            // Crear tabla Users (copia de Usuarios con estructura normalizada)
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            // Crear tabla ProductionOrders (copia de OrdensProducao)
            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    UniqueCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ProductDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    QuantityCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrentStage = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CurrentStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DelegationNote = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            // Crear tabla ProductionHistories
            migrationBuilder.CreateTable(
                name: "ProductionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    PreviousStage = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NewStage = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PreviousStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ModificationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionHistories_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ProductionHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            // ============================================
            // STEP 2: COPIAR DATOS DE TABLAS ANTIGUAS
            // ============================================

            migrationBuilder.Sql(@"
                INSERT INTO Users (Id, Name, Email, PasswordHash, Role, IsActive)
                SELECT Id, Nome, Email, HashPassword, Perfil, Ativo 
                FROM Usuarios;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ProductionOrders (Id, UniqueCode, ProductDescription, Quantity, QuantityCompleted, 
                                             CurrentStage, CurrentStatus, CreationDate, EstimatedDeliveryDate, 
                                             CompletionDate, UserId, DelegationNote)
                SELECT Id, CodigoUnico, DescricaoProduto, Cantidad, 0,
                       EtapaAtual, StatusAtual, DataCriacao, DataEstimadaEntrega,
                       DataConclusao, UsuarioId, NULL
                FROM OrdensProducao;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ProductionHistories (Id, ProductionOrderId, PreviousStage, NewStage, 
                                                PreviousStatus, NewStatus, UserId, ModificationDate, Note)
                SELECT Id, OrdemProducaoId, EtapaAnterior, EtapaNova,
                       StatusAnterior, StatusNovo, UsuarioId, DataModificacao, Observacao
                FROM HistoricoProducoes;
            ");

            // ============================================
            // STEP 3: ELIMINAR TABLAS ANTIGUAS
            // ============================================

            migrationBuilder.DropTable(name: "HistoricoProducoes");
            migrationBuilder.DropTable(name: "OrdensProducao");
            migrationBuilder.DropTable(name: "Usuarios");

            // ============================================
            // STEP 4: CREAR ÍNDICES
            // ============================================

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_UniqueCode",
                table: "ProductionOrders",
                column: "UniqueCode",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_UserId",
                table: "ProductionOrders",
                column: "UserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHistories_ProductionOrderId",
                table: "ProductionHistories",
                column: "ProductionOrderId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHistories_UserId",
                table: "ProductionHistories",
                column: "UserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // ROLLBACK: RECREAR TABLAS ANTIGUAS
            // ============================================

            migrationBuilder.DropTable(name: "ProductionHistories");
            migrationBuilder.DropTable(name: "ProductionOrders");
            migrationBuilder.DropTable(name: "Users");

            // Recrear tablas originales
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "varchar(150)", nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", nullable: false),
                    HashPassword = table.Column<string>(type: "longtext", nullable: false),
                    Perfil = table.Column<string>(type: "varchar(50)", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "OrdensProducao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CodigoUnico = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DescricaoProduto = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    EtapaAtual = table.Column<string>(type: "varchar(50)", nullable: false),
                    StatusAtual = table.Column<string>(type: "varchar(50)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataEstimadaEntrega = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensProducao", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "HistoricoProducoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    OrdemProducaoId = table.Column<int>(type: "int", nullable: false),
                    EtapaAnterior = table.Column<string>(type: "varchar(50)", nullable: false),
                    EtapaNova = table.Column<string>(type: "varchar(50)", nullable: false),
                    StatusAnterior = table.Column<string>(type: "varchar(50)", nullable: false),
                    StatusNovo = table.Column<string>(type: "varchar(50)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    DataModificacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observacao = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoProducoes", x => x.Id);
                }
            );
        }
    }
}
```

---

### PASO 3: Simplificar AppDbContext

**Editar:** `Data/AppDbContext.cs`

**Cambiar:** Remover todos los `ToTable()` y `HasColumnName()` que sean duplicados.

**De:**
```csharp
modelBuilder.Entity<User>().ToTable("Usuarios");
modelBuilder.Entity<User>().Property(u => u.Name).HasColumnName("Nome");
modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("HashPassword");
// ... etc
```

**A:**
```csharp
// Ya no necesitamos mapeos! Las convenciones de EF Core lo hacen automáticamente:
// User ? Users (tabla)
// Name ? Name (columna)
// etc.

// Solo dejar lo esencial:
modelBuilder.Entity<ProductionOrder>()
    .HasIndex(po => po.UniqueCode)
    .IsUnique();

// Y las relaciones y configuraciones de enums
```

---

### PASO 4: Aplicar Migration

```bash
cd GestionProduccion
dotnet ef database update
```

**Esperado:**
```
Build started...
Applying migration 'NormalizeDbToEnglish'.
Done. Successfully applied the migration 'NormalizeDbToEnglish' to database 'GestionProduccionDB'.
```

---

### PASO 5: Verificar cambios en BD

```bash
mysql -u root -p'Cualquiera1' GestionProduccionDB

# En MySQL prompt:
mysql> SHOW TABLES;
# Resultado esperado:
# ProductionHistories
# ProductionOrders
# Users

mysql> DESC Users;
# Resultado esperado:
# Name (no "Nome")
# PasswordHash (no "HashPassword")
# Role (no "Perfil")
# IsActive (no "Ativo")

mysql> SELECT * FROM Users LIMIT 1;
# Resultado esperado: Admin user existe

mysql> QUIT;
```

---

## ? CHECKLIST FASE 2

```
[ ] Migration creada: dotnet ef migrations add NormalizeDbToEnglish
[ ] SQL escrito en migration
[ ] AppDbContext simplificado (remover HasColumnName)
[ ] Migration aplicada: dotnet ef database update
[ ] Verificado: Tablas en inglés (Users, ProductionOrders, ProductionHistories)
[ ] Verificado: Columnas en inglés (Name, PasswordHash, etc.)
[ ] Verificado: Datos preservados
[ ] Commit a Git: git add . && git commit -m "feat: normalize database to english"
```

---

## ?? SI ALGO FALLA

### Rollback (volver atrás)

```bash
# Revertir última migration
dotnet ef database update [NombreMigrationAnterior]

# O restaurar desde backup
mysql -u root -p'Cualquiera1' GestionProduccionDB < ./backups/backup_GestionProduccionDB_[TIMESTAMP].sql
```

---

## ?? DESPUÉS DE FASE 2

- ? BD normalizada a inglés
- ? AppDbContext simplificado
- ? Datos preservados
- ?? **SIGUIENTE:** Fase 3 - Completar Servicios

---

**Siguiente paso:** Ejecutar los pasos 1-5 cuando estés listo.

