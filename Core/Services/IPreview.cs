namespace Core.Services
{
    /// <summary>
    /// Servicio que ofrece previsualización de etiquetas.
    /// </summary>
    public interface IPreview
    {
        /// <summary>
        /// Contenido procesado para generar la preview
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Lista de errores encontrados durante el proceso
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Devuelve las observaciones realizadas en el codigo.
        /// </summary>
        /// <param name="codigo">Codigo del producto</param>
        /// <returns>A si mismo, para concatenar metodos</returns>
        public Task<string[]?> Linting(string codigo, string dpi, string size);

        /// <summary>
        /// Completa las variables de una etiqueta con los datos de un producto especificado.
        /// </summary>
        /// <returns>A si mismo, para concatenar metodos</returns>
        public IPreview LoadVariables();

        /// <summary>
        /// Procesa las configuraciones definidas en la etiqueta para el <see cref="IPreview"/>.
        /// </summary>
        /// <returns>A si mismo, para concatenar metodos</returns>
        public IPreview ParseMetadata();

        /// <summary>
        /// Analiza si la etiqueta contiene o no configuraciones especiales para <see cref="IPreview"/>.
        /// </summary>
        public bool HasMetadata();

        /// <summary>
        /// Genera la previsualización de la etiqueta.
        /// </summary>
        public Task<List<byte[]?>?> Build(string dpi, string size);
    }
}
