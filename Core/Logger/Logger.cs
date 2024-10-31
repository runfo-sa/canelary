using System.IO;

namespace Core.Logger
{
    public static class Logger
    {
        /// <summary>
        /// Registra el contenido pasado a un archivo .log.
        /// <br/>
        /// El archivo es creado en 'C:\ProgramData\Canelary\yyyy_MM_dd.log'
        /// </summary>
        /// <returns>La ruta al archivo</returns>
        public static string Log(string content)
        {
            DateTime date = DateTime.Now;

            var commonpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var path = Path.Combine(commonpath, "Canelary");
            var file = Path.Combine(path, date.ToString("yyyy_MM_dd") + ".log");
            Directory.CreateDirectory(path);

            string separator = new('-', 128);
            File.AppendAllText(file,
                $"[Error] - [{date:HH:mm:ss}]{Environment.NewLine}{content}{Environment.NewLine}{separator}{Environment.NewLine}");

            return file;
        }
    }
}
