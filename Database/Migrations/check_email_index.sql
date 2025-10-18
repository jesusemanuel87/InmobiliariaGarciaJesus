USE inmobiliaria;

-- Verificar índices en la columna Email
SHOW INDEX FROM Usuarios WHERE Key_name LIKE '%Email%' OR Key_name LIKE '%email%';
