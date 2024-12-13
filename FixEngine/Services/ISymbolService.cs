using FixEngine.Entity;
using FixEngine.Resources;

namespace FixEngine.Services
{
    public interface ISymbolService : IGenericService<Symbol>
    {
        List<SymbolResource> GetSymbols();
    }
}
