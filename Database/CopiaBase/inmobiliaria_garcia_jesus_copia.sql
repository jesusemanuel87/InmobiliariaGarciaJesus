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


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `inmobiliaria`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `configuraciones`
--
USE inmobiliaria;

CREATE TABLE `configuraciones` (
  `Id` int(11) NOT NULL,
  `Clave` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Valor` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcion` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Tipo` int(11) NOT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `FechaModificacion` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



ALTER TABLE configuraciones
  ADD COLUMN IF NOT EXISTS Clave VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Valor VARCHAR(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  ADD COLUMN IF NOT EXISTS Descripcion VARCHAR(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS Tipo INT(11) NOT NULL,
  ADD COLUMN IF NOT EXISTS FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS FechaModificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

--
-- Volcado de datos para la tabla `configuraciones`
--
delete from configuraciones;
INSERT INTO `configuraciones` (`Id`, `Clave`, `Valor`, `Descripcion`, `Tipo`, `FechaCreacion`, `FechaModificacion`) VALUES
(1, 'MESES_MINIMOS_6', 'True', 'Opción de 6 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-26 15:45:06'),
(2, 'MESES_MINIMOS_12', 'True', 'Opción de 12 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-26 15:45:06'),
(3, 'MESES_MINIMOS_18', 'True', 'Opción de 18 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-26 15:45:06'),
(4, 'MESES_MINIMOS_24', 'True', 'Opción de 24 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-26 15:45:06'),
(5, 'MESES_MINIMOS_30', 'True', 'Opción de 30 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-26 15:45:06'),
(6, 'MESES_MINIMOS_36', 'True', 'Opción de 36 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-26 15:45:06'),
(7, 'MULTA_TERMINACION_TEMPRANA', '2', 'Meses de multa si se cumplió menos de la mitad del contrato', 2, '2025-08-31 13:34:01', '2025-08-31 13:34:01'),
(8, 'MULTA_TERMINACION_TARDIA', '1', 'Meses de multa si se cumplió más de la mitad del contrato', 3, '2025-08-31 13:34:01', '2025-09-07 13:48:10'),
(9, 'INTERES_VENCIMIENTO_10_20', '10', 'Porcentaje de interés para vencimientos de 10-20 días', 4, '2025-08-31 13:34:01', '2025-08-31 13:34:01'),
(10, 'INTERES_VENCIMIENTO_20_PLUS', '15', 'Porcentaje de interés para vencimientos de más de 20 días', 5, '2025-08-31 13:34:01', '2025-09-07 13:52:35'),
(11, 'INTERES_VENCIMIENTO_MENSUAL', '20', 'Porcentaje de interés mensual', 6, '2025-08-31 13:34:01', '2025-09-07 13:52:49');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `contratos`
--

CREATE TABLE `contratos` (
  `Id` int(11) NOT NULL,
  `FechaInicio` date NOT NULL,
  `FechaFin` date NOT NULL,
  `Precio` decimal(15,2) NOT NULL,
  `InquilinoId` int(11) NOT NULL,
  `InmuebleId` int(11) NOT NULL,
  `Estado` enum('Activo','Finalizado','Reservado','Cancelado') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Activo',
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `CreadoPorId` int(11) DEFAULT NULL,
  `TerminadoPorId` int(11) DEFAULT NULL,
  `FechaTerminacion` datetime DEFAULT NULL,
  `MotivoCancelacion` text COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fecha_finalizacion_real` date DEFAULT NULL,
  `multa_finalizacion` decimal(15,2) DEFAULT NULL,
  `meses_adeudados` int(11) DEFAULT NULL,
  `importe_adeudado` decimal(15,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `contratos`
--

INSERT INTO `contratos` (`Id`, `FechaInicio`, `FechaFin`, `Precio`, `InquilinoId`, `InmuebleId`, `Estado`, `FechaCreacion`, `CreadoPorId`, `TerminadoPorId`, `FechaTerminacion`, `MotivoCancelacion`, `fecha_finalizacion_real`, `multa_finalizacion`, `meses_adeudados`, `importe_adeudado`) VALUES
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

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `empleados`
--

CREATE TABLE `empleados` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(50) CHARACTER SET utf8 NOT NULL,
  `Apellido` varchar(50) CHARACTER SET utf8 NOT NULL,
  `Dni` varchar(20) CHARACTER SET utf8 NOT NULL,
  `Telefono` varchar(20) CHARACTER SET utf8 NOT NULL,
  `Email` varchar(100) CHARACTER SET utf8 NOT NULL,
  `FotoPerfil` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Rol` int(11) NOT NULL DEFAULT 1,
  `FechaIngreso` date NOT NULL DEFAULT curdate(),
  `Observaciones` varchar(500) CHARACTER SET utf8 DEFAULT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  `FechaCreacion` timestamp NOT NULL DEFAULT current_timestamp(),
  `FechaModificacion` datetime DEFAULT NULL,
  `FechaActualizacion` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `empleados`
--

INSERT INTO `empleados` (`Id`, `Nombre`, `Apellido`, `Dni`, `Telefono`, `Email`, `FotoPerfil`, `Rol`, `FechaIngreso`, `Observaciones`, `Estado`, `FechaCreacion`, `FechaModificacion`, `FechaActualizacion`) VALUES
(3, 'Admin', 'Sistema', '00000000', '0000000000', 'admin@inmobiliaria.com', NULL, 2, '2025-09-07', 'Usuario administrador del sistema', 1, '2025-09-08 00:44:01', '2025-09-07 21:44:01', '2025-09-08 00:47:53'),
(4, 'empleado', 'empleado', '55666777', '2664555555', 'empleado@inmobiliaria.com', NULL, 1, '2025-09-14', NULL, 1, '2025-09-14 23:47:42', '2025-09-20 23:37:08', '2025-09-21 02:37:08'),
(5, 'Empleado Test ', 'Inactivado', '55111222', '2661122334', 'empleado02@inmueble.com', NULL, 1, '2025-09-26', NULL, 0, '2025-09-26 21:27:42', '2025-09-26 15:31:12', '2025-09-26 18:31:12');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inmuebleimagenes`
--

CREATE TABLE `inmuebleimagenes` (
  `Id` int(11) NOT NULL,
  `InmuebleId` int(11) NOT NULL,
  `NombreArchivo` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `RutaArchivo` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `EsPortada` tinyint(1) DEFAULT 0,
  `Descripcion` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `TamanoBytes` bigint(20) DEFAULT NULL,
  `TipoMime` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `FechaCreacion` datetime DEFAULT current_timestamp(),
  `FechaActualizacion` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `inmuebleimagenes`
--

INSERT INTO `inmuebleimagenes` (`Id`, `InmuebleId`, `NombreArchivo`, `RutaArchivo`, `EsPortada`, `Descripcion`, `TamanoBytes`, `TipoMime`, `FechaCreacion`, `FechaActualizacion`) VALUES
(2, 1, '735808b4-bccc-42cb-9a39-a63c3c74fcb3.jpg', '/uploads/inmuebles/1/735808b4-bccc-42cb-9a39-a63c3c74fcb3.jpg', 1, NULL, 9377, 'image/jpeg', '2025-09-06 13:14:45', '2025-09-07 14:21:00'),
(4, 2, '182d114e-0da6-4033-a63b-3faaef7d4f91.jpg', '/uploads/inmuebles/2/182d114e-0da6-4033-a63b-3faaef7d4f91.jpg', 1, 'casa4', 8064, 'image/jpeg', '2025-09-06 16:57:43', '2025-09-06 17:12:04'),
(6, 3, '458b3fce-b86b-4df5-8ea9-0daa72fb73d4.jpg', '/uploads/inmuebles/3/458b3fce-b86b-4df5-8ea9-0daa72fb73d4.jpg', 1, NULL, 8995, 'image/jpeg', '2025-09-06 17:11:45', '2025-09-06 17:11:45'),
(7, 1, '1a4046e9-e8df-4236-8aaf-6cd53bcd253f.jpg', '/uploads/inmuebles/1/1a4046e9-e8df-4236-8aaf-6cd53bcd253f.jpg', 0, NULL, 11144, 'image/jpeg', '2025-09-07 14:20:36', '2025-09-07 14:21:00'),
(8, 4, 'c03b7cb0-c832-4f98-915c-cb1c8e141762.jpg', '/uploads/inmuebles/4/c03b7cb0-c832-4f98-915c-cb1c8e141762.jpg', 1, NULL, 8995, 'image/jpeg', '2025-09-26 15:07:03', '2025-09-26 15:07:03'),
(9, 5, '38fda3b8-6c87-4ce6-9a12-01a293fa414b.jpg', '/uploads/inmuebles/5/38fda3b8-6c87-4ce6-9a12-01a293fa414b.jpg', 1, NULL, 7864, 'image/jpeg', '2025-09-26 23:09:42', '2025-09-26 23:09:42'),
(10, 6, 'accba503-c290-49e6-a3d6-129c580baaa6.jpg', '/uploads/inmuebles/6/accba503-c290-49e6-a3d6-129c580baaa6.jpg', 1, NULL, 13305, 'image/jpeg', '2025-09-26 23:15:02', '2025-09-26 23:15:02'),
(11, 7, 'f893c8d3-a5ec-4bf2-9017-938d3a632d79.jpeg', '/uploads/inmuebles/7/f893c8d3-a5ec-4bf2-9017-938d3a632d79.jpeg', 1, NULL, 8528, 'image/jpeg', '2025-09-26 23:16:13', '2025-09-26 23:16:13');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inmuebles`
--

CREATE TABLE `inmuebles` (
  `Id` int(11) NOT NULL,
  `Direccion` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Ambientes` int(11) NOT NULL,
  `Superficie` decimal(10,2) NOT NULL,
  `Latitud` decimal(10,8) DEFAULT NULL,
  `Longitud` decimal(11,8) DEFAULT NULL,
  `PropietarioId` int(11) NOT NULL,
  `TipoId` int(11) NOT NULL,
  `Precio` decimal(15,2) DEFAULT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  `Uso` enum('Residencial','Comercial','Industrial') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Residencial',
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `Localidad` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Provincia` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `inmuebles`
--

INSERT INTO `inmuebles` (`Id`, `Direccion`, `Ambientes`, `Superficie`, `Latitud`, `Longitud`, `PropietarioId`, `TipoId`, `Precio`, `Estado`, `Uso`, `FechaCreacion`, `Localidad`, `Provincia`) VALUES
(1, 'Mirador 1 Casa 22, San Luis', 4, '120.50', '-33.25743300', '-66.33420200', 1, 1, '85000.00', 1, 'Residencial', '2025-08-23 15:40:21', 'San Luis', 'San Luis'),
(2, 'Mirador 2 Depto 10, San Luis', 3, '75.00', '-33.25819600', '-66.33419100', 2, 5, '65000.00', 0, 'Comercial', '2025-08-23 15:40:21', 'San Luis', 'San Luis'),
(3, 'Calle 51 número 1120', 2, '70.00', '-33.25819600', '-66.33419100', 1, 2, '250000.00', 1, 'Comercial', '2025-08-30 21:11:41', 'San Luis', 'San Luis'),
(4, 'Las Heras 480', 5, '100.00', '-33.68324458', '-65.46910645', 8, 1, '300000.00', 1, 'Residencial', '2025-09-26 15:05:31', 'Villa Mercedes', 'San Luis'),
(5, 'Pueyrredon 859', 8, '120.00', '-33.67635087', '-65.46774590', 1, 9, '400000.00', 1, 'Residencial', '2025-09-26 23:08:34', 'Villa Mercedes', 'San Luis'),
(6, 'Av. Los Crespones y Los Abetos', 6, '85.00', '-33.26848806', '-66.22638992', 8, 2, '350000.00', 1, 'Residencial', '2025-09-26 23:12:49', 'Juana Koslay', 'San Luis'),
(7, 'Int. Garro Zollo s/n', 2, '1000.00', '-33.32742011', '-66.37223601', 5, 7, '500000.00', 1, 'Industrial', '2025-09-26 23:14:36', 'San Luis', 'San Luis');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inquilinos`
--

CREATE TABLE `inquilinos` (
  `Id` int(11) NOT NULL,
  `DNI` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Apellido` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Telefono` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Email` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Direccion` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `Estado` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `inquilinos`
--

INSERT INTO `inquilinos` (`Id`, `DNI`, `Nombre`, `Apellido`, `Telefono`, `Email`, `Direccion`, `FechaCreacion`, `Estado`) VALUES
(1, '30111222', 'Carlos', 'Rodríguez', '2651111111', 'carlos.rodriguez@email.com', 'Mitre 789, San Luis', '2025-08-23 15:40:20', 1),
(2, '33000111', 'Ana', 'Martínez', '2652222222', 'ana.martinez@email.com', 'Belgrano 321, San Luis', '2025-08-23 15:40:20', 1),
(3, '22222222', 'Jesus Emanuel', 'Garcia', '2664123456', 'testinquilino@gmail.com', 'Mira 2', '2025-08-23 15:50:25', 0),
(4, '32654789', 'Test', 'Modals', '2664567890', 'modal@inmuebles.com', 'Mira 3', '2025-09-12 11:20:27', 1),
(5, '99888555', 'Propietario', 'Apellido Prop', '2664444444', 'propietario@inmueble.com', 'Juan Llerena 55, Villa Mercedes, San Luis', '2025-09-21 15:40:45', 0),
(6, '33666777', 'Inquilino Test VM', 'Garcia', '2657774411', 'inquilino.vm@test.com', 'Belgrano 216', '2025-09-26 15:43:21', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pagos`
--

CREATE TABLE `pagos` (
  `Id` int(11) NOT NULL,
  `Numero` int(11) NOT NULL,
  `FechaPago` date DEFAULT NULL,
  `ContratoId` int(11) NOT NULL,
  `Importe` decimal(15,2) NOT NULL,
  `Estado` enum('Pendiente','Pagado','Vencido') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Pendiente',
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `CreadoPorId` int(11) DEFAULT NULL,
  `AnuladoPorId` int(11) DEFAULT NULL,
  `FechaAnulacion` datetime DEFAULT NULL,
  `metodo_pago` enum('MercadoPago','Transferencia','Efectivo','Cheque','Otro') COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `observaciones` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Intereses` decimal(18,2) NOT NULL DEFAULT 0.00,
  `Multas` decimal(18,2) NOT NULL DEFAULT 0.00,
  `FechaVencimiento` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `pagos`
--

INSERT INTO `pagos` (`Id`, `Numero`, `FechaPago`, `ContratoId`, `Importe`, `Estado`, `FechaCreacion`, `CreadoPorId`, `AnuladoPorId`, `FechaAnulacion`, `metodo_pago`, `observaciones`, `Intereses`, `Multas`, `FechaVencimiento`) VALUES
(1, 1, '2025-08-01', 5, '350000.00', 'Vencido', '2025-08-31 14:08:28', 3, NULL, NULL, NULL, '.', '105000.00', '0.00', '2025-08-10 00:00:00'),
(3, 3, '2025-09-12', 5, '350000.00', 'Pagado', '2025-08-31 14:08:28', 3, NULL, NULL, 'Efectivo', NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(4, 1, '2025-08-05', 1, '650000.00', 'Pagado', '2025-08-31 20:10:54', 3, NULL, NULL, 'MercadoPago', NULL, '0.00', '0.00', '2025-08-10 00:00:00'),
(5, 2, '2025-09-08', 1, '650000.00', 'Pagado', '2025-08-31 20:11:23', 3, NULL, NULL, 'MercadoPago', NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(6, 1, '2025-08-01', 6, '65000.00', 'Vencido', '2025-08-31 23:40:37', 3, NULL, NULL, NULL, '.', '19500.00', '0.00', '2025-08-10 00:00:00'),
(11, 6, '2025-09-01', 6, '65000.00', 'Vencido', '2025-08-31 23:40:37', 3, NULL, NULL, NULL, NULL, '6500.00', '0.00', '2025-09-10 00:00:00'),
(17, 6, '2025-09-01', 7, '250000.00', 'Vencido', '2025-09-06 18:52:31', 3, NULL, NULL, NULL, NULL, '25000.00', '0.00', '2025-09-10 00:00:00'),
(36, 1, '2025-09-01', 11, '65000.00', 'Vencido', '2025-09-06 19:44:07', 3, NULL, NULL, NULL, NULL, '6500.00', '0.00', '2025-09-10 00:00:00'),
(42, 1, '2025-09-01', 12, '85000.00', 'Vencido', '2025-09-06 20:05:19', 3, NULL, NULL, NULL, NULL, '8500.00', '0.00', '2025-09-10 00:00:00'),
(48, 1, '2025-09-01', 13, '250000.00', 'Vencido', '2025-09-06 20:10:24', 3, NULL, NULL, NULL, NULL, '25000.00', '0.00', '2025-09-10 00:00:00'),
(54, 1, '2025-09-01', 14, '100000.00', 'Vencido', '2025-09-06 20:16:30', 3, NULL, NULL, NULL, ' - Incluye multa por finalización temprana ($200.000)', '10000.00', '0.00', '2025-09-10 00:00:00'),
(60, 1, '2025-09-01', 15, '250000.00', 'Vencido', '2025-09-06 20:36:25', 3, NULL, NULL, NULL, ' - Incluye multa por finalización temprana ($500.000)', '25000.00', '0.00', '2025-09-10 00:00:00'),
(66, 1, '2025-09-01', 16, '50000.00', 'Vencido', '2025-09-06 20:42:05', 3, NULL, NULL, NULL, ' - Incluye multa por finalización temprana ($100.000)', '5000.00', '0.00', '2025-09-10 00:00:00'),
(72, 1, '2025-09-01', 17, '100000.00', 'Vencido', '2025-09-06 20:48:43', 3, NULL, NULL, NULL, ' - Incluye multa por finalización temprana ($200.000)', '10000.00', '0.00', '2025-09-10 00:00:00'),
(78, 1, '2025-09-07', 18, '100000.00', 'Pagado', '2025-09-07 11:18:16', 3, NULL, NULL, 'MercadoPago', 'm', '0.00', '200000.00', '2025-09-10 00:00:00'),
(84, 1, '2025-09-15', 19, '85000.00', 'Pagado', '2025-09-08 19:48:41', 3, NULL, NULL, 'Efectivo', NULL, '0.00', '0.00', '2025-10-10 00:00:00'),
(85, 2, '2025-11-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-11-10 00:00:00'),
(86, 3, '2025-12-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-12-10 00:00:00'),
(87, 4, '2026-01-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-01-10 00:00:00'),
(88, 5, '2026-02-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-02-10 00:00:00'),
(89, 6, '2026-03-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-03-10 00:00:00'),
(90, 7, '2026-04-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-04-10 00:00:00'),
(91, 8, '2026-05-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-05-10 00:00:00'),
(92, 9, '2026-06-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-06-10 00:00:00'),
(93, 10, '2026-07-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-07-10 00:00:00'),
(94, 11, '2026-08-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-08-10 00:00:00'),
(95, 12, '2026-09-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-09-10 00:00:00'),
(96, 13, '2026-10-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-10-10 00:00:00'),
(97, 14, '2026-11-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-11-10 00:00:00'),
(98, 15, '2026-12-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-12-10 00:00:00'),
(99, 16, '2027-01-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2027-01-10 00:00:00'),
(100, 17, '2027-02-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2027-02-10 00:00:00'),
(101, 18, '2027-03-01', 19, '85000.00', 'Pendiente', '2025-09-08 19:48:41', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2027-03-10 00:00:00'),
(102, 1, '2025-09-01', 20, '250000.00', 'Vencido', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '25000.00', '0.00', '2025-09-10 00:00:00'),
(103, 2, '2025-10-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-10-10 00:00:00'),
(104, 3, '2025-11-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-11-10 00:00:00'),
(105, 4, '2025-12-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-12-10 00:00:00'),
(106, 5, '2026-01-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-01-10 00:00:00'),
(107, 6, '2026-02-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-02-10 00:00:00'),
(108, 7, '2026-03-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-03-10 00:00:00'),
(109, 8, '2026-04-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-04-10 00:00:00'),
(110, 9, '2026-05-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-05-10 00:00:00'),
(111, 10, '2026-06-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-06-10 00:00:00'),
(112, 11, '2026-07-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-07-10 00:00:00'),
(113, 12, '2026-08-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-08-10 00:00:00'),
(114, 13, '2026-09-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-09-10 00:00:00'),
(115, 14, '2026-10-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-10-10 00:00:00'),
(116, 15, '2026-11-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-11-10 00:00:00'),
(117, 16, '2026-12-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-12-10 00:00:00'),
(118, 17, '2027-01-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2027-01-10 00:00:00'),
(119, 18, '2027-02-01', 20, '250000.00', 'Pendiente', '2025-09-15 06:38:08', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2027-02-10 00:00:00'),
(120, 1, '2025-09-01', 21, '300000.00', 'Vencido', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '45000.00', '0.00', '2025-09-10 00:00:00'),
(121, 2, '2025-10-01', 21, '300000.00', 'Pendiente', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-10-10 00:00:00'),
(122, 3, '2025-11-01', 21, '300000.00', 'Pendiente', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-11-10 00:00:00'),
(123, 4, '2025-12-01', 21, '300000.00', 'Pendiente', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2025-12-10 00:00:00'),
(124, 5, '2026-01-01', 21, '300000.00', 'Pendiente', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-01-10 00:00:00'),
(125, 6, '2026-02-01', 21, '300000.00', 'Pendiente', '2025-09-26 15:45:24', 3, NULL, NULL, NULL, NULL, '0.00', '0.00', '2026-02-10 00:00:00');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `propietarios`
--

CREATE TABLE `propietarios` (
  `Id` int(11) NOT NULL,
  `DNI` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Apellido` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Telefono` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Email` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Direccion` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `Estado` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `propietarios`
--

INSERT INTO `propietarios` (`Id`, `DNI`, `Nombre`, `Apellido`, `Telefono`, `Email`, `Direccion`, `FechaCreacion`, `Estado`) VALUES
(1, '35456987', 'José Maria', 'Pérez', '2657123456', 'jose.perez@email.com', 'Av. San Martín 1234, San Luis', '2025-08-23 15:40:17', 1),
(2, '36987456', 'María', 'González', '2664896547', 'maria.gonzalez@email.com', 'Rivadavia 456, San Luis', '2025-08-23 15:40:17', 1),
(3, '55555555', 'Omar', 'Propo', '2664789654', 'testpropietario@a.com', 'Casa s/n', '2025-08-23 17:12:59', 0),
(4, '11222333', 'Test', 'Prueba', '2664112233', 'test@test.com', 'Direccion 123', '2025-09-07 11:28:08', 0),
(5, '44555666', 'Ruben', 'Lopez', '2661111111', 'ruben@propietario.com', 'Colon 469', '2025-09-14 20:45:50', 1),
(7, '99888555', 'Propietario', 'Apellido Prop', '2664444444', 'propietario@inmueble.com', NULL, '2025-09-20 14:09:38', 1),
(8, '11444777', 'Rafa', 'Gomez', '2662222222', 'test02@inmueble.com', 'Mirador 3', '2025-09-21 15:54:03', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tiposinmueble`
--

CREATE TABLE `tiposinmueble` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcion` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `tiposinmueble`
--

INSERT INTO `tiposinmueble` (`Id`, `Nombre`, `Descripcion`, `Estado`, `FechaCreacion`) VALUES
(1, 'Casa', 'Casa unifamiliar', 1, '2025-09-27 00:40:08'),
(2, 'Departamento', 'Departamento en edificio', 1, '2025-09-27 00:40:08'),
(3, 'Monoambiente', 'Departamento de un solo ambiente', 1, '2025-09-27 00:40:08'),
(4, 'Local', 'Local comercial', 1, '2025-09-27 00:40:08'),
(5, 'Oficina', 'Oficina comercial', 1, '2025-09-27 00:40:08'),
(6, 'Terreno', 'Terreno baldío', 1, '2025-09-27 00:40:08'),
(7, 'Galpón', 'Galpón industrial', 1, '2025-09-27 00:40:08'),
(8, 'Campo', 'Campo rural', 1, '2025-09-27 01:49:35'),
(9, 'Quinta', 'Quinta', 1, '2025-09-27 01:56:19');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `Id` int(11) NOT NULL,
  `NombreUsuario` varchar(50) CHARACTER SET utf8 NOT NULL,
  `Email` varchar(100) CHARACTER SET utf8 NOT NULL,
  `ClaveHash` varchar(255) CHARACTER SET utf8 NOT NULL,
  `FotoPerfil` varchar(255) CHARACTER SET utf8 DEFAULT NULL,
  `Rol` int(11) NOT NULL,
  `FechaCreacion` timestamp NOT NULL DEFAULT current_timestamp(),
  `UltimoAcceso` timestamp NULL DEFAULT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT 1,
  `EmpleadoId` int(11) DEFAULT NULL,
  `PropietarioId` int(11) DEFAULT NULL,
  `InquilinoId` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`Id`, `NombreUsuario`, `Email`, `ClaveHash`, `FotoPerfil`, `Rol`, `FechaCreacion`, `UltimoAcceso`, `Estado`, `EmpleadoId`, `PropietarioId`, `InquilinoId`) VALUES
(3, 'admin', 'admin@inmobiliaria.com', '$2a$11$n4zTqE601oaEZkLIo./fzOpbwOC/MCv2DF/z02HPHG50sNZGjdDJ.', '/uploads/profiles/user_3_8d30b0b7-914d-44b9-b39a-7ab29c39f299.jpg', 4, '2025-09-08 00:44:02', '2025-09-27 17:07:44', 1, 3, NULL, NULL),
(4, 'eempleado', 'empleado@inmobiliaria.com', '$2a$11$4Ohais3PBVswY2a1l25Co.u./Nu8.ARLnpQmFSJncdD/FRSUq556S', '/uploads/profiles/user_4_324c8e83-52ff-402d-87ab-3ae93918f412.jpg', 3, '2025-09-14 23:47:42', '2025-09-27 17:08:28', 1, 4, NULL, NULL),
(5, 'propietario', 'propietario@inmueble.com', '$2a$11$VUIUnDYQrnmUd5spL1rnGe4d1xCxiT.J9YFHMvbWbv2q9dTHBG/tS', '/uploads/profiles/user_default.png', 1, '2025-09-20 17:09:39', '2025-09-21 18:29:57', 1, NULL, 7, NULL),
(7, 'einactivo', 'empleado02@inmueble.com', '$2a$11$qq6s2qy326e0zuKAHc.D7OTCqCAxZdNHVHQ2nHZHixTkPXcWgEsua', '/uploads/profiles/user_default.png', 3, '2025-09-26 18:27:42', '2025-09-26 18:31:28', 1, 5, NULL, NULL);

--
-- Disparadores `usuarios`
--
DELIMITER $$
CREATE TRIGGER `tr_usuarios_validate_role_fk` BEFORE INSERT ON `usuarios` FOR EACH ROW BEGIN
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
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `tr_usuarios_validate_role_fk_update` BEFORE UPDATE ON `usuarios` FOR EACH ROW BEGIN
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
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Estructura Stand-in para la vista `vw_usuarios_completo`
-- (Véase abajo para la vista actual)
--
CREATE TABLE `vw_usuarios_completo` (
`Id` int(11)
,`NombreUsuario` varchar(50)
,`Email` varchar(100)
,`FotoPerfil` varchar(255)
,`Rol` int(11)
,`FechaCreacion` timestamp
,`UltimoAcceso` timestamp
,`Estado` tinyint(1)
,`RolDescripcion` varchar(13)
,`NombreCompleto` varchar(201)
,`Dni` varchar(20)
,`Telefono` varchar(20)
,`RolEmpleado` int(11)
,`FechaIngreso` date
);

-- --------------------------------------------------------

--
-- Estructura para la vista `vw_usuarios_completo`
--
DROP TABLE IF EXISTS `vw_usuarios_completo`;

CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`localhost` SQL SECURITY DEFINER VIEW `vw_usuarios_completo`  AS SELECT `u`.`Id` AS `Id`, `u`.`NombreUsuario` AS `NombreUsuario`, `u`.`Email` AS `Email`, `u`.`FotoPerfil` AS `FotoPerfil`, `u`.`Rol` AS `Rol`, `u`.`FechaCreacion` AS `FechaCreacion`, `u`.`UltimoAcceso` AS `UltimoAcceso`, `u`.`Estado` AS `Estado`, CASE `u`.`Rol` WHEN 1 THEN 'Propietario' WHEN 2 THEN 'Inquilino' WHEN 3 THEN 'Empleado' WHEN 4 THEN 'Administrador' ELSE 'Sin Rol' END AS `RolDescripcion`, CASE WHEN `u`.`Rol` = 1 THEN concat(`p`.`Nombre`,' ',`p`.`Apellido`) WHEN `u`.`Rol` = 2 THEN concat(`i`.`Nombre`,' ',`i`.`Apellido`) WHEN `u`.`Rol` in (3,4) THEN concat(`e`.`Nombre`,' ',`e`.`Apellido`) ELSE 'Sin Nombre' END AS `NombreCompleto`, CASE WHEN `u`.`Rol` = 1 THEN `p`.`DNI` WHEN `u`.`Rol` = 2 THEN `i`.`DNI` WHEN `u`.`Rol` in (3,4) THEN `e`.`Dni` ELSE NULL END AS `Dni`, CASE WHEN `u`.`Rol` = 1 THEN `p`.`Telefono` WHEN `u`.`Rol` = 2 THEN `i`.`Telefono` WHEN `u`.`Rol` in (3,4) THEN `e`.`Telefono` ELSE NULL END AS `Telefono`, `e`.`Rol` AS `RolEmpleado`, `e`.`FechaIngreso` AS `FechaIngreso` FROM (((`usuarios` `u` left join `propietarios` `p` on(`u`.`PropietarioId` = `p`.`Id`)) left join `inquilinos` `i` on(`u`.`InquilinoId` = `i`.`Id`)) left join `empleados` `e` on(`u`.`EmpleadoId` = `e`.`Id`)) ;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `configuraciones`
--
ALTER TABLE `configuraciones`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Clave` (`Clave`),
  ADD KEY `idx_clave` (`Clave`),
  ADD KEY `idx_tipo` (`Tipo`);

--
-- Indices de la tabla `contratos`
--
ALTER TABLE `contratos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IDX_contratos_inquilino` (`InquilinoId`),
  ADD KEY `IDX_contratos_inmueble` (`InmuebleId`),
  ADD KEY `idx_creado_por` (`CreadoPorId`),
  ADD KEY `idx_terminado_por` (`TerminadoPorId`),
  ADD KEY `idx_fecha_terminacion` (`FechaTerminacion`);

--
-- Indices de la tabla `empleados`
--
ALTER TABLE `empleados`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Dni` (`Dni`),
  ADD UNIQUE KEY `Email` (`Email`),
  ADD KEY `idx_empleados_dni` (`Dni`),
  ADD KEY `idx_empleados_email` (`Email`),
  ADD KEY `idx_empleados_estado` (`Estado`);

--
-- Indices de la tabla `inmuebleimagenes`
--
ALTER TABLE `inmuebleimagenes`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `idx_inmueble_id` (`InmuebleId`),
  ADD KEY `idx_es_portada` (`InmuebleId`,`EsPortada`);

--
-- Indices de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IDX_inmuebles_propietario` (`PropietarioId`),
  ADD KEY `idx_inmuebles_estado` (`Estado`),
  ADD KEY `idx_tipo_id` (`TipoId`);

--
-- Indices de la tabla `inquilinos`
--
ALTER TABLE `inquilinos`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UK_inquilinos_DNI` (`DNI`),
  ADD UNIQUE KEY `UK_inquilinos_Email` (`Email`);

--
-- Indices de la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IDX_pagos_contrato` (`ContratoId`),
  ADD KEY `IX_Pagos_FechaVencimiento` (`FechaVencimiento`),
  ADD KEY `idx_pagos_creado_por` (`CreadoPorId`),
  ADD KEY `idx_pagos_anulado_por` (`AnuladoPorId`),
  ADD KEY `idx_pagos_fecha_anulacion` (`FechaAnulacion`);

--
-- Indices de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UK_propietarios_DNI` (`DNI`),
  ADD UNIQUE KEY `UK_propietarios_Email` (`Email`);

--
-- Indices de la tabla `tiposinmueble`
--
ALTER TABLE `tiposinmueble`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Nombre` (`Nombre`),
  ADD KEY `idx_nombre` (`Nombre`),
  ADD KEY `idx_estado` (`Estado`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `NombreUsuario` (`NombreUsuario`),
  ADD UNIQUE KEY `Email` (`Email`),
  ADD KEY `idx_usuarios_email` (`Email`),
  ADD KEY `idx_usuarios_nombreusuario` (`NombreUsuario`),
  ADD KEY `idx_usuarios_rol` (`Rol`),
  ADD KEY `idx_usuarios_estado` (`Estado`),
  ADD KEY `idx_usuarios_empleado` (`EmpleadoId`),
  ADD KEY `idx_usuarios_propietario` (`PropietarioId`),
  ADD KEY `idx_usuarios_inquilino` (`InquilinoId`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `configuraciones`
--
ALTER TABLE `configuraciones`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=16;

--
-- AUTO_INCREMENT de la tabla `contratos`
--
ALTER TABLE `contratos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=22;

--
-- AUTO_INCREMENT de la tabla `empleados`
--
ALTER TABLE `empleados`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `inmuebleimagenes`
--
ALTER TABLE `inmuebleimagenes`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `inquilinos`
--
ALTER TABLE `inquilinos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `pagos`
--
ALTER TABLE `pagos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=126;

--
-- AUTO_INCREMENT de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT de la tabla `tiposinmueble`
--
ALTER TABLE `tiposinmueble`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `contratos`
--
ALTER TABLE `contratos`
  ADD CONSTRAINT `FK_contratos_inmuebles_v2` FOREIGN KEY (`InmuebleId`) REFERENCES `inmuebles` (`Id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_contratos_inquilinos_v2` FOREIGN KEY (`InquilinoId`) REFERENCES `inquilinos` (`Id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_contratos_creado_por` FOREIGN KEY (`CreadoPorId`) REFERENCES `usuarios` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_contratos_terminado_por` FOREIGN KEY (`TerminadoPorId`) REFERENCES `usuarios` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Filtros para la tabla `inmuebleimagenes`
--
ALTER TABLE `inmuebleimagenes`
  ADD CONSTRAINT `inmuebleimagenes_ibfk_1` FOREIGN KEY (`InmuebleId`) REFERENCES `inmuebles` (`Id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  ADD CONSTRAINT `FK_inmuebles_propietarios_v2` FOREIGN KEY (`PropietarioId`) REFERENCES `propietarios` (`Id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_inmuebles_tipo` FOREIGN KEY (`TipoId`) REFERENCES `tiposinmueble` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD CONSTRAINT `FK_pagos_contratos_v2` FOREIGN KEY (`ContratoId`) REFERENCES `contratos` (`Id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_pagos_anulado_por` FOREIGN KEY (`AnuladoPorId`) REFERENCES `usuarios` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_pagos_creado_por` FOREIGN KEY (`CreadoPorId`) REFERENCES `usuarios` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Filtros para la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD CONSTRAINT `usuarios_ibfk_1` FOREIGN KEY (`EmpleadoId`) REFERENCES `empleados` (`Id`) ON DELETE SET NULL,
  ADD CONSTRAINT `usuarios_ibfk_2` FOREIGN KEY (`PropietarioId`) REFERENCES `propietarios` (`Id`) ON DELETE SET NULL,
  ADD CONSTRAINT `usuarios_ibfk_3` FOREIGN KEY (`InquilinoId`) REFERENCES `inquilinos` (`Id`) ON DELETE SET NULL;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
