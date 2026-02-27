$ResourceGroup = "gestionproduccion-new-rg"
$Location = "eastus2"
$RandomSuffix = Get-Random -Minimum 1000 -Maximum 9999
$AcrName = "gpacr$RandomSuffix"
$EnvName = "gp-env"
$DbAppName = "gp-db"
$WebAppName = "gp-app"

# Database Credentials
$DbUser = "gp_admin"
$DbPass = "gp_secure_pass_2026"
$DbRootPass = "gp_root_secure_2026"
$DbName = "GestionProduccionDB"

Write-Host "[*] Iniciando Despliegue Maestro en Azure..." -ForegroundColor Cyan

# 1. Create Resource Group
Write-Host "[1] Creando Resource Group: $ResourceGroup..."
az group create --name $ResourceGroup --location $Location

# 2. Create ACR
Write-Host "[2] Creando Azure Container Registry: $AcrName..."
az acr create --resource-group $ResourceGroup --name $AcrName --sku Basic --admin-enabled true

# Get ACR Credentials
$AcrLoginServer = az acr show --name $AcrName --query loginServer --output tsv
$AcrUsername = az acr credential show --name $AcrName --query username --output tsv
$AcrPassword = az acr credential show --name $AcrName --query passwords[0].value --output tsv

# 3. Create Container Apps Environment
Write-Host "[3] Creando Container Apps Environment: $EnvName..."
az containerapp env create --name $EnvName --resource-group $ResourceGroup --location $Location

# 4. Deploy MySQL Database (Internal)
Write-Host "[4] Desplegando Base de Datos MySQL (Interna)..."
az containerapp create `
  --name $DbAppName `
  --resource-group $ResourceGroup `
  --environment $EnvName `
  --image mysql:8.0 `
  --target-port 3306 `
  --ingress internal `
  --env-vars MYSQL_ROOT_PASSWORD=$DbRootPass MYSQL_DATABASE=$DbName MYSQL_USER=$DbUser MYSQL_PASSWORD=$DbPass `
  --cpu 0.5 --memory 1.0Gi

# Get DB Internal FQDN
$DbFqdn = az containerapp show --name $DbAppName --resource-group $ResourceGroup --query properties.configuration.ingress.fqdn --output tsv

# 5. Build and Push .NET App
Write-Host "[5] Construyendo y subiendo imagen de la aplicacion web..."
docker build -t "$AcrLoginServer/gestionproduccion:latest" .
docker login $AcrLoginServer -u $AcrUsername -p $AcrPassword
docker push "$AcrLoginServer/gestionproduccion:latest"

# 6. Deploy .NET Web App
Write-Host "[6] Desplegando Aplicacion Web .NET (Externa)..."
$ConnectionString = "Server=$DbFqdn;Port=3306;Database=$DbName;User Id=$DbUser;Password=$DbPass;AllowUserVariables=true;ConvertZeroDateTime=true;"

az containerapp create `
  --name $WebAppName `
  --resource-group $ResourceGroup `
  --environment $EnvName `
  --image "$AcrLoginServer/gestionproduccion:latest" `
  --target-port 8080 `
  --ingress external `
  --registry-server $AcrLoginServer `
  --registry-username $AcrUsername `
  --registry-password $AcrPassword `
  --env-vars ASPNETCORE_URLS=http://+:8080 ASPNETCORE_ENVIRONMENT=Production DOTNET_RUNNING_IN_CONTAINER=true ConnectionStrings__DefaultConnection=$ConnectionString Jwt__Key="9f8e7d6c5b4a3f2e1d0c9b8a7f6e5d4c3b2a1f0e9d8c7b6a5f4e3d2c1b0a9f8e" Jwt__Issuer="GestionProduccion" Jwt__Audience="GestionProduccionAPI" `
  --cpu 0.5 --memory 1.0Gi

# Result
$AppUrl = az containerapp show --name $WebAppName --resource-group $ResourceGroup --query properties.configuration.ingress.fqdn --output tsv
Write-Host "[*] Despliegue Completado!" -ForegroundColor Green
Write-Host "URL de la aplicacion: https://$AppUrl" -ForegroundColor Cyan
