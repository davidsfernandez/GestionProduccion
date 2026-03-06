/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

#!/bin/bash

# Script para hacer backup de la BD GestionProduccionDB
# Uso: bash backup_database.sh

BACKUP_DIR="./backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="$BACKUP_DIR/backup_GestionProduccionDB_$TIMESTAMP.sql"

# Crear directorio si no existe
mkdir -p "$BACKUP_DIR"

echo "?? Creando backup de GestionProduccionDB..."
echo "?? Archivo: $BACKUP_FILE"

# Exportar BD
mysqldump -u root -p'REPLACE_WITH_YOUR_PASSWORD' GestionProduccionDB > "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    echo "? Backup creado exitosamente"
    echo "?? Tamaï¿½o: $(ls -lh "$BACKUP_FILE" | awk '{print $5}')"
else
    echo "? Error al crear backup"
    exit 1
fi

