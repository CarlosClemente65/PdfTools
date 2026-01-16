using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;


namespace PdfTools.Datos
{
    // Parametros generales para gestionar el proceso
    public class ConfiguracionGeneral
    {
        public string PdfEntrada { get; set; } // Fichero PDF de entrada
        public string PdfSalida { get; set; } // Fichero PDF de salida
        public string RutaFicheros { get; set; } // Ruta base para ficheros de entrada y salida
        public string FicheroSalida { get; set; } // Fichero de control para gestionar cuando termina el programa.
        public string CarpetaEntrada { get; set; } // Carpeta de entrada si se procesan varios ficheros
        public string CarpetaSalida { get; set; } // Carpeta de salida si se procesan varios ficheros
        public bool ProcesarCarpeta { get; set; } // Indica si se procesa una carpeta completa
        public List<string> ListaArchivos { get; set; } // Lista de archivos para procesar si se pasa una carpeta

        public ConfiguracionGeneral()
        {
            RutaFicheros = Directory.GetCurrentDirectory();
            FicheroSalida = Path.Combine(RutaFicheros, "resultado.txt");
            ProcesarCarpeta = false;
            ListaArchivos = new List<string>();
        }

    }
}
