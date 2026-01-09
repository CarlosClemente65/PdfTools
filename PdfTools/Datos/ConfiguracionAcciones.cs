using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfTools.Datos
{
    public class ConfiguracionAcciones
    {
        // Controla si hay que realizar alguna accion con el PDF
        public bool EjecutarAcciones { get; set; }


        // Acción a realizar con el PDF
        public Enums.AccionesPDF AccionPDF { get; set; }
        

        // Control para cerrar el visor
        public bool CerrarVisor { get; set; }

        public ConfiguracionAcciones()
        {
            EjecutarAcciones = false;
            CerrarVisor = false;
        }
    }
}
