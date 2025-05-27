using GestorEventosMusicales.Modelos;
using Npgsql;

// clase encargada de TODA la conexión y trabajo con la base de datos
namespace GestorEventosMusicales.Data
{
    public class DatabaseService
    {
        // variable encargada de la definición para la conexión
        private readonly string _connectionString = "Host=7.tcp.eu.ngrok.io;Port=16877;Username=postgres;Password=0603;Database=GestorEventosDB";

        // Registra un usurio
        public int GuardarManager(Manager manager)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string query = @"INSERT INTO managers (nombre, correo, telefono, contrasena)
                     VALUES (@nombre, @correo, @telefono, @contrasena)
                     RETURNING id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nombre", manager.Nombre);
            cmd.Parameters.AddWithValue("@correo", manager.Correo);
            cmd.Parameters.AddWithValue("@telefono", manager.Telefono);
            cmd.Parameters.AddWithValue("@contrasena", manager.Contrasena);

            return (int)cmd.ExecuteScalar();
        }

        // valida los datos del usuario sean correctos antes de iniciar sección
        public Manager? ValidarCredenciales(string correo, string contrasena)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            conexion.Open();

            string query = "SELECT id, nombre, correo, telefono, contrasena FROM managers WHERE correo = @correo AND contrasena = @contrasena";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@correo", correo);
            cmd.Parameters.AddWithValue("@contrasena", contrasena);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Manager
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Correo = reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Contrasena = reader.GetString(4)
                };
            }

            return null;
        }

        // MANAGER ADMIN



        // detecta cuál es el id de la sección actual del manager conectado
        public async Task<int> ObtenerManagerIdActualAsync()
        {
            if (Preferences.ContainsKey("usuarioId"))
            {
                int id = Preferences.Get("usuarioId", -1);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ID recuperado: {id}");
                if (id > 0)
                    return await Task.FromResult(id);
            }

            System.Diagnostics.Debug.WriteLine("[DEBUG] usuarioId no encontrado en Preferences.");
            throw new Exception("No hay usuario autenticado.");
        }

        // actualiza los dats del manager
        public bool ActualizarDatosManager(int id, string nombre, string correo, string telefono, byte[] imagen)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string query = @"UPDATE managers 
                     SET nombre = @nombre, correo = @correo, telefono = @telefono, imagen = @imagen 
                     WHERE id = @id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nombre", nombre);
            cmd.Parameters.AddWithValue("@correo", correo);
            cmd.Parameters.AddWithValue("@telefono", telefono ?? "");
            cmd.Parameters.AddWithValue("@id", id);

            // Verifica si la imagen es nula y pasa DBNull si lo es
            cmd.Parameters.AddWithValue("@imagen", imagen ?? (object)DBNull.Value);

            int filasAfectadas = cmd.ExecuteNonQuery();
            return filasAfectadas > 0;
        }

        // cambia la contraseña
        public bool CambiarContrasena(int id, string nuevaContrasena)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string query = @"UPDATE managers 
                     SET contrasena = @contrasena 
                     WHERE id = @id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@contrasena", nuevaContrasena);
            cmd.Parameters.AddWithValue("@id", id);

            int filasAfectadas = cmd.ExecuteNonQuery();
            return filasAfectadas > 0;
        }

        //Muestra todos los instrumentos para luego enlistarlos
        public List<Instrumento> ObtenerInstrumentos(int managerId)
        {
            var lista = new List<Instrumento>();

            using var conexion = new NpgsqlConnection(_connectionString);
            conexion.Open();
            string query = "SELECT id, nombre, cantidad, proveedor FROM instrumentos WHERE manager_id = @managerId";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@managerId", managerId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Instrumento
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Cantidad = reader.GetInt32(2),
                    Proveedor = reader.GetString(3)
                });
            }

            return lista;
        }



        // busca un instrumento según su id
        public async Task<Instrumento?> ObtenerInstrumentoPorIdAsync(int instrumentoId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = "SELECT id, nombre, cantidad, proveedor FROM instrumentos WHERE id = @instrumentoId";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@instrumentoId", instrumentoId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Instrumento
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Cantidad = reader.GetInt32(2),
                    Proveedor = reader.GetString(3)
                };
            }

            return null; // No encontrado
        }

        public async Task<List<Instrumento>> ObtenerTodosLosInstrumentosAsync()
        {
            var lista = new List<Instrumento>();

            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = "SELECT id, nombre, cantidad, proveedor FROM instrumentos";

            using var cmd = new NpgsqlCommand(query, conexion);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Instrumento
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Cantidad = reader.GetInt32(2),
                    Proveedor = reader.GetString(3)
                });
            }

            return lista;
        }

        // añade un instrumento a la base de datos
        public async Task InsertarInstrumentoAsync(Instrumento instrumento)
        {

            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"INSERT INTO instrumentos (nombre, cantidad, proveedor, manager_id)
                     VALUES (@nombre, @cantidad, @proveedor, @managerId)";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@nombre", instrumento.Nombre);
            cmd.Parameters.AddWithValue("@cantidad", instrumento.Cantidad);
            cmd.Parameters.AddWithValue("@proveedor", instrumento.Proveedor);
            cmd.Parameters.AddWithValue("@managerId", instrumento.ManagerId);

            await cmd.ExecuteNonQueryAsync();
        }


        // elimina un instrumento de la base de datos
        public async Task EliminarInstrumentoAsync(int instrumentoId, int managerId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = "DELETE FROM instrumentos WHERE id = @id AND manager_id = @managerId";
            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@id", instrumentoId);
            cmd.Parameters.AddWithValue("@managerId", managerId);

            await cmd.ExecuteNonQueryAsync();
        }

        // actualiza los datos de un instrumento
        public async Task ActualizarInstrumentoAsync(Instrumento instrumento)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"UPDATE instrumentos 
                     SET nombre = @nombre, cantidad = @cantidad, proveedor = @proveedor 
                     WHERE id = @id AND manager_id = @managerId";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@nombre", instrumento.Nombre);
            cmd.Parameters.AddWithValue("@cantidad", instrumento.Cantidad);
            cmd.Parameters.AddWithValue("@proveedor", instrumento.Proveedor);
            cmd.Parameters.AddWithValue("@id", instrumento.Id);
            cmd.Parameters.AddWithValue("@managerId", instrumento.ManagerId);

            await cmd.ExecuteNonQueryAsync();
        }

        // metodo encargado de buscar, es el buscador
        public async Task<int> ObtenerLocacionIdPorNombreAsync(string nombreLocacion, int managerId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = "SELECT id FROM locacion WHERE nombre = @nombreLocacion AND manager_id = @manager_id LIMIT 1";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@nombreLocacion", nombreLocacion);
            cmd.Parameters.AddWithValue("@manager_id", managerId);

            var resultado = await cmd.ExecuteScalarAsync();

            return resultado != null ? Convert.ToInt32(resultado) : 0;
        }


        // Método para obtener todos los lugares
        public List<Locacion> ObtenerLocaciones(int managerId)
        {
            var lista = new List<Locacion>();

            using var conexion = new NpgsqlConnection(_connectionString);
            conexion.Open();
            string query = @"SELECT id, nombre, direccion, ciudad, region, codigo_postal, pais, capacidad, telefono, email 
                     FROM locacion WHERE manager_id = @managerId";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@managerId", managerId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Locacion
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Direccion = reader.GetString(2),
                    Ciudad = reader.GetString(3),
                    Region = reader.GetString(4),
                    CodigoPostal = reader.GetString(5),
                    Pais = reader.GetString(6),
                    Capacidad = reader.GetInt32(7),
                    Telefono = reader.GetString(8),
                    Email = reader.GetString(9),
                    ManagerId = managerId
                });
            }
            return lista;
        }

        // añadir un lugar a la base de datos
        public async Task<int> InsertarLocacionAsync(Locacion locacion, int managerId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
            INSERT INTO locacion 
            (nombre, direccion, ciudad, region, codigo_postal, pais, capacidad, telefono, email, manager_id)
            VALUES 
            (@nombre, @direccion, @ciudad, @region, @codigo_postal, @pais, @capacidad, @telefono, @email, @manager_id)
            RETURNING id", conn);

            cmd.Parameters.AddWithValue("@nombre", locacion.Nombre ?? "");
            cmd.Parameters.AddWithValue("@direccion", locacion.Direccion ?? "");
            cmd.Parameters.AddWithValue("@ciudad", locacion.Ciudad ?? "");
            cmd.Parameters.AddWithValue("@region", locacion.Region ?? "");
            cmd.Parameters.AddWithValue("@codigo_postal", locacion.CodigoPostal ?? "");
            cmd.Parameters.AddWithValue("@pais", locacion.Pais ?? "");
            cmd.Parameters.AddWithValue("@capacidad", locacion.Capacidad);
            cmd.Parameters.AddWithValue("@telefono", locacion.Telefono ?? "");
            cmd.Parameters.AddWithValue("@email", locacion.Email ?? "");
            cmd.Parameters.AddWithValue("@manager_id", managerId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }


        // actualizar los datos de un lugar
        public async Task<int> ActualizarLocacionAsync(Locacion locacion, int managerId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
            UPDATE locacion
            SET nombre = @nombre,
            direccion = @direccion,
            ciudad = @ciudad,
            region = @region,
            codigo_postal = @codigo_postal,
            pais = @pais,
            capacidad = @capacidad,
            telefono = @telefono,
            email = @email
            WHERE id = @id AND manager_id = @manager_id", conn);

            cmd.Parameters.AddWithValue("@id", locacion.Id);
            cmd.Parameters.AddWithValue("@manager_id", managerId);
            cmd.Parameters.AddWithValue("@nombre", locacion.Nombre ?? "");
            cmd.Parameters.AddWithValue("@direccion", locacion.Direccion ?? "");
            cmd.Parameters.AddWithValue("@ciudad", locacion.Ciudad ?? "");
            cmd.Parameters.AddWithValue("@region", locacion.Region ?? "");
            cmd.Parameters.AddWithValue("@codigo_postal", locacion.CodigoPostal ?? "");
            cmd.Parameters.AddWithValue("@pais", locacion.Pais ?? "");
            cmd.Parameters.AddWithValue("@capacidad", locacion.Capacidad);
            cmd.Parameters.AddWithValue("@telefono", locacion.Telefono ?? "");
            cmd.Parameters.AddWithValue("@email", locacion.Email ?? "");

            return await cmd.ExecuteNonQueryAsync();
        }


        // eliminar un lugar de la BD
        public async Task<int> EliminarLocacionAsync(int locacionId, int managerId)
{
    int filasAfectadas = 0;

    using var conn = new NpgsqlConnection(_connectionString);
    await conn.OpenAsync();

    // Obtener manager para saber si es admin
    var manager = await ObtenerManagerPorIdAsync(managerId);
    bool esAdmin = manager?.Rol?.ToLower() == "admin";

    string query;
    using var cmd = new NpgsqlCommand();
    cmd.Connection = conn;

    if (esAdmin)
    {
        // Admin puede borrar cualquier locación sin importar manager_id
        query = "DELETE FROM locacion WHERE id = @id RETURNING id;";
        cmd.CommandText = query;
        cmd.Parameters.AddWithValue("id", locacionId);
    }
    else
    {
        // Manager normal solo puede borrar si es dueño (manager_id)
        query = "DELETE FROM locacion WHERE id = @id AND manager_id = @manager_id RETURNING id;";
        cmd.CommandText = query;
        cmd.Parameters.AddWithValue("id", locacionId);
        cmd.Parameters.AddWithValue("manager_id", managerId);
    }

    var result = await cmd.ExecuteScalarAsync();
    if (result != null && int.TryParse(result.ToString(), out int id))
    {
        filasAfectadas = 1;
    }

    return filasAfectadas;
}







        // Busca y retorna los instrumentos de un manager
        public async Task<List<Instrumento>> ObtenerInstrumentosAsync(int managerId)
        {
            var instrumentos = new List<Instrumento>();

            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query;

            if (managerId == 0) // admin, todos los instrumentos sin filtro
            {
                query = @"SELECT id, nombre, cantidad, proveedor FROM instrumentos";
            }
            else
            {
                query = @"SELECT id, nombre, cantidad, proveedor FROM instrumentos WHERE manager_id = @managerId";
            }

            using var cmd = new NpgsqlCommand(query, conexion);

            if (managerId != 0)
                cmd.Parameters.AddWithValue("@managerId", managerId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                instrumentos.Add(new Instrumento
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Cantidad = reader.GetInt32(2),
                    Proveedor = reader.GetString(3)
                });
            }

            return instrumentos;
        }





        // obtiene los artistas de un manager
        public async Task<List<Artista>> ObtenerArtistasAsync(int managerId)
        {
            var lista = new List<Artista>();

            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query;

            if (managerId == 0) // admin, devuelve todos los artistas sin filtro
            {
                query = @"
        SELECT id, nombre, fecha_nacimiento, nacionalidad, banda, imagen
        FROM artistas";
            }
            else
            {
                query = @"
        SELECT a.id, a.nombre, a.fecha_nacimiento, a.nacionalidad, a.banda, a.imagen
        FROM artistas a
        INNER JOIN artistas_managers am ON a.id = am.artista_id
        WHERE am.manager_id = @managerId";
            }

            using var cmd = new NpgsqlCommand(query, conexion);

            if (managerId != 0)
                cmd.Parameters.AddWithValue("@managerId", managerId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Artista
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    FechaNacimiento = reader.GetDateTime(2),
                    Nacionalidad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Banda = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Imagen = reader.IsDBNull(5) ? null : (byte[])reader["imagen"]
                });
            }

            return lista;
        }

        public async Task<List<Artista>> ObtenerTodosLosArtistasAsync()
        {
            var lista = new List<Artista>();

            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"
SELECT id, nombre, fecha_nacimiento, nacionalidad, banda, imagen
FROM artistas";

            using var cmd = new NpgsqlCommand(query, conexion);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Artista
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    FechaNacimiento = reader.GetDateTime(2),
                    Nacionalidad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Banda = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Imagen = reader.IsDBNull(5) ? null : (byte[])reader["imagen"]
                });
            }

            return lista;
        }


        // añade un nuevo artista
        public async Task<int> InsertarArtistaAsync(Artista artista, List<Manager> managers)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string insertQuery = @"
                                INSERT INTO artistas (nombre, banda, nacionalidad, fecha_nacimiento, imagen)
                                VALUES (@nombre, @banda, @nacionalidad, @fechaNacimiento, @imagen)
                                RETURNING id";

            int artistaId;

            using (var cmd = new NpgsqlCommand(insertQuery, conn))
            {
                cmd.Parameters.AddWithValue("@nombre", artista.Nombre);
                cmd.Parameters.AddWithValue("@banda", (object?)artista.Banda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nacionalidad", (object?)artista.Nacionalidad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaNacimiento", artista.FechaNacimiento);
                cmd.Parameters.AddWithValue("@imagen", (object?)artista.Imagen ?? DBNull.Value);

                artistaId = (int)await cmd.ExecuteScalarAsync();
            }

            foreach (var manager in managers)
            {
                string relQuery = "INSERT INTO artistas_managers (artista_id, manager_id) VALUES (@artistaId, @managerId)";
                using var relCmd = new NpgsqlCommand(relQuery, conn);
                relCmd.Parameters.AddWithValue("@artistaId", artistaId);
                relCmd.Parameters.AddWithValue("@managerId", manager.Id);
                await relCmd.ExecuteNonQueryAsync();
            }

            return artistaId;
        }

        // actualiza los datos de un artista
        public async Task ActualizarArtistaAsync(Artista artista, List<Manager> managers)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string checkQuery = "SELECT 1 FROM artistas_managers WHERE artista_id = @id AND manager_id = ANY(@managerIds)";
            using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@id", artista.Id);
                checkCmd.Parameters.AddWithValue("@managerIds", managers.Select(m => m.Id).ToArray());
                var exists = await checkCmd.ExecuteScalarAsync();
                if (exists == null) return; // No autorizado
            }

            string updateQuery = @"
                    UPDATE artistas 
                    SET nombre = @nombre, banda = @banda, nacionalidad = @nacionalidad, 
                    fecha_nacimiento = @fechaNacimiento, imagen = @imagen
                    WHERE id = @id";

            using var cmd = new NpgsqlCommand(updateQuery, conn);
            cmd.Parameters.AddWithValue("@nombre", artista.Nombre);
            cmd.Parameters.AddWithValue("@banda", (object?)artista.Banda ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nacionalidad", (object?)artista.Nacionalidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fechaNacimiento", artista.FechaNacimiento);
            cmd.Parameters.AddWithValue("@imagen", (object?)artista.Imagen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", artista.Id);
            await cmd.ExecuteNonQueryAsync();

            // Eliminar relaciones existentes
            string deleteQuery = "DELETE FROM artistas_managers WHERE artista_id = @artistaId";
            using (var deleteCmd = new NpgsqlCommand(deleteQuery, conn))
            {
                deleteCmd.Parameters.AddWithValue("@artistaId", artista.Id);
                await deleteCmd.ExecuteNonQueryAsync();
            }

            // Insertar nuevas relaciones
            foreach (var manager in managers)
            {
                string insertRel = "INSERT INTO artistas_managers (artista_id, manager_id) VALUES (@artistaId, @managerId)";
                using var insertCmd = new NpgsqlCommand(insertRel, conn);
                insertCmd.Parameters.AddWithValue("@artistaId", artista.Id);
                insertCmd.Parameters.AddWithValue("@managerId", manager.Id);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }


        // busca y retorna los valores de un artista el cual se hizo clic a editar
        public async Task<Artista> ObtenerArtistaPorIdAsync(int id)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"
                SELECT a.id, a.nombre, a.banda, a.nacionalidad, a.fecha_nacimiento, a.imagen
                FROM artistas a
                WHERE a.id = @id";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Artista
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Banda = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Nacionalidad = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    FechaNacimiento = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                    Imagen = reader.IsDBNull(5) ? null : (byte[])reader[5]
                };
            }

            return null;
        }

        // retorna datos del manager actual
        public async Task<Manager> ObtenerManagerPorIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Incluye el campo 'rol' en la consulta para poder identificar si es admin
            string query = "SELECT id, nombre, correo, telefono, imagen, rol FROM managers WHERE id = @id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Manager
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Correo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Imagen = reader.IsDBNull(4) ? null : (byte[])reader["imagen"],
                    Rol = reader.IsDBNull(5) ? "" : reader.GetString(5)  // Agrega el campo rol aquí
                };
            }

            return null;
        }


        // actualiza la tabla de la relación de managers y artistas
        internal async Task ActualizarManagersDeArtistaAsync(int artista_id, List<int> nuevosManagerIds, int managerId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            // Verificar que el manager que llama tiene acceso a ese artista
            string checkQuery = "SELECT 1 FROM artistas_managers WHERE artista_id = @artista_id AND manager_id = @managerId";
            using var cmdCheck = new NpgsqlCommand(checkQuery, conexion);
            cmdCheck.Parameters.AddWithValue("@artista_id", artista_id);
            cmdCheck.Parameters.AddWithValue("@managerId", managerId);

            var existe = await cmdCheck.ExecuteScalarAsync();
            if (existe == null) return; // No autorizado

            // Eliminar todas las relaciones existentes del artista con los managers
            string deleteQuery = "DELETE FROM artistas_managers WHERE artista_id = @artista_id";
            using var cmdDelete = new NpgsqlCommand(deleteQuery, conexion);
            cmdDelete.Parameters.AddWithValue("@artista_id", artista_id);
            await cmdDelete.ExecuteNonQueryAsync();

            // Insertar nuevas relaciones
            foreach (var nuevoManagerId in nuevosManagerIds)
            {
                string insertQuery = "INSERT INTO artistas_managers (artista_id, manager_id) VALUES (@artista_id, @manager_id)";
                using var cmdInsert = new NpgsqlCommand(insertQuery, conexion);
                cmdInsert.Parameters.AddWithValue("@artista_id", artista_id);
                cmdInsert.Parameters.AddWithValue("@manager_id", nuevoManagerId);
                await cmdInsert.ExecuteNonQueryAsync();
            }
        }


        // obtiene los managers de un artista
        public async Task<List<Manager>> ObtenerManagersPorArtistaIdAsync(int artistaId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"
            SELECT m.id, m.nombre, m.correo, m.telefono
            FROM managers m
            INNER JOIN artistas_managers am ON m.id = am.manager_id
            WHERE am.artista_id = @artistaId";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@artistaId", artistaId);

            using var reader = await cmd.ExecuteReaderAsync();
            var managers = new List<Manager>();

            while (await reader.ReadAsync())
            {
                managers.Add(new Manager
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Correo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }

            return managers;
        }


        // elimina de la BD al artista si es que no está asociado a ningún otro manager.
        public async Task<int> EliminarArtistaAsync(int artista_id, int managerId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Verificar si el manager es admin
            var manager = await ObtenerManagerPorIdAsync(managerId);
            bool esAdmin = manager?.Rol?.ToLower() == "admin";

            if (esAdmin)
            {
                // Eliminar todas las relaciones (evento_artistas y artistas_managers) y luego el artista
                string eliminarRelacionesEvento = "DELETE FROM evento_artistas WHERE artista_id = @artista_id";
                using (var cmd = new NpgsqlCommand(eliminarRelacionesEvento, conn))
                {
                    cmd.Parameters.AddWithValue("artista_id", artista_id);
                    await cmd.ExecuteNonQueryAsync();
                }

                string eliminarManagers = "DELETE FROM artistas_managers WHERE artista_id = @artista_id";
                using (var cmd = new NpgsqlCommand(eliminarManagers, conn))
                {
                    cmd.Parameters.AddWithValue("artista_id", artista_id);
                    await cmd.ExecuteNonQueryAsync();
                }

                string eliminarArtista = "DELETE FROM artistas WHERE id = @id RETURNING id";
                using (var cmd = new NpgsqlCommand(eliminarArtista, conn))
                {
                    cmd.Parameters.AddWithValue("id", artista_id);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? 1 : 0;
                }
            }

            // Si no es admin, continuar con el flujo normal

            // Verificar que el artista esté vinculado al manager
            string checkQuery = "SELECT 1 FROM artistas_managers WHERE artista_id = @id AND manager_id = @managerId";
            using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@id", artista_id);
                checkCmd.Parameters.AddWithValue("@managerId", managerId);
                var result = await checkCmd.ExecuteScalarAsync();
                if (result == null)
                    return -1; // No pertenece a este manager
            }

            // Ver cuántos managers hay asociados a este artista
            string countQuery = "SELECT COUNT(*) FROM artistas_managers WHERE artista_id = @id";
            int cantidadManagers = 0;
            using (var countCmd = new NpgsqlCommand(countQuery, conn))
            {
                countCmd.Parameters.AddWithValue("@id", artista_id);
                var result = await countCmd.ExecuteScalarAsync();
                cantidadManagers = Convert.ToInt32(result);
            }

            if (cantidadManagers > 1)
            {
                // Solo borrar la relación con este manager
                string deleteRelacion = "DELETE FROM artistas_managers WHERE artista_id = @artista_id AND manager_id = @manager_id";
                using (var cmd = new NpgsqlCommand(deleteRelacion, conn))
                {
                    cmd.Parameters.AddWithValue("artista_id", artista_id);
                    cmd.Parameters.AddWithValue("manager_id", managerId);
                    await cmd.ExecuteNonQueryAsync();
                }

                return 2; // Solo se eliminó la relación
            }

            // Si tiene un solo manager, se puede eliminar todo
            string eliminarRelacionesQuery = "DELETE FROM evento_artistas WHERE artista_id = @artista_id";
            using (var cmd = new NpgsqlCommand(eliminarRelacionesQuery, conn))
            {
                cmd.Parameters.AddWithValue("artista_id", artista_id);
                await cmd.ExecuteNonQueryAsync();
            }

            string eliminarRelacionesManager = "DELETE FROM artistas_managers WHERE artista_id = @artista_id";
            using (var cmd = new NpgsqlCommand(eliminarRelacionesManager, conn))
            {
                cmd.Parameters.AddWithValue("artista_id", artista_id);
                await cmd.ExecuteNonQueryAsync();
            }

            string deleteQuery = "DELETE FROM artistas WHERE id = @id RETURNING id";
            using (var cmd = new NpgsqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("id", artista_id);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? 1 : 0;
            }
        }

        // Eliminar eventos solo si no tienen ningún manager asociado, en caso contrario, solo eliminará la relación
        public async Task<int> EliminarEventoAsync(int evento_id, int managerId)
        {
            int filasAfectadas = 0;

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Verificar si el manager es admin
            var manager = await ObtenerManagerPorIdAsync(managerId);
            bool esAdmin = manager?.Rol?.ToLower() == "admin";

            if (!esAdmin)
            {
                // Verificar si el evento le pertenece al manager
                string checkQuery = "SELECT 1 FROM evento_managers WHERE evento_id = @id AND manager_id = @managerId";
                using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@id", evento_id);
                    checkCmd.Parameters.AddWithValue("@managerId", managerId);
                    var result = await checkCmd.ExecuteScalarAsync();
                    if (result == null) return 0;
                }

                // Contar cuántos managers tiene ese evento
                string countQuery = "SELECT COUNT(*) FROM evento_managers WHERE evento_id = @id";
                int cantidadManagers = 0;
                using (var countCmd = new NpgsqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@id", evento_id);
                    var result = await countCmd.ExecuteScalarAsync();
                    if (result is long countLong)
                        cantidadManagers = (int)countLong;
                }

                if (cantidadManagers > 1)
                {
                    // Solo eliminar la relación con este manager
                    string deleteRelacion = "DELETE FROM evento_managers WHERE evento_id = @evento_id AND manager_id = @manager_id";
                    using (var cmd = new NpgsqlCommand(deleteRelacion, conn))
                    {
                        cmd.Parameters.AddWithValue("evento_id", evento_id);
                        cmd.Parameters.AddWithValue("manager_id", managerId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return 2; // Relación eliminada, evento sigue existiendo
                }
            }

            // Si es admin o es el único manager, eliminar todas las relaciones y luego el evento
            string eliminarRelacionesArtistas = "DELETE FROM evento_artistas WHERE evento_id = @evento_id";
            using (var cmd = new NpgsqlCommand(eliminarRelacionesArtistas, conn))
            {
                cmd.Parameters.AddWithValue("evento_id", evento_id);
                await cmd.ExecuteNonQueryAsync();
            }

            string eliminarRelacionesInstrumentos = "DELETE FROM evento_instrumentos WHERE evento_id = @evento_id";
            using (var cmd = new NpgsqlCommand(eliminarRelacionesInstrumentos, conn))
            {
                cmd.Parameters.AddWithValue("evento_id", evento_id);
                await cmd.ExecuteNonQueryAsync();
            }

            string eliminarRelacionesManagers = "DELETE FROM evento_managers WHERE evento_id = @evento_id";
            using (var cmd = new NpgsqlCommand(eliminarRelacionesManagers, conn))
            {
                cmd.Parameters.AddWithValue("evento_id", evento_id);
                await cmd.ExecuteNonQueryAsync();
            }

            string deleteQuery = "DELETE FROM eventos WHERE id = @id RETURNING id";
            using (var cmd = new NpgsqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("id", evento_id);
                var result = await cmd.ExecuteScalarAsync();

                if (result != null)
                {
                    filasAfectadas = 1;
                }
                else
                {
                    throw new Exception("El DELETE no devolvió ningún resultado. ¿Existe ese evento?");
                }
            }

            return filasAfectadas;
        }




        // Obtener las locaciones pero de la lista
        public async Task<List<Locacion>> ObtenerLocacionesAsync(int managerId)
        {
            var lista = new List<Locacion>();

            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query;
            if (managerId == 0)  // 0 = admin, ver todas las locaciones
            {
                query = @"
        SELECT id, nombre, direccion, ciudad, region, codigo_postal, pais, capacidad, telefono, email
        FROM locacion";
            }
            else
            {
                query = @"
        SELECT id, nombre, direccion, ciudad, region, codigo_postal, pais, capacidad, telefono, email
        FROM locacion
        WHERE manager_id = @managerId";
            }

            using var cmd = new NpgsqlCommand(query, conexion);
            if (managerId != 0)
                cmd.Parameters.AddWithValue("@managerId", managerId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Locacion
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Direccion = reader.GetString(2),
                    Ciudad = reader.GetString(3),
                    Region = reader.GetString(4),
                    CodigoPostal = reader.GetString(5),
                    Pais = reader.GetString(6),
                    Capacidad = reader.GetInt32(7),
                    Telefono = reader.GetString(8),
                    Email = reader.GetString(9)
                });
            }

            return lista;
        }

        public async Task<List<Locacion>> ObtenerTodasLasLocacionesAsync()
        {
            var lista = new List<Locacion>();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT id, nombre, direccion, ciudad, region, codigo_postal, pais, capacidad, telefono, email FROM locacion";

            using var cmd = new NpgsqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var locacion = new Locacion
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Direccion = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Ciudad = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Region = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    CodigoPostal = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Pais = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Capacidad = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                    Telefono = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    Email = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    IsVisible = true
                };

                lista.Add(locacion);
            }

            return lista;
        }

        // devolverá la lista de los managers
        public async Task<List<Manager>> ObtenerManagersAsync()
        {
            var lista = new List<Manager>();

            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = "SELECT id, nombre, correo, telefono FROM managers";

            using var cmd = new NpgsqlCommand(query, conexion);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Manager
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Correo = reader.GetString(2),
                    Telefono = reader.GetString(3)
                });
            }

            return lista;
        }

        // Guardar evento y retornar su ID
        public async Task<int> GuardarEventoAsync(Evento evento, int locacionId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            try
            {
                // Insertar directamente el locacion_id en la tabla eventos
                string query = @"
            INSERT INTO eventos (nombre, fecha_evento, fecha_montaje, locacion_id)
            VALUES (@nombre, @fecha_evento, @fecha_montaje, @locacion_id)
            RETURNING id;
        ";

                using var cmd = new NpgsqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", evento.Nombre);
                cmd.Parameters.AddWithValue("@fecha_evento", evento.FechaEvento);
                cmd.Parameters.AddWithValue("@fecha_montaje", evento.FechaMontaje);
                cmd.Parameters.AddWithValue("@locacion_id", locacionId);

                var eventoId = (int)await cmd.ExecuteScalarAsync();
                return eventoId;
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al guardar el evento con su locación.", ex);
            }
        }


        // Relacionar artista con evento
        public async Task AgregarArtistaEventoAsync(int eventoId, int artistaId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = "INSERT INTO evento_artistas (evento_id, artista_id) VALUES (@evento_id, @artista_id)";
            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@evento_id", eventoId);
            cmd.Parameters.AddWithValue("@artista_id", artistaId);

            await cmd.ExecuteNonQueryAsync();
        }

        // relacionar manager con evento
        public async Task AgregarManagerEventoAsync(int eventoId, int managerId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = "INSERT INTO evento_managers (evento_id, manager_id) VALUES (@evento_id, @manager_id)";
            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@evento_id", eventoId);
            cmd.Parameters.AddWithValue("@manager_id", managerId);

            await cmd.ExecuteNonQueryAsync();
        }

        // Relacionar instrumento con evento
        public async Task AgregarInstrumentoEventoAsync(int eventoId, int instrumentoId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();


            string query = "INSERT INTO evento_instrumentos (evento_id, instrumento_id) VALUES (@evento_id, @instrumento_id)";
            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@evento_id", eventoId);
            cmd.Parameters.AddWithValue("@instrumento_id", instrumentoId);

            await cmd.ExecuteNonQueryAsync();
        }





        // me devuelve la locación actual, es usada para mostrar el lugar al que está asociado el evento

        public async Task<Locacion> ObtenerLocacionPorIdAsync(int locacionId)
        {
            Locacion locacion = null;

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
            SELECT l.*
            FROM locacion l
            WHERE l.id = @locacionId;
    ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@locacionId", locacionId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                locacion = new Locacion
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                    Direccion = reader.IsDBNull(reader.GetOrdinal("direccion")) ? null : reader.GetString(reader.GetOrdinal("direccion")),
                    Ciudad = reader.IsDBNull(reader.GetOrdinal("ciudad")) ? null : reader.GetString(reader.GetOrdinal("ciudad")),
                    Region = reader.IsDBNull(reader.GetOrdinal("region")) ? null : reader.GetString(reader.GetOrdinal("region")),
                    CodigoPostal = reader.IsDBNull(reader.GetOrdinal("codigo_postal")) ? null : reader.GetString(reader.GetOrdinal("codigo_postal")),
                    Pais = reader.IsDBNull(reader.GetOrdinal("pais")) ? null : reader.GetString(reader.GetOrdinal("pais")),
                    Capacidad = (int)(reader.IsDBNull(reader.GetOrdinal("capacidad")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("capacidad")))
                };
            }

            return locacion;
        }

        // retorna los instrumentos de un evento
        public async Task<List<Instrumento>> ObtenerInstrumentosPorEventoIdAsync(int eventId)
        {
            var instrumentos = new List<Instrumento>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT i.*
FROM instrumentos i
INNER JOIN evento_instrumentos ei ON i.id = ei.instrumento_id
WHERE ei.evento_id = @eventId
AND ei.instrumento_id IS NOT NULL;

    ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@eventId", eventId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                instrumentos.Add(new Instrumento
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombre"))
                });
            }

            return instrumentos;
        }

        // retorna los artistas de un evento
        public async Task<List<Artista>> ObtenerArtistasPorEventoIdAsync(int eventId)
        {
            var artistas = new List<Artista>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT a.*
        FROM artistas a
        INNER JOIN evento_artistas ea ON a.id = ea.artista_id
        WHERE ea.evento_id = @eventId;
    ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@eventId", eventId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                artistas.Add(new Artista
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                    Banda = reader.IsDBNull(reader.GetOrdinal("banda")) ? null : reader.GetString(reader.GetOrdinal("banda")),
                    Nacionalidad = reader.IsDBNull(reader.GetOrdinal("nacionalidad")) ? null : reader.GetString(reader.GetOrdinal("nacionalidad")),
                    FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("fecha_nacimiento")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("fecha_nacimiento")),
                    Imagen = reader.IsDBNull(reader.GetOrdinal("imagen")) ? null : (byte[])reader["imagen"]
                });
            }

            return artistas;
        }

        // retorna los managers de un evento
        public async Task<List<Manager>> ObtenerManagersPorEventoIdAsync(int eventId)
        {
            var managers = new List<Manager>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT m.*
        FROM managers m
        INNER JOIN evento_managers em ON m.id = em.manager_id
        WHERE em.evento_id = @eventId;
    ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@eventId", eventId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                managers.Add(new Manager
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                    Correo = reader.IsDBNull(reader.GetOrdinal("correo")) ? null : reader.GetString(reader.GetOrdinal("correo")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString(reader.GetOrdinal("telefono"))
                });
            }

            return managers;
        }


        // actualiza la tabla de evento_managers ya sea añadiendo o quitando algún manager
        internal async Task ActualizarManagersDeEventoAsync(int eventoId, List<int> managerIds)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            var deleteCmd = new NpgsqlCommand("DELETE FROM evento_managers WHERE evento_id = @evento_id", conexion);
            deleteCmd.Parameters.AddWithValue("@evento_id", eventoId);
            await deleteCmd.ExecuteNonQueryAsync();

            foreach (var id in managerIds)
            {
                var insertCmd = new NpgsqlCommand("INSERT INTO evento_managers (evento_id, manager_id) VALUES (@evento_id, @manager_id)", conexion);
                insertCmd.Parameters.AddWithValue("@evento_id", eventoId);
                insertCmd.Parameters.AddWithValue("@manager_id", id);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }

        // actualiza la tabla de evento_artistas que es la relación de eventos y artistas
        internal async Task ActualizarArtistasDeEventoAsync(int eventoId, List<int> artistaIds)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            // Eliminar relaciones antiguas
            var deleteCmd = new NpgsqlCommand("DELETE FROM evento_artistas WHERE evento_id = @evento_id", conexion);
            deleteCmd.Parameters.AddWithValue("@evento_id", eventoId);
            await deleteCmd.ExecuteNonQueryAsync();

            // Insertar nuevas relaciones
            foreach (var id in artistaIds)
            {
                var insertCmd = new NpgsqlCommand("INSERT INTO evento_artistas (evento_id, artista_id) VALUES (@evento_id, @artista_id)", conexion);
                insertCmd.Parameters.AddWithValue("@evento_id", eventoId);
                insertCmd.Parameters.AddWithValue("@artista_id", id);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }


        // Actualiza la tabla que es la relación entre eventos e instrumentos
        internal async Task ActualizarInstrumentosDeEventoAsync(int eventoId, List<int> instrumentoIds)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            var deleteCmd = new NpgsqlCommand("DELETE FROM evento_instrumentos WHERE evento_id = @evento_id", conexion);
            deleteCmd.Parameters.AddWithValue("@evento_id", eventoId);
            await deleteCmd.ExecuteNonQueryAsync();

            foreach (var id in instrumentoIds.Where(id => id != 0))
            {
                var insertCmd = new NpgsqlCommand("INSERT INTO evento_instrumentos (evento_id, instrumento_id) VALUES (@evento_id, @instrumento_id)", conexion);
                insertCmd.Parameters.AddWithValue("@evento_id", eventoId);
                insertCmd.Parameters.AddWithValue("@instrumento_id", id);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }


        // actualiza la relación de los eventos e instrumentos convirtiendo el valor a null en caso de que el instrumento sea borrado
        public async Task ActualizarInstrumentoAEventosNullAsync(int instrumentoId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            // Consulta para actualizar las relaciones en la tabla evento_instrumentos
            var updateQuery = "UPDATE evento_instrumentos SET instrumento_id = NULL WHERE instrumento_id = @instrumento_id";

            using var cmd = new NpgsqlCommand(updateQuery, conexion);
            cmd.Parameters.AddWithValue("@instrumento_id", instrumentoId);

            // Ejecutar la actualización
            await cmd.ExecuteNonQueryAsync();
        }

        
        public async Task<int> InsertarEventoAsync(Evento evento, int locacionId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Insertar el evento
            var insertEventoQuery = @"
    INSERT INTO eventos (nombre, fecha_evento, fecha_montaje)
    VALUES (@nombre, @fechaEvento, @fechaMontaje)
    RETURNING id;
    ";

            using (var insertEventoCommand = new NpgsqlCommand(insertEventoQuery, connection))
            {
                insertEventoCommand.Parameters.AddWithValue("@nombre", evento.Nombre);
                insertEventoCommand.Parameters.AddWithValue("@fechaEvento", evento.FechaEvento);
                insertEventoCommand.Parameters.AddWithValue("@fechaMontaje", evento.FechaMontaje);

                var eventoId = await insertEventoCommand.ExecuteScalarAsync();

                // Insertar en la tabla de relación evento_locacion
                var insertEventoLocacionQuery = @"
        INSERT INTO evento_locacion (evento_id, locacion_id)
        VALUES (@eventoId, @locacionId);
        ";

                using (var insertEventoLocacionCommand = new NpgsqlCommand(insertEventoLocacionQuery, connection))
                {
                    insertEventoLocacionCommand.Parameters.AddWithValue("@eventoId", eventoId);
                    insertEventoLocacionCommand.Parameters.AddWithValue("@locacionId", locacionId);

                    await insertEventoLocacionCommand.ExecuteNonQueryAsync();
                }

                return Convert.ToInt32(eventoId);
            }
        }


        // me devuelve los eventos de un manager
        public async Task<Evento> ObtenerEventoPorIdAsync(int id)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"
