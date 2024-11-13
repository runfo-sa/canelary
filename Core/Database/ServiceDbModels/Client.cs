using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Database.ServiceDbModels
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Cliente), IsUnique = true)]
    public class Client
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        /// <summary>
        /// IPv4 de un Cliente del servicio
        /// </summary>
        public string Cliente { get; set; } = String.Empty;

        /// <summary>
        /// Estado en el que se encuentra el cliente
        /// </summary>
        public ClientStatus Estado { get; set; }

        /// <summary>
        /// Ultima conexión del cliente con el servicio
        /// </summary>
        public DateTime UltimaConexion { get; set; }
    }
}