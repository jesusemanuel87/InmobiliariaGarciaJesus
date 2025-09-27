-- Migración: Agregar campos de auditoría a la tabla contratos
-- Fecha: 2025-01-27
-- Descripción: Agrega campos para rastrear quién creó y terminó contratos

USE inmobiliaria;

-- Agregar campos de auditoría a contratos
ALTER TABLE contratos 
ADD COLUMN CreadoPorId INT NULL AFTER FechaCreacion,
ADD COLUMN TerminadoPorId INT NULL AFTER CreadoPorId,
ADD COLUMN FechaTerminacion DATETIME NULL AFTER TerminadoPorId;

-- Agregar foreign keys para auditoría
ALTER TABLE contratos
ADD CONSTRAINT fk_contratos_creado_por 
    FOREIGN KEY (CreadoPorId) REFERENCES usuarios(Id) 
    ON DELETE SET NULL ON UPDATE CASCADE,
ADD CONSTRAINT fk_contratos_terminado_por 
    FOREIGN KEY (TerminadoPorId) REFERENCES usuarios(Id) 
    ON DELETE SET NULL ON UPDATE CASCADE;

-- Agregar índices para mejorar rendimiento
ALTER TABLE contratos
ADD INDEX idx_creado_por (CreadoPorId),
ADD INDEX idx_terminado_por (TerminadoPorId),
ADD INDEX idx_fecha_terminacion (FechaTerminacion);

-- Actualizar registros existentes con el usuario administrador por defecto
-- (Asumiendo que el usuario con ID 1 es el administrador principal)
UPDATE contratos 
SET CreadoPorId = 3 
WHERE CreadoPorId IS NULL;

-- Verificar migración
SELECT 'Migración de auditoría para contratos completada' as Status;

SELECT 
    c.Id,
    c.FechaCreacion,
    u1.NombreUsuario as CreadoPor,
    c.FechaTerminacion,
    u2.NombreUsuario as TerminadoPor,
    c.Estado
FROM contratos c
LEFT JOIN usuarios u1 ON c.CreadoPorId = u1.Id
LEFT JOIN usuarios u2 ON c.TerminadoPorId = u2.Id
LIMIT 5;
