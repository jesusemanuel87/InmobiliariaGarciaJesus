-- =====================================================
-- Script: Crear tablas para cache de Georef API
-- Descripción: Almacena provincias y localidades de Argentina
--              para usar como fallback si la API no está disponible
-- Fecha: 2025-10-28
-- =====================================================

USE inmobiliaria;

-- Tabla de Provincias
CREATE TABLE IF NOT EXISTS georef_provincias (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GeorefId VARCHAR(10) NOT NULL UNIQUE COMMENT 'ID de la API Georef',
    Nombre VARCHAR(100) NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion DATETIME NULL,
    INDEX idx_nombre (Nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Cache de provincias argentinas desde API Georef';

-- Tabla de Localidades
CREATE TABLE IF NOT EXISTS georef_localidades (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GeorefId VARCHAR(10) NOT NULL UNIQUE COMMENT 'ID de la API Georef',
    Nombre VARCHAR(200) NOT NULL,
    ProvinciaId INT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion DATETIME NULL,
    INDEX idx_nombre (Nombre),
    INDEX idx_provincia (ProvinciaId),
    FOREIGN KEY (ProvinciaId) REFERENCES georef_provincias(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Cache de localidades argentinas desde API Georef';

-- Tabla de control de sincronización
CREATE TABLE IF NOT EXISTS georef_sync_log (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TipoSincronizacion VARCHAR(50) NOT NULL COMMENT 'provincias, localidades',
    FechaSincronizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Exitosa BOOLEAN NOT NULL,
    CantidadRegistros INT NULL,
    MensajeError TEXT NULL,
    INDEX idx_fecha (FechaSincronizacion),
    INDEX idx_tipo (TipoSincronizacion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Log de sincronizaciones con API Georef';

-- =====================================================
-- Datos iniciales (fallback si API no responde)
-- =====================================================

-- Insertar provincias principales de Argentina
INSERT INTO georef_provincias (GeorefId, Nombre) VALUES
('02', 'Buenos Aires'),
('06', 'Ciudad Autónoma de Buenos Aires'),
('10', 'Catamarca'),
('14', 'Córdoba'),
('18', 'Corrientes'),
('22', 'Chaco'),
('26', 'Chubut'),
('30', 'Entre Ríos'),
('34', 'Formosa'),
('38', 'Jujuy'),
('42', 'La Pampa'),
('46', 'La Rioja'),
('50', 'Mendoza'),
('54', 'Misiones'),
('58', 'Neuquén'),
('62', 'Río Negro'),
('66', 'Salta'),
('70', 'San Juan'),
('74', 'San Luis'),
('78', 'Santa Cruz'),
('82', 'Santa Fe'),
('86', 'Santiago del Estero'),
('90', 'Tucumán'),
('94', 'Tierra del Fuego')
ON DUPLICATE KEY UPDATE Nombre = VALUES(Nombre);

-- Insertar localidades principales de San Luis (tu provincia)
INSERT INTO georef_localidades (GeorefId, Nombre, ProvinciaId) VALUES
('740007', 'San Luis', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740014', 'Villa Mercedes', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740021', 'Merlo', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740028', 'La Punta', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740035', 'Justo Daract', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740042', 'Villa de la Quebrada', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740049', 'Tilisarao', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740056', 'Concarán', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740063', 'Nueva Galia', (SELECT Id FROM georef_provincias WHERE GeorefId = '74')),
('740070', 'Naschel', (SELECT Id FROM georef_provincias WHERE GeorefId = '74'))
ON DUPLICATE KEY UPDATE Nombre = VALUES(Nombre);

-- Registro inicial de sincronización
INSERT INTO georef_sync_log (TipoSincronizacion, Exitosa, CantidadRegistros, MensajeError)
VALUES ('inicial', TRUE, 34, 'Datos iniciales cargados manualmente');

SELECT 'Tablas de Georef creadas exitosamente' AS Resultado;
SELECT COUNT(*) AS TotalProvincias FROM georef_provincias;
SELECT COUNT(*) AS TotalLocalidades FROM georef_localidades;
