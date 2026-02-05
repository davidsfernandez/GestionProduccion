-- ============================================
-- NORMALIZACIÓN DE BD A INGLÉS
-- GestionProduccionDB
-- ============================================
-- 
-- Este script renombra tablas y columnas de portugués a inglés
-- BACKUP RECOMENDADO ANTES DE EJECUTAR

USE GestionProduccionDB;

-- ============================================
-- STEP 1: DESHABILITAR FOREIGN KEYS
-- ============================================
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================
-- STEP 2: RENOMBRAR TABLAS
-- ============================================

-- Renombrar tabla Usuarios -> Users
RENAME TABLE Usuarios TO Users;

-- Renombrar tabla OrdensProducao -> ProductionOrders
RENAME TABLE OrdensProducao TO ProductionOrders;

-- Renombrar tabla HistoricoProducoes -> ProductionHistories
RENAME TABLE HistoricoProducoes TO ProductionHistories;

-- ============================================
-- STEP 3: RENOMBRAR COLUMNAS EN Users
-- ============================================

ALTER TABLE Users
    CHANGE COLUMN Nome Name VARCHAR(150),
    CHANGE COLUMN HashPassword PasswordHash LONGTEXT,
    CHANGE COLUMN Perfil Role VARCHAR(50),
    CHANGE COLUMN Ativo IsActive TINYINT(1);

-- ============================================
-- STEP 4: RENOMBRAR COLUMNAS EN ProductionOrders
-- ============================================

ALTER TABLE ProductionOrders
    CHANGE COLUMN CodigoUnico UniqueCode VARCHAR(50),
    CHANGE COLUMN DescricaoProduto ProductDescription VARCHAR(500),
    CHANGE COLUMN Cantidad Quantity INT,
    CHANGE COLUMN EtapaAtual CurrentStage VARCHAR(50),
    CHANGE COLUMN StatusAtual CurrentStatus VARCHAR(50),
    CHANGE COLUMN DataCriacao CreationDate DATETIME(6),
    CHANGE COLUMN DataEstimadaEntrega EstimatedDeliveryDate DATETIME(6),
    CHANGE COLUMN DataConclusao CompletionDate DATETIME(6),
    CHANGE COLUMN UsuarioId UserId INT;

-- Agregar columna QuantityCompleted si no existe
ALTER TABLE ProductionOrders ADD COLUMN IF NOT EXISTS QuantityCompleted INT DEFAULT 0;

-- ============================================
-- STEP 5: RENOMBRAR COLUMNAS EN ProductionHistories
-- ============================================

ALTER TABLE ProductionHistories
    CHANGE COLUMN OrdemProducaoId ProductionOrderId INT,
    CHANGE COLUMN EtapaAnterior PreviousStage VARCHAR(50),
    CHANGE COLUMN EtapaNova NewStage VARCHAR(50),
    CHANGE COLUMN StatusAnterior PreviousStatus VARCHAR(50),
    CHANGE COLUMN StatusNovo NewStatus VARCHAR(50),
    CHANGE COLUMN UsuarioId UserId INT,
    CHANGE COLUMN DataModificacao ModificationDate DATETIME(6),
    CHANGE COLUMN Observacao Note VARCHAR(500);

-- ============================================
-- STEP 6: RECREAR ÍNDICES
-- ============================================

-- Eliminar índices antiguos si existen
DROP INDEX IF EXISTS IX_OrdensProducao_CodigoUnico ON ProductionOrders;
DROP INDEX IF EXISTS IX_OrdensProducao_UsuarioId ON ProductionOrders;
DROP INDEX IF EXISTS IX_HistoricoProducoes_OrdemProducaoId ON ProductionHistories;
DROP INDEX IF EXISTS IX_HistoricoProducoes_UsuarioId ON ProductionHistories;
DROP INDEX IF EXISTS IX_Usuarios_Email ON Users;

-- Crear nuevos índices con nombres en inglés
CREATE UNIQUE INDEX IX_ProductionOrders_UniqueCode ON ProductionOrders(UniqueCode);
CREATE INDEX IX_ProductionOrders_UserId ON ProductionOrders(UserId);
CREATE INDEX IX_ProductionHistories_ProductionOrderId ON ProductionHistories(ProductionOrderId);
CREATE INDEX IX_ProductionHistories_UserId ON ProductionHistories(UserId);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- ============================================
-- STEP 7: REHABILITAR FOREIGN KEYS
-- ============================================
SET FOREIGN_KEY_CHECKS = 1;

-- ============================================
-- VERIFICACIÓN
-- ============================================

-- Mostrar estructura de tablas
SHOW TABLES;
DESC Users;
DESC ProductionOrders;
DESC ProductionHistories;

-- Contar registros
SELECT COUNT(*) as UsersCount FROM Users;
SELECT COUNT(*) as ProductionOrdersCount FROM ProductionOrders;
SELECT COUNT(*) as HistoriesCount FROM ProductionHistories;

-- Resultado esperado:
-- ? Tablas renombradas a inglés
-- ? Columnas renombradas a inglés
-- ? Datos preservados
-- ? Índices recreados
