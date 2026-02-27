$LogFile = "deploy-watcher.log"
Write-Output "=== Vigilante de Despliegue de Azure Iniciado ===" > $LogFile
Write-Output "Esperando la eliminacion del grupo 'gestionproduccion' para liberar la cuota global..." >> $LogFile

while ($true) {
    $exists = az group exists --name gestionproduccion
    if ($exists -eq "false") {
        Write-Output "[OK] Entorno antiguo eliminado por completo." >> $LogFile
        Write-Output ">>> Iniciando el script de despliegue maestro: deploy-azure-scratch.ps1 <<<" >> $LogFile
        .\deploy-azure-scratch.ps1 >> $LogFile 2>&1
        Write-Output "=== Proceso Finalizado ===" >> $LogFile
        break
    }
    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Output "[$timestamp] Azure sigue purgando recursos. Reintentando en 15 segundos..." >> $LogFile
    Start-Sleep -Seconds 15
}