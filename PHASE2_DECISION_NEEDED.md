# FASE 2 - ESTRATEGIA PRAGMÁTICA FINAL

**Estado:** BD muy inconsistente después de múltiples intentos  
**Decisión:** Tomar aproximación diferente

---

## ?? NUEVA ESTRATEGIA

En lugar de luchar con EF Core migrations que están parcialmente aplicadas, vamos a:

### PASO 1: Borrar TODO de DB y código
```bash
# 1. Drop todas las migrations en código (excepto InitialCreate)
# 2. Drop complete BD
dotnet ef database drop -f

# 3. Recrear BD desde InitialCreate
dotnet ef database update 0
```

### PASO 2: Ejecutar SQL directamente contra BD
Ejecutar el script SQL `normalize_db_to_english.sql` directamente vía MySQL client

### PASO 3: Reconstruir migrations con snapshot actualizado
```bash
# Una vez BD esté normalizada, generar migration que capture el nuevo estado
dotnet ef migrations add DbNormalizationComplete
dotnet ef database update
```

---

## ?? ALTERNATIVA: Simplificar y SOLO hacer cambios de Código

Si Fase 2 (BD) es demasiado compleja, podemos:

1. ? Mantener BD en portugués (como está ahora)
2. ? Dejar los mapeos `HasColumnName()` en AppDbContext
3. ?? Enfocarse en FASES 3-7:
   - Completar servicios
   - Crear controllers
   - Traducir UI
   - Integrar SignalR
   - Testing

**Ventaja:** MVP funcional sin problemas con migraciones  
**Desventaja:** BD sigue en portugués (puede mejorarse después)

---

## ?? RECOMENDACIÓN

Dado el tiempo invertido en FASE 2, recomiendo:

**? Pausar FASE 2 normalización BD**  
**? Continuar con FASES 3-7 (lógica de aplicación)**  
**? Volver a FASE 2 después (baja prioridad)

Esto permitirá tener MVP funcional sin quedar atrapados en problemas de migraciones.

---

**Estado:** Esperando decisión  
**Opciones claras arriba**  
**¿Qué preferís?**

