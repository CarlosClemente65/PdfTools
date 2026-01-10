using System.Text;
using System.IO;

namespace PdfTools.Logica
{
    public static class Logger
    {
        private static StringBuilder _log = new StringBuilder();

        public static void Agregar(string mensaje)
        {
            _log.AppendLine($"Error: {mensaje}");
        }

        public static bool TieneErrores()
        {
            return _log.Length > 0;
        }

        public static void Guardar(string rutaFichero)
        {
            if(!TieneErrores())
            {
                _log.AppendLine("OK");
            }

            File.WriteAllText(rutaFichero, _log.ToString());
        }

        public static void Limpiar()
        {
            _log.Clear();
        }
    }
}
