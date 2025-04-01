namespace PreviewLabelary.Models;

/// <summary>
/// Metadata para una etiqueta,
/// util para tener variables de como se generaria la muestra
/// o para integrar con alguna funcionalidad del IDE.
/// </summary>
public class Metadata
{
    /// <summary>
    /// Lista de lenguajes con alfabetos no encontrados en la tabla ASCII.
    /// </summary>
    public List<Language>? Languages { get; set; }

    public int? ProductId { get; set; }
}