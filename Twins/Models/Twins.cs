using Core.Database.IdeDbModels;
using Core.Services;
using Core.Services.BackendModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TwinsBackend.Database;

namespace TwinsBackend.Models
{
    public class Twins : IBackend
    {
        public List<String> GetAttributes()
        {
            using var context = new TwinsDbContext();
            return [.. context.Variables.Select(v => v.Name)];
        }

        public List<Product> GetProducts(string label)
        {
            using var context = new TwinsDbContext();
            var param = new SqlParameter("@Etiqueta", label);
            return [.. context.Database.SqlQueryRaw<Product>("Twins.ListarProductos @Etiqueta", param)];
        }

        public string GetTranslation(int languageId, string description)
        {
            using var context = new TwinsDbContext();
            var id = new SqlParameter("@Idioma", languageId);
            var desc = new SqlParameter("@Descripcion", description);

            var rc = context.Database
                .SqlQueryRaw<string?>("Twins.ObtenerTraduccion @Idioma, @Descripcion", id, desc)
                .AsEnumerable()
                .FirstOrDefault();
            return rc ?? "";
        }

        public List<KeyValuePair<string, string?>> GetValues(int id)
        {
            int cte = 2;
            var queryBuild = new StringBuilder(
                ";WITH cte1 AS (SELECT * FROM TwinsDBQuatro053.configuracion.Mercaderias WITH(NOLOCK) WHERE Id = @Id)");

            using var context = new TwinsDbContext();
            foreach (var val in context.Variables)
            {
                queryBuild.Append($", cte{cte++} AS ({val.Query})");
            }

            queryBuild.Append(
                " SELECT T2.N.value('local-name(.)', 'nvarchar(128)') as [Key], T2.N.value('text()[1]', 'nvarchar(MAX)') as [Value] FROM (SELECT * FROM cte2");

            for (int i = 3; i < cte; i++)
            {
                queryBuild.Append($", cte{i}");
            }

            queryBuild.Append(" for xml path(''), type) as T1(X) CROSS APPLY T1.X.nodes('/*') as T2(N)");

            var query = queryBuild.ToString();
            var idParam = new SqlParameter("@Id", id);
            var queryParam = new SqlParameter("@Query", query);

            return context.Database
                .SqlQueryRaw<KeyPair>("Twins.RunQuery @Query, @Id", queryParam, idParam)
                .AsEnumerable()
                .Select(v => new KeyValuePair<string, string?>(v.Key, v.Value))
                .Distinct()
                .ToList();
        }

        public List<KeyValuePair<Product, List<KeyValuePair<string, string?>>>> GetValues(List<Product> products, List<RuleAttributes> attributes)
        {
            int cte = 2;
            var queryBuild = new StringBuilder(
                ";WITH cte1 AS (SELECT * FROM TwinsDBQuatro053.configuracion.Mercaderias WITH(NOLOCK) WHERE Id = @Id)");
            using var context = new TwinsDbContext();
            foreach (var attr in attributes)
            {
                var val = context.Variables.First(v => v.Name == attr.Name);
                queryBuild.Append($", cte{cte++} AS ({val.Query})");
            }

            queryBuild.Append(
                " SELECT T2.N.value('local-name(.)', 'nvarchar(128)') as [Key], T2.N.value('text()[1]', 'nvarchar(MAX)') as [Value] FROM (SELECT * FROM cte2");

            for (int i = 3; i < cte; i++)
            {
                queryBuild.Append($", cte{i}");
            }

            queryBuild.Append(" for xml path(''), type) as T1(X) CROSS APPLY T1.X.nodes('/*') as T2(N)");

            var list = new List<KeyValuePair<Product, List<KeyValuePair<string, string?>>>>();
            var query = queryBuild.ToString();

            foreach (var prod in products)
            {
                var idParam = new SqlParameter("@Id", prod.Id);
                var queryParam = new SqlParameter("@Query", query);

                list.Add(new KeyValuePair<Product, List<KeyValuePair<string, string?>>>(prod, context.Database
                    .SqlQueryRaw<KeyPair>("Twins.RunQuery @Query, @Id", queryParam, idParam)
                    .AsEnumerable()
                    .Select(v => new KeyValuePair<string, string?>(v.Key, v.Value))
                    .Distinct()
                    .ToList()));
            }

            return list;
        }

        private class KeyPair
        {
            public string Key { get; set; } = string.Empty;
            public string? Value { get; set; }
        }
    }
}
