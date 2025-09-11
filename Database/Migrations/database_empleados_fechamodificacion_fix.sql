-- Script para agregar la columna FechaModificacion faltante en la tabla Empleados
-- Esta columna es requerida por el EmpleadoRepository

-- Verificar estructura actual de la tabla Empleados
SELECT 'Estructura actual de la tabla Empleados:' as mensaje;
DESCRIBE Empleados;

-- Agregar la columna FechaModificacion si no existe
ALTER TABLE Empleados 
ADD COLUMN IF NOT EXISTS FechaModificacion DATETIME NULL 
AFTER FechaCreacion;

-- Verificar que la columna se agregó correctamente
SELECT 'Estructura actualizada de la tabla Empleados:' as mensaje;
DESCRIBE Empleados;

-- Actualizar registros existentes con FechaModificacion = FechaCreacion
UPDATE Empleados 
SET FechaModificacion = FechaCreacion 
WHERE FechaModificacion IS NULL;

SELECT 'Columna FechaModificacion agregada exitosamente a la tabla Empleados' as mensaje;
