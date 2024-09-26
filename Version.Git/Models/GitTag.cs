namespace VersionGit.Models
{
    public record struct GitTag(string Tag, string Date, string Message)
    {
        /// <summary>
        /// Convierte una string a una instancia de GitTag.<br/>
        /// Los parametros deben estar separados por '|'.
        /// </summary>
        public static GitTag Parse(string input)
        {
            string[] inputs = input.Split('|', StringSplitOptions.TrimEntries);
            return new GitTag(inputs[0], inputs[1], inputs[2]);
        }

        private static GitTag _local = new("Local", DateTime.Now.ToString(), "Cambios locales");
        public static GitTag Local => _local;
    }
}
