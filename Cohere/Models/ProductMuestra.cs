namespace Cohere.Models
{
    public class ProductoMuestra(string codigo, string nombre, string senasa, bool muestra) : BindableBase
    {
        public string Codigo => codigo;
        public string Nombre => nombre;
        public string Senasa => senasa;

        private bool _muestra = muestra;
        public bool Muestra
        {
            get => _muestra;
            set => SetProperty(ref _muestra, value);
        }
    }
}
