USE GestionProduccionDB;
UPDATE Users SET Role = 'Operational', SewingTeamId = 1 WHERE Id = 2;
UPDATE ProductionOrders SET SewingTeamId = 1, UserId = 2, CurrentStage = 'Sewing', CurrentStatus = 'InProduction' WHERE Id = 1;
