using System.Collections.Generic;
using System.Windows.Documents;

namespace PdfTools.Datos
{
    // Clase para control de las acciones a realizar
    public class ConfiguracionAcciones
    {
        // Controla si hay que realizar alguna accion con el PDF
        public bool EjecutarAcciones { get; set; }

        // Acción a realizar con el PDF
        public Enums.AccionesPDF AccionPDF { get; set; }

        // Lista de acciones a realizar
        public List<Enums.AccionesPDF> AccionesPDF { get; set; }

        // Control para si es necesario abrir el visor
        public bool AbrirVisor {  get; set; }

        // Control para cerrar el visor
        public bool CerrarVisor { get; set; }

        public ConfiguracionAcciones()
        {
            EjecutarAcciones = false;
            AbrirVisor = false;
            CerrarVisor = false;
            AccionesPDF = new List<Enums.AccionesPDF>();
        }
    }
}
