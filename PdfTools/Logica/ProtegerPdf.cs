using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PdfTools.Datos;

namespace PdfTools.Logica
{
    public class ProtegerPdf
    {
        public static void AplicarProteccion(ConfiguracionGeneral parametros, ContextoEjecucion contextoFichero)
        {
            try
            {
                // Abre el documento (importante usar PdfDocumentOpenMode.Modify)
                using(PdfDocument document = PdfReader.Open(parametros.PdfEntrada, PdfDocumentOpenMode.Modify))
                {
                    // Accede a la configuración de seguridad
                    PdfSecuritySettings securitySettings = document.SecuritySettings;

                    // Asigna la contraseña de apertura
                    if(!string.IsNullOrEmpty(parametros.PasswordApertura))
                    {
                        securitySettings.UserPassword = parametros.PasswordApertura;
                    }

                    // Asigna la contraseña de edicion
                    if(!string.IsNullOrEmpty(parametros.PasswordEdicion))
                    {
                        securitySettings.OwnerPassword = parametros.PasswordEdicion;
                    }

                    // Restringir acciones específicas si hay contraseña de edición
                    // Acciones permitidas
                    securitySettings.PermitPrint = true; // Permiso para imprimir
                    securitySettings.PermitFullQualityPrint = true; // Permiso para imprimir en alta resolucion
                    securitySettings.PermitAnnotations = true; // Permiso para crear anotaciones
                    securitySettings.PermitExtractContent = true; // Permiso para extraer texto (seleccionar y copiar)
                    securitySettings.PermitFormsFill = true; // Permiso para rellenar formularios

                    // Acciones no permitidas
                    securitySettings.PermitModifyDocument = false; // Bloquea cualquer cambio en el documento (se desactiva)
                    securitySettings.PermitAssembleDocument = false; // No permite insertar, rotar o eliminar hojas

                    // Guarda el PDF con el nombre de salida
                    document.Save(parametros.PdfSalida);

                    // Marcamos la acción como ejecutada si no hubo errores críticos
                    contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.Proteger);
                }
            }
            catch(PdfReaderException)
            {
                Logger.Agregar("No se puede abrir el PDF. Puede estar protegido con contraseña");
            }
            catch(Exception ex)
            {
                Logger.Agregar($"Error al proteger el PDF: {ex.Message}");
            }
        }
    }
}
