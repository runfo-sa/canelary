namespace Core.Services
{
    /// <summary>
    /// Servicio que ofrece analisis, completado de variables y muestra visual para el lenguaje ZPL.
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
        ///
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
        /// <b>DEBE SER LLAMADO ANTES DE <see cref="LoadVariables"/></b><br/><br/>
        /// Procesa los parametros definidos en la metadata.
        /// </summary>
        /// <returns>A si mismo, para concatenar metodos</returns>
        public IPreview ParseMetadata();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool HasMetadata();

        /// <summary>
        /// Crea la preview de la etiqueta
        /// </summary>
        public Task<List<byte[]?>?> Build(string dpi, string size);
    }
}
