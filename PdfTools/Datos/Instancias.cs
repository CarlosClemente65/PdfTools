using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfTools.Datos
{
    public static class Instancias
    {
        // Instancia de las acciones a realizar
        public static ConfiguracionAcciones Acciones { get; private set; }

        public static ConfiguracionGeneral ConfiguracionGeneral { get; private set; }

        public static ConfiguracionQR ConfiguracionQR { get; private set; }


        public static void Inicializar()
        {
            Acciones = new ConfiguracionAcciones();
            ConfiguracionGeneral = new ConfiguracionGeneral();
            ConfiguracionQR = new ConfiguracionQR();
        }
    }
}
