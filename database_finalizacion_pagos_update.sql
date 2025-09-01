-- Script de migración para agregar campos de finalización de contratos y métodos de pago
-- Ejecutar en la base de datos inmobiliaria_db

-- Agregar campos al modelo Contrato para finalización
ALTER TABLE contratos 
ADD COLUMN fecha_finalizacion_real DATE NULL,
ADD COLUMN multa_finalizacion DECIMAL(15,2) NULL,
ADD COLUMN meses_adeudados INT NULL,
ADD COLUMN importe_adeudado DECIMAL(15,2) NULL;

-- Agregar campos al modelo Pago para métodos de pago y observaciones
ALTER TABLE pagos 
ADD COLUMN metodo_pago ENUM('MercadoPago', 'Transferencia', 'Efectivo', 'Cheque', 'Otro') NULL,
ADD COLUMN observaciones VARCHAR(500) NULL;

-- Insertar configuraciones de multa si no existen
INSERT IGNORE INTO configuracion (clave, valor, descripcion, tipo, fecha_creacion, fecha_modificacion) VALUES
('MULTA_MENOR_MITAD', '50', 'Porcentaje de multa cuando se cumple menos del 50% del contrato', 2, NOW(), NOW()),
('MULTA_MAYOR_MITAD', '25', 'Porcentaje de multa cuando se cumple 50% o más del contrato', 2, NOW(), NOW());

-- Verificar que las columnas se agregaron correctamente
DESCRIBE contratos;
DESCRIBE pagos;

-- Verificar configuraciones insertadas
SELECT * FROM configuracion WHERE clave IN ('MULTA_MENOR_MITAD', 'MULTA_MAYOR_MITAD');

-- Comentarios sobre los nuevos campos:
-- 
-- CONTRATOS:
-- - fecha_finalizacion_real: Fecha real de finalización del contrato (preserva fecha original)
-- - multa_finalizacion: Monto de multa calculada por finalización temprana
-- - meses_adeudados: Cantidad de meses con pagos atrasados al momento de finalización
-- - importe_adeudado: Monto total adeudado al momento de finalización
--
-- PAGOS:
-- - metodo_pago: Método utilizado para realizar el pago
-- - observaciones: Notas adicionales sobre el pago
