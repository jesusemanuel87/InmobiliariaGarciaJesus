-- Script para crear las tablas del sistema de autenticación
-- Ejecutar después de la base de datos principal

USE inmobiliaria;

-- Tabla Empleados
CREATE TABLE IF NOT EXISTS Empleados (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL,
    Dni NVARCHAR(20) NOT NULL UNIQUE,
    Telefono NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Rol INT NOT NULL DEFAULT 1, -- 1: Empleado, 2: Administrador
    FechaIngreso DATE NOT NULL DEFAULT (CURRENT_DATE),
    Observaciones NVARCHAR(500) NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaCreacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,    
    INDEX idx_empleados_dni (Dni),
    INDEX idx_empleados_email (Email),
    INDEX idx_empleados_estado (Estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla Usuarios
CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreUsuario NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    ClaveHash NVARCHAR(255) NOT NULL,
    FotoPerfil NVARCHAR(255) NULL,
    Rol INT NOT NULL, -- 1: Propietario, 2: Inquilino, 3: Empleado, 4: Administrador
    FechaCreacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UltimoAcceso TIMESTAMP NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,    
    -- Foreign Keys (solo uno será válido según el rol)
    EmpleadoId INT NULL,
    PropietarioId INT NULL,
    InquilinoId INT NULL,    
    FOREIGN KEY (EmpleadoId) REFERENCES Empleados(Id) ON DELETE SET NULL,
    FOREIGN KEY (PropietarioId) REFERENCES Propietarios(Id) ON DELETE SET NULL,
    FOREIGN KEY (InquilinoId) REFERENCES Inquilinos(Id) ON DELETE SET NULL,
    INDEX idx_usuarios_email (Email),
    INDEX idx_usuarios_nombreusuario (NombreUsuario),
    INDEX idx_usuarios_rol (Rol),
    INDEX idx_usuarios_estado (Estado),
    INDEX idx_usuarios_empleado (EmpleadoId),
    INDEX idx_usuarios_propietario (PropietarioId),
    INDEX idx_usuarios_inquilino (InquilinoId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Trigger para validar que solo un FK esté activo según el rol
DELIMITER //
CREATE TRIGGER tr_usuarios_validate_role_fk
BEFORE INSERT ON Usuarios
FOR EACH ROW
BEGIN
    -- Validar que solo el FK correspondiente al rol esté presente
    IF NEW.Rol = 1 THEN -- Propietario
        IF NEW.PropietarioId IS NULL 
           OR NEW.EmpleadoId IS NOT NULL 
           OR NEW.InquilinoId IS NOT NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Usuario Propietario debe tener PropietarioId válido y otros FKs nulos';
        END IF;
    ELSEIF NEW.Rol = 2 THEN -- Inquilino
        IF NEW.InquilinoId IS NULL 
           OR NEW.EmpleadoId IS NOT NULL 
           OR NEW.PropietarioId IS NOT NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Usuario Inquilino debe tener InquilinoId válido y otros FKs nulos';
        END IF;
    ELSEIF NEW.Rol IN (3,4) THEN -- Empleado o Administrador
        IF NEW.EmpleadoId IS NULL 
           OR NEW.PropietarioId IS NOT NULL 
           OR NEW.InquilinoId IS NOT NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Usuario Empleado/Administrador debe tener EmpleadoId válido y otros FKs nulos';
        END IF;
    END IF;
END//
 DELIMITER //

CREATE TRIGGER tr_usuarios_validate_role_fk_update
BEFORE UPDATE ON Usuarios
FOR EACH ROW
BEGIN
    -- Validar que solo el FK correspondiente al rol esté presente
    IF NEW.Rol = 1 THEN -- Propietario
        IF NEW.PropietarioId IS NULL 
           OR NEW.EmpleadoId IS NOT NULL 
           OR NEW.InquilinoId IS NOT NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Usuario Propietario debe tener PropietarioId válido y otros FKs nulos';
        END IF;

    ELSEIF NEW.Rol = 2 THEN -- Inquilino
        IF NEW.InquilinoId IS NULL 
           OR NEW.EmpleadoId IS NOT NULL 
           OR NEW.PropietarioId IS NOT NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Usuario Inquilino debe tener InquilinoId válido y otros FKs nulos';
        END IF;

    ELSEIF NEW.Rol IN (3,4) THEN -- Empleado o Administrador
        IF NEW.EmpleadoId IS NULL 
           OR NEW.PropietarioId IS NOT NULL 
           OR NEW.InquilinoId IS NOT NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Usuario Empleado/Administrador debe tener EmpleadoId válido y otros FKs nulos';
        END IF;
    END IF;
END//

DELIMITER ;


-- Insertar empleado administrador por defecto
INSERT INTO Empleados (Nombre, Apellido, Dni, Telefono, Email, Rol, FechaIngreso, Observaciones) 
VALUES ('Admin', 'Sistema', '00000000', '0000000000', 'admin@inmobiliaria.com', 2, CURRENT_DATE, 'Usuario administrador del sistema')
ON DUPLICATE KEY UPDATE Email = VALUES(Email);

-- Insertar usuario administrador por defecto (password: admin123)
-- Hash BCrypt para 'admin123': $2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi
INSERT INTO Usuarios (NombreUsuario, Email, ClaveHash, Rol, EmpleadoId) 
SELECT 'admin', 'admin@inmobiliaria.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 4, e.Id
FROM Empleados e 
WHERE e.Email = 'admin@inmobiliaria.com'
ON DUPLICATE KEY UPDATE Email = VALUES(Email);

-- Crear vista para obtener información completa de usuarios
CREATE OR REPLACE VIEW vw_usuarios_completo AS
SELECT 
    u.Id,
    u.NombreUsuario,
    u.Email,
    u.FotoPerfil,
    u.Rol,
    u.FechaCreacion,
    u.UltimoAcceso,
    u.Estado,
    CASE u.Rol
        WHEN 1 THEN 'Propietario'
        WHEN 2 THEN 'Inquilino'
        WHEN 3 THEN 'Empleado'
        WHEN 4 THEN 'Administrador'
        ELSE 'Sin Rol'
    END AS RolDescripcion,
    CASE 
        WHEN u.Rol IN (1) THEN CONCAT(p.Nombre, ' ', p.Apellido)
        WHEN u.Rol IN (2) THEN CONCAT(i.Nombre, ' ', i.Apellido)
        WHEN u.Rol IN (3, 4) THEN CONCAT(e.Nombre, ' ', e.Apellido)
        ELSE 'Sin Nombre'
    END AS NombreCompleto,
    CASE 
        WHEN u.Rol IN (1) THEN p.Dni
        WHEN u.Rol IN (2) THEN i.Dni
        WHEN u.Rol IN (3, 4) THEN e.Dni
        ELSE NULL
    END AS Dni,
    CASE 
        WHEN u.Rol IN (1) THEN p.Telefono
        WHEN u.Rol IN (2) THEN i.Telefono
        WHEN u.Rol IN (3, 4) THEN e.Telefono
        ELSE NULL
    END AS Telefono,
    e.Rol as RolEmpleado,
    e.FechaIngreso
FROM Usuarios u
LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id
LEFT JOIN Empleados e ON u.EmpleadoId = e.Id;

COMMIT;
