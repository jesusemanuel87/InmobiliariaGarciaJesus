-- Migración: Agregar campos de auditoría a la tabla pagos
-- Fecha: 2025-01-27
-- Descripción: Agrega campos para rastrear quién creó y anuló pagos

USE inmobiliaria;

-- Agregar campos de auditoría a pagos
ALTER TABLE pagos 
ADD COLUMN CreadoPorId INT NULL AFTER FechaCreacion,
ADD COLUMN AnuladoPorId INT NULL AFTER CreadoPorId,
ADD COLUMN FechaAnulacion DATETIME NULL AFTER AnuladoPorId;

-- Agregar foreign keys para auditoría
ALTER TABLE pagos
ADD CONSTRAINT fk_pagos_creado_por 
    FOREIGN KEY (CreadoPorId) REFERENCES usuarios(Id) 
    ON DELETE SET NULL ON UPDATE CASCADE,
ADD CONSTRAINT fk_pagos_anulado_por 
    FOREIGN KEY (AnuladoPorId) REFERENCES usuarios(Id) 
    ON DELETE SET NULL ON UPDATE CASCADE;

-- Agregar índices para mejorar rendimiento
ALTER TABLE pagos
ADD INDEX idx_pagos_creado_por (CreadoPorId),
ADD INDEX idx_pagos_anulado_por (AnuladoPorId),
ADD INDEX idx_pagos_fecha_anulacion (FechaAnulacion);

-- Actualizar registros existentes con el usuario administrador por defecto
-- (Asumiendo que el usuario con ID 1 es el administrador principal)
UPDATE pagos 
SET CreadoPorId = 3 
WHERE CreadoPorId IS NULL;

-- Verificar migración
SELECT 'Migración de auditoría para pagos completada' as Status;

SELECT 
    p.Id,
    p.FechaCreacion,
    u1.NombreUsuario as CreadoPor,
    p.FechaAnulacion,
    u2.NombreUsuario as AnuladoPor,
    p.Estado
FROM pagos p
LEFT JOIN usuarios u1 ON p.CreadoPorId = u1.Id
LEFT JOIN usuarios u2 ON p.AnuladoPorId = u2.Id
LIMIT 5;
