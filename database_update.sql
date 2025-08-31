-- Agregar columna MotivoCancelacion a la tabla contratos
USE inmobiliaria_db;

ALTER TABLE contratos ADD COLUMN MotivoCancelacion TEXT NULL;

-- Verificar que la columna se agregó correctamente
DESCRIBE contratos;
