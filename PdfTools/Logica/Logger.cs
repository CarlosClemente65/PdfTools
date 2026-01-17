using System.Text;
using System.IO;

namespace PdfTools.Logica
{
    // Clase para registar un log con los mensajes de error o del resultado de proceso por lotes 
    public static class Logger
    {
        private static StringBuilder _log = new StringBuilder();

        // Añade un mensaje al log
        public static void Agregar(string mensaje)
        {
            _log.AppendLine($"Error: {mensaje}");
        }

        // Añade un mensaje al log (sobrecarga del anterior con stringbuilder)
        public static void Agregar(StringBuilder mensaje)
        {
            // Sobrecarga para grabar el resultado en el procesado por lotes
            _log.Append(mensaje);
        }

        // Controla si tiene contenido el log
        public static bool TieneContenido()
        {
            return _log.Length > 0;
        }

        public static bool EstaVacio()
        {
            return _log.Length == 0;
        }

        // Guarda en la ruta el log generado
        public static void Guardar(string rutaFichero)
        {
            // Si no hay ningun mensaje, se pone un OK para no dejar vacio el fichero con el resultado
            if(EstaVacio())
            {
                _log.AppendLine("OK");
            }

            File.WriteAllText(rutaFichero, _log.ToString());
        }

        // Vacia el log (util en el procesado por lotes)
        public static void Limpiar()
        {
            _log.Clear();
        }

        // Devuelve el contenido del log para enlazar procesos
        public static StringBuilder Contenido()
        {
            return _log; 
        }
    }
}
