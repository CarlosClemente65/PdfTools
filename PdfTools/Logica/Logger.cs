using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PdfTools.Datos;

namespace PdfTools.Logica
{
    public static class Logger
    {
        private static StringBuilder _log = new StringBuilder();

        public static void Agregar(string mensaje)
        {
            _log.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}");
        }

        public static bool TieneErrores()
        {
            return _log.Length > 0;
        }

        public static void Guardar()
        {
            if(!TieneErrores())
            {
                _log.AppendLine("OK");
            }

            File.WriteAllText(Instancias.ConfiguracionGeneral.FicheroSalida, _log.ToString());
        }

        public static void Limpiar()
        {
            _log.Clear();
        }
    }
}
