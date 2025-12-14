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
        public static bool EjecutarAcciones { get; set; } = false;


        // Acción a realizar con el PDF
        public static AccionesPDF AccionPDF { get; set; }
        

        // Control para cerrar el visor
        public static bool CerrarVisor { get; set; } = false;


        // Lista de acciones adicionales a realizar con el PDF
        public enum AccionesPDF
        {
            Ninguna,
            Imprimir,
            Abrir,
            Visualizar
        }
    }
}
