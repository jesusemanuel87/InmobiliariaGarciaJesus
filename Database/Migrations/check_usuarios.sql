USE inmobiliaria;

-- Ver todos los usuarios registrados
SELECT 
    Id,
    NombreUsuario,
    Email,
    CASE Rol
        WHEN 1 THEN 'Propietario'
        WHEN 2 THEN 'Inquilino'
        WHEN 3 THEN 'Empleado'
        WHEN 4 THEN 'Administrador'
    END AS RolNombre,
    Rol,
    Estado,
    PropietarioId,
    InquilinoId,
    EmpleadoId
FROM Usuarios
ORDER BY Id;
