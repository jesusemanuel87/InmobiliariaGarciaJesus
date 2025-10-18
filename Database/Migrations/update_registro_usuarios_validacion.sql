-- ============================================================================
-- Migración: Sistema de Validación de Registro de Usuarios
-- Fecha: 2025-01-18
-- Descripción: Mejora del sistema de registro con validación de administrador
-- ============================================================================

USE inmobiliaria;

-- ============================================================================
-- IMPORTANTE: Cambios en la lógica de registro
-- ============================================================================
-- Los nuevos registros de usuarios (Propietarios/Inquilinos) se crearán con:
-- 1. Usuario.Estado = FALSE (Inactivo - Requiere validación)
-- 2. Persona.Estado = FALSE (Propietario/Inquilino también inactivo)
-- 3. Un administrador debe activar manualmente la cuenta

-- ============================================================================
-- FUNCIONALIDADES IMPLEMENTADAS
-- ============================================================================
-- 1. Reutilización de Personas Existentes:
--    - Si un Propietario/Inquilino ya existe con el mismo DNI pero SIN usuario,
--      se reutiliza su ID en lugar de crear duplicados
--    - Si ya tiene usuario, se rechaza el registro con mensaje claro
--
-- 2. Estado Inactivo por Defecto:
--    - Nuevos usuarios creados con Estado = FALSE
--    - Nuevas personas (Propietario/Inquilino) creadas con Estado = FALSE
--    - Previene acceso hasta validación de administrador
--
-- 3. Validación en Login:
--    - Si usuario.Estado = FALSE, mensaje: "Cuenta pendiente de validación"
--    - Si persona.Estado = FALSE, credenciales inválidas
--    - Doble capa de seguridad

-- ============================================================================
-- CONSULTAS ÚTILES PARA ADMINISTRADORES
-- ============================================================================

-- Ver usuarios pendientes de validación
SELECT 
    u.Id,
    u.NombreUsuario,
    u.Email,
    u.Rol,
    CASE u.Rol
        WHEN 1 THEN CONCAT(p.Nombre, ' ', p.Apellido)
        WHEN 2 THEN CONCAT(i.Nombre, ' ', i.Apellido)
        ELSE 'N/A'
    END AS NombreCompleto,
    CASE u.Rol
        WHEN 1 THEN p.Dni
        WHEN 2 THEN i.Dni
        ELSE 'N/A'
    END AS DNI,
    u.FechaCreacion
FROM Usuarios u
LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id
WHERE u.Estado = 0  -- Usuarios inactivos
ORDER BY u.FechaCreacion DESC;

-- ============================================================================
-- ACTIVAR USUARIO VALIDADO (Ejecutar después de verificar identidad)
-- ============================================================================
-- Reemplazar {USUARIO_ID} con el ID del usuario a activar

/*
-- Activar usuario
UPDATE Usuarios 
SET Estado = 1 
WHERE Id = {USUARIO_ID};

-- Activar la persona asociada
UPDATE Propietarios 
SET Estado = 1 
WHERE Id = (SELECT PropietarioId FROM Usuarios WHERE Id = {USUARIO_ID} AND PropietarioId IS NOT NULL);

UPDATE Inquilinos 
SET Estado = 1 
WHERE Id = (SELECT InquilinoId FROM Usuarios WHERE Id = {USUARIO_ID} AND InquilinoId IS NOT NULL);
*/

-- ============================================================================
-- PROCEDIMIENTO ALMACENADO: Activar Usuario y Persona Asociada
-- ============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS sp_ActivarUsuario//

CREATE PROCEDURE sp_ActivarUsuario(
    IN p_usuario_id INT
)
BEGIN
    DECLARE v_rol INT;
    DECLARE v_persona_id INT;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Error al activar usuario';
    END;
    
    START TRANSACTION;
    
    -- Obtener rol y persona_id del usuario
    SELECT Rol, 
           CASE 
               WHEN Rol = 1 THEN PropietarioId
               WHEN Rol = 2 THEN InquilinoId
               ELSE NULL
           END
    INTO v_rol, v_persona_id
    FROM Usuarios
    WHERE Id = p_usuario_id;
    
    -- Activar usuario
    UPDATE Usuarios 
    SET Estado = 1 
    WHERE Id = p_usuario_id;
    
    -- Activar persona según el rol
    IF v_rol = 1 AND v_persona_id IS NOT NULL THEN
        UPDATE Propietarios 
        SET Estado = 1 
        WHERE Id = v_persona_id;
    ELSEIF v_rol = 2 AND v_persona_id IS NOT NULL THEN
        UPDATE Inquilinos 
        SET Estado = 1 
        WHERE Id = v_persona_id;
    END IF;
    
    COMMIT;
    
    SELECT 'Usuario activado exitosamente' AS Mensaje;
END//

DELIMITER ;

-- ============================================================================
-- EJEMPLO DE USO DEL PROCEDIMIENTO
-- ============================================================================
-- CALL sp_ActivarUsuario(123);  -- Reemplazar 123 con el ID del usuario

-- ============================================================================
-- VERIFICAR ACTIVACIÓN
-- ============================================================================
/*
SELECT 
    u.Id AS UsuarioId,
    u.NombreUsuario,
    u.Email,
    u.Estado AS UsuarioEstado,
    CASE 
        WHEN u.Rol = 1 THEN p.Estado
        WHEN u.Rol = 2 THEN i.Estado
        ELSE NULL
    END AS PersonaEstado
FROM Usuarios u
LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id
WHERE u.Id = {USUARIO_ID};
*/

-- ============================================================================
-- NOTAS PARA ADMINISTRADORES
-- ============================================================================
-- 1. Verificar identidad del usuario antes de activar (DNI, documentación)
-- 2. Validar que los datos personales sean correctos
-- 3. Verificar que no sea un usuario duplicado
-- 4. Usar el procedimiento sp_ActivarUsuario para activación segura
-- 5. El usuario recibirá mensaje en login cuando intente acceder antes de activación

COMMIT;
