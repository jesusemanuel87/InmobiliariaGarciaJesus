-- Script temporal para corregir FechaVencimiento basado en FechaPago existente
-- Ejecutar este script para actualizar los registros existentes

USE inmobiliaria;

-- Actualizar FechaVencimiento para pagos que tienen FechaPago
-- Establecer el día 10 del mes correspondiente a FechaPago
UPDATE Pagos 
SET FechaVencimiento = STR_TO_DATE(
    CONCAT(
        YEAR(FechaPago),
        '-',
        LPAD(MONTH(FechaPago), 2, '0'),
        '-10'
    ), '%Y-%m-%d'
)
WHERE FechaPago IS NOT NULL 
  AND (FechaVencimiento IS NULL OR FechaVencimiento = '0001-01-01' OR FechaVencimiento = CURDATE());


UPDATE Pagos 
SET FechaVencimiento = STR_TO_DATE(
    CONCAT(
        YEAR(FechaPago), '-', 
        LPAD(MONTH(FechaPago), 2, '0'), 
        '-10'
    ), '%Y-%m-%d'
)
WHERE FechaPago IS NOT NULL 
  AND (FechaVencimiento IS NULL 
       OR FechaVencimiento = '0001-01-01' 
       OR DATE(FechaVencimiento) = CURDATE());

-- Verificar los cambios realizados
SELECT 
    Id,
    Numero,
    ContratoId,
    FechaPago,
    FechaVencimiento,
    'Actualizado correctamente' as Estado
FROM Pagos 
WHERE FechaPago IS NOT NULL
ORDER BY ContratoId, Numero;

SELECT 'Script de corrección de FechaVencimiento ejecutado exitosamente' AS mensaje;
