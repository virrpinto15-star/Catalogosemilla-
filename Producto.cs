using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoPapa.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string? Descripcion { get; set; }

        public string? Imagen { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 1000000.00, ErrorMessage = "El precio debe ser mayor a 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        public Categoria? Categoria { get; set; }
    }
}
