using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookRenting.Models
{
    [Table("rented_books")]
    public class RentedBook
    {
        [Key]
        [Column("rent_id")]
        public int RentId { get; set; }

        [Required]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Column("address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("contact_number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [Column("book_title")]
        public string BookTitle { get; set; } = string.Empty;

        [Required]
        [Column("book_type")]
        public string BookType { get; set; } = string.Empty;

        [Column("reference_number")]
        public string? ReferenceNumber { get; set; }

        [Column("receipt_path")]
        public string? ReceiptPath { get; set; }

        [Required]
        [Column("author")]
        public string Author { get; set; } = string.Empty;

       [Required] 
       [Column("borrow_date")] 
       public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Column("return_date")]
public DateTime? ReturnDate { get; set; } = DateTime.Now;

        [Required]
        [Column("book_price")]
        public decimal BookPrice { get; set; } = 0m;

        [Required]
        [Column("borrow_type")]
        public string BorrowType { get; set; } = string.Empty;

        [Required]
        [Column("deposit")]
        public decimal Deposit { get; set; } = 0m;

        [Required]
        [Column("shipping_fee")]
        public decimal ShippingFee { get; set; } = 0m;

        [Required]
        [Column("payment_total")]
        public decimal PaymentTotal { get; set; } = 0m;

        [Required]
        [Column("payment_mode")]
        public string PaymentMode { get; set; } = string.Empty;

        [Required]
        [Column("amount_paid")]
        public decimal AmountPaid { get; set; }      

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        public decimal LateFee { get; set; }

[NotMapped] // This tells EF not to look for it in the database
    public string FilePath { get; set; } = string.Empty;
        // public string PaymentStatus { get; internal set; } 
    }
    }           