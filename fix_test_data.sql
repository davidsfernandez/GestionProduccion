/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

USE GestionProduccionDB;
UPDATE Users SET Role = 'Operational', SewingTeamId = 1 WHERE Id = 2;
UPDATE ProductionOrders SET SewingTeamId = 1, UserId = 2, CurrentStage = 'Sewing', CurrentStatus = 'InProduction' WHERE Id = 1;


