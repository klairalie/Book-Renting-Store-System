using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookRenting.Models
{
    [Table("books")]
    public class Book
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("reference_number")]
        [Display(Name = "Reference Number")]
        public string ReferenceNumber { get; set; } = null!;

        [Required]
        [Column("title")]
        public string Title { get; set; } = null!;

        [Required]
        [Column("author")]
        public string Author { get; set; } = null!;

        [Required]
        [Column("genre")]
        public string Genre { get; set; } = null!;

        [Required]
        [Column("status")]
        public string Status { get; set; } = null!; // Available Digital, Available Physical, etc.

        [Required]
        [Column("price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Column("image_path")]
        public string? ImagePath { get; set; }

        [Column("file_path")]
        public string? FilePath { get; set; }

        [Required]
        [Column("synopsis")]
        public string Synopsis { get; set; } = null!;
    }
}
