-- ============================================
-- LIMPIAR BD - Eliminar tablas creadas parcialmente
-- ============================================
USE GestionProduccionDB;

-- Deshabilitar FK
SET FOREIGN_KEY_CHECKS = 0;

-- Eliminar tablas si existen (case-insensitive en MySQL)
DROP TABLE IF EXISTS ProductionHistories;
DROP TABLE IF EXISTS ProductionOrders;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS HistoricoProducoes;
DROP TABLE IF EXISTS OrdensProducao;
DROP TABLE IF EXISTS Usuarios;

-- Rehabilitar FK
SET FOREIGN_KEY_CHECKS = 1;

-- Verificación
SHOW TABLES;
