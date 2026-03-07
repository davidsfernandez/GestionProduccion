/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

-- ============================================
-- DATABASE NORMALIZATION TO ENGLISH (ALIGNED WITH C# ENTITIES)
-- Project: GestionProduccion
-- ============================================
-- 
-- This script renames tables and columns from Portuguese/Spanish to English
-- RECOMMENDED BACKUP BEFORE RUNNING

USE GestionProduccionDB;

-- ============================================
-- STEP 1: DISABLE FOREIGN KEYS
-- ============================================
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================
-- STEP 2: RENAME TABLES
-- ============================================

-- Rename Usuarios -> Users
RENAME TABLE Usuarios TO Users;

-- Rename OrdensProducao -> ProductionOrders
RENAME TABLE OrdensProducao TO ProductionOrders;

-- Rename HistoricoProducoes -> ProductionHistories
RENAME TABLE HistoricoProducoes TO ProductionHistories;

-- ============================================
-- STEP 3: RENAME COLUMNS IN Users
-- ============================================

ALTER TABLE Users
    CHANGE COLUMN Nome Name VARCHAR(150),
    CHANGE COLUMN HashPassword PasswordHash LONGTEXT,
    CHANGE COLUMN Perfil Role VARCHAR(50),
    CHANGE COLUMN Ativo IsActive TINYINT(1);

-- ============================================
-- STEP 4: RENAME COLUMNS IN ProductionOrders
-- ============================================

ALTER TABLE ProductionOrders
    CHANGE COLUMN CodigoUnico LotCode VARCHAR(50),
    CHANGE COLUMN DescricaoProduto ProductDescription VARCHAR(500),
    CHANGE COLUMN Cantidad Quantity INT,
    CHANGE COLUMN EtapaAtual CurrentStage VARCHAR(50),
    CHANGE COLUMN StatusAtual CurrentStatus VARCHAR(50),
    CHANGE COLUMN DataCriacao CreatedAt DATETIME(6),
    CHANGE COLUMN DataEstimadaEntrega EstimatedCompletionAt DATETIME(6),
    CHANGE COLUMN DataConclusao CompletedAt DATETIME(6),
    CHANGE COLUMN UsuarioId UserId INT,
    CHANGE COLUMN DataAtualizacao UpdatedAt DATETIME(6);

-- Ensure standard audit columns exist
ALTER TABLE ProductionOrders ADD COLUMN IF NOT EXISTS UpdatedAt DATETIME(6);

-- ============================================
-- STEP 5: RENAME COLUMNS IN ProductionHistories
-- ============================================

ALTER TABLE ProductionHistories
    CHANGE COLUMN OrdemProducaoId ProductionOrderId INT,
    CHANGE COLUMN EtapaAnterior PreviousStage VARCHAR(50),
    CHANGE COLUMN EtapaNova NewStage VARCHAR(50),
    CHANGE COLUMN StatusAnterior PreviousStatus VARCHAR(50),
    CHANGE COLUMN StatusNovo NewStatus VARCHAR(50),
    CHANGE COLUMN UsuarioId UserId INT,
    CHANGE COLUMN DataModificacao CreatedAt DATETIME(6),
    CHANGE COLUMN Observacao Note VARCHAR(500);

-- ============================================
-- STEP 6: RECREATE INDEXES
-- ============================================

-- Drop old indexes if they exist
DROP INDEX IF EXISTS IX_OrdensProducao_CodigoUnico ON ProductionOrders;
DROP INDEX IF EXISTS IX_OrdensProducao_UsuarioId ON ProductionOrders;
DROP INDEX IF EXISTS IX_HistoricoProducoes_OrdemProducaoId ON ProductionHistories;
DROP INDEX IF EXISTS IX_HistoricoProducoes_UsuarioId ON ProductionHistories;
DROP INDEX IF EXISTS IX_Usuarios_Email ON Users;

-- Create new indexes with English names
CREATE UNIQUE INDEX IX_ProductionOrders_LotCode ON ProductionOrders(LotCode);
CREATE INDEX IX_ProductionOrders_UserId ON ProductionOrders(UserId);
CREATE INDEX IX_ProductionHistories_ProductionOrderId ON ProductionHistories(ProductionOrderId);
CREATE INDEX IX_ProductionHistories_UserId ON ProductionHistories(UserId);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- ============================================
-- STEP 7: RE-ENABLE FOREIGN KEYS
-- ============================================
SET FOREIGN_KEY_CHECKS = 1;

-- ============================================
-- VERIFICATION
-- ============================================

SHOW TABLES;
DESC Users;
DESC ProductionOrders;
DESC ProductionHistories;
