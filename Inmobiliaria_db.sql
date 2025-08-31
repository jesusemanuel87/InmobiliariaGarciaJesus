-- Script completo para MariaDB - Limpieza total y recreación
-- Ejecutar como usuario con privilegios DROP DATABASE

-- Paso 1: Eliminar completamente la base de datos
DROP DATABASE IF EXISTS `inmobiliaria`;

-- Paso 2: Crear nueva base de datos
CREATE DATABASE `inmobiliaria` 
DEFAULT CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `inmobiliaria`;

-- Tabla de Propietarios
CREATE TABLE `propietarios` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `DNI` varchar(20) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Telefono` varchar(20) DEFAULT NULL,
  `Email` varchar(100) NOT NULL,
  `Direccion` varchar(200) DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UK_propietarios_DNI` (`DNI`),
  UNIQUE KEY `UK_propietarios_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de Inquilinos
CREATE TABLE `inquilinos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `DNI` varchar(20) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Telefono` varchar(20) DEFAULT NULL,
  `Email` varchar(100) NOT NULL,
  `Direccion` varchar(200) DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UK_inquilinos_DNI` (`DNI`),
  UNIQUE KEY `UK_inquilinos_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de Inmuebles
CREATE TABLE `inmuebles` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Direccion` varchar(200) NOT NULL,
  `Ambientes` int(11) NOT NULL,
  `Superficie` decimal(10,2) NOT NULL,
  `Latitud` decimal(10,8) DEFAULT NULL,
  `Longitud` decimal(11,8) DEFAULT NULL,
  `PropietarioId` int(11) NOT NULL,
  `Tipo` enum('Casa','Departamento','Local','Oficina','Terreno') NOT NULL,
  `Precio` decimal(15,2) DEFAULT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  `Uso` enum('Residencial','Comercial','Industrial') NOT NULL DEFAULT 'Residencial',
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IDX_inmuebles_propietario` (`PropietarioId`),
  CONSTRAINT `FK_inmuebles_propietarios_v2` FOREIGN KEY (`PropietarioId`) REFERENCES `propietarios` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de Contratos
CREATE TABLE `contratos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FechaInicio` date NOT NULL,
  `FechaFin` date NOT NULL,
  `Precio` decimal(15,2) NOT NULL,
  `InquilinoId` int(11) NOT NULL,
  `InmuebleId` int(11) NOT NULL,
  `Estado` enum('Reservado','Activo','Finalizado','Cancelado') NOT NULL DEFAULT 'Reservado',
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IDX_contratos_inquilino` (`InquilinoId`),
  INDEX `IDX_contratos_inmueble` (`InmuebleId`),
  CONSTRAINT `FK_contratos_inquilinos_v2` FOREIGN KEY (`InquilinoId`) REFERENCES `inquilinos` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_contratos_inmuebles_v2` FOREIGN KEY (`InmuebleId`) REFERENCES `inmuebles` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de Pagos
CREATE TABLE `pagos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Numero` int(11) NOT NULL,
  `FechaPago` date DEFAULT NULL,
  `ContratoId` int(11) NOT NULL,
  `Importe` decimal(15,2) NOT NULL,
  `Estado` enum('Pendiente','Pagado','Vencido') NOT NULL DEFAULT 'Pendiente',
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IDX_pagos_contrato` (`ContratoId`),
  CONSTRAINT `FK_pagos_contratos_v2` FOREIGN KEY (`ContratoId`) REFERENCES `contratos` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de Usuarios
CREATE TABLE `usuarios` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Rol` enum('Administrador','Empleado') NOT NULL DEFAULT 'Empleado',
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Clave` varchar(255) NOT NULL,
  `Avatar` varchar(200) DEFAULT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UK_usuarios_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar datos de ejemplo
INSERT INTO `propietarios` (`DNI`, `Nombre`, `Apellido`, `Telefono`, `Email`, `Direccion`) VALUES
('35456987', 'José', 'Pérez', '2657123456', 'jose.perez@email.com', 'Av. San Martín 123, San Luis'),
('36987456', 'María', 'González', '2664896547', 'maria.gonzalez@email.com', 'Rivadavia 456, San Luis');

INSERT INTO `inquilinos` (`DNI`, `Nombre`, `Apellido`, `Telefono`, `Email`, `Direccion`) VALUES
('30111222', 'Carlos', 'Rodríguez', '2651111111', 'carlos.rodriguez@email.com', 'Mitre 789, San Luis'),
('33000111', 'Ana', 'Martínez', '2652222222', 'ana.martinez@email.com', 'Belgrano 321, San Luis');

INSERT INTO `inmuebles` (`Direccion`, `Ambientes`, `Superficie`, `Latitud`, `Longitud`, `PropietarioId`, `Tipo`, `Precio`, `Uso`) VALUES
('Mirador 1 Casa 22, San Luis', 4, 120.50, -33.257433, -66.334202, 1, 'Casa', 85000.00, 'Residencial'),
('Calle 51 número 1120', 3, 95.00, -33.258000, -66.335000, 1, 'Departamento', 250000.00, 'Comercial');

INSERT INTO `contratos` (`FechaInicio`, `FechaFin`, `Precio`, `InquilinoId`, `InmuebleId`, `Estado`) VALUES
('2025-08-01', '2025-10-31', 650000.00, 1, 1, 'Activo'),
('2025-08-20', '2025-09-06', 400000.00, 2, 1, 'Cancelado');

INSERT INTO `usuarios` (`Rol`, `Nombre`, `Apellido`, `Email`, `Clave`) VALUES
('Administrador', 'Admin', 'Sistema', 'admin@inmobiliaria.com', 'admin123');
