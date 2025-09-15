-- Agregar campos de ubicación a la tabla Inmuebles
-- Fecha: 2025-01-07
-- Descripción: Agrega campos Localidad y Provincia para mejorar la información de ubicación

USE InmobiliariaDB;
GO

-- Agregar columnas Localidad y Provincia
ALTER TABLE Inmuebles 
ADD Localidad NVARCHAR(100) NULL,
    Provincia NVARCHAR(100) NULL;
GO

-- Verificar que las columnas se agregaron correctamente
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Inmuebles' 
AND COLUMN_NAME IN ('Localidad', 'Provincia');
GO

PRINT 'Campos de ubicación agregados exitosamente a la tabla Inmuebles';
