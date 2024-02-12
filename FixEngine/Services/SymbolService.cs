using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Resources;
using Services;

namespace FixEngine.Services
{
    public class SymbolService
    {
        private ILogger<SymbolService> _logger;
        private DatabaseContext _databaseContext;
        public SymbolService(ILogger<SymbolService> logger, DatabaseContext context) {
            _logger = logger;
            _databaseContext = context;
        }
        public bool AddSymbol(AddSymbolResource param) { return false; }
        public bool RemoveSymbol(int id) { return false; }
        public bool ContainsSymbol(string symbol) {  return false; }
        public Symbol? GetSymbol(string symbol) {  
            return _databaseContext.symbols.FirstOrDefault(item => item.LPSymbolName == symbol);

        }
        public List<Symbol> GetSymbols() {  throw new NotImplementedException(); }
        public List<Symbol> GetSymbolsByLP(string lp) {  throw new NotImplementedException(); }
    }
}
