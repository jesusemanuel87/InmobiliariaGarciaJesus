using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly MySqlConnectionManager _connectionManager;

        public PropietariosController(MySqlConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        // GET: Propietarios
        public async Task<IActionResult> Index()
        {
            var propietarios = new List<Propietario>();
            
            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT * FROM propietarios WHERE Estado = 1 ORDER BY Apellido, Nombre";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                propietarios.Add(new Propietario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    DNI = reader["DNI"].ToString(),
                    Nombre = reader["Nombre"].ToString(),
                    Apellido = reader["Apellido"].ToString(),
                    Telefono = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"].ToString(),
                    Email = reader["Email"].ToString(),
                    Direccion = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"].ToString(),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    Estado = Convert.ToBoolean(reader["Estado"])
                });
            }
            
            return View(propietarios);
        }

        // GET: Propietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using var connection = await _connectionManager.GetOpenConnectionAsync();
            
            // Obtener propietario
            var query = "SELECT * FROM propietarios WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }
            
            var propietario = new Propietario
            {
                Id = Convert.ToInt32(reader["Id"]),
                DNI = reader["DNI"].ToString(),
                Nombre = reader["Nombre"].ToString(),
                Apellido = reader["Apellido"].ToString(),
                Telefono = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"].ToString(),
                Email = reader["Email"].ToString(),
                Direccion = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"].ToString(),
                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                Estado = Convert.ToBoolean(reader["Estado"])
            };
            
            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DNI,Nombre,Apellido,Telefono,Email,Direccion")] Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = await _connectionManager.GetOpenConnectionAsync();
                    
                    // Verificar si ya existe un propietario con el mismo DNI
                    var checkDNI = "SELECT COUNT(*) FROM propietarios WHERE DNI = @dni";
                    using var dniCommand = new MySqlCommand(checkDNI, connection);
                    dniCommand.Parameters.AddWithValue("@dni", propietario.DNI);
                    var dniCount = Convert.ToInt32(await dniCommand.ExecuteScalarAsync());
                    
                    if (dniCount > 0)
                    {
                        ModelState.AddModelError("DNI", "Ya existe un propietario con este DNI.");
                        return View(propietario);
                    }

                    // Verificar si ya existe un propietario con el mismo Email
                    var checkEmail = "SELECT COUNT(*) FROM propietarios WHERE Email = @email";
                    using var emailCommand = new MySqlCommand(checkEmail, connection);
                    emailCommand.Parameters.AddWithValue("@email", propietario.Email);
                    var emailCount = Convert.ToInt32(await emailCommand.ExecuteScalarAsync());
                    
                    if (emailCount > 0)
                    {
                        ModelState.AddModelError("Email", "Ya existe un propietario con este Email.");
                        return View(propietario);
                    }

                    // Insertar nuevo propietario
                    var insertQuery = @"INSERT INTO propietarios (DNI, Nombre, Apellido, Telefono, Email, Direccion, FechaCreacion, Estado) 
                                       VALUES (@dni, @nombre, @apellido, @telefono, @email, @direccion, @fechaCreacion, @estado)";
                    
                    using var insertCommand = new MySqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@dni", propietario.DNI);
                    insertCommand.Parameters.AddWithValue("@nombre", propietario.Nombre);
                    insertCommand.Parameters.AddWithValue("@apellido", propietario.Apellido);
                    insertCommand.Parameters.AddWithValue("@telefono", propietario.Telefono ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@email", propietario.Email);
                    insertCommand.Parameters.AddWithValue("@direccion", propietario.Direccion ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@fechaCreacion", DateTime.Now);
                    insertCommand.Parameters.AddWithValue("@estado", true);
                    
                    await insertCommand.ExecuteNonQueryAsync();
                    
                    TempData["Success"] = "Propietario creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el propietario: " + ex.Message);
                }
            }
            return View(propietario);
        }

        // GET: Propietarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT * FROM propietarios WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }
            
            var propietario = new Propietario
            {
                Id = Convert.ToInt32(reader["Id"]),
                DNI = reader["DNI"].ToString(),
                Nombre = reader["Nombre"].ToString(),
                Apellido = reader["Apellido"].ToString(),
                Telefono = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"].ToString(),
                Email = reader["Email"].ToString(),
                Direccion = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"].ToString(),
                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                Estado = Convert.ToBoolean(reader["Estado"])
            };
            
            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DNI,Nombre,Apellido,Telefono,Email,Direccion,FechaCreacion,Estado")] Propietario propietario)
        {
            if (id != propietario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = await _connectionManager.GetOpenConnectionAsync();
                    
                    // Verificar si ya existe otro propietario con el mismo DNI
                    var checkDNI = "SELECT COUNT(*) FROM propietarios WHERE DNI = @dni AND Id != @id";
                    using var dniCommand = new MySqlCommand(checkDNI, connection);
                    dniCommand.Parameters.AddWithValue("@dni", propietario.DNI);
                    dniCommand.Parameters.AddWithValue("@id", propietario.Id);
                    var dniCount = Convert.ToInt32(await dniCommand.ExecuteScalarAsync());
                    
                    if (dniCount > 0)
                    {
                        ModelState.AddModelError("DNI", "Ya existe otro propietario con este DNI.");
                        return View(propietario);
                    }

                    // Verificar si ya existe otro propietario con el mismo Email
                    var checkEmail = "SELECT COUNT(*) FROM propietarios WHERE Email = @email AND Id != @id";
                    using var emailCommand = new MySqlCommand(checkEmail, connection);
                    emailCommand.Parameters.AddWithValue("@email", propietario.Email);
                    emailCommand.Parameters.AddWithValue("@id", propietario.Id);
                    var emailCount = Convert.ToInt32(await emailCommand.ExecuteScalarAsync());
                    
                    if (emailCount > 0)
                    {
                        ModelState.AddModelError("Email", "Ya existe otro propietario con este Email.");
                        return View(propietario);
                    }

                    // Actualizar propietario
                    var updateQuery = @"UPDATE propietarios SET DNI = @dni, Nombre = @nombre, Apellido = @apellido, 
                                       Telefono = @telefono, Email = @email, Direccion = @direccion WHERE Id = @id";
                    
                    using var updateCommand = new MySqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@dni", propietario.DNI);
                    updateCommand.Parameters.AddWithValue("@nombre", propietario.Nombre);
                    updateCommand.Parameters.AddWithValue("@apellido", propietario.Apellido);
                    updateCommand.Parameters.AddWithValue("@telefono", propietario.Telefono ?? (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@email", propietario.Email);
                    updateCommand.Parameters.AddWithValue("@direccion", propietario.Direccion ?? (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@id", propietario.Id);
                    
                    await updateCommand.ExecuteNonQueryAsync();
                    
                    TempData["Success"] = "Propietario actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el propietario: " + ex.Message);
                }
            }
            return View(propietario);
        }

        // GET: Propietarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT * FROM propietarios WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }
            
            var propietario = new Propietario
            {
                Id = Convert.ToInt32(reader["Id"]),
                DNI = reader["DNI"].ToString(),
                Nombre = reader["Nombre"].ToString(),
                Apellido = reader["Apellido"].ToString(),
                Telefono = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"].ToString(),
                Email = reader["Email"].ToString(),
                Direccion = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"].ToString(),
                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                Estado = Convert.ToBoolean(reader["Estado"])
            };

            return View(propietario);
        }

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                using var connection = await _connectionManager.GetOpenConnectionAsync();
                
                // Verificar si tiene inmuebles asociados
                var checkInmuebles = "SELECT COUNT(*) FROM inmuebles WHERE PropietarioId = @id AND Estado = 1";
                using var checkCommand = new MySqlCommand(checkInmuebles, connection);
                checkCommand.Parameters.AddWithValue("@id", id);
                var inmuebleCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                
                if (inmuebleCount > 0)
                {
                    TempData["Error"] = "No se puede eliminar el propietario porque tiene inmuebles asociados.";
                    return RedirectToAction(nameof(Index));
                }

                // Eliminación lógica
                var updateQuery = "UPDATE propietarios SET Estado = 0 WHERE Id = @id";
                using var updateCommand = new MySqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@id", id);
                await updateCommand.ExecuteNonQueryAsync();
                
                TempData["Success"] = "Propietario eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el propietario: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> PropietarioExists(int id)
        {
            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT COUNT(*) FROM propietarios WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }
}
