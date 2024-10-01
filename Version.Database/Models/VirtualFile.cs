using Core.FileTree;
using Core.View;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VersionDatabase.Db;

namespace Version.Database.Models
{
    public class VirtualFile(string name, int id) : IFile
    {
        public string Path { get; set; } = $"{name}@{id}";

        public string Name { get; set; } = name;

        private readonly int _id = id;

        public string Read()
        {
            using var context = new DatabaseDbContext();

            var idParam = new SqlParameter("@idEtiqueta", _id);
            var code = context.Database
                .SqlQueryRaw<string?>("EXEC [etiquetas].[GenerarCodigo] @idEtiqueta", idParam)
                .AsEnumerable()
                .FirstOrDefault();

            return code ?? "";
        }

        public void Write(string content)
        {
            using var context = new DatabaseDbContext();

            var idParam = new SqlParameter("@idEtiqueta", _id);
            var codParam = new SqlParameter("@codigo", content);

            context.Database.ExecuteSqlRaw("EXEC [etiquetas].[ActualizarEtiqueta] @idEtiqueta, @codigo", idParam, codParam);
        }

        public IEnumerable<string> ListVersions()
        {
            var context = new DatabaseDbContext();
            return context.Etiquetas
                .Where(e => e.IdEtiqueta == _id)
                .Select(e => e.Version.ToString())
                .ToList();
        }

        public bool Create(string content)
        {
            using var context = new DatabaseDbContext();

            var nombreParam = new SqlParameter("@nombre", Name);
            var codParam = new SqlParameter("@codigo", content);

            try
            {
                context.Database.ExecuteSqlRaw("EXEC [etiquetas].[CrearEtiqueta] @nombre, @codigo", nombreParam, codParam);
            }
            catch (SqlException ex)
            {
                ExceptionPopUp popUp = new(ex.GetBaseException().Message);
                popUp.ShowDialog();
                return false;
            }
            return true;
        }
    }
}
