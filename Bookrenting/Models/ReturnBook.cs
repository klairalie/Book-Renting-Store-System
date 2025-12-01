    using System;
    using System.ComponentModel.DataAnnotations;

    using System.ComponentModel.DataAnnotations.Schema;

    namespace BookRenting.Models
    {
        [Table("return_books")]
        public class ReturnBook
        {
            [Key]
            public int Id { get; set; }

            public string BookTitle { get; set; } = string.Empty;
            public string BookType { get; set; } = string.Empty;

            public DateTime BorrowDate { get; set; }
            public DateTime? ReturnDate { get; set; }

            [DataType(DataType.Currency)]
            public decimal LateFee { get; set; }

            [DataType(DataType.Currency)]
            public decimal PaymentTotal { get; set; }

            [DataType(DataType.Currency)]
            public decimal AmountPaid { get; set; }

            public string ReturnType { get; set; } = "Walk-in";  // Walk-in or Ship
            public string PaymentMode { get; set; } = "Cash";   // Cash or GCash
            public string ReferenceNumber { get; set; } = string.Empty;

            public string Status { get; set; } = "Pending";         // Pending / Returned
            public string PaymentStatus { get; set; } = "Pending";  // Pending / Paid
        }
    }
