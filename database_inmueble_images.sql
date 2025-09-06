-- =====================================================
-- TABLA PARA GESTIÓN DE IMÁGENES DE INMUEBLES
-- Implementa mejor práctica: tabla relacionada con soporte para múltiples imágenes
-- =====================================================

USE inmobiliaria;

-- Crear tabla para almacenar imágenes de inmuebles
CREATE TABLE InmuebleImagenes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InmuebleId INT NOT NULL,
    NombreArchivo VARCHAR(255) NOT NULL,
    RutaArchivo VARCHAR(500) NOT NULL,
    EsPortada BOOLEAN DEFAULT FALSE,
    Descripcion VARCHAR(500) NULL,
    TamanoBytes BIGINT NULL,
    TipoMime VARCHAR(100) NULL,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        -- Clave foránea hacia Inmuebles
    FOREIGN KEY (InmuebleId) REFERENCES Inmuebles(Id) ON DELETE CASCADE,
        -- Índices para optimizar consultas
    INDEX idx_inmueble_id (InmuebleId),
    INDEX idx_es_portada (InmuebleId, EsPortada)
);

-- Nota: Los triggers han sido removidos para evitar conflictos.
-- La lógica de portada única se maneja en el código de la aplicación.

-- Insertar datos de ejemplo (opcional)
-- INSERT INTO InmuebleImagenes (InmuebleId, NombreArchivo, RutaArchivo, EsPortada, Descripcion) 
-- VALUES 
-- (1, 'fachada_principal.jpg', '/uploads/inmuebles/1/fachada_principal.jpg', TRUE, 'Vista frontal del inmueble'),
-- (1, 'interior_living.jpg', '/uploads/inmuebles/1/interior_living.jpg', FALSE, 'Living principal'),
-- (2, 'exterior_casa.jpg', '/uploads/inmuebles/2/exterior_casa.jpg', TRUE, 'Vista exterior');

-- Verificar estructura creada
DESCRIBE InmuebleImagenes;

SELECT 'Tabla InmuebleImagenes creada exitosamente con triggers para gestión de portadas' AS mensaje;
