-- Script para crear tabla de notificaciones para la app móvil
-- Ejecutar en MySQL Workbench o terminal

USE inmobiliaria;

-- Tabla de notificaciones
CREATE TABLE IF NOT EXISTS notificaciones (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PropietarioId INT NOT NULL,
    Tipo VARCHAR(50) NOT NULL COMMENT 'PagoRegistrado, PagoVencido, NuevoContrato, etc.',
    Titulo VARCHAR(200) NOT NULL,
    Mensaje TEXT NOT NULL,
    Datos JSON NULL COMMENT 'Datos adicionales en formato JSON (pagoId, contratoId, etc.)',
    Leida BOOLEAN DEFAULT FALSE,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaLeida DATETIME NULL,
    
    INDEX idx_propietario (PropietarioId),
    INDEX idx_leida (Leida),
    INDEX idx_fecha (FechaCreacion),
    INDEX idx_tipo (Tipo),
    
    CONSTRAINT fk_notificacion_propietario 
        FOREIGN KEY (PropietarioId) 
        REFERENCES propietarios(Id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar notificaciones de ejemplo (opcional - para testing)
INSERT INTO notificaciones (PropietarioId, Tipo, Titulo, Mensaje, Datos, Leida) VALUES
(1, 'PagoRegistrado', '💰 Pago Recibido', 'Se ha registrado el pago de la cuota #1 del inmueble en Av. Illia 123', 
 '{"pagoId": 1, "contratoId": 1, "inmuebleId": 5, "monto": 150000}', FALSE),
(1, 'PagoRegistrado', '💰 Pago Recibido', 'Se ha registrado el pago de la cuota #2 del inmueble en Av. Illia 123', 
 '{"pagoId": 2, "contratoId": 1, "inmuebleId": 5, "monto": 150000}', TRUE);

SELECT 'Tabla de notificaciones creada exitosamente' AS Resultado;
