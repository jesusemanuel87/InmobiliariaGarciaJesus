-- ============================================================================
-- Migración: Permitir mismo email con roles diferentes (Multi-rol)
-- Fecha: 2025-01-18
-- Descripción: Elimina restricción UNIQUE de Email y agrega UNIQUE(Email, Rol)
-- ============================================================================

USE inmobiliaria;

-- ============================================================================
-- PASO 1: Verificar estado actual
-- ============================================================================
SELECT 
    CONSTRAINT_NAME,
    COLUMN_NAME,
    TABLE_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'Usuarios' 
  AND TABLE_SCHEMA = 'inmobiliaria'
  AND CONSTRAINT_NAME LIKE '%Email%';

-- ============================================================================
-- PASO 2: Eliminar índice UNIQUE de Email (si existe)
-- ============================================================================
-- Nota: El nombre del índice puede variar según cómo se creó la tabla
-- Intenta varios nombres posibles

-- Opción 1: Índice con nombre generado automáticamente
DROP INDEX Email ON Usuarios;

-- Si el anterior falla, intenta estos:
-- DROP INDEX UK_Usuarios_Email ON Usuarios;
-- DROP INDEX idx_usuarios_email ON Usuarios;

-- ============================================================================
-- PASO 3: Agregar índice UNIQUE compuesto (Email + Rol)
-- ============================================================================
ALTER TABLE Usuarios
ADD UNIQUE INDEX UK_Usuarios_Email_Rol (Email, Rol);

-- Agregar índice normal para Email (para búsquedas rápidas)
CREATE INDEX idx_usuarios_email ON Usuarios(Email);

-- ============================================================================
-- PASO 4: Verificar cambios
-- ============================================================================
SHOW INDEX FROM Usuarios WHERE Key_name LIKE '%Email%' OR Key_name LIKE '%Rol%';

-- ============================================================================
-- PRUEBAS DE VALIDACIÓN
-- ============================================================================

-- Esta consulta debería mostrar usuarios con mismo email pero roles diferentes
SELECT 
    Email,
    Rol,
    CASE Rol
        WHEN 1 THEN 'Propietario'
        WHEN 2 THEN 'Inquilino'
        WHEN 3 THEN 'Empleado'
        WHEN 4 THEN 'Administrador'
    END AS RolNombre,
    NombreUsuario,
    Estado
FROM Usuarios
WHERE Email IN (
    SELECT Email 
    FROM Usuarios 
    GROUP BY Email 
    HAVING COUNT(*) > 1
)
ORDER BY Email, Rol;

-- ============================================================================
-- NOTAS IMPORTANTES
-- ============================================================================
-- 1. Esta migración permite que un mismo email tenga múltiples usuarios con roles diferentes
-- 2. El NombreUsuario sigue siendo ÚNICO globalmente (no cambió)
-- 3. La validación en login debe buscar por Email + Contraseña, luego el usuario elegirá el rol
-- 4. Cada combinación Email+Rol es única
-- 5. Ejemplo válido:
--    - usuario1@mail.com + Propietario (ID: 1)
--    - usuario1@mail.com + Inquilino (ID: 2)
-- 6. Ejemplo INVÁLIDO (rechazado):
--    - usuario1@mail.com + Propietario (ID: 1) ✅
--    - usuario1@mail.com + Propietario (ID: 3) ❌ Duplicado Email+Rol

COMMIT;
