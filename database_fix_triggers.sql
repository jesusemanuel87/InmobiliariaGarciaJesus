-- Script para corregir problemas con triggers en InmuebleImagenes
USE inmobiliaria;

-- Eliminar triggers existentes si existen
DROP TRIGGER IF EXISTS tr_inmueble_imagen_portada_before_insert;
DROP TRIGGER IF EXISTS tr_inmueble_imagen_portada_before_update;

-- Eliminar constraint único problemático si existe
ALTER TABLE InmuebleImagenes DROP INDEX IF EXISTS uk_inmueble_portada;

-- Verificar estructura actual
DESCRIBE InmuebleImagenes;

SELECT 'Triggers eliminados y constraint único removido. La lógica de portada se maneja en la aplicación.' AS mensaje;
