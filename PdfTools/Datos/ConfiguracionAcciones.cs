namespace PdfTools.Datos
{
    // Clase para control de las acciones a realizar
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
