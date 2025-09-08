-- phpMyAdmin SQL Dump
-- version 5.1.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 08-09-2025 a las 17:59:48
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

CREATE TABLE `configuraciones` (
  `Id` int(11) NOT NULL,
  `Clave` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Valor` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcion` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Tipo` int(11) NOT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `FechaModificacion` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `configuraciones`
--

INSERT INTO `configuraciones` (`Id`, `Clave`, `Valor`, `Descripcion`, `Tipo`, `FechaCreacion`, `FechaModificacion`) VALUES
(1, 'MESES_MINIMOS_6', 'True', 'Opción de 6 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-07 14:19:51'),
(2, 'MESES_MINIMOS_12', 'True', 'Opción de 12 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-07 14:19:51'),
(3, 'MESES_MINIMOS_18', 'True', 'Opción de 18 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-07 14:19:51'),
(4, 'MESES_MINIMOS_24', 'True', 'Opción de 24 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-07 14:19:51'),
(5, 'MESES_MINIMOS_30', 'True', 'Opción de 30 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-07 14:19:51'),
(6, 'MESES_MINIMOS_36', 'True', 'Opción de 36 meses de alquiler mínimo', 1, '2025-08-31 13:34:01', '2025-09-07 14:19:51'),
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
  `MotivoCancelacion` text COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fecha_finalizacion_real` date DEFAULT NULL,
  `multa_finalizacion` decimal(15,2) DEFAULT NULL,
  `meses_adeudados` int(11) DEFAULT NULL,
  `importe_adeudado` decimal(15,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `contratos`
--

INSERT INTO `contratos` (`Id`, `FechaInicio`, `FechaFin`, `Precio`, `InquilinoId`, `InmuebleId`, `Estado`, `FechaCreacion`, `MotivoCancelacion`, `fecha_finalizacion_real`, `multa_finalizacion`, `meses_adeudados`, `importe_adeudado`) VALUES
(1, '2025-08-01', '2025-12-31', '650000.00', 1, 1, 'Finalizado', '2025-08-30 14:13:05', NULL, NULL, NULL, NULL, NULL),
(2, '2025-08-20', '2025-09-06', '400000.00', 2, 1, 'Cancelado', '2025-08-30 18:12:38', NULL, NULL, NULL, NULL, NULL),
(4, '2025-09-01', '2025-12-31', '300000.00', 2, 2, 'Cancelado', '2025-08-31 13:54:54', NULL, NULL, NULL, NULL, NULL),
(5, '2025-09-01', '2025-12-31', '350000.00', 2, 3, 'Finalizado', '2025-08-31 14:08:28', NULL, NULL, NULL, NULL, NULL),
(6, '2025-09-01', '2026-03-31', '65000.00', 2, 2, 'Finalizado', '2025-08-31 23:40:37', 'xxx', NULL, NULL, NULL, NULL),
(7, '2025-09-01', '2026-03-31', '250000.00', 1, 3, 'Finalizado', '2025-09-06 18:52:31', NULL, NULL, NULL, NULL, NULL),
(8, '2025-09-06', '2026-03-31', '85000.00', 1, 1, 'Finalizado', '2025-09-06 19:29:42', NULL, NULL, NULL, NULL, NULL),
(9, '2025-09-06', '2026-03-31', '85000.00', 1, 1, 'Finalizado', '2025-09-06 19:31:26', NULL, NULL, NULL, NULL, NULL),
(10, '2025-09-01', '2026-02-28', '250000.00', 2, 3, 'Finalizado', '2025-09-06 19:32:59', NULL, NULL, NULL, NULL, NULL),
(11, '2025-09-01', '2026-03-31', '65000.00', 2, 2, 'Finalizado', '2025-09-06 19:44:07', NULL, NULL, NULL, NULL, NULL),
(12, '2025-09-01', '2026-03-31', '85000.00', 1, 1, 'Finalizado', '2025-09-06 20:05:19', NULL, NULL, NULL, NULL, NULL),
(13, '2025-09-01', '2026-03-31', '250000.00', 2, 3, 'Finalizado', '2025-09-06 20:10:24', NULL, NULL, NULL, NULL, NULL),
(14, '2025-09-01', '2026-03-31', '100000.00', 1, 1, 'Finalizado', '2025-09-06 20:16:30', NULL, NULL, NULL, NULL, NULL),
(15, '2025-09-01', '2026-03-31', '250000.00', 1, 3, 'Finalizado', '2025-09-06 20:36:25', NULL, NULL, NULL, NULL, NULL),
(16, '2025-09-01', '2026-03-31', '50000.00', 2, 2, 'Finalizado', '2025-09-06 20:42:05', NULL, NULL, NULL, NULL, NULL),
(17, '2025-09-06', '2026-03-31', '100000.00', 2, 1, 'Finalizado', '2025-09-06 20:48:43', NULL, NULL, NULL, NULL, NULL),
(18, '2025-09-01', '2026-03-31', '100000.00', 1, 1, 'Finalizado', '2025-09-07 11:18:16', NULL, NULL, NULL, NULL, NULL);

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
(3, 'Admin', 'Sistema', '00000000', '0000000000', 'admin@inmobiliaria.com', NULL, 2, '2025-09-07', 'Usuario administrador del sistema', 1, '2025-09-08 00:44:01', '2025-09-07 21:44:01', '2025-09-08 00:47:53');

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
(7, 1, '1a4046e9-e8df-4236-8aaf-6cd53bcd253f.jpg', '/uploads/inmuebles/1/1a4046e9-e8df-4236-8aaf-6cd53bcd253f.jpg', 0, NULL, 11144, 'image/jpeg', '2025-09-07 14:20:36', '2025-09-07 14:21:00');

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
  `Tipo` enum('Casa','Departamento','Monoambiente','Local','Oficina','Terreno','Galpón') COLLATE utf8mb4_unicode_ci NOT NULL,
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

INSERT INTO `inmuebles` (`Id`, `Direccion`, `Ambientes`, `Superficie`, `Latitud`, `Longitud`, `PropietarioId`, `Tipo`, `Precio`, `Estado`, `Uso`, `FechaCreacion`, `Localidad`, `Provincia`) VALUES
(1, 'Mirador 1 Casa 22, San Luis', 4, '120.50', '-33.25743300', '-66.33420200', 1, 'Casa', '85000.00', 1, 'Residencial', '2025-08-23 15:40:21', 'San Luis', 'San Luis'),
(2, 'Mirador 2 Depto 10, San Luis', 3, '75.00', '-33.25819600', '-66.33419100', 2, 'Oficina', '65000.00', 0, 'Comercial', '2025-08-23 15:40:21', 'San Luis', 'San Luis'),
(3, 'Calle 51 número 1120', 2, '70.00', '-33.25819600', '-66.33419100', 1, 'Departamento', '250000.00', 1, 'Comercial', '2025-08-30 21:11:41', 'San Luis', 'San Luis');

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
(3, '22222222', 'Jesus', 'Garcia', '2664123456', 'testinquilino@gmail.com', 'Mira 2', '2025-08-23 15:50:25', 0);

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
  `metodo_pago` enum('MercadoPago','Transferencia','Efectivo','Cheque','Otro') COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `observaciones` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Intereses` decimal(18,2) NOT NULL DEFAULT 0.00,
  `Multas` decimal(18,2) NOT NULL DEFAULT 0.00,
  `FechaVencimiento` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `pagos`
--

INSERT INTO `pagos` (`Id`, `Numero`, `FechaPago`, `ContratoId`, `Importe`, `Estado`, `FechaCreacion`, `metodo_pago`, `observaciones`, `Intereses`, `Multas`, `FechaVencimiento`) VALUES
(1, 1, '2025-08-01', 5, '350000.00', 'Vencido', '2025-08-31 14:08:28', NULL, '.', '105000.00', '0.00', '2025-08-10 00:00:00'),
(3, 3, '2025-09-01', 5, '350000.00', 'Pendiente', '2025-08-31 14:08:28', NULL, NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(4, 1, '2025-08-05', 1, '650000.00', 'Pagado', '2025-08-31 20:10:54', 'MercadoPago', NULL, '0.00', '0.00', '2025-08-10 00:00:00'),
(5, 2, '2025-09-01', 1, '650000.00', 'Pendiente', '2025-08-31 20:11:23', NULL, NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(6, 1, '2025-08-01', 6, '65000.00', 'Vencido', '2025-08-31 23:40:37', NULL, '.', '19500.00', '0.00', '2025-08-10 00:00:00'),
(11, 6, '2025-09-01', 6, '65000.00', 'Pendiente', '2025-08-31 23:40:37', NULL, NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(17, 6, '2025-09-01', 7, '250000.00', 'Pendiente', '2025-09-06 18:52:31', NULL, NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(36, 1, '2025-09-01', 11, '65000.00', 'Pendiente', '2025-09-06 19:44:07', NULL, NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(42, 1, '2025-09-01', 12, '85000.00', 'Pendiente', '2025-09-06 20:05:19', NULL, NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(48, 1, '2025-09-01', 13, '250000.00', 'Pendiente', '2025-09-06 20:10:24', NULL, NULL, '0.00', '0.00', '2025-09-10 00:00:00'),
(54, 1, '2025-09-01', 14, '100000.00', 'Pendiente', '2025-09-06 20:16:30', NULL, ' - Incluye multa por finalización temprana ($200.000)', '0.00', '0.00', '2025-09-10 00:00:00'),
(60, 1, '2025-09-01', 15, '250000.00', 'Pendiente', '2025-09-06 20:36:25', NULL, ' - Incluye multa por finalización temprana ($500.000)', '0.00', '0.00', '2025-09-10 00:00:00'),
(66, 1, '2025-09-01', 16, '50000.00', 'Pendiente', '2025-09-06 20:42:05', NULL, ' - Incluye multa por finalización temprana ($100.000)', '0.00', '0.00', '2025-09-10 00:00:00'),
(72, 1, '2025-09-01', 17, '100000.00', 'Pendiente', '2025-09-06 20:48:43', NULL, ' - Incluye multa por finalización temprana ($200.000)', '0.00', '0.00', '2025-09-10 00:00:00'),
(78, 1, '2025-09-07', 18, '100000.00', 'Pagado', '2025-09-07 11:18:16', 'MercadoPago', 'm', '0.00', '200000.00', '2025-09-10 00:00:00');

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
(1, '35456987', 'José Maria', 'Pérez', '2657123456', 'jose.perez@email.com', 'Av. San Martín 123, San Luis', '2025-08-23 15:40:17', 1),
(2, '36987456', 'María', 'González', '2664896547', 'maria.gonzalez@email.com', 'Rivadavia 456, San Luis', '2025-08-23 15:40:17', 1),
(3, '55555555', 'Omar', 'Propo', '2664789654', 'testpropietario@a.com', 'Casa s/n', '2025-08-23 17:12:59', 0),
(4, '11222333', 'Test', 'Prueba', '2664112233', 'test@test.com', 'Direccion 123', '2025-09-07 11:28:08', 0);

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
(3, 'admin', 'admin@inmobiliaria.com', '$2a$11$n4zTqE601oaEZkLIo./fzOpbwOC/MCv2DF/z02HPHG50sNZGjdDJ.', '/uploads/profiles/user_3_b1193c52-3b4d-40ee-a601-4fdaaa59fc5e.jpg', 4, '2025-09-08 00:44:02', '2025-09-08 00:44:14', 1, 3, NULL, NULL);

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
  ADD KEY `IDX_contratos_inmueble` (`InmuebleId`);

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
  ADD KEY `idx_inmuebles_estado` (`Estado`);

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
  ADD KEY `IX_Pagos_FechaVencimiento` (`FechaVencimiento`);

--
-- Indices de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UK_propietarios_DNI` (`DNI`),
  ADD UNIQUE KEY `UK_propietarios_Email` (`Email`);

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
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT de la tabla `empleados`
--
ALTER TABLE `empleados`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `inmuebleimagenes`
--
ALTER TABLE `inmuebleimagenes`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `inquilinos`
--
ALTER TABLE `inquilinos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `pagos`
--
ALTER TABLE `pagos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=84;

--
-- AUTO_INCREMENT de la tabla `propietarios`
--
ALTER TABLE `propietarios`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `contratos`
--
ALTER TABLE `contratos`
  ADD CONSTRAINT `FK_contratos_inmuebles_v2` FOREIGN KEY (`InmuebleId`) REFERENCES `inmuebles` (`Id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_contratos_inquilinos_v2` FOREIGN KEY (`InquilinoId`) REFERENCES `inquilinos` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `inmuebleimagenes`
--
ALTER TABLE `inmuebleimagenes`
  ADD CONSTRAINT `inmuebleimagenes_ibfk_1` FOREIGN KEY (`InmuebleId`) REFERENCES `inmuebles` (`Id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `inmuebles`
--
ALTER TABLE `inmuebles`
  ADD CONSTRAINT `FK_inmuebles_propietarios_v2` FOREIGN KEY (`PropietarioId`) REFERENCES `propietarios` (`Id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD CONSTRAINT `FK_pagos_contratos_v2` FOREIGN KEY (`ContratoId`) REFERENCES `contratos` (`Id`) ON UPDATE CASCADE;

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
