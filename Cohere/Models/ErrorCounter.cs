namespace Cohere.Models
{
    public class ErrorCounter(int errorCount, int productsCount)
    {
        public int ErrorCount => errorCount;
        public int ProductsCount => productsCount;
    }
}