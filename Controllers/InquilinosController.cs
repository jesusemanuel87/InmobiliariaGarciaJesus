using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly MySqlConnectionManager _connectionManager;

        public InquilinosController(MySqlConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        // GET: Inquilinos
        public async Task<IActionResult> Index()
        {
            var inquilinos = new List<Inquilino>();
            
            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT * FROM inquilinos WHERE Estado = 1 ORDER BY Apellido, Nombre";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                inquilinos.Add(new Inquilino
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
            
            return View(inquilinos);
        }

        // GET: Inquilinos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using var connection = await _connectionManager.GetOpenConnectionAsync();
            
            var query = "SELECT * FROM inquilinos WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }
            
            var inquilino = new Inquilino
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
            
            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DNI,Nombre,Apellido,Telefono,Email,Direccion")] Inquilino inquilino)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = await _connectionManager.GetOpenConnectionAsync();
                    
                    // Verificar si ya existe un inquilino con el mismo DNI
                    var checkDNI = "SELECT COUNT(*) FROM inquilinos WHERE DNI = @dni";
                    using var dniCommand = new MySqlCommand(checkDNI, connection);
                    dniCommand.Parameters.AddWithValue("@dni", inquilino.DNI);
                    var dniCount = Convert.ToInt32(await dniCommand.ExecuteScalarAsync());
                    
                    if (dniCount > 0)
                    {
                        ModelState.AddModelError("DNI", "Ya existe un inquilino con este DNI.");
                        return View(inquilino);
                    }

                    // Verificar si ya existe un inquilino con el mismo Email
                    var checkEmail = "SELECT COUNT(*) FROM inquilinos WHERE Email = @email";
                    using var emailCommand = new MySqlCommand(checkEmail, connection);
                    emailCommand.Parameters.AddWithValue("@email", inquilino.Email);
                    var emailCount = Convert.ToInt32(await emailCommand.ExecuteScalarAsync());
                    
                    if (emailCount > 0)
                    {
                        ModelState.AddModelError("Email", "Ya existe un inquilino con este Email.");
                        return View(inquilino);
                    }

                    // Insertar nuevo inquilino
                    var insertQuery = @"INSERT INTO inquilinos (DNI, Nombre, Apellido, Telefono, Email, Direccion, FechaCreacion, Estado) 
                                       VALUES (@dni, @nombre, @apellido, @telefono, @email, @direccion, @fechaCreacion, @estado)";
                    
                    using var insertCommand = new MySqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@dni", inquilino.DNI);
                    insertCommand.Parameters.AddWithValue("@nombre", inquilino.Nombre);
                    insertCommand.Parameters.AddWithValue("@apellido", inquilino.Apellido);
                    insertCommand.Parameters.AddWithValue("@telefono", inquilino.Telefono ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@email", inquilino.Email);
                    insertCommand.Parameters.AddWithValue("@direccion", inquilino.Direccion ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@fechaCreacion", DateTime.Now);
                    insertCommand.Parameters.AddWithValue("@estado", true);
                    
                    await insertCommand.ExecuteNonQueryAsync();
                    
                    TempData["Success"] = "Inquilino creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el inquilino: " + ex.Message);
                }
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT * FROM inquilinos WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }
            
            var inquilino = new Inquilino
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
            
            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DNI,Nombre,Apellido,Telefono,Email,Direccion,FechaCreacion,Estado")] Inquilino inquilino)
        {
            if (id != inquilino.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = await _connectionManager.GetOpenConnectionAsync();
                    
                    // Verificar si ya existe otro inquilino con el mismo DNI
                    var checkDNI = "SELECT COUNT(*) FROM inquilinos WHERE DNI = @dni AND Id != @id";
                    using var dniCommand = new MySqlCommand(checkDNI, connection);
                    dniCommand.Parameters.AddWithValue("@dni", inquilino.DNI);
                    dniCommand.Parameters.AddWithValue("@id", inquilino.Id);
                    var dniCount = Convert.ToInt32(await dniCommand.ExecuteScalarAsync());
                    
                    if (dniCount > 0)
                    {
                        ModelState.AddModelError("DNI", "Ya existe un inquilino con este DNI.");
                        return View(inquilino);
                    }

                    // Verificar si ya existe otro inquilino con el mismo Email
                    var checkEmail = "SELECT COUNT(*) FROM inquilinos WHERE Email = @email AND Id != @id";
                    using var emailCommand = new MySqlCommand(checkEmail, connection);
                    emailCommand.Parameters.AddWithValue("@email", inquilino.Email);
                    emailCommand.Parameters.AddWithValue("@id", inquilino.Id);
                    var emailCount = Convert.ToInt32(await emailCommand.ExecuteScalarAsync());
                    
                    if (emailCount > 0)
                    {
                        ModelState.AddModelError("Email", "Ya existe un inquilino con este Email.");
                        return View(inquilino);
                    }

                    // Actualizar inquilino
                    var updateQuery = @"UPDATE inquilinos SET DNI = @dni, Nombre = @nombre, Apellido = @apellido, 
                                       Telefono = @telefono, Email = @email, Direccion = @direccion WHERE Id = @id";
                    
                    using var updateCommand = new MySqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@dni", inquilino.DNI);
                    updateCommand.Parameters.AddWithValue("@nombre", inquilino.Nombre);
                    updateCommand.Parameters.AddWithValue("@apellido", inquilino.Apellido);
                    updateCommand.Parameters.AddWithValue("@telefono", inquilino.Telefono ?? (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@email", inquilino.Email);
                    updateCommand.Parameters.AddWithValue("@direccion", inquilino.Direccion ?? (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@id", inquilino.Id);
                    
                    await updateCommand.ExecuteNonQueryAsync();
                    
                    TempData["Success"] = "Inquilino actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el inquilino: " + ex.Message);
                }
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT * FROM inquilinos WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
            {
                return NotFound();
            }
            
            var inquilino = new Inquilino
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
            
            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                using var connection = await _connectionManager.GetOpenConnectionAsync();
                
                // Verificar si tiene contratos asociados
                var checkContratos = "SELECT COUNT(*) FROM contratos WHERE InquilinoId = @id AND Estado = 1";
                using var checkCommand = new MySqlCommand(checkContratos, connection);
                checkCommand.Parameters.AddWithValue("@id", id);
                var contratoCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                
                if (contratoCount > 0)
                {
                    TempData["Error"] = "No se puede eliminar el inquilino porque tiene contratos asociados.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Eliminación lógica
                var updateQuery = "UPDATE inquilinos SET Estado = 0 WHERE Id = @id";
                using var updateCommand = new MySqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@id", id);
                await updateCommand.ExecuteNonQueryAsync();
                
                TempData["Success"] = "Inquilino eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el inquilino: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> InquilinoExists(int id)
        {
            using var connection = await _connectionManager.GetOpenConnectionAsync();
            var query = "SELECT COUNT(*) FROM inquilinos WHERE Id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }
}
