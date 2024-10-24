using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixFront.Models
{
    public class SymbolQuote
    {
        public int SymbolId { get; set; }
        public string SymbolName { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public int Digits { get; set; }
    }
}
