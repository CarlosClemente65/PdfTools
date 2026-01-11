using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PdfTools.Datos;


namespace PdfTools
{
    public static class Utilidades
    {
        // Ruta del ejecutable SumatraPDF 
        public static string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
        public static string rutaSumatra = Path.Combine(rutaBase, "SumatraPDF.exe");
        public static string cacheSumatra = Path.Combine(rutaBase, "sumatrapdfcache");

        // Carga los parámetros desde el archivo de guion
        public static ConfiguracionQR CargarParametros(ConfiguracionGeneral parametros, ConfiguracionQR datosQR, ConfiguracionAcciones acciones, string guion)
        {
            // Instancia para acceso a los metodos de la gestion de parametros
            Logica.GestionParametros gestor = new Logica.GestionParametros();

            // Leer el archivo de guion y asignar los parámetros
            foreach(string linea in File.ReadAllLines(guion))
            {
                // Salta las lineas vacias
                if(string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                // Separa las lineas del guion en clave y valor
                string[] partes = linea
                    .Split(new char[] { '=' }, 2)
                    .Select(p => p.Trim())
                    .ToArray();

                // Chequea que tenga dos partes (clave y valor) antes de asignar los parametros
                if(partes.Length == 2)
                {
                    gestor.AsignaParametros(partes[0].ToLower(), partes[1], parametros, datosQR, acciones); // La clave se pasa a minusculas para unificar textos
                }
                else if(string.Equals(partes[0], "cerrarvisor", StringComparison.OrdinalIgnoreCase))
                {
                    // El parametro 'cerrarvisor' no tiene dos partes y se trata de forma independiente
                    acciones.CerrarVisor = true;
                }
            }

            return datosQR;
        }



        // Establece la ruta para insertar el QR en funcion del entorno y si aplica Verifactu
        public static string ObtenerUrl(ConfiguracionQR datosQR)
        {
            string urlBase = datosQR.DatosUrl.EntornoProduccion ? datosQR.DatosUrl.UrlProduccionBase : datosQR.DatosUrl.UrlPruebasBase;

            if(datosQR.DatosUrl.VeriFactu)
            {
                return urlBase + "ValidarQR";
            }
            else
            {
                return urlBase + "ValidarQRNoVerifactu";
            }
        }

        // Genera la URL con los parámetros del QR UTF-8
        public static void GenerarURL(ConfiguracionQR datosQR)
        {
            // Genera la URL con los parámetros del QR UTF-8
            StringBuilder urlCompleta = new StringBuilder();
            urlCompleta.Append(datosQR.DatosUrl.UrlEnvio).Append("?");
            urlCompleta.Append("nif=").Append(Uri.EscapeUriString(datosQR.DatosFactura.NifEmisor)).Append("&");
            urlCompleta.Append("numserie=").Append(Uri.EscapeUriString(datosQR.DatosFactura.NumeroFactura)).Append("&");
            urlCompleta.Append("fecha=").Append(datosQR.DatosFactura.FechaFactura.ToString("dd-MM-yyyy")).Append("&");
            urlCompleta.Append("importe=").Append(datosQR.DatosFactura.TotalFactura.ToString("F2").Replace(',', '.')); // Asegurar que el decimal es punto
            urlCompleta.Append("&idioma=").Append(datosQR.DatosUrl.IdiomaQR.ToString());

            // Construir la URL completa
            datosQR.DatosUrl.UrlEnvio = urlCompleta.ToString();
        }


        // Comprueba que el codigo de color sea valido
        public static bool ColorValido(string colorHex)
        {
            return Regex.IsMatch(colorHex, @"^#(?:[0-9a-fA-F]{6})$");
        }


        // Cierra todas las instancias del visor SumatraPDF que esten abiertas, matando la tarea del administrador de tareas (no implementado, lo dejo para futuras consultas)
        public static void ForzarCerrarVisor()
        {
            // Crea una lista con todos los procesos que hay abiertos de la aplicacion
            foreach(var proceso in Process.GetProcessesByName("SumatraPDF"))
            {
                // Forzar cierre si sigue activo
                proceso.Kill();
            }
        }

        // Cierra todas las instancias del visor SumatraPDF que esten abiertas, mandando un comando al propio programa para cerrarse.
        public static void CerrarVisor()
        {
            // Crea una lista con todos los procesos que hay abiertos de la aplicacion
            foreach(var proceso in Process.GetProcessesByName("SumatraPDF"))
            {
                string argumentos = $"-dde [CmdExit]"; // Comando para cerrar la aplicacion
                var psi = new ProcessStartInfo(rutaSumatra, argumentos); // Crea el proceso
                psi.CreateNoWindow = true; // No crea una ventana 
                psi.UseShellExecute = false; // No se manda como un comando de la Shell
                Process.Start(psi); // Lanza el comando
            }
        }
    }
}