SELECT id, nombre, fecha_evento, fecha_montaje, locacion_id
FROM eventos
WHERE id = @id
";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Evento
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    FechaEvento = reader.GetDateTime(2),
                    FechaMontaje = reader.GetDateTime(3),
                    LocacionId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                };
            }

            return null;
        }





        // actualiza todo lo relacionado a los eventos
        public async Task ActualizarEventoAsync(Evento evento)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
UPDATE eventos 
SET nombre = @nombre, 
    fecha_evento = @fechaEvento, 
    fecha_montaje = @fechaMontaje,
    locacion_id = @locacionId
WHERE id = @id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nombre", evento.Nombre);
            cmd.Parameters.AddWithValue("@fechaEvento", evento.FechaEvento);
            cmd.Parameters.AddWithValue("@fechaMontaje", evento.FechaMontaje);
            cmd.Parameters.AddWithValue("@locacionId", evento.LocacionId == 0 ? DBNull.Value : evento.LocacionId);
            cmd.Parameters.AddWithValue("@id", evento.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        // me muestra los eventos
        public async Task<List<Evento>> ObtenerEventosAsync()
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"
                SELECT e.id, e.nombre, e.fecha_evento, e.fecha_montaje, e.locacion_id, l.nombre, l.direccion 
                FROM eventos e
                LEFT JOIN locacion l ON e.locacion_id = l.id
                ORDER BY e.fecha_evento ASC
                ";

            using var cmd = new NpgsqlCommand(query, conexion);
            using var reader = await cmd.ExecuteReaderAsync();

            var eventos = new List<Evento>();
            while (await reader.ReadAsync())
            {
                var evento = new Evento
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    FechaEvento = reader.GetDateTime(2),
                    FechaMontaje = reader.GetDateTime(3),
                    LocacionId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    Locacion = reader.IsDBNull(4) ? new Locacion { Nombre = "Lugar no asignado", Direccion = "Lugar no asignado" } : new Locacion
                    {
                        Nombre = reader.IsDBNull(5) ? "Lugar no asignado" : reader.GetString(5),
                        Direccion = reader.IsDBNull(6) ? "Lugar no asignado" : reader.GetString(6)
                    }
                };
                eventos.Add(evento);
            }

            return eventos;
        }

        // me retorna los managers de un evento
        public async Task<List<Manager>> ObtenerManagersPorEventoAsync(int eventoId)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string query = @"
