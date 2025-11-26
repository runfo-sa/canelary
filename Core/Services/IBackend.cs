using System.Text;

using Core.Database.IdeDbModels;
using Core.Services.BackendModel;

namespace Core.Services;

/// <summary>
/// Servicio que proporciona los datos necesarios para completar las variables encontradas en las etiquetas.
/// </summary>
public interface IBackend
{
    /// <summary>
    /// Devuelve el valor de todos los atributos para un determinado producto.
    /// </summary>
    public List<KeyValuePair<string, string?>> GetValues(int id);

    /// <summary>
    /// Devuelve el valor de los atributos indicados para todos los productos especificados.
    /// </summary>
    public List<KeyValuePair<Product, List<KeyValuePair<string, string?>>>> GetValues(List<Product> products, List<RuleAttributes> attributes);

    /// <summary>
    /// Lista de atributos disponibles.
    /// </summary>
    /// <returns></returns>
    public List<string> GetAttributes();

    /// <summary>
    /// Devuelve la lista de productos que utilizan la etiqueta indicada.
    /// </summary>
    public List<Product> GetProducts(string label);

    /// <summary>
    /// Obtiene la traduccion para un determinado idioma.
    /// </summary>
    public string GetTranslation(int languageId, string description);

    /// <summary>
    /// Devuelve el codigo ZPL con las variables reemplazadas. Ademas de reportar los errores encontrados.
    /// </summary>
    public string LoadVariables<T>(string content, int id, ref StringBuilder error, T? extraData = null) where T : class;

    public string LoadVariables(string content, int id, ref StringBuilder error);
}