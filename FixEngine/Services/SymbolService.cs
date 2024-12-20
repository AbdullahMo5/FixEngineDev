﻿using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Resources;

namespace FixEngine.Services
{
    public class SymbolService : GenericService<Symbol>, ISymbolService
    {
        private static SymbolResource[] demoSymbols = {
                new SymbolResource("1", "EURUSD", "CENTROID", "EURUSD", 5),
                new SymbolResource("2", "GBPUSD", "CENTROID", "GBPUSD", 5),
                new SymbolResource("3", "AUDUSD", "CENTROID", "AUDUSD", 5),
                new SymbolResource("4", "USDJPY", "CENTROID", "USDJPY", 5),
                new SymbolResource("5", "USDCAD", "CENTROID", "USDCAD", 5),
                new SymbolResource("6", "NZDUSD", "CENTROID", "NZDUSD", 5),
                new SymbolResource("7", "USDCHF", "CENTROID", "USDCHF", 5),
                new SymbolResource("8", "USDCHF", "CENTROID", "USDCHF", 5),
                new SymbolResource("9", "GBPJPY", "CENTROID", "GBPJPY", 5),
                new SymbolResource("10", "AUDJPY", "CENTROID", "AUDJPY", 5),

        };
        private ILogger<SymbolService> _logger;
        private DatabaseContext _databaseContext;
        public SymbolService(ILogger<SymbolService> logger, DatabaseContext context) : base(context)
        {
            _logger = logger;
            _databaseContext = context;
        }
        public bool AddSymbol(AddSymbolResource param) { return false; }
        public bool RemoveSymbol(int id) { return false; }
        public bool ContainsSymbol(string symbol) { return false; }
        public Symbol? GetSymbol(string symbol)
        {
            return _databaseContext.symbols.FirstOrDefault(item => item.LPSymbolName == symbol);

        }
        public List<SymbolResource> GetSymbols() { return demoSymbols.ToList(); }
        public SymbolResource? GetSymbolByLP(string lp, string symbol)
        {
            return demoSymbols.FirstOrDefault(item => (item.LP == lp && item.LPSymbolName == symbol));
        }
        public List<SymbolResource> GetSymbolsByLP(string lp) { throw new NotImplementedException(); }
    }
}
