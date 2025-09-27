-- Migración: Crear tabla TiposInmueble y refactorizar campo Tipo en inmuebles
-- Fecha: 2025-01-27
-- Descripción: Convierte el campo Tipo de enum/string a relación con tabla TiposInmueble
USE inmobiliaria;
-- 1. Crear tabla TiposInmueble
CREATE TABLE IF NOT EXISTS TiposInmueble (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(200) NULL,
    Estado BOOLEAN NOT NULL DEFAULT TRUE,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_nombre (Nombre),
    INDEX idx_estado (Estado)
);

-- 2. Insertar datos iniciales de tipos
INSERT INTO TiposInmueble (Id, Nombre, Descripcion, Estado, FechaCreacion) VALUES
(1, 'Casa', 'Casa unifamiliar', TRUE, NOW()),
(2, 'Departamento', 'Departamento en edificio', TRUE, NOW()),
(3, 'Monoambiente', 'Departamento de un solo ambiente', TRUE, NOW()),
(4, 'Local', 'Local comercial', TRUE, NOW()),
(5, 'Oficina', 'Oficina comercial', TRUE, NOW()),
(6, 'Terreno', 'Terreno baldío', TRUE, NOW()),
(7, 'Galpón', 'Galpón industrial', TRUE, NOW())
ON DUPLICATE KEY UPDATE 
    Nombre = VALUES(Nombre),
    Descripcion = VALUES(Descripcion);

-- 3. Agregar nueva columna TipoId a la tabla inmuebles
ALTER TABLE inmuebles 
ADD COLUMN TipoId INT NULL AFTER Tipo,
ADD INDEX idx_tipo_id (TipoId);

-- 4. Migrar datos existentes del campo Tipo al nuevo TipoId
UPDATE inmuebles SET TipoId = 1 WHERE Tipo = 'Casa';
UPDATE inmuebles SET TipoId = 2 WHERE Tipo = 'Departamento';
UPDATE inmuebles SET TipoId = 3 WHERE Tipo = 'Monoambiente';
UPDATE inmuebles SET TipoId = 4 WHERE Tipo = 'Local';
UPDATE inmuebles SET TipoId = 5 WHERE Tipo = 'Oficina';
UPDATE inmuebles SET TipoId = 6 WHERE Tipo = 'Terreno';
UPDATE inmuebles SET TipoId = 7 WHERE Tipo = 'Galpón';

-- 5. Establecer valores por defecto para registros sin tipo específico
UPDATE inmuebles SET TipoId = 1 WHERE TipoId IS NULL;

-- 6. Hacer TipoId NOT NULL y agregar foreign key
ALTER TABLE inmuebles 
MODIFY COLUMN TipoId INT NOT NULL,
ADD CONSTRAINT fk_inmuebles_tipo 
    FOREIGN KEY (TipoId) REFERENCES TiposInmueble(Id) 
    ON DELETE RESTRICT ON UPDATE CASCADE;

-- 7. Opcional: Eliminar la columna Tipo antigua (comentado por seguridad)
ALTER TABLE inmuebles DROP COLUMN Tipo;

-- Verificar migración
SELECT 'Migración completada. Verificando datos...' as Status;

SELECT 
    t.Nombre as TipoNombre,
    COUNT(i.Id) as CantidadInmuebles
FROM TiposInmueble t
LEFT JOIN inmuebles i ON t.Id = i.TipoId
GROUP BY t.Id, t.Nombre
ORDER BY t.Id;
