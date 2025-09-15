USE `inmobiliaria`;

-- Crear tabla de configuraciones del sistema
CREATE TABLE IF NOT EXISTS configuraciones (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Clave VARCHAR(100) NOT NULL UNIQUE,
    Valor VARCHAR(500) NOT NULL,
    Descripcion VARCHAR(200),
    Tipo INT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_clave (Clave),
    INDEX idx_tipo (Tipo)
);

-- Insertar configuraciones por defecto
INSERT INTO configuraciones (Clave, Valor, Descripcion, Tipo) VALUES
-- Meses mínimos (todos habilitados por defecto)
('MESES_MINIMOS_6', 'true', 'Opción de 6 meses de alquiler mínimo', 1),
('MESES_MINIMOS_12', 'true', 'Opción de 12 meses de alquiler mínimo', 1),
('MESES_MINIMOS_18', 'true', 'Opción de 18 meses de alquiler mínimo', 1),
('MESES_MINIMOS_24', 'true', 'Opción de 24 meses de alquiler mínimo', 1),
('MESES_MINIMOS_30', 'true', 'Opción de 30 meses de alquiler mínimo', 1),
('MESES_MINIMOS_36', 'true', 'Opción de 36 meses de alquiler mínimo', 1),
-- Multas por terminación
('MULTA_TERMINACION_TEMPRANA', '2', 'Meses de multa si se cumplió menos de la mitad del contrato', 2),
('MULTA_TERMINACION_TARDIA', '1', 'Meses de multa si se cumplió más de la mitad del contrato', 3),
-- Intereses por vencimiento
('INTERES_VENCIMIENTO_10_20', '10', 'Porcentaje de interés para vencimientos de 10-20 días', 4),
('INTERES_VENCIMIENTO_20_PLUS', '15', 'Porcentaje de interés para vencimientos de más de 20 días', 5),
('INTERES_VENCIMIENTO_MENSUAL', '20', 'Porcentaje de interés mensual', 6)

ON DUPLICATE KEY UPDATE 
    Valor = VALUES(Valor),
    Descripcion = VALUES(Descripcion),
    FechaModificacion = CURRENT_TIMESTAMP;

-- Comentarios sobre los tipos de configuración:
-- Tipo 1: MesesMinimos
-- Tipo 2: MultaTerminacionTemprana  
-- Tipo 3: MultaTerminacionTardia
-- Tipo 4: InteresVencimiento (10-20 días)
-- Tipo 5: InteresVencimientoExtendido (20+ días)
-- Tipo 6: InteresVencimientoMensual
