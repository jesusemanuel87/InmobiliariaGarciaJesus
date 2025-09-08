-- Script para corregir el usuario administrador
-- Este script elimina el usuario admin existente (con hash incorrecto) 
-- para que el DatabaseSeederService pueda recrearlo correctamente

-- Verificar usuario admin actual
SELECT 'Usuario admin actual:' as mensaje;
SELECT u.Id, u.NombreUsuario, u.Email, u.ClaveHash, u.Estado, e.Nombre, e.Apellido 
FROM Usuarios u 
LEFT JOIN Empleados e ON u.EmpleadoId = e.Id 
WHERE u.Email = 'admin@inmobiliaria.com';

-- Eliminar usuario admin existente (esto permitirá que el seeder lo recree)
DELETE FROM Usuarios WHERE Email = 'admin@inmobiliaria.com';

-- Eliminar empleado admin existente
DELETE FROM Empleados WHERE Email = 'admin@inmobiliaria.com';

-- Verificar que se eliminaron
SELECT 'Verificando eliminación:' as mensaje;
SELECT COUNT(*) as usuarios_admin FROM Usuarios WHERE Email = 'admin@inmobiliaria.com';
SELECT COUNT(*) as empleados_admin FROM Empleados WHERE Email = 'admin@inmobiliaria.com';

SELECT 'Usuario admin eliminado. Reinicia la aplicación para que se recree automáticamente con el hash correcto.' as mensaje;
