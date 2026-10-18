using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrisasdelTenorio.Models
{
    public class PrecioEspecial
    {
        [Key]
        public int PrecioEspecialId { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }
    }
}