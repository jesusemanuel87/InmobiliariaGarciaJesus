-- Script de actualización para agregar columnas de intereses, multas y fecha de vencimiento a la tabla Pagos
-- Ejecutar este script para actualizar la base de datos existente
USE inmobiliaria;
-- Agregar nuevas columnas a la tabla Pagos
ALTER TABLE Pagos 
ADD Intereses DECIMAL(18,2) NOT NULL DEFAULT 0,
ADD Multas DECIMAL(18,2) NOT NULL DEFAULT 0,
ADD FechaVencimiento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
-- Actualizar registros existentes para establecer fecha de vencimiento
-- Asumiendo que los pagos existentes vencen el día 10 del mes correspondiente
UPDATE Pagos 
SET FechaVencimiento = STR_TO_DATE(
    CONCAT(
        YEAR(IFNULL(FechaPago, DATE_ADD(CURDATE(), INTERVAL Numero-1 MONTH))),
        '-',
        LPAD(MONTH(IFNULL(FechaPago, DATE_ADD(CURDATE(), INTERVAL Numero-1 MONTH))), 2, '0'),
        '-10'
    ), '%Y-%m-%d'
)
WHERE FechaVencimiento = CURDATE();


-- Agregar configuraciones para intereses y multas si no existen
INSERT INTO Configuraciones (Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion)
SELECT 'INTERES_VENCIMIENTO_10_20', '10', 'Porcentaje de interés aplicado del día 11 al 20 después del vencimiento', 4, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM Configuraciones WHERE Clave = 'INTERES_VENCIMIENTO_10_20');

INSERT INTO Configuraciones (Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion)
SELECT 'INTERES_VENCIMIENTO_20_PLUS', '15', 'Porcentaje de interés aplicado del día 21 hasta fin de mes', 5, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM Configuraciones WHERE Clave = 'INTERES_VENCIMIENTO_20_PLUS');

INSERT INTO Configuraciones (Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion)
SELECT 'INTERES_VENCIMIENTO_MENSUAL', '20', 'Porcentaje de interés aplicado después del mes de vencimiento', 6, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM Configuraciones WHERE Clave = 'INTERES_VENCIMIENTO_MENSUAL');

INSERT INTO Configuraciones (Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion)
SELECT 'MULTA_TERMINACION_TEMPRANA', '2', 'Meses de multa por terminación temprana del contrato', 2, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM Configuraciones WHERE Clave = 'MULTA_TERMINACION_TEMPRANA');

INSERT INTO Configuraciones (Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion)
SELECT 'MULTA_TERMINACION_TARDIA', '1', 'Meses de multa por terminación tardía del contrato', 3, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM Configuraciones WHERE Clave = 'MULTA_TERMINACION_TARDIA');

-- Crear índices para mejorar el rendimiento
CREATE INDEX IX_Pagos_FechaVencimiento ON Pagos(FechaVencimiento);
CREATE INDEX IX_Pagos_Estado_FechaVencimiento ON Pagos(Estado, FechaVencimiento);

SELECT 'Actualización de tabla Pagos completada exitosamente' AS mensaje;
SELECT 'Se agregaron columnas: Intereses, Multas, FechaVencimiento' AS mensaje;
SELECT 'Se agregaron configuraciones de intereses y multas' AS mensaje;
