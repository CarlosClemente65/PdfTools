<h1 align="center"> PdfTools </h1>
<br>

<h2> Herramientas para gestion de PDFs. </h2>
<br>
<h4> @ Carlos Clemente (Diagram Software Europa S.L.) - 10/2025 </h4>

<h3>Descripción</h3>
Añade el codigo QR obligatorio en facturas para sistemas Veri*Factu.
Permite añadir una marca de agua, y al finalizar puede imprimir o abrir el PDF generado
Tambien puede usarse como visualizador de ficheros PDF, y para fusionar varios PDFs en uno
<br><br>

### Control versiones

* v1.0.0.0 Primera versión funcional
* v1.1.0.0 Incorporada la opción para el procesado mediante guion
* v1.2.0.0 Incorporada la opción para añadir una marca de agua
* v1.3.0.0 Incorporada la opcion para pasar la imagen del QR generada de forma externa
* v1.4.0.0 Incorporada la opcion para imprimir el PDF ademas de guardarlo en disco
* v1.5.0.0 Incorporada la opcion para abrir el PDF en el visor SumatraPDF
* v1.6.0.0 Incorporada la opcion para visualizar cualquier PDF que se pase por parametro (sin utilizar la insercion del QR)
* v1.7.0.0 Incorporada la opcion para cerrar los procesos abiertos del visor SumatraPDF
* v1.8.0.0 Incorporada la posibilidad de añadir la marca de agua a cualquier PDF que se pase
* v2.0.0.0 Modificado el proceso de cerrar el visor para pasarlo de forma independiente
* v3.0.0.0 Modificado nombre de la aplicacion 
* v3.1.0.0 Añadido parametro para indicar el idioma de respuesta de la AEAT en el QR de las facturas
* v3.2.0.0 Añadidos parametros para procesar un lote de facturas de una carpeta de entrada
* v3.3.0.0 Añadido parametros para unir en un solo PDF los ficheros de una carpeta
* v3.4.0.0 Añadida posibilidad de ejecutar varias acciones adicionales
* v3.4.1.0 Añadida posibilidad de ejecutar acciones individuales por cada fichero																				  
* v3.5.0.0 Modificado ejecucion acciones en lotes para permitir acciones globales o por fichero

<br><br>


### Uso:
```
PdfTools.exe ds123456 guion.txt
```
<br>

#### Parametros guion
- Parametros generales:
	* PdfEntrada=Nombre del pdf con la fatura (obligatorio)
	* PdfSalida=Nombre del pdf con el QR (opcional)
	* FicheroSalida=Nombre del fichero para controlar la finalizacion del proceso (opcional)
	* CarpetaEntrada=Nombre de la carpeta para procesar un lote de los PDFs que haya dentro
	* CarpetaSalida=Nombre de la carpeta donde dejar los PDFs procesados por lotes (opcional)
	* ListaFicheros=Nombre de los ficheros de CarpetaEntrada separados por comas a procesar(opcional)
	* TextoMarca=Texto para insertar una marca de agua en el documento (opcional)
	* ColorMarca=Color de la marca de agua en formato hexadecimal; defecto #E1E1E1 (gris claro)
	* Acciones=Acciones a realizar en el proceso separadas por comas y por orden de ejecucion:
		- InsertarQR: Añade el QR a un solo fichero
		- InsertarLoteQR: Añade el QR a un lote de ficheros de una carpeta
		- Unir: fusiona en un solo PDF todos los ficheros de la carpeta de entrada
		- InsertarMarca: Añade una marca de agua; es necesario pasar el parametro 'TextoMarca'
		- Imprimir: Imprime el documento de salida
		- Abrir: Abre el documento de salida con el visor y espera al cierre para continuar
		- Visualizar: Igual que 'Abrir' pero no espera al cierre.
		- CerrarVisor: Cierra el visor SumatraPDF en el caso de que este abierto.

- Parametros QR:
	* Entorno='pruebas' para forzar el envio a la web de pruebas (opcional)
	* Verifactu=[SI/NO | S/N | true/false] para indicar si son facturas verificables (opcional)
	* FicheroQR=Nombre del fichero con la imagen del QR; si no se pasa es obligatorio los campos nifemisor y datos factura (opcional)
	* url=direccion url para la validacion (opcional)
	* NifEmisor=NIF del emisor de la factura para incluir en el QR (obligatorio si no se pasa un ficheroQR)
	* NumeroFactura=Numero de de factura para incluir en el QR (obligatorio si no se pasa un ficheroQR)
	* FechaFactura=Fecha de la factura para incluir en el QR (obligatorio si no se pasa un ficheroQR)
	* TotalFactura=Importe total de la factura para incluir en el QR (obligatorio si no se pasa un ficheroQR)
	* Posicionx=posicion en milimetros desde el margen izquierdo (opcional)
	* Posiciony=posicion en milimetros desde el margen superior (opcional)
	* Ancho=ancho del QR en milimetros (el alto sera el mismo) (opcional)
	* Color=Color del QR en formato hexadecimal (opcional); defecto #000000 (negro)
	* Idioma=Idioma de respuesta de la AEAT en el QR (opcional) entre uno de los siguientes:
		- gl: gallego
		- ca: catalán
		- eu: euskera
		- es: castellano (defecto)
		- va: valenciano
		- en: inglés

