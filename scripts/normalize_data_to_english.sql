-- ============================================
-- DATA NORMALIZATION: PORTUGUESE TO ENGLISH ENUMS
-- ============================================

USE GestionProduccionDB;

-- 1. Normalize Roles in Users table
UPDATE Users SET Role = 'Administrator' WHERE Role IN ('Administrador', 'Admin');
UPDATE Users SET Role = 'Leader' WHERE Role IN ('Líder', 'Lider');
UPDATE Users SET Role = 'Operational' WHERE Role IN ('Costureira', 'Costureiro', 'Operacional', 'Operator');
UPDATE Users SET Role = 'Office' WHERE Role IN ('Oficina', 'Escritório');

-- 2. Normalize CurrentStage in ProductionOrders
UPDATE ProductionOrders SET CurrentStage = 'Cutting' WHERE CurrentStage IN ('Corte');
UPDATE ProductionOrders SET CurrentStage = 'Sewing' WHERE CurrentStage IN ('Costura');
UPDATE ProductionOrders SET CurrentStage = 'Review' WHERE CurrentStage IN ('Revisão', 'Revisao', 'Review');
UPDATE ProductionOrders SET CurrentStage = 'Packaging' WHERE CurrentStage IN ('Embalagem', 'Embalado');

-- 3. Normalize CurrentStatus in ProductionOrders
UPDATE ProductionOrders SET CurrentStatus = 'Pending' WHERE CurrentStatus IN ('Pendente');
UPDATE ProductionOrders SET CurrentStatus = 'InProduction' WHERE CurrentStatus IN ('Em Produção', 'Em Producao', 'In Production');
UPDATE ProductionOrders SET CurrentStatus = 'Paused' WHERE CurrentStatus IN ('Pausado');
UPDATE ProductionOrders SET CurrentStatus = 'Completed' WHERE CurrentStatus IN ('Finalizado', 'Concluído', 'Concluido');
UPDATE ProductionOrders SET CurrentStatus = 'Stopped' WHERE CurrentStatus IN ('Parado');

-- 4. Normalize History Stages and Status
UPDATE ProductionHistories SET PreviousStage = 'Cutting' WHERE PreviousStage IN ('Corte');
UPDATE ProductionHistories SET PreviousStage = 'Sewing' WHERE PreviousStage IN ('Costura');
UPDATE ProductionHistories SET PreviousStage = 'Review' WHERE PreviousStage IN ('Revisão', 'Revisao');
UPDATE ProductionHistories SET PreviousStage = 'Packaging' WHERE PreviousStage IN ('Embalagem');

UPDATE ProductionHistories SET NewStage = 'Cutting' WHERE NewStage IN ('Corte');
UPDATE ProductionHistories SET NewStage = 'Sewing' WHERE NewStage IN ('Costura');
UPDATE ProductionHistories SET NewStage = 'Review' WHERE NewStage IN ('Revisão', 'Revisao');
UPDATE ProductionHistories SET NewStage = 'Packaging' WHERE NewStage IN ('Embalagem');

UPDATE ProductionHistories SET PreviousStatus = 'Pending' WHERE PreviousStatus IN ('Pendente');
UPDATE ProductionHistories SET PreviousStatus = 'InProduction' WHERE PreviousStatus IN ('Em Produção', 'Em Producao');
UPDATE ProductionHistories SET PreviousStatus = 'Paused' WHERE PreviousStatus IN ('Pausado');
UPDATE ProductionHistories SET PreviousStatus = 'Completed' WHERE PreviousStatus IN ('Finalizado', 'Concluído');
UPDATE ProductionHistories SET PreviousStatus = 'Stopped' WHERE PreviousStatus IN ('Parado');

UPDATE ProductionHistories SET NewStatus = 'Pending' WHERE NewStatus IN ('Pendente');
UPDATE ProductionHistories SET NewStatus = 'InProduction' WHERE NewStatus IN ('Em Produção', 'Em Producao');
UPDATE ProductionHistories SET NewStatus = 'Paused' WHERE NewStatus IN ('Pausado');
UPDATE ProductionHistories SET NewStatus = 'Completed' WHERE NewStatus IN ('Finalizado', 'Concluído');
UPDATE ProductionHistories SET NewStatus = 'Stopped' WHERE NewStatus IN ('Parado');
