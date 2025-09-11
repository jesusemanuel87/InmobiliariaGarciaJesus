-- Script para crear usuario administrador por defecto
-- Ejecutar este script si el login falla con "Credenciales inválidas"

-- Verificar si existe el empleado admin
SELECT 'Verificando empleado admin...' as mensaje;
SELECT COUNT(*) as empleados_admin FROM Empleados WHERE Email = 'admin@inmobiliaria.com';

-- Crear empleado administrador si no existe
INSERT IGNORE INTO Empleados (Nombre, Apellido, Dni, Telefono, Email, Rol, FechaIngreso, Observaciones, FechaCreacion, Estado) 
VALUES ('Admin', 'Sistema', '00000000', '0000000000', 'admin@inmobiliaria.com', 2, CURRENT_DATE, 'Usuario administrador del sistema', NOW(), 1);

-- Verificar si existe el usuario admin
SELECT 'Verificando usuario admin...' as mensaje;
SELECT COUNT(*) as usuarios_admin FROM Usuarios WHERE Email = 'admin@inmobiliaria.com';

-- Crear usuario administrador si no existe
-- Password: admin123
-- Hash BCrypt generado: $2a$11$8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8O (este es un ejemplo, usar el hash correcto)
INSERT IGNORE INTO Usuarios (NombreUsuario, Email, ClaveHash, Rol, EmpleadoId, FechaCreacion, Estado) 
SELECT 'admin', 'admin@inmobiliaria.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 4, e.Id, NOW(), 1
FROM Empleados e 
WHERE e.Email = 'admin@inmobiliaria.com';

-- Verificar resultado final
SELECT 'Resultado final:' as mensaje;
SELECT u.Id, u.NombreUsuario, u.Email, u.Rol, u.Estado, e.Nombre, e.Apellido 
FROM Usuarios u 
JOIN Empleados e ON u.EmpleadoId = e.Id 
WHERE u.Email = 'admin@inmobiliaria.com';
