using System.ComponentModel.DataAnnotations;

namespace CatalogoPapa.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres")]
        public string? Descripcion { get; set; }

        public List<Producto>? Productos { get; set; }
    }
}
