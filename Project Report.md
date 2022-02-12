# Moogle!

![](moogle.png)

> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.

>Raudel Alejandro Gómez Molina Grupo C111

## Implementación en MoogleEngine

Estructura de la biblioteca de clases `MoogleEngine`

### Procesamineto de las palabras del Corpus

- El proyecto cuenta con un método `Index_Corpus` de la clase `Moogle`, el cual se encarga de leer los documentos, y crear un objeto de la clase `Document` para cada documento de la carpeta `Content`.
- La clase `Document` se encarga de procesar el texto contenido dentro de nuestro documento, separar por espacios y eliminar los signos de puntuación.
- En la clase `Corpus_Data` se almacena la información de cada una de las palabras en el diccionario `vocabulary` que tiene como valor un objeto de la clase `DataStructure` donde se guarda: la frecuencia y una lista de indices con las posiciones de la palabra en el documento (estructurando los vectores documento).
- Una vez terminado este proceso procedemos a calcular el *TF-IDF* de las palabras de nuestro corpus, mediante el método `Tf_IdfDoc` de la clase `Document`.

### Procesamiento de la Query 

- Cuando el usuario introduce una nueva *query* se crea un objeto de la clase `QueryClass`, en dicha clase procedemos a extraer las palabras de nuestra *query*.
- Identificamos  los operadores de búsqueda mediante el método `Operators`. Se ha añadido un nuevo operador de búsqueda, identificado por un par de comillas `""` (e.j., `"Licenciatura en Ciencias de la Computación"`) indica que que el texto dentro de las comillas **debe aparecer literalmente en cada uno de los documentos devueltos**.
- Una vez identificados los operadores procedemos a comprobar la existencia de las palabras de la *query* en nuestro corpus, en caso contrario, vamos al método `suggestion` donde combinamos la *Distancia de Levenstein* con el peso de las palabras de nuestro Corpus y construimos la nueva *query* donde incluimos la palabra sugerida.
- Para dar mejores resultados en la búsqueda al usuario identificamos las palabras que posean las mismas raíces o el mismo significado que las de nuestra *query*, mediante la clase `Snowball` que se encarga de realizar el stemming en español y la lista `synonymous` de la clase `Corpus_Data`.
- Procedemos a calcular el *TF-IDF* de las palabras de nuestra *query*.

### Resultados de la Búsqueda

- Comparamos los datos de los vectores documento almacenados en `Corpus_Data`, con el vector consulta mediante el método `SimVectors` y calculamos el *Score* de cada documento teniendo en cuenta la influencia de los operadores mediante el método `ResultSearch`.
- Para tener en cuenta las condiciones de los operadores `Close` y `SearchLiteral`, empleamos la clase `Distance_Word` donde tenemos el método `Shortest_Distance_Word` que nos devuelve la mínima distancia entre una lista de palabras en un determinado documento y el método `Literal` que se encarga de buscar en un documento y determinar la posición de las palabras que están especificadas en el operador `SearchLiteral`.
- Procedemos a construir el *Snippet* de cada uno de nuestro docuemnto mediante el método `Snippet`, si tenemos resultados del operador `SearchLiteral` mostramos una línea por cada grupo de palabras de dicho operador. Por otro lado hemos definido un tamaño máximo de 20 palabras para cada línea, luego auxiliandonos en el método `Shortest_Distance_Word` de la clase `Distance_Word`, determinamos el máximo número de palabras resultantes de la búsqueda que ocupan una ventana del texto de tamaño 20 y las posiciones en que estas se encuentran, si todas estas palabras no fueron contenidas en dicha ventana procedemos a realizar el mismo procedimiento con las restantes.
- Con las posiciones obtenidas en el método `Snippet`, leemos el documento y guardamos el texto contenido en dichas posiciones mediante el método `BuildSinipped`.
-  Una vez concluida la búsqueda construimos el objeto `SearchResult` que devuelve el método `Query` de la clase `Moogle`, mediante un arreglo de objetos `SearchItem`, que adicionalmente, cada uno contiene un arreglo de *SnippetResult*, *Pos_SnippetResult* y *Words_not_result*, con las líneas del *Snippet*, las posiciones de dichas lineas en el documento y la lista de palabras de la *query* que no fueron encontradas en el documento.

## Implementación en Moogle Server 

Hemos añadido a nuestro proyecto una nueva página `Doc.razor`, donde le brindaremos al usuario la opción de poder visualizar el documento directamente desde el navegador.

### Autocompletar

- Para el autocompletamiento hemos añadido el evento `bind:event="oninput"` el cual permite actualizar el valor del string `query` cada vez que el usuario teclea o borra un nuevo carácter.
- Añadimos el evento `onkeyup` que llama al método `Press`, el cual identifica la última porción de palabra tecleada por el usuario y llama al método `AutoComplete` de la clase `Server` el cual devuelve como máximo las 5 palabras de nuestro corpus más cercanas a completar el texto escrito por el usuario.
- Una vez obtenidas las palabras para autocompletar procedemos a mostrarselas al usuario mediante un `datalist`

### Suggestion

- Añadimos el evento `onclick` que permite igualar el string `query` al string `suggestion` y realizar una nueva consulta.

### Visualizar el Documento

- Cada etiqueta `Tittle` y `Snippet` contiene un enlace a la página `Doc.razor`, la cual recive como parámetros el título del documento, la posición de la línea y la página donde se encuentra el *Snippet*.
- La página `Doc.razor` llama al método `Read` de la clase `Server` el cual devuelve las 100 líneas de la página del documento que se quiere mostrar. Además tenemos la posibilidad de visualizar la página anterior, la siguiente y cualquier página del documento a la que quiera acceder el usuario.
