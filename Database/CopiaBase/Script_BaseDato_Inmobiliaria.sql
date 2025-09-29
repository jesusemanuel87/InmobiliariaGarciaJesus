-- phpMyAdmin SQL Dump
-- version 5.1.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 27-09-2025 a las 19:23:26
-- Versión del servidor: 10.4.21-MariaDB
-- Versión de PHP: 8.0.11

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

--
-- Base de datos: `inmobiliaria`
--
-- --------------------------------------------------------
-- ========================================================
-- Script para crear/actualizar esquema INMOBILIARIA
-- ========================================================
-- Crear base si no existe
CREATE DATABASE IF NOT EXISTS inmobiliaria 
  DEFAULT CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

USE inmobiliaria;

-- ===============================================
-- TABLA CONFIGURACIONES
-- ===============================================
CREATE TABLE IF NOT EXISTS configuraciones (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Clave VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  Valor VARCHAR(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  Descripcion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  Tipo INT(11) NOT NULL,
  FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas (en caso de upgrades)
ALTER TABLE configuraciones
  ADD COLUMN IF NOT EXISTS Clave VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Valor VARCHAR(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Descripcion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Tipo INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

-- Datos de configuración
TRUNCATE TABLE configuraciones;
INSERT IGNORE INTO configuraciones (Id, Clave, Valor, Descripcion, Tipo) VALUES
(1, 'MESES_MINIMOS_6', 'True', 'Opción de 6 meses de alquiler mínimo', 1),
(2, 'MESES_MINIMOS_12', 'True', 'Opción de 12 meses de alquiler mínimo', 1),
(3, 'MESES_MINIMOS_18', 'True', 'Opción de 18 meses de alquiler mínimo', 1),
(4, 'MESES_MINIMOS_24', 'True', 'Opción de 24 meses de alquiler mínimo', 1),
(5, 'MESES_MINIMOS_30', 'True', 'Opción de 30 meses de alquiler mínimo', 1),
(6, 'MESES_MINIMOS_36', 'True', 'Opción de 36 meses de alquiler mínimo', 1),
(7, 'MULTA_TERMINACION_TEMPRANA', '2', 'Meses de multa si se cumplió menos de la mitad del contrato', 2),
(8, 'MULTA_TERMINACION_TARDIA', '1', 'Meses de multa si se cumplió más de la mitad del contrato', 3),
(9, 'INTERES_VENCIMIENTO_10_20', '10', 'Porcentaje de interés para vencimientos de 10-20 días', 4),
(10, 'INTERES_VENCIMIENTO_20_PLUS', '15', 'Porcentaje de interés para vencimientos de más de 20 días', 5),
(11, 'INTERES_VENCIMIENTO_MENSUAL', '20', 'Porcentaje de interés mensual', 6);

-- ===============================================
-- TABLA CONTRATOS
-- ===============================================
CREATE TABLE IF NOT EXISTS contratos (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  FechaInicio DATE NOT NULL,
  FechaFin DATE NOT NULL,
  Precio DECIMAL(15,2) NOT NULL,
  InquilinoId INT(11) NOT NULL,
  InmuebleId INT(11) NOT NULL,
  Estado ENUM('Activo','Finalizado','Reservado','Cancelado') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Activo',
  FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CreadoPorId INT(11) DEFAULT NULL,
  TerminadoPorId INT(11) DEFAULT NULL,
  FechaTerminacion DATETIME DEFAULT NULL,
  MotivoCancelacion TEXT COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  fecha_finalizacion_real DATE DEFAULT NULL,
  multa_finalizacion DECIMAL(15,2) DEFAULT NULL,
  meses_adeudados INT(11) DEFAULT NULL,
  importe_adeudado DECIMAL(15,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas adicionales en contratos (en caso de upgrades)
ALTER TABLE contratos
  ADD COLUMN IF NOT EXISTS FechaInicio DATE NOT NULL,
  ADD COLUMN IF NOT EXISTS FechaFin DATE NOT NULL,
  ADD COLUMN IF NOT EXISTS Precio DECIMAL(15,2) NOT NULL,
  ADD COLUMN IF NOT EXISTS InquilinoId INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS InmuebleId INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS Estado ENUM('Activo','Finalizado','Reservado','Cancelado') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Activo',
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS CreadoPorId INT(11) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS TerminadoPorId INT(11) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS FechaTerminacion DATETIME DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS MotivoCancelacion TEXT COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS fecha_finalizacion_real DATE DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS multa_finalizacion DECIMAL(15,2) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS meses_adeudados INT(11) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS importe_adeudado DECIMAL(15,2) DEFAULT NULL;

-- Datos de contratos
-- Usar DELETE en lugar de TRUNCATE para evitar errores de foreign key
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM contratos;
SET FOREIGN_KEY_CHECKS = 1;
INSERT IGNORE INTO contratos (Id, FechaInicio, FechaFin, Precio, InquilinoId, InmuebleId, Estado, FechaCreacion, CreadoPorId, TerminadoPorId, FechaTerminacion, MotivoCancelacion, fecha_finalizacion_real, multa_finalizacion, meses_adeudados, importe_adeudado) VALUES
(1, '2025-08-01', '2025-12-31', '650000.00', 1, 1, 'Finalizado', '2025-08-30 14:13:05', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(2, '2025-08-20', '2025-09-06', '400000.00', 2, 1, 'Cancelado', '2025-08-30 18:12:38', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(4, '2025-09-01', '2025-12-31', '300000.00', 2, 2, 'Cancelado', '2025-08-31 13:54:54', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(5, '2025-09-01', '2025-12-31', '350000.00', 2, 3, 'Finalizado', '2025-08-31 14:08:28', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(6, '2025-09-01', '2026-03-31', '65000.00', 2, 2, 'Finalizado', '2025-08-31 23:40:37', 3, NULL, NULL, 'xxx', NULL, NULL, NULL, NULL),
(7, '2025-09-01', '2026-03-31', '250000.00', 1, 3, 'Finalizado', '2025-09-06 18:52:31', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(8, '2025-09-06', '2026-03-31', '85000.00', 1, 1, 'Finalizado', '2025-09-06 19:29:42', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(9, '2025-09-06', '2026-03-31', '85000.00', 1, 1, 'Finalizado', '2025-09-06 19:31:26', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(10, '2025-09-01', '2026-02-28', '250000.00', 2, 3, 'Finalizado', '2025-09-06 19:32:59', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(11, '2025-09-01', '2026-03-31', '65000.00', 2, 2, 'Finalizado', '2025-09-06 19:44:07', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(12, '2025-09-01', '2026-03-31', '85000.00', 1, 1, 'Finalizado', '2025-09-06 20:05:19', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(13, '2025-09-01', '2026-03-31', '250000.00', 2, 3, 'Finalizado', '2025-09-06 20:10:24', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(14, '2025-09-01', '2026-03-31', '100000.00', 1, 1, 'Finalizado', '2025-09-06 20:16:30', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(15, '2025-09-01', '2026-03-31', '250000.00', 1, 3, 'Finalizado', '2025-09-06 20:36:25', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(16, '2025-09-01', '2026-03-31', '50000.00', 2, 2, 'Finalizado', '2025-09-06 20:42:05', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(17, '2025-09-06', '2026-03-31', '100000.00', 2, 1, 'Finalizado', '2025-09-06 20:48:43', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(18, '2025-09-01', '2026-03-31', '100000.00', 1, 1, 'Finalizado', '2025-09-07 11:18:16', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(19, '2025-10-01', '2027-04-30', '85000.00', 3, 1, 'Reservado', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(20, '2025-09-15', '2027-03-31', '250000.00', 2, 3, 'Activo', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(21, '2025-09-26', '2026-03-31', '300000.00', 6, 4, 'Activo', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ===============================================
-- TABLA EMPLEADOS
-- ===============================================
CREATE TABLE IF NOT EXISTS empleados (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nombre VARCHAR(50) CHARACTER SET utf8 NOT NULL,
  Apellido VARCHAR(50) CHARACTER SET utf8 NOT NULL,
  Dni VARCHAR(20) CHARACTER SET utf8 NOT NULL,
  Telefono VARCHAR(20) CHARACTER SET utf8 NOT NULL,
  Email VARCHAR(100) CHARACTER SET utf8 NOT NULL,
  FotoPerfil VARCHAR(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  Rol INT(11) NOT NULL DEFAULT 1,
  FechaIngreso DATE NOT NULL DEFAULT (CURDATE()),
  Observaciones VARCHAR(500) CHARACTER SET utf8 DEFAULT NULL,
  Estado TINYINT(1) NOT NULL DEFAULT 1,
  FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FechaModificacion DATETIME DEFAULT NULL,
  FechaActualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en empleados (en caso de upgrades)
ALTER TABLE empleados
  ADD COLUMN IF NOT EXISTS Nombre VARCHAR(50) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS Apellido VARCHAR(50) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS Dni VARCHAR(20) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS Telefono VARCHAR(20) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS Email VARCHAR(100) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS FotoPerfil VARCHAR(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Rol INT(11) NOT NULL DEFAULT 1,
  ADD COLUMN IF NOT EXISTS FechaIngreso DATE NOT NULL DEFAULT (CURDATE()),
  ADD COLUMN IF NOT EXISTS Observaciones VARCHAR(500) CHARACTER SET utf8 DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Estado TINYINT(1) NOT NULL DEFAULT 1,
  ADD COLUMN IF NOT EXISTS FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS FechaModificacion DATETIME DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS FechaActualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

-- ===============================================
-- TABLA TIPOS INMUEBLE
-- ===============================================
CREATE TABLE IF NOT EXISTS tiposinmueble (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nombre VARCHAR(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  Descripcion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  Estado TINYINT(1) NOT NULL DEFAULT 1,
  FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en tiposinmueble (en caso de upgrades)
ALTER TABLE tiposinmueble
  ADD COLUMN IF NOT EXISTS Nombre VARCHAR(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Descripcion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Estado TINYINT(1) NOT NULL DEFAULT 1,
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;

-- ===============================================
-- TABLA PROPIETARIOS
-- ===============================================
CREATE TABLE IF NOT EXISTS propietarios (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DNI VARCHAR(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  Nombre VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  Apellido VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  Telefono VARCHAR(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  Email VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  Direccion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  Estado TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en propietarios (en caso de upgrades)
ALTER TABLE propietarios
  ADD COLUMN IF NOT EXISTS DNI VARCHAR(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Nombre VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Apellido VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Telefono VARCHAR(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Email VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Direccion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS Estado TINYINT(1) NOT NULL DEFAULT 1;

-- ===============================================
-- TABLA INQUILINOS
-- ===============================================
CREATE TABLE IF NOT EXISTS inquilinos (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DNI VARCHAR(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  Nombre VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  Apellido VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  Telefono VARCHAR(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  Email VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  Direccion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  Estado TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en inquilinos (en caso de upgrades)
ALTER TABLE inquilinos
  ADD COLUMN IF NOT EXISTS DNI VARCHAR(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Nombre VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Apellido VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Telefono VARCHAR(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Email VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Direccion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS Estado TINYINT(1) NOT NULL DEFAULT 1;

-- ===============================================
-- TABLA INMUEBLES
-- ===============================================
CREATE TABLE IF NOT EXISTS inmuebles (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Direccion VARCHAR(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  Ambientes INT(11) NOT NULL,
  Superficie DECIMAL(10,2) NOT NULL,
  Latitud DECIMAL(10,8) DEFAULT NULL,
  Longitud DECIMAL(11,8) DEFAULT NULL,
  PropietarioId INT(11) NOT NULL,
  TipoId INT(11) NOT NULL,
  Precio DECIMAL(15,2) DEFAULT NULL,
  Estado TINYINT(1) NOT NULL DEFAULT 1,
  Uso ENUM('Residencial','Comercial','Industrial') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Residencial',
  FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  Localidad VARCHAR(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  Provincia VARCHAR(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en inmuebles (en caso de upgrades)
ALTER TABLE inmuebles
  ADD COLUMN IF NOT EXISTS Direccion VARCHAR(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Ambientes INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS Superficie DECIMAL(10,2) NOT NULL,
  ADD COLUMN IF NOT EXISTS Latitud DECIMAL(10,8) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Longitud DECIMAL(11,8) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS PropietarioId INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS TipoId INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS Precio DECIMAL(15,2) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Estado TINYINT(1) NOT NULL DEFAULT 1,
  ADD COLUMN IF NOT EXISTS Uso ENUM('Residencial','Comercial','Industrial') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Residencial',
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS Localidad VARCHAR(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Provincia VARCHAR(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL;

-- ===============================================
-- TABLA PAGOS
-- ===============================================
CREATE TABLE IF NOT EXISTS pagos (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Numero INT(11) NOT NULL,
  FechaPago DATE DEFAULT NULL,
  ContratoId INT(11) NOT NULL,
  Importe DECIMAL(15,2) NOT NULL,
  Estado ENUM('Pendiente','Pagado','Vencido') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Pendiente',
  FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CreadoPorId INT(11) DEFAULT NULL,
  AnuladoPorId INT(11) DEFAULT NULL,
  FechaAnulacion DATETIME DEFAULT NULL,
  metodo_pago ENUM('MercadoPago','Transferencia','Efectivo','Cheque','Otro') COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  observaciones VARCHAR(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  Intereses DECIMAL(18,2) NOT NULL DEFAULT 0.00,
  Multas DECIMAL(18,2) NOT NULL DEFAULT 0.00,
  FechaVencimiento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en pagos (en caso de upgrades)
ALTER TABLE pagos
  ADD COLUMN IF NOT EXISTS Numero INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS FechaPago DATE DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS ContratoId INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS Importe DECIMAL(15,2) NOT NULL,
  ADD COLUMN IF NOT EXISTS Estado ENUM('Pendiente','Pagado','Vencido') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Pendiente',
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS CreadoPorId INT(11) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS AnuladoPorId INT(11) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS FechaAnulacion DATETIME DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS metodo_pago ENUM('MercadoPago','Transferencia','Efectivo','Cheque','Otro') COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS observaciones VARCHAR(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Intereses DECIMAL(18,2) NOT NULL DEFAULT 0.00,
  ADD COLUMN IF NOT EXISTS Multas DECIMAL(18,2) NOT NULL DEFAULT 0.00,
  ADD COLUMN IF NOT EXISTS FechaVencimiento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;

-- ===============================================
-- TABLA USUARIOS
-- ===============================================
CREATE TABLE IF NOT EXISTS usuarios (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  NombreUsuario VARCHAR(50) CHARACTER SET utf8 NOT NULL,
  Email VARCHAR(100) CHARACTER SET utf8 NOT NULL,
  ClaveHash VARCHAR(255) CHARACTER SET utf8 NOT NULL,
  FotoPerfil VARCHAR(255) CHARACTER SET utf8 DEFAULT NULL,
  Rol INT(11) NOT NULL,
  FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UltimoAcceso TIMESTAMP NULL DEFAULT NULL,
  Estado TINYINT(1) NOT NULL DEFAULT 1,
  EmpleadoId INT(11) DEFAULT NULL,
  PropietarioId INT(11) DEFAULT NULL,
  InquilinoId INT(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en usuarios (en caso de upgrades)
ALTER TABLE usuarios
  ADD COLUMN IF NOT EXISTS NombreUsuario VARCHAR(50) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS Email VARCHAR(100) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS ClaveHash VARCHAR(255) CHARACTER SET utf8 NOT NULL,
  ADD COLUMN IF NOT EXISTS FotoPerfil VARCHAR(255) CHARACTER SET utf8 DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Rol INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS UltimoAcceso TIMESTAMP NULL DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Estado TINYINT(1) NOT NULL DEFAULT 1,
  ADD COLUMN IF NOT EXISTS EmpleadoId INT(11) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS PropietarioId INT(11) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS InquilinoId INT(11) DEFAULT NULL;

-- ===============================================
-- TABLA INMUEBLE IMAGENES
-- ===============================================
CREATE TABLE IF NOT EXISTS inmuebleimagenes (
  Id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  InmuebleId INT(11) NOT NULL,
  NombreArchivo VARCHAR(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  RutaArchivo VARCHAR(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  EsPortada TINYINT(1) DEFAULT 0,
  Descripcion VARCHAR(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  TamanoBytes BIGINT(20) DEFAULT NULL,
  TipoMime VARCHAR(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
  FechaActualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asegurar columnas en inmuebleimagenes (en caso de upgrades)
ALTER TABLE inmuebleimagenes
  ADD COLUMN IF NOT EXISTS InmuebleId INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS NombreArchivo VARCHAR(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS RutaArchivo VARCHAR(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS EsPortada TINYINT(1) DEFAULT 0,
  ADD COLUMN IF NOT EXISTS Descripcion VARCHAR(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS TamanoBytes BIGINT(20) DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS TipoMime VARCHAR(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS FechaActualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

-- ========================================================
-- INSERCIÓN DE DATOS EN TODAS LAS TABLAS
-- ========================================================

-- Datos de empleados
-- Usar DELETE en lugar de TRUNCATE para evitar errores de foreign key
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM empleados;
SET FOREIGN_KEY_CHECKS = 1;
INSERT IGNORE INTO empleados (Id, Nombre, Apellido, Dni, Telefono, Email, FotoPerfil, Rol, FechaIngreso, Observaciones, Estado, FechaCreacion, FechaModificacion, FechaActualizacion) VALUES
(3, 'Admin', 'Sistema', '00000000', '0000000000', 'admin@inmobiliaria.com', NULL, 2, '2025-09-07', 'Usuario administrador del sistema', 1, '2025-09-08 00:44:01', '2025-09-07 21:44:01', '2025-09-08 00:47:53'),
(4, 'empleado', 'empleado', '55666777', '2664555555', 'empleado@inmobiliaria.com', NULL, 1, '2025-09-14', NULL, 1, '2025-09-14 23:47:42', '2025-09-20 23:37:08', '2025-09-21 02:37:08'),
(5, 'Empleado Test ', 'Inactivado', '55111222', '2661122334', 'empleado02@inmueble.com', NULL, 1, '2025-09-26', NULL, 0, '2025-09-26 21:27:42', '2025-09-26 15:31:12', '2025-09-26 18:31:12');

-- Datos de tipos inmueble
DELETE FROM tiposinmueble;
INSERT IGNORE INTO tiposinmueble (Id, Nombre, Descripcion, Estado, FechaCreacion) VALUES
(1, 'Casa', 'Casa unifamiliar', 1, '2025-09-27 00:40:08'),
(2, 'Departamento', 'Departamento en edificio', 1, '2025-09-27 00:40:08'),
(3, 'Monoambiente', 'Departamento de un solo ambiente', 1, '2025-09-27 00:40:08'),
(4, 'Local', 'Local comercial', 1, '2025-09-27 00:40:08'),
(5, 'Oficina', 'Oficina comercial', 1, '2025-09-27 00:40:08'),
(6, 'Terreno', 'Terreno baldío', 1, '2025-09-27 00:40:08'),
(7, 'Galpón', 'Galpón industrial', 1, '2025-09-27 00:40:08'),
(8, 'Campo', 'Campo rural', 1, '2025-09-27 01:49:35'),
(9, 'Quinta', 'Quinta', 1, '2025-09-27 01:56:19');

-- Datos de propietarios
-- Usar DELETE en lugar de TRUNCATE para evitar errores de foreign key
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM propietarios;
SET FOREIGN_KEY_CHECKS = 1;
INSERT IGNORE INTO propietarios (Id, DNI, Nombre, Apellido, Telefono, Email, Direccion, FechaCreacion, Estado) VALUES
(1, '35456987', 'José Maria', 'Pérez', '2657123456', 'jose.perez@email.com', 'Av. San Martín 1234, San Luis', '2025-08-23 15:40:17', 1),
(2, '36987456', 'María', 'González', '2664896547', 'maria.gonzalez@email.com', 'Rivadavia 456, San Luis', '2025-08-23 15:40:17', 1),
(3, '55555555', 'Omar', 'Propo', '2664789654', 'testpropietario@a.com', 'Casa s/n', '2025-08-23 17:12:59', 0),
(4, '11222333', 'Test', 'Prueba', '2664112233', 'test@test.com', 'Direccion 123', '2025-09-07 11:28:08', 0),
(5, '44555666', 'Ruben', 'Lopez', '2661111111', 'ruben@propietario.com', 'Colon 469', '2025-09-14 20:45:50', 1),
(7, '99888555', 'Propietario', 'Apellido Prop', '2664444444', 'propietario@inmueble.com', NULL, '2025-09-20 14:09:38', 1),
(8, '11444777', 'Rafa', 'Gomez', '2662222222', 'test02@inmueble.com', 'Mirador 3', '2025-09-21 15:54:03', 1);

-- Datos de inquilinos
-- Usar DELETE en lugar de TRUNCATE para evitar errores de foreign key
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM inquilinos;
SET FOREIGN_KEY_CHECKS = 1;
INSERT IGNORE INTO inquilinos (Id, DNI, Nombre, Apellido, Telefono, Email, Direccion, FechaCreacion, Estado) VALUES
(1, '30111222', 'Carlos', 'Rodríguez', '2651111111', 'carlos.rodriguez@email.com', 'Mitre 789, San Luis', '2025-08-23 15:40:20', 1),
(2, '33000111', 'Ana', 'Martínez', '2652222222', 'ana.martinez@email.com', 'Belgrano 321, San Luis', '2025-08-23 15:40:20', 1),
(3, '22222222', 'Jesus Emanuel', 'Garcia', '2664123456', 'testinquilino@gmail.com', 'Mira 2', '2025-08-23 15:50:25', 0),
(4, '32654789', 'Test', 'Modals', '2664567890', 'modal@inmuebles.com', 'Mira 3', '2025-09-12 11:20:27', 1),
(5, '99888555', 'Propietario', 'Apellido Prop', '2664444444', 'propietario@inmueble.com', 'Juan Llerena 55, Villa Mercedes, San Luis', '2025-09-21 15:40:45', 0),
(6, '33666777', 'Inquilino Test VM', 'Garcia', '2657774411', 'inquilino.vm@test.com', 'Belgrano 216', '2025-09-26 15:43:21', 1);

-- Datos de inmuebles
-- Usar DELETE en lugar de TRUNCATE para evitar errores de foreign key
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM inmuebles;
SET FOREIGN_KEY_CHECKS = 1;
INSERT IGNORE INTO inmuebles (Id, Direccion, Ambientes, Superficie, Latitud, Longitud, PropietarioId, TipoId, Precio, Estado, Uso, FechaCreacion, Localidad, Provincia) VALUES
(1, 'Mirador 1 Casa 22, San Luis', 4, '120.50', '-33.25743300', '-66.33420200', 1, 1, '85000.00', 1, 'Residencial', '2025-08-23 15:40:21', 'San Luis', 'San Luis'),
(2, 'Mirador 2 Depto 10, San Luis', 3, '75.00', '-33.25819600', '-66.33419100', 2, 5, '65000.00', 0, 'Comercial', '2025-08-23 15:40:21', 'San Luis', 'San Luis'),
(3, 'Calle 51 número 1120', 2, '70.00', '-33.25819600', '-66.33419100', 1, 2, '250000.00', 1, 'Comercial', '2025-08-30 21:11:41', 'San Luis', 'San Luis'),
(4, 'Las Heras 480', 5, '100.00', '-33.68324458', '-65.46910645', 8, 1, '300000.00', 1, 'Residencial', '2025-09-26 15:05:31', 'Villa Mercedes', 'San Luis'),
(5, 'Pueyrredon 859', 8, '120.00', '-33.67635087', '-65.46774590', 1, 9, '400000.00', 1, 'Residencial', '2025-09-26 23:08:34', 'Villa Mercedes', 'San Luis'),
(6, 'Av. Los Crespones y Los Abetos', 6, '85.00', '-33.26848806', '-66.22638992', 8, 2, '350000.00', 1, 'Residencial', '2025-09-26 23:12:49', 'Juana Koslay', 'San Luis'),
(7, 'Int. Garro Zollo s/n', 2, '1000.00', '-33.32742011', '-66.37223601', 5, 7, '500000.00', 1, 'Industrial', '2025-09-26 23:14:36', 'San Luis', 'San Luis');

-- Datos de usuarios
-- Usar DELETE en lugar de TRUNCATE para evitar errores de foreign key
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM usuarios;
SET FOREIGN_KEY_CHECKS = 1;
INSERT IGNORE INTO usuarios (Id, NombreUsuario, Email, ClaveHash, FotoPerfil, Rol, FechaCreacion, UltimoAcceso, Estado, EmpleadoId, PropietarioId, InquilinoId) VALUES
(3, 'admin', 'admin@inmobiliaria.com', '$2a$11$n4zTqE601oaEZkLIo./fzOpbwOC/MCv2DF/z02HPHG50sNZGjdDJ.', '/uploads/profiles/user_3_8d30b0b7-914d-44b9-b39a-7ab29c39f299.jpg', 4, '2025-09-08 00:44:02', '2025-09-27 17:07:44', 1, 3, NULL, NULL),
(4, 'eempleado', 'empleado@inmobiliaria.com', '$2a$11$4Ohais3PBVswY2a1l25Co.u./Nu8.ARLnpQmFSJncdD/FRSUq556S', '/uploads/profiles/user_4_324c8e83-52ff-402d-87ab-3ae93918f412.jpg', 3, '2025-09-14 23:47:42', '2025-09-27 17:08:28', 1, 4, NULL, NULL),
(5, 'propietario', 'propietario@inmueble.com', '$2a$11$VUIUnDYQrnmUd5spL1rnGe4d1xCxiT.J9YFHMvbWbv2q9dTHBG/tS', '/uploads/profiles/user_default.png', 1, '2025-09-20 17:09:39', '2025-09-21 18:29:57', 1, NULL, 7, NULL),
(7, 'einactivo', 'empleado02@inmueble.com', '$2a$11$qq6s2qy326e0zuKAHc.D7OTCqCAxZdNHVHQ2nHZHixTkPXcWgEsua', '/uploads/profiles/user_default.png', 3, '2025-09-26 18:27:42', '2025-09-26 18:31:28', 1, 5, NULL, NULL);

-- Datos de inmueble imagenes
DELETE FROM inmuebleimagenes;
INSERT IGNORE INTO inmuebleimagenes (Id, InmuebleId, NombreArchivo, RutaArchivo, EsPortada, Descripcion, TamanoBytes, TipoMime, FechaCreacion, FechaActualizacion) VALUES
(2, 1, '735808b4-bccc-42cb-9a39-a63c3c74fcb3.jpg', '/uploads/inmuebles/1/735808b4-bccc-42cb-9a39-a63c3c74fcb3.jpg', 1, NULL, 9377, 'image/jpeg', '2025-09-06 13:14:45', '2025-09-07 14:21:00'),
(4, 2, '182d114e-0da6-4033-a63b-3faaef7d4f91.jpg', '/uploads/inmuebles/2/182d114e-0da6-4033-a63b-3faaef7d4f91.jpg', 1, 'casa4', 8064, 'image/jpeg', '2025-09-06 16:57:43', '2025-09-06 17:12:04'),
(6, 3, '458b3fce-b86b-4df5-8ea9-0daa72fb73d4.jpg', '/uploads/inmuebles/3/458b3fce-b86b-4df5-8ea9-0daa72fb73d4.jpg', 1, NULL, 8995, 'image/jpeg', '2025-09-06 17:11:45', '2025-09-06 17:11:45'),
(7, 1, '1a4046e9-e8df-4236-8aaf-6cd53bcd253f.jpg', '/uploads/inmuebles/1/1a4046e9-e8df-4236-8aaf-6cd53bcd253f.jpg', 0, NULL, 11144, 'image/jpeg', '2025-09-07 14:20:36', '2025-09-07 14:21:00'),
(8, 4, 'c03b7cb0-c832-4f98-915c-cb1c8e141762.jpg', '/uploads/inmuebles/4/c03b7cb0-c832-4f98-915c-cb1c8e141762.jpg', 1, NULL, 8995, 'image/jpeg', '2025-09-26 15:07:03', '2025-09-26 15:07:03'),
(9, 5, '38fda3b8-6c87-4ce6-9a12-01a293fa414b.jpg', '/uploads/inmuebles/5/38fda3b8-6c87-4ce6-9a12-01a293fa414b.jpg', 1, NULL, 7864, 'image/jpeg', '2025-09-26 23:09:42', '2025-09-26 23:09:42'),
(10, 6, 'accba503-c290-49e6-a3d6-129c580baaa6.jpg', '/uploads/inmuebles/6/accba503-c290-49e6-a3d6-129c580baaa6.jpg', 1, NULL, 13305, 'image/jpeg', '2025-09-26 23:15:02', '2025-09-26 23:15:02'),
(11, 7, 'f893c8d3-a5ec-4bf2-9017-938d3a632d79.jpeg', '/uploads/inmuebles/7/f893c8d3-a5ec-4bf2-9017-938d3a632d79.jpeg', 1, NULL, 8528, 'image/jpeg', '2025-09-26 23:16:13', '2025-09-26 23:16:13');

-- Datos de pagos (muestra representativa)
-- Usar DELETE en lugar de TRUNCATE para evitar errores de foreign key
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM pagos;
SET FOREIGN_KEY_CHECKS = 1;
INSERT IGNORE INTO pagos (Id, Numero, FechaPago, ContratoId, Importe, Estado, FechaCreacion, CreadoPorId, AnuladoPorId, FechaAnulacion, metodo_pago, observaciones, Intereses, Multas, FechaVencimiento) VALUES
(1, 1, '2025-08-01', 5, '350000.00', 'Vencido', '2025-08-31 14:08:28', 3, NULL, NULL, NULL, '.', '105000.00', '0.00', '2025-08-10 00:00:00'),
(3, 3, '2025-09-12', 5, '350000.00', 'Pagado', '2025-08-31 14:08:28', 3, NULL, NULL, 'Efectivo', NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(4, 1, '2025-08-05', 1, '650000.00', 'Pagado', '2025-08-31 20:10:54', 3, NULL, NULL, 'MercadoPago', NULL, '0.00', '0.00', '2025-08-10 00:00:00'),
(5, 2, '2025-09-08', 1, '650000.00', 'Pagado', '2025-08-31 20:11:23', 3, NULL, NULL, 'MercadoPago', NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(6, 1, '2025-08-01', 6, '65000.00', 'Vencido', '2025-08-31 23:40:37', 3, NULL, NULL, NULL, '.', '19500.00', '0.00', '2025-08-10 00:00:00'),
(78, 1, '2025-09-07', 18, '100000.00', 'Pagado', '2025-09-07 11:18:16', 3, NULL, NULL, 'MercadoPago', 'm', '0.00', '200000.00', '2025-09-10 00:00:00'),
(84, 1, '2025-09-15', 19, '85000.00', 'Pagado', '2025-09-08 19:48:41', 3, NULL, NULL, 'Efectivo', NULL, '0.00', '0.00', '2025-10-10 00:00:00'),
(85, 2, '2025-11-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-11-10 00:00:00'),
(102, 1, '2025-09-01', 20, '250000.00', 'Vencido', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '25000.00', '0.00', '2025-09-10 00:00:00'),
(103, 2, '2025-10-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-10-10 00:00:00'),
(120, 1, '2025-09-01', 21, '300000.00', 'Vencido', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '45000.00', '0.00', '2025-09-10 00:00:00'),
(121, 2, '2025-10-01', 21, '300000.00', 'Pendiente', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-10-10 00:00:00');

-- ========================================================
-- ÍNDICES Y RESTRICCIONES
-- ========================================================

-- Índices para configuraciones
ALTER TABLE configuraciones
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD UNIQUE KEY IF NOT EXISTS idx_clave_unique (Clave),
  ADD KEY IF NOT EXISTS idx_clave (Clave),
  ADD KEY IF NOT EXISTS idx_tipo (Tipo);

-- Índices para contratos
ALTER TABLE contratos
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD KEY IF NOT EXISTS IDX_contratos_inquilino (InquilinoId),
  ADD KEY IF NOT EXISTS IDX_contratos_inmueble (InmuebleId),
  ADD KEY IF NOT EXISTS idx_creado_por (CreadoPorId),
  ADD KEY IF NOT EXISTS idx_terminado_por (TerminadoPorId),
  ADD KEY IF NOT EXISTS idx_fecha_terminacion (FechaTerminacion);

-- Índices para empleados
ALTER TABLE empleados
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD UNIQUE KEY IF NOT EXISTS idx_dni_unique (Dni),
  ADD UNIQUE KEY IF NOT EXISTS idx_email_unique (Email),
  ADD KEY IF NOT EXISTS idx_empleados_dni (Dni),
  ADD KEY IF NOT EXISTS idx_empleados_email (Email),
  ADD KEY IF NOT EXISTS idx_empleados_estado (Estado);

-- Índices para tiposinmueble
ALTER TABLE tiposinmueble
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD UNIQUE KEY IF NOT EXISTS idx_nombre_unique (Nombre),
  ADD KEY IF NOT EXISTS idx_nombre (Nombre),
  ADD KEY IF NOT EXISTS idx_estado (Estado);

-- Índices para propietarios
ALTER TABLE propietarios
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD UNIQUE KEY IF NOT EXISTS UK_propietarios_DNI (DNI),
  ADD UNIQUE KEY IF NOT EXISTS UK_propietarios_Email (Email);

-- Índices para inquilinos
ALTER TABLE inquilinos
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD UNIQUE KEY IF NOT EXISTS UK_inquilinos_DNI (DNI),
  ADD UNIQUE KEY IF NOT EXISTS UK_inquilinos_Email (Email);

-- Índices para inmuebles
ALTER TABLE inmuebles
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD KEY IF NOT EXISTS IDX_inmuebles_propietario (PropietarioId),
  ADD KEY IF NOT EXISTS idx_inmuebles_estado (Estado),
  ADD KEY IF NOT EXISTS idx_tipo_id (TipoId);

-- Índices para pagos
ALTER TABLE pagos
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD KEY IF NOT EXISTS IDX_pagos_contrato (ContratoId),
  ADD KEY IF NOT EXISTS IX_Pagos_FechaVencimiento (FechaVencimiento),
  ADD KEY IF NOT EXISTS idx_pagos_creado_por (CreadoPorId),
  ADD KEY IF NOT EXISTS idx_pagos_anulado_por (AnuladoPorId),
  ADD KEY IF NOT EXISTS idx_pagos_fecha_anulacion (FechaAnulacion);

-- Índices para usuarios
ALTER TABLE usuarios
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD UNIQUE KEY IF NOT EXISTS idx_nombreusuario_unique (NombreUsuario),
  ADD UNIQUE KEY IF NOT EXISTS idx_email_usuarios_unique (Email),
  ADD KEY IF NOT EXISTS idx_usuarios_email (Email),
  ADD KEY IF NOT EXISTS idx_usuarios_nombreusuario (NombreUsuario),
  ADD KEY IF NOT EXISTS idx_usuarios_rol (Rol),
  ADD KEY IF NOT EXISTS idx_usuarios_estado (Estado),
  ADD KEY IF NOT EXISTS idx_usuarios_empleado (EmpleadoId),
  ADD KEY IF NOT EXISTS idx_usuarios_propietario (PropietarioId),
  ADD KEY IF NOT EXISTS idx_usuarios_inquilino (InquilinoId);

-- Índices para inmuebleimagenes
ALTER TABLE inmuebleimagenes
  ADD PRIMARY KEY IF NOT EXISTS (Id),
  ADD KEY IF NOT EXISTS idx_inmueble_id (InmuebleId),
  ADD KEY IF NOT EXISTS idx_es_portada (InmuebleId,EsPortada);

-- AUTO_INCREMENT para las tablas
ALTER TABLE configuraciones MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=16;
ALTER TABLE contratos MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=22;
ALTER TABLE empleados MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;
ALTER TABLE tiposinmueble MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;
ALTER TABLE propietarios MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;
ALTER TABLE inquilinos MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;
ALTER TABLE inmuebles MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;
ALTER TABLE pagos MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=126;
ALTER TABLE usuarios MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;
ALTER TABLE inmuebleimagenes MODIFY Id INT(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

-- ========================================================
-- FOREIGN KEYS (RESTRICCIONES DE INTEGRIDAD REFERENCIAL)
-- Compatible con MariaDB/MySQL
-- ========================================================

-- Eliminar foreign keys existentes si existen (para re-ejecuciones)
ALTER TABLE contratos DROP FOREIGN KEY IF EXISTS FK_contratos_inmuebles_v2;
ALTER TABLE contratos DROP FOREIGN KEY IF EXISTS FK_contratos_inquilinos_v2;
ALTER TABLE contratos DROP FOREIGN KEY IF EXISTS fk_contratos_creado_por;
ALTER TABLE contratos DROP FOREIGN KEY IF EXISTS fk_contratos_terminado_por;

ALTER TABLE inmuebles DROP FOREIGN KEY IF EXISTS FK_inmuebles_propietarios_v2;
ALTER TABLE inmuebles DROP FOREIGN KEY IF EXISTS fk_inmuebles_tipo;

ALTER TABLE pagos DROP FOREIGN KEY IF EXISTS FK_pagos_contratos_v2;
ALTER TABLE pagos DROP FOREIGN KEY IF EXISTS fk_pagos_creado_por;
ALTER TABLE pagos DROP FOREIGN KEY IF EXISTS fk_pagos_anulado_por;

ALTER TABLE usuarios DROP FOREIGN KEY IF EXISTS usuarios_ibfk_1;
ALTER TABLE usuarios DROP FOREIGN KEY IF EXISTS usuarios_ibfk_2;
ALTER TABLE usuarios DROP FOREIGN KEY IF EXISTS usuarios_ibfk_3;

ALTER TABLE inmuebleimagenes DROP FOREIGN KEY IF EXISTS inmuebleimagenes_ibfk_1;

-- Crear foreign keys (sin IF NOT EXISTS para compatibilidad con MariaDB)
-- Foreign Keys para contratos
ALTER TABLE contratos
  ADD CONSTRAINT FK_contratos_inmuebles_v2 FOREIGN KEY (InmuebleId) REFERENCES inmuebles (Id) ON UPDATE CASCADE;

ALTER TABLE contratos
  ADD CONSTRAINT FK_contratos_inquilinos_v2 FOREIGN KEY (InquilinoId) REFERENCES inquilinos (Id) ON UPDATE CASCADE;

ALTER TABLE contratos
  ADD CONSTRAINT fk_contratos_creado_por FOREIGN KEY (CreadoPorId) REFERENCES usuarios (Id) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE contratos
  ADD CONSTRAINT fk_contratos_terminado_por FOREIGN KEY (TerminadoPorId) REFERENCES usuarios (Id) ON DELETE SET NULL ON UPDATE CASCADE;

-- Foreign Keys para inmuebles
ALTER TABLE inmuebles
  ADD CONSTRAINT FK_inmuebles_propietarios_v2 FOREIGN KEY (PropietarioId) REFERENCES propietarios (Id) ON UPDATE CASCADE;

ALTER TABLE inmuebles
  ADD CONSTRAINT fk_inmuebles_tipo FOREIGN KEY (TipoId) REFERENCES tiposinmueble (Id) ON UPDATE CASCADE;

-- Foreign Keys para pagos
ALTER TABLE pagos
  ADD CONSTRAINT FK_pagos_contratos_v2 FOREIGN KEY (ContratoId) REFERENCES contratos (Id) ON UPDATE CASCADE;

ALTER TABLE pagos
  ADD CONSTRAINT fk_pagos_creado_por FOREIGN KEY (CreadoPorId) REFERENCES usuarios (Id) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE pagos
  ADD CONSTRAINT fk_pagos_anulado_por FOREIGN KEY (AnuladoPorId) REFERENCES usuarios (Id) ON DELETE SET NULL ON UPDATE CASCADE;

-- Foreign Keys para usuarios
ALTER TABLE usuarios
  ADD CONSTRAINT usuarios_ibfk_1 FOREIGN KEY (EmpleadoId) REFERENCES empleados (Id) ON DELETE SET NULL;

ALTER TABLE usuarios
  ADD CONSTRAINT usuarios_ibfk_2 FOREIGN KEY (PropietarioId) REFERENCES propietarios (Id) ON DELETE SET NULL;

ALTER TABLE usuarios
  ADD CONSTRAINT usuarios_ibfk_3 FOREIGN KEY (InquilinoId) REFERENCES inquilinos (Id) ON DELETE SET NULL;

-- Foreign Keys para inmuebleimagenes
ALTER TABLE inmuebleimagenes
  ADD CONSTRAINT inmuebleimagenes_ibfk_1 FOREIGN KEY (InmuebleId) REFERENCES inmuebles (Id) ON DELETE CASCADE;

-- ========================================================
-- TRIGGERS Y VISTAS
-- ========================================================

-- Trigger para validar roles de usuarios en INSERT
DELIMITER $$
DROP TRIGGER IF EXISTS tr_usuarios_validate_role_fk$$
CREATE TRIGGER tr_usuarios_validate_role_fk BEFORE INSERT ON usuarios FOR EACH ROW 
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
END$$
DELIMITER ;

-- Trigger para validar roles de usuarios en UPDATE
DELIMITER $$
DROP TRIGGER IF EXISTS tr_usuarios_validate_role_fk_update$$
CREATE TRIGGER tr_usuarios_validate_role_fk_update BEFORE UPDATE ON usuarios FOR EACH ROW 
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
END$$
DELIMITER ;

-- Vista para usuarios completos
DROP VIEW IF EXISTS vw_usuarios_completo;
CREATE VIEW vw_usuarios_completo AS 
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
        WHEN u.Rol = 1 THEN CONCAT(p.Nombre, ' ', p.Apellido)
        WHEN u.Rol = 2 THEN CONCAT(i.Nombre, ' ', i.Apellido)
        WHEN u.Rol IN (3,4) THEN CONCAT(e.Nombre, ' ', e.Apellido)
        ELSE 'Sin Nombre' 
    END AS NombreCompleto,
    CASE 
        WHEN u.Rol = 1 THEN p.DNI
        WHEN u.Rol = 2 THEN i.DNI
        WHEN u.Rol IN (3,4) THEN e.Dni
        ELSE NULL 
    END AS Dni,
    CASE 
        WHEN u.Rol = 1 THEN p.Telefono
        WHEN u.Rol = 2 THEN i.Telefono
        WHEN u.Rol IN (3,4) THEN e.Telefono
        ELSE NULL 
    END AS Telefono,
    e.Rol AS RolEmpleado,
    e.FechaIngreso
FROM usuarios u
LEFT JOIN propietarios p ON u.PropietarioId = p.Id
LEFT JOIN inquilinos i ON u.InquilinoId = i.Id
LEFT JOIN empleados e ON u.EmpleadoId = e.Id;

-- ========================================================
-- FINALIZACIÓN DEL SCRIPT
-- ========================================================

COMMIT;

-- Mensaje de finalización
SELECT 'Script de base de datos INMOBILIARIA ejecutado exitosamente' AS Resultado;
