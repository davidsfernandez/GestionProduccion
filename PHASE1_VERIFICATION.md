# ? VERIFICACIÓN PRE-DEVELOPMENT - FASE 1

**Fecha:** Febrero 2026  
**Estado:** Verificación de Sistemas

---

## ?? CHECKLIST DE PREPARACIÓN

### ? GIT Y CONTROL DE VERSIONES

```
[?] Rama feature/db-normalization creada
[?] Cambios en rama correcta
[ ] Documentación de análisis commitida
[ ] Cambios de código limpios (sin build artifacts)
```

### ? BASE DE DATOS

```
[ ] MySQL está corriendo (verificar con `mysql -u root -p`)
[ ] BD GestionProduccionDB existe
[ ] Backup creado en ./backups/
[ ] Backup verificado (archivo > 100 KB)
[ ] Connection string válida en appsettings.json
```

### ? PROYECTOS .NET

```
[ ] Visual Studio / VS Code abierto
[ ] GestionProduccion.csproj carga sin errores
[ ] GestionProduccion.Client.csproj carga sin errores
[ ] Restaurar dependencias: `dotnet restore`
[ ] Build sin errores: `dotnet build`
```

### ? ENTIDAD FRAMEWORK

```
[ ] dotnet ef tools instaladas: `dotnet tool list -g`
[ ] Ver migraciones: `dotnet ef migrations list`
[ ] Current DB schema actualizado
```

### ? DOCUMENTACIÓN

```
[?] ClientRequeriments.txt ampliado
[?] ARCHITECTURE_ANALYSIS.md creado
[?] ACTION_PLAN.md creado
[?] EXECUTIVE_SUMMARY.md creado
[?] USEFUL_COMMANDS.md creado
[?] DOCUMENTATION_INDEX.md creado
[?] STATUS_REPORT.md creado
[?] BACKUP_INSTRUCTIONS.md creado
[?] Este archivo de verificación
```

---

## ?? COMANDOS DE VERIFICACIÓN RÁPIDA

### Verificar MySQL

```bash
# Conectar a MySQL y verificar BD
mysql -u root -p
# Escribir contraseña: Cualquiera1
# En MySQL prompt:
mysql> SHOW DATABASES;
mysql> USE GestionProduccionDB;
mysql> SHOW TABLES;
mysql> QUIT;
```

### Verificar .NET

```bash
# Backend
cd GestionProduccion
dotnet build

# Frontend
cd Client/GestionProduccion.Client
dotnet build

# Regresar
cd ../..
```

### Verificar EF Core

```bash
cd GestionProduccion
dotnet ef --version
dotnet ef migrations list
```

---

## ?? ANTES DE EMPEZAR FASE 2

**Completar el siguiente checklist:**

```
?? SISTEMAS LISTA PARA USAR
???????????????????????????????????????????
? [ ] MySQL está running                  ?
? [ ] Backup creado y verificado          ?
? [ ] Rama feature/db-normalization       ?
? [ ] Documentación actualizada           ?
? [ ] dotnet build sin errores            ?
? [ ] ef migrations list funciona         ?
???????????????????????????????????????????

SI TODOS ESTÁN ?: Proceder a FASE 2
SI ALGUNO ESTÁ ?: Resolver antes de continuar
```

---

## ?? REFERENCIAS RÁPIDAS

**Si necesitas ayuda:**
- Migraciones: Ver `USEFUL_COMMANDS.md` ? "Migraciones de BD"
- BD: Ver `BACKUP_INSTRUCTIONS.md`
- Comandos: Ver `USEFUL_COMMANDS.md`
- Plan: Ver `ACTION_PLAN.md` ? "FASE 2"

---

**Siguiente paso:** Completar verificaciones y proceder a FASE 2 (Normalización de BD)