SELECT m.id, m.nombre, m.correo, m.telefono
FROM evento_managers em
JOIN managers m ON em.manager_id = m.id
WHERE em.evento_id = @eventoId";

            using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@eventoId", eventoId);

            using var reader = await cmd.ExecuteReaderAsync();
            var managers = new List<Manager>();

            while (await reader.ReadAsync())
            {
                var manager = new Manager
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Correo = reader.GetString(2),
                    Telefono = reader.GetString(3)
                };
                managers.Add(manager);
            }

            return managers;
        }

        // me da los valores para poder editar un luar
        public async Task<List<Locacion>> ObtenerLocacionesParaEdicionAsync(int eventoId, int managerId)
        {
            var locaciones = await ObtenerLocacionesAsync(managerId);

            // Obtener la locación asociada al evento, sin importar el manager
            var evento = await ObtenerEventoPorIdAsync(eventoId);
            if (evento != null && evento.LocacionId != 0 && !locaciones.Any(l => l.Id == evento.LocacionId))
            {
                var locacion = await ObtenerLocacionPorIdAsync(evento.LocacionId);
                if (locacion != null)
                    locaciones.Add(locacion); // Agregarla si no estaba
            }

            return locaciones;
        }

        // me muestra los proximos eventos en general
        public async Task<List<Evento>> ObtenerProximosEventosConManagersAsync(DateTime fechaActual, int cantidad)
        {
            using var conexion = new NpgsqlConnection(_connectionString);
            await conexion.OpenAsync();

            string queryEventos = @"
                SELECT e.id, e.nombre, e.fecha_evento, e.fecha_montaje, e.locacion_id, l.nombre, l.direccion 
                FROM eventos e
                LEFT JOIN locacion l ON e.locacion_id = l.id
                WHERE e.fecha_evento >= @fechaActual
                ORDER BY e.fecha_evento ASC
                LIMIT @cantidad
                ";

            using var cmd = new NpgsqlCommand(queryEventos, conexion);
            cmd.Parameters.AddWithValue("@fechaActual", fechaActual);
            cmd.Parameters.AddWithValue("@cantidad", cantidad);

            var eventos = new List<Evento>();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var evento = new Evento
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        FechaEvento = reader.GetDateTime(2),
                        FechaMontaje = reader.GetDateTime(3),
                        LocacionId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        Locacion = new Locacion
                        {
                            Nombre = reader.IsDBNull(5) ? "Lugar no asignado" : reader.GetString(5),
                            Direccion = reader.IsDBNull(6) ? "Dirección no asignada" : reader.GetString(6)
                        },
                        Managers = new List<Manager>()
                    };

                    eventos.Add(evento);
                }
            }

            // Cargar managers por cada evento
            foreach (var evento in eventos)
            {
                string queryManagers = @"
        SELECT m.id, m.nombre, m.correo
        FROM evento_managers em
        JOIN managers m ON em.manager_id = m.id
        WHERE em.evento_id = @eventoId";

                using var cmdManagers = new NpgsqlCommand(queryManagers, conexion);
                cmdManagers.Parameters.AddWithValue("@eventoId", evento.Id);

                using var readerManagers = await cmdManagers.ExecuteReaderAsync();
                while (await readerManagers.ReadAsync())
                {
                    evento.Managers.Add(new Manager
                    {
                        Id = readerManagers.GetInt32(0),
                        Nombre = readerManagers.GetString(1),
                        Correo = readerManagers.GetString(2)
                    });
                }
            }

            return eventos;
        }
    }

}