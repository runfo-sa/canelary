namespace Core.Models
{
    public record struct ProductReport(string Attribute, string? Value, ProductError Error, string? Comments) { }
}