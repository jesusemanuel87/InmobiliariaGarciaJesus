-- =============================================
-- Script: Agregar columna Disponible a tabla inmuebles
-- Descripción: Agrega la columna Disponible (BOOLEAN) a la tabla inmuebles
-- Fecha: 2025-01-19
-- =============================================

USE inmobiliaria;

-- Verificar si la columna ya existe antes de agregarla
SET @column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'inmobiliaria'
    AND TABLE_NAME = 'inmuebles'
    AND COLUMN_NAME = 'Disponible'
);

-- Agregar columna Disponible si no existe
SET @sql = IF(@column_exists = 0,
    'ALTER TABLE inmuebles ADD COLUMN Disponible TINYINT(1) NOT NULL DEFAULT 1 AFTER PropietarioId',
    'SELECT "La columna Disponible ya existe" AS Mensaje'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar que la columna se agregó correctamente
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    COLUMN_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'inmobiliaria'
    AND TABLE_NAME = 'inmuebles'
    AND COLUMN_NAME = 'Disponible';

-- Actualizar inmuebles existentes según su estado
-- Si Estado = 'Activo' -> Disponible = 1
-- Si Estado = 'Inactivo' -> Disponible = 0
UPDATE inmuebles
SET Disponible = CASE 
    WHEN Estado = 'Activo' THEN 1
    ELSE 0
END
WHERE Disponible IS NULL OR TRUE; -- Actualizar todos

-- Mostrar resumen
SELECT 
    Estado,
    Disponible,
    COUNT(*) AS Cantidad
FROM inmuebles
GROUP BY Estado, Disponible
ORDER BY Estado, Disponible;

SELECT CONCAT('✅ Migración completada. Columna Disponible agregada exitosamente.') AS Resultado;
