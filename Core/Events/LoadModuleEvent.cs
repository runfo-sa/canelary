namespace Core.Events
{
    /// <summary>
    /// Evento disparado cuando se carga un nuevo modulo. <br/>
    /// El unico parametro que tiene es el nombre del modulo.
    /// </summary>
    public class LoadModuleEvent : PubSubEvent<string>
    { }
}