<br>

### Notas:
* No es necesario pasar los parametros con comillas aunque tengan espacios; se toma el valor que hay a continuacion del '='
* Los nombres de los parametros pueden ir en mayusculas o minusculas
* Si no se pasa el nombre del pdf de salida, se utiliza el mismo que el de entrada con un sufijo (_salida)
* La url es opcional (debe estar bien formada), y si no se pasa, se genera en base a los datos de la factura, entorno y verifactu
* El entorno por defecto es la web de produccion (real), por lo que en pruebas debe pasarse el parametro entorno=pruebas
* Por defecto se funciona en modo NO VeriFactu, por lo que para trabajar de ese modo se debe pasar el parametro verifactu
* Si no se pasa el fichero con la imagen QR, es obligatorio pasar los campos nifemisor y datos factura (fecha, numero e importe)
* Si no se pasan los datos del emisor y factura o el fichero con la imagen, no se añadira el QR.
* Las posiciones X e Y del QR estan puestas por defecto a 10 mm de los margenes
* El ancho del QR tiene un defecto de 30 mm; no tiene limitacion pero deberia estar entre 25 y 40 mm (alto y ancho)
* El texto de la marca de agua admite saltos de linea añadiendo '\n' en la posicion donde insertarlo
* Si se produce algun error por algun parametro que falte o no sea correcto, se genera el fichero "errores.txt" con el detalle
* Si se incluye el parametro 'FicheroSalida' la aplicacion genera un fichero que puede usarse para controlar si la aplicacion ha terminado o no. 
* Con el parametro 'ListaFicheros' debe tenerse en cuenta lo siguiente:
	- Solo se trataran los ficheros incluidos en la lista
	- Se procesaran por el orden en el estan incluidos
	- No es necesaria la ruta completa ni la extension de los ficheros (valido factura o factura.pdf)
* Si no se incluye el parametro 'ListaFicheros' y se procesa una carpeta, se procesaran todos los ficheros de la carpeta por el orden del sistema.
* En el caso de procesado de una carpeta para insertar un QR se debe tener en cuenta lo siguiente:
	- El parametro 'CarpetaEntrada' es obligatorio
	- Ademas del PDF debe haber un guion.txt con los parametros para generar el QR y mismo nombre que el PDF
	- Si no se incluye el guion.txt, el PDF de entrada se dejara sin procesar en la carpeta de salida (no genera error)
	- Si se quiere usar una imagen ya generada para el QR, debe copiarse a la carpeta de entrada con el mismo nombre que el PDF (factura.pdf - factura.bmp)
	- En caso de usar una imagen, en el guion solo son necesarios los parametros de posicion y marca de agua
	- Si la 'CarpetaSalida' no existe se crea, y grabara los ficheros con el mismo nombre que en la entrada
	- Si no se pasa el parametro 'CarpetaSalida' los ficheros se pondran en la 'CarpetaEntrada' teniendo en cuenta lo siguiente:
		- Si el guion.txt tiene el parametro 'PdfSalida' se utilizara ese nombre
		- En caso contrario, se pondra el mismo que el de entrada con el sufijo '_salida'
	- Se pueden pasar acciones individuales por cada fichero en el guion.txt
	- Si se pasan acciones globales en el guion principal, se aplicaran a todos los ficheros (no se tienen en cuenta las acciones individuales)
* En el caso del proceso de union de PDFs se debe tener en cuenta lo siguiente:
	- Si no se incluye la 'ListaFicheros' se añadiran los ficheros de la carpeta de entrada (ordenados segun lectura del sistema)
	- Si no se incluye la 'CarpetaSalida' el pdf generado se dejara en la carpeta de entrada
	- Si no se incluye el 'PdfSalida' se genera uno por defecto "fichero_salida.pdf"
* En la ruta de ejecucion deben estar los siguientes ficheros:
	- PdfSharp.dll
	- QRCoder.dll
	- SumatraPDF.exe
* El fichero 'Configuracion_visor.txt' es una copia modificada con los parametros del visor, 
  Para usarla debe renombrarse como "SumatraPDF-settings.txt" y ubicarla en la misma ruta que el visor SumatraPDF.
  
 