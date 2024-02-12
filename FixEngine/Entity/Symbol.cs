using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FixEngine.Entity
{
    [Table("Symbols")]
    public class Symbol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
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
