# Script para hacer backup de la BD GestionProduccionDB (Windows)
# Uso: PowerShell -ExecutionPolicy Bypass -File backup_database.ps1

param(
    [string]$BackupDir = "./backups",
    [string]$DatabaseName = "GestionProduccionDB",
    [string]$Username = "root",
    [string]$Password = "REPLACE_WITH_YOUR_PASSWORD",
    [string]$Host = "localhost"
)

# Crear directorio si no existe
if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir | Out-Null
    Write-Host "?? Directorio creado: $BackupDir"
}

# Generar nombre de archivo con timestamp
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupFile = Join-Path $BackupDir "backup_${DatabaseName}_${Timestamp}.sql"

Write-Host "?? Creando backup de $DatabaseName..." -ForegroundColor Cyan
Write-Host "?? Archivo: $BackupFile"

try {
    # Intentar encontrar mysqldump
    $MysqldumpPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe"
    
    if (-not (Test-Path $MysqldumpPath)) {
        # Buscar en otras ubicaciones comunes
        $MysqldumpPath = Get-Command mysqldump -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
    }
    
    if (-not $MysqldumpPath) {
        throw "mysqldump no encontrado. Aseg�rate de que MySQL est� instalado y en PATH."
    }
    
    # Ejecutar mysqldump
    & $MysqldumpPath -h $Host -u $Username -p"$Password" $DatabaseName > $BackupFile
    
    # Verificar si fue exitoso
    if ($LASTEXITCODE -eq 0) {
        $FileSize = (Get-Item $BackupFile).Length
        $FileSizeKB = [math]::Round($FileSize / 1KB, 2)
        Write-Host "? Backup creado exitosamente" -ForegroundColor Green
        Write-Host "?? Tama�o: $FileSizeKB KB"
    } else {
        throw "Error al ejecutar mysqldump (exit code: $LASTEXITCODE)"
    }
} catch {
    Write-Host "? Error al crear backup: $_" -ForegroundColor Red
    exit 1
}
