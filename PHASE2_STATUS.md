# FASE 2 - ESTADO ACTUAL Y OPCIONES

**Fecha:** Día 2  
**Problema:** Migration parcialmente aplicada  
**Estado:** BD en estado inconsistente

---

## ?? PROBLEMA

La migration se aplicó parcialmente en el primer intento antes de fijar el error de `Quantidade`. Ahora:

- ? Tabla `Usuarios` renombrada a `Users`
- ? Algunas columnas en `Users` renombradas
- ? Tabla `OrdensProducao` renombrada a `ProductionOrders` 
- ? La migration intenta volver a renombrar (falla porque ya existe)

---

## ??? SOLUCIONES POSIBLES

### OPCIÓN A: Limpiar BD y empezar de cero (RECOMENDADO)

Necesitamos restaurar desde backup y volver a intentar.

```bash
# 1. Restaurar backup
mysql -u root -p'Cualquiera1' GestionProduccionDB < ./backups/backup_before_normalization_XXXXXX.sql

# 2. Remover migration fallida
dotnet ef migrations remove

# 3. Crear nueva migration CORRECTA
dotnet ef migrations add DbNormalizationToEnglish

# 4. Aplicar migration
dotnet ef database update
```

### OPCIÓN B: Limpiar BD manualmente (Avanzado)

Si no hay backup, podemos limpiar directo en MySQL:

```sql
DROP TABLE IF EXISTS ProductionHistories;
DROP TABLE IF EXISTS ProductionOrders;
DROP TABLE IF EXISTS Users;

-- Esperar a que se ejecute, luego:
-- - Restaurar tabla antigua Usuarios si existe
-- - O recrear desde schema original
```

---

## ?? PRÓXIMOS PASOS

Recomiendo:

1. ? Listar backups creados
2. ? Restaurar el más reciente
3. ? Remover migration fallida
4. ? Intentar aplicación de nuevo CON LA CORRECCIÓN

---

**Estado del documento:** Pendiente acción  
**Requiere:** Decisión de usuario sobre qué hacer

