using Core.Services.BackendModel;

namespace Cohere.Models;

public class ErrorCounter(int errorCount, IEnumerable<Product> productsCount)
{
    public int ErrorCount => errorCount;
    public IEnumerable<Product> ProductsCount => productsCount;
}