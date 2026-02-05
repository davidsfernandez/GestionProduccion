# BACKUP DE BD - INSTRUCCIONES

**Fecha:** Febrero 2026  
**Propósito:** Guardar copia segura antes de normalización de BD

---

## ?? OPCIONES PARA HACER BACKUP

### ? OPCIÓN 1: Script PowerShell (Windows)

**Requisitos:**
- PowerShell 5.0+
- MySQL instalado y mysqldump disponible

**Pasos:**

1. Abrir PowerShell como Administrador
2. Navegar al directorio del proyecto:
```powershell
cd "C:\Users\david\GestionProduccion"
```

3. Ejecutar script de backup:
```powershell
PowerShell -ExecutionPolicy Bypass -File scripts\backup_database.ps1
```

4. El backup se creará en: `./backups/backup_GestionProduccionDB_YYYYMMDD_HHMMSS.sql`

---

### ? OPCIÓN 2: Script Bash (WSL/Linux/Mac)

**Requisitos:**
- Bash shell
- mysqldump disponible

**Pasos:**

1. Abrir Terminal/WSL
2. Navegar al directorio:
```bash
cd /mnt/c/Users/david/GestionProduccion
```

3. Ejecutar script:
```bash
chmod +x scripts/backup_database.sh
bash scripts/backup_database.sh
```

---

### ? OPCIÓN 3: Comando Manual PowerShell

**Ejecutar directamente:**

```powershell
# Windows - Encontrar mysqldump manualmente
$MysqlPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe"
$BackupDir = "backups"

# Crear directorio
if (-not (Test-Path $BackupDir)) { 
    New-Item -ItemType Directory -Path $BackupDir 
}

# Hacer backup
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupFile = "$BackupDir\backup_GestionProduccionDB_$Timestamp.sql"

& $MysqlPath -u root -p'Cualquiera1' GestionProduccionDB > $BackupFile

Write-Host "? Backup creado: $BackupFile"
```

---

### ? OPCIÓN 4: MySQL Workbench (GUI)

**Si tienes MySQL Workbench instalado:**

1. Abrir MySQL Workbench
2. Conectar a servidor local
3. Click derecho en base de datos `GestionProduccionDB`
4. Seleccionar: **Data Export**
5. Guardar archivo `.sql` en carpeta `./backups/`

---

### ? OPCIÓN 5: Comando CLI directo

**Si mysqldump está en PATH:**

```bash
mysqldump -u root -p'Cualquiera1' GestionProduccionDB > ./backups/backup_$(date +%Y%m%d_%H%M%S).sql
```

**Luego verificar:**
```bash
ls -la ./backups/
```

---

## ?? VERIFICAR QUE EL BACKUP EXISTE

Una vez completado, verificar:

```powershell
# PowerShell
Get-ChildItem -Path "./backups/" -Filter "*.sql" | 
  Select-Object Name, @{Name="SizeKB";Expression={[math]::Round($_.Length/1KB,2)}}

# Resultado esperado:
# Name                                        SizeKB
# ----                                        ------
# backup_GestionProduccionDB_20260215_143022.sql  125.45
```

---

## ?? PRÓXIMOS PASOS

Después de confirmar el backup:

1. ? Backup creado y verificado
2. ?? **SIGUIENTE:** Crear migration de normalización BD
3. ?? **DESPUÉS:** Aplicar migration
4. ?? **FINALMENTE:** Validar cambios

---

## ?? SI ALGO SALE MAL

Si necesitas restaurar el backup:

```bash
# Restaurar desde backup
mysql -u root -p'Cualquiera1' GestionProduccionDB < ./backups/backup_GestionProduccionDB_20260215_143022.sql
```

---

**Backup es CRÍTICO para esta fase.**  
**No continuar hasta confirmar que existe el archivo.**

