using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FixEngine.Entity
{
    [Table("Symbols")]
    public class Symbol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column(TypeName = "varchar(200)")]
        [Required]
        public string Name { get; set; }
        [Column(TypeName = "varchar(200)")]
        [Required]
        public string LP { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string LPSymbolName { get; set; }
        [Column(TypeName = "varchar(200)")]
        public int Digits { get; set; }
    }
}
