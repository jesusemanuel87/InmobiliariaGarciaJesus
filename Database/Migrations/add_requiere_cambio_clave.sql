-- ===================================================================
-- Migración: Agregar campo RequiereCambioClave a tabla Usuarios
-- Fecha: 2025-01-18
-- Descripción: Agrega campo para forzar cambio de contraseña en primer login
-- ===================================================================

USE inmobiliaria;

-- Verificar si la columna ya existe
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'inmobiliaria'
  AND TABLE_NAME = 'Usuarios'
  AND COLUMN_NAME = 'RequiereCambioClave';

-- Agregar columna si no existe
ALTER TABLE Usuarios
ADD COLUMN IF NOT EXISTS RequiereCambioClave TINYINT(1) NOT NULL DEFAULT 0
COMMENT 'Indica si el usuario debe cambiar su contraseña en el próximo login';

-- Actualizar usuarios existentes sin password propio (si tienen DNI como contraseña)
-- Por seguridad, marcar que requieren cambio de clave
UPDATE Usuarios u
INNER JOIN Propietarios p ON u.PropietarioId = p.Id
SET u.RequiereCambioClave = 1
WHERE u.PropietarioId IS NOT NULL
  AND u.RequiereCambioClave = 0;

UPDATE Usuarios u
INNER JOIN Inquilinos i ON u.InquilinoId = i.Id
SET u.RequiereCambioClave = 1
WHERE u.InquilinoId IS NOT NULL
  AND u.RequiereCambioClave = 0;

-- Verificar resultado
SELECT 
    u.Id,
    u.NombreUsuario,
    u.Email,
    CASE u.Rol
        WHEN 1 THEN 'Propietario'
        WHEN 2 THEN 'Inquilino'
        WHEN 3 THEN 'Empleado'
        WHEN 4 THEN 'Administrador'
    END AS Rol,
    u.Estado,
    u.RequiereCambioClave
FROM Usuarios u
ORDER BY u.FechaCreacion DESC
LIMIT 10;

-- ===================================================================
-- Script exitoso
-- ===================================================================
