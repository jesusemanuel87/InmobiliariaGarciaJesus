-- Script para agregar soporte de fotos de perfil
-- Agregar columna FotoPerfil a la tabla Empleados y Usuarios

-- Verificar estructura actual
SELECT 'Estructura actual de Empleados:' as mensaje;
DESCRIBE Empleados;

SELECT 'Estructura actual de Usuarios:' as mensaje;
DESCRIBE Usuarios;

-- Agregar columna FotoPerfil a Empleados
ALTER TABLE Empleados 
ADD COLUMN IF NOT EXISTS FotoPerfil VARCHAR(500) NULL 
AFTER Email;

-- Agregar columna FotoPerfil a Usuarios  
ALTER TABLE Usuarios
ADD COLUMN IF NOT EXISTS FotoPerfil VARCHAR(500) NULL
AFTER Email;

-- Crear directorio para fotos de perfil en el sistema de archivos
-- /uploads/profiles/

-- Verificar cambios
SELECT 'Estructura actualizada de Empleados:' as mensaje;
DESCRIBE Empleados;

SELECT 'Estructura actualizada de Usuarios:' as mensaje;
DESCRIBE Usuarios;

SELECT 'Columnas FotoPerfil agregadas exitosamente' as mensaje;
