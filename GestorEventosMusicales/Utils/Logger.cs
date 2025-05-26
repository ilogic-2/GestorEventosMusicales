using System.IO;
using System.Threading.Tasks;

public static class Logger
{
    public static async Task GuardarLogAsync(string mensaje)
    {
        try
        {
            // Usar la carpeta de Documentos, accesible por el usuario
            string logFilePath = Path.Combine(FileSystem.AppDataDirectory, "Documents", "log.txt");

            // Crear la carpeta si no existe
            var logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Escribir el mensaje de log al archivo
            using (var writer = new StreamWriter(logFilePath, true)) // El "true" asegura que se añadan los logs sin sobrescribir
            {
                await writer.WriteLineAsync($"{DateTime.Now}: {mensaje}");
            }
        }
        catch (Exception ex)
        {
            // Si hay un error al guardar el log, lo mostramos en la consola
            Console.WriteLine($"Error al guardar el log: {ex.Message}");
        }
    }
}
