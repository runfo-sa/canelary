using BackendTwins.Database;
using Core.Database.IdeDbModels;
using Core.Services;
using Core.Services.BackendModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

namespace BackendTwins.Models
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

        private sealed class KeyPair
        {
            public string Key { get; set; } = string.Empty;
            public string? Value { get; set; } = null;
        }

        public string ParseVariable(string key, ref List<KeyValuePair<string, string?>> dictionary)
        {
            var parts = key.Split(';');
            var reg = dictionary.Find(v => v.Key.Equals(parts[0], StringComparison.CurrentCultureIgnoreCase));
            if (reg.Value != null)
            {
                parts[0] = reg.Value;
            }
            else
            {
                return "";
            }

            if (parts.Length > 1)
            {
                var functions = parts[1].Split('-');
                foreach (var func in functions)
                {
                    var function = func[..2];
                    switch (function)
                    {
                        case "FK":
                            parts[0] = Convert.ToDecimal((double)Convert.ToInt32(parts[0]) / 1000.0)
                                .ToString(func[2..]);
                            break;

                        case "FF":
                            parts[0] = DateTime
                                .ParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture)
                                .ToString(func[2..]);
                            break;

                        case "FD":
                            parts[0] = Convert.ToDecimal(parts[0]).ToString(func[2..]);
                            break;

                        case "FR":
                            char padChar = func[2];
                            parts[0] = parts[0].PadLeft(Convert.ToInt32(func[3..]), padChar);
                            break;

                        case "FC":
                            parts[0] = (func[2..4] == "SI") ? parts[0].Replace(",", "") : parts[0].Replace(".", ",");
                            break;

                        case "FP":
                            parts[0] = (func[2..4] == "SI") ? parts[0].Replace(".", "") : parts[0].Replace(",", ".");
                            break;

                        case "FI":
                            parts[0] = BackendServiceProvider.Backend.GetTranslation(func[2] - '0', parts[0]);
                            break;
                    }
                }
            }

            return parts[0];
        }

        public string LoadVariables(string content, Int32 id, ref StringBuilder error)
        {
            var dictionary = GetValues(id);

            int startIdx;
            int endIdx = content.LastIndexOf("@]");

            while (endIdx > 0)
            {
                startIdx = content.LastIndexOf("[@");
                if (startIdx > 0)
                {
                    var key = content[(startIdx + 2)..endIdx];
                    var value = ParseVariable(key, ref dictionary);
                    if (value.IsNullOrEmpty())
                    {
                        error.AppendLine($"Variable [@{key}@] no esta cargada para el producto");
                    }
                    content = content.Replace($"[@{key}@]", value, StringComparison.CurrentCultureIgnoreCase);
                    endIdx = content.LastIndexOf("@]");
                }
                else
                {
                    error.AppendLine($"Variable definida erroneamente, falta '[@' pos: {endIdx}.");
                    endIdx = content.LastIndexOf("@]", endIdx - 2);
                }
            }

            return content;
        }
    }
}
