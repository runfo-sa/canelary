namespace Core.Database.ServiceDbModels;

/// <summary>
/// Estado en el que se encuentra el cliente
/// </summary>
public enum ClientStatus : byte
{
    Okay = 0,
    Desactualizada = 1,
    Sobrantes = 2,
    DesactualizadaSobrantes = 3,
    MultipleInstalaciones = 4,
    NoInstalado = 5,
}