using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookRenting.Models
{
    [Table("reports")]
public class Report
{
    [Key]
    [Column("report_id")]
    public int ReportId { get; set; }

    [Required]
    [Column("date")]
    public DateTime Date { get; set; }

    [Column("books_approved")]
    public int BooksApproved { get; set; }

    [Column("books_returned")]
    public int BooksReturned { get; set; }

    [Column("books_late")]
    public int BooksLate { get; set; }

    [Column("books_lost")]
    public int BooksLost { get; set; }

    [Column("total_sales")]
    public decimal TotalSales { get; set; }
}
    
}
