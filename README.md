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

<br><br>


### Uso:
```
PdfTools.exe ds123456 guion.txt
```
<br>

#### Parametros guion
- Parametros generales:
	* pdfentrada=Nombre del pdf con la fatura (obligatorio)
	* pdfsalida=Nombre del pdf con el QR (opcional)
	* ficherosalida=nombre del fichero para controlar la finalizacion del proceso (opcional)
	* carpetaentrada=Nombre de la carpeta para procesar por lotes los PDFs que haya dentro
	* carpetasalida=Nombre de la carpeta donde dejar los PDFs procesados por lotes
	* accionpdf=[imprimir | abrir | visualizar | unir]; Acciones adicionales a realizar con el PDF (opcional)
	* listaficheros=Nombre de los ficheros de la carpetaentrada separados por comas a unir 
	* cerrarvisor ;Permite dar la orden de cerrar el visor (opcional)

- Parametros QR:
	* entorno='pruebas' para forzar el envio a la web de pruebas (opcional)
	* verifactu=SI/NO para indicar si son facturas verificables (opcional)
	* ficheroqr=Nombre del fichero con la imagen del QR; si no se pasa es obligatorio los campos nifemisor y datos factura (opcional)
	* url=direccion url para la validacion (opcional)
	* nifemisor=NIF del emisor de la factura para incluir en el QR (opcional)
	* numerofactura=Numero de de factura para incluir en el QR (obligatorio si nifemisor <> "")
	* fechafactura=Fecha de la factura para incluir en el QR (obligatorio si nifemisor <> "")
	* totalfactura=Importe total de la factura para incluir en el QR (obligatorio si nifemisor <> "")
	* posicionx=posicion en milimetros desde el margen izquierdo (opcional)
	* posiciony=posicion en milimetros desde el margen superior (opcional)
	* ancho=ancho del QR en milimetros (el alto sera el mismo) (opcional)
	* color=Color del QR en formato hexadecimal (opcional); defecto #000000 (negro)
	* marcaagua=Texto para insertar una marca de agua en el documento (opcional)
	* colormarca=Color de la marca de agua en formato hexadecimal; defecto #E1E1E1 (gris claro)
	* idioma=[gl |ca | eu | es | va | en ]; idioma de respuesta de la AEAT en el QR (opcional)

<br>

### Notas:
* No es necesario pasar los parametros con comillas si hay espacios; se toma el valor que hay a continuacion del '='
* Los nombres de los parametros pueden ir en mayusculas o minusculas (se convierten a minusculas)
* Si no se pasa el nombre del pdf de salida, se utiliza el mismo que el de entrada con un sufijo (_salida)
* La url se puede pasar (debe estar bien formada), y si no se pasa, se genera en base a los datos de la factura, entorno y verifactu
* El entorno por defecto es la web de produccion (real), por lo que en pruebas debe pasarse el parametro entorno=pruebas
* Por defecto se funciona en modo NO VeriFactu, por lo que para trabajar de ese modo se debe pasar el parametro verifactu=si
* Si no se pasa el fichero con la imagen QR, es obligatorio pasar los campos nifemisor y datos factura (fecha,numero e importe)
* Si no se pasa el NIF del emisor no se añadira el QR; si se pasa es obligatorio pasar los demas parametros de la factura.
* Las posiciones X e Y del QR estan puestas por defecto a 10 mm de los margenes
* El ancho del QR tiene un defecto de 30 mm; no tiene limitacion pero deberia estar entre 25 y 40 mm (alto y ancho)
* El texto de la marca de agua admite saltos de linea añadiendo '\n' en la posicion donde insertarlo
* Si se produce algun error por algun parametro que falte o no sea correcto, se genera el fichero "errores.txt" con el detalle
* El parametro 'accionpdf= permite realizar acciones adicionales con el PDF utilizando el programa SumatraPDF
	- 'imprimir' = Lanza el PDF generado por la impresora predeterminada
	- 'abrir' = Abre el PDF generado con el visor; la aplicacion espera a que se cierre el visor para continuar
	- 'visualizar' = Abre el PDF pasado por parametro con el visor; la aplicacion continua sin esperar al cierre del visor
	- 'unir' = Fusiona en un solo PDF los ficheros de la carpetaentrada.
* Para realizar varias acciones, deben incluirse tantos parametros 'accionpdf' como sean necesarios y por el orden de ejecucion
* El parametro cerrarvisor' permite cerrar todos los procesos abiertos del visor SumatraPDF; se puede pasar como un parametro adicional ademas del resto
* Si se incluye el parametro 'ficherosalida' la aplicacion genera un fichero que puede usarse para controlar si la aplicacion ha terminado o no. 
  Con el parametro 'visualizar' la aplicacion no se detiene aunque no se cierre el visor, por lo que se generara (si se ha indicado) el fichero de salida
* El parametro 'idioma' permite que la respuesta de Hacienda al chequear el QR sea en uno de los idiomas siguientes:
	- gl: gallego
	- ca: catalán
	- eu: euskera
	- es: castellano
	- va: valenciano
	- en: inglés
* En el caso de procesado de una carpeta se debe tener en cuenta lo siguiente:
	- El parametro 'carpetaentrada' es obligatorio
	- En la carpeta de entrada, ademas del PDF debe haber un fichero.txt con los parametros para generar el QR
	- Si se quiere usar una imagen para insertar, debe ponerse tambien en la carpeta de entrada con el mismo nombre que el PDF (factura.pdf - factura.bmp)
	- En caso de usar una imagen, en el guion solo son necesarios los parametros de posicion y marca de agua
	- Si se pasa el parametro 'carpetasalida' grabara en esa carpeta (si no existe se crea) los ficheros con el mismo nombre que en la entrada
	- Si no se pasa el parametro 'carpetasalida' los ficheros se pondran en la misma carpeta de entrada, teniendo en cuenta lo siguiente:
		- Si el guion del fichero tiene el parametro 'pdfsalida' se utilizara ese nombre
		- Si el guion del fichero no tiene el parametro 'pdfsalida', se pondra el mismo que el de entrada con el sufijo '_salida'
	- Si se incluyen acciones en el guion inicial, se ejecutaran para todos los ficheros de la carpeta de entrada
	- Si en el guion inicial no se indica ninguna accion, en el guion de cada fichero se pueden indicar acciones para ese fichero (abrir, imprimir, etc)
* En el caso del proceso de union de PDFs se debe tener en cueta lo siguiente:
	- En 'listaficheros' estaran los nombres de los ficheros a añadir por orden de insercion y separados por comas
	- No es necesaria la ruta completa ni la extension de la lista de ficheros (valido factura o factura.pdf)
	- Si no se incluye el parametro 'listaficheros' se añadiran los ficheros de la carpeta de entrada (ordenados segun lectura del sistema)
	- Si no se incluye el parametro 'carpetasalida' el pdf generado se dejara en la carpeta de entrada
	- Si no se incluye el parametro 'pdfsalida' se genera uno por defecto "fichero_salida.pdf"
* En la ruta de ejecucion deben estar los siguientes ficheros:
	- PdfSharp.dll
	- QRCoder.dll
	- SumatraPDF.exe
* El fichero 'Configuracion_visor.txt' es una copia modificada con los parametros del visor, 
  Para usarla debe renombrarse como "SumatraPDF-settings.txt" y ubicarla en la misma ruta que el visor SumatraPDF.
  
  