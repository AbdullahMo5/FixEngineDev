using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FixEngine.Entity
{
    [Table("Executions")]
    public class Execution
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string ExecId { get; set; }
        [Column(TypeName = "varchar(200)")]
        [Required]
        public string OrderId { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string PosId { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string ClOrdId { get; set; }

        [Column(TypeName = "varchar(1)")]
        [Required]
        public string ExecType { get; set; }

        [Column(TypeName = "varchar(1)")]
        [Required]
        public string OrdStatus { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string SymbolId { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Symbol { get; set; }
        [Column(TypeName = "varchar(5)")]
        public string Side { get; set; }
        public DateTime TransactTime { get; set; }

        [Column(TypeName = "decimal(20,5)")]
        public decimal AvgPx { get; set; }

        [Column(TypeName = "decimal(20,5)")]
        public decimal OrderQty { get; set; }

        [Column(TypeName = "decimal(20,5)")]
        public decimal LeavesQty { get; set; }

        [Column(TypeName = "decimal(20,5)")]
        public decimal CumQty { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string OrdType { get; set; }

        [Column(TypeName = "decimal(20,5)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(20,5)")]
        public decimal StopPrice { get; set; }

        [Column(TypeName = "varchar(26)")]
        public string TimeInForce { get; set; }
        public DateTime? ExpireTime { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Text { get; set; }

        [Column(TypeName = "varchar(44)")]
        public string OrdRejReason { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Designation { get; set; }

    }
}
