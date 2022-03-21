# Moogle!

![](moogle.png)

> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.

>Raudel Alejandro Gómez Molina C111

## Descripción del Proyecto

### Algoritmos de Búsqueda

La búsqueda está basada en el modelo vectorial de recuperación de la información *SRI*, utilizando el *TF-IDF* (frecuencia de término - frecuencia inversa de documento), el cual determina la relevancia de una palabra asociada a un documento en una determinada colección, sumado a la *Similitud del Coseno*, método mediante el cual se asigna un *Score* a cada documento y se establece un ranking de resultados para el usuario.

### Operadores

El proyecto cuenta con varios operadores para mejorar la búsqueda del usuario:
- *Exclusión*, identificado con con un `!` delante de una palabra, (e.j., `!computación`)
 indica que `computación` **no debe aparecer en ningún documento devuelto**.
- *Inclusión*, identificado con con un `^` delante de una palabra, (e.j., `^computación`)
 indica que `computación` **debe aparecer en todos los documentos devueltos**.
- *Mayor Relevancia*, identificado por varios `*` delante de una palabra, (e.j., `*computación`) indica que `computación` es más relevante que las demás palabras de la *Query* tantas veces como `*` tenga delante de ella.
- *Cercanía* identificado con un `~` entre las palabras (e.j., `M~N~P`) indica que los documentos que contengan una ventana del texto con `M`, `N` y `P` tendrán mayor *score*, en dependencia del tamaño de esta ventana.
- *Búsqueda Literal*, identificado por un par de comillas `""` (e.j., `"Licenciatura en Ciencias de la Computación"`) indica que que el texto dentro de las comillas **debe aparecer literalmente en cada uno de los documentos devueltos**, si dentro del texto que está en las comillas aparece un `?` (e.j., `"Licenciatura en ? de la Computación"`) indica que **cualquier palabra puede aparecer en esa posición**.

### Sugerencia

Para brindar una mayor exactitud en la búsqueda, el proyecto cuenta con un corrector de palabras, el cual se encarga de dar una sugerencia al usuario en caso de que la búsqueda no coincida con los datos almacenados.

### Resultados de la Búsqueda

Una vez rankeados los documentos, se le muestra al usuario una lista de los mismos, con el título y el *Snippet* donde se encontraron las palabras buscadas por este. Adicionalmente, se cuenta con la posibilidad de poder visualizar un fragmento más amplio del documento donde se hallaron los resultados, así como la opción de poder leer cualquier parte del documento.

### Ejecutando el proyecto

- Debe colocar los documentos en los que quiere desarrollar la búsqueda, en la carpeta
`Content`, en formato `.txt`.  

- Este proyecto está desarrollado para la versión objetivo de .NET Core 6.0. Para ejecutarlo debe ir a la ruta en la que está ubicada el proyecto y ejecutar en la terminal de Linux:

```bash
make dev
```

- Si está en Windows, debe poder hacer lo mismo desde la terminal del WSL (Windows Subsystem for Linux), en caso contrario puede ejecutar:

```bash
dotnet watch run --project MoogleServer
```

## Implementación MoogleEngine

Estructura de la biblioteca de clases `MoogleEngine`.

### Procesamiento de las palabras del Corpus

Al iniciar el servidor, se llama al método `IndexCorpus` de la clase `Moogle`, el cual se encarga de leer los documentos, y crear un objeto de la clase `Document` para cada documento de la carpeta `Content`:
- La clase `Document` se encarga de procesar el texto contenido dentro del documento, separar por espacios y eliminar los signos de puntuación (mediante el método `SignPuntuation`).
- Para eliminar los signos de puntuación, se recorre la palabra desde el principio hasta el final y se guarda la posición del primer *char* alfanumérico, se realiza el mismo procedimiento en sentido inverso y se devuelve la porción de string determinada por los dos índices obtenidos. 
- En la clase `CorpusData` se almacena la información de cada una de las palabras en el diccionario `Vocabulary` que tiene como valor un objeto de la clase `DataStructure` donde se guarda: un arreglo con el peso ,un arreglo de listas de índices con las posiciones de la palabra en cada documento y la cantidad de documentos que contienen la palabra (estructurando los vectores documento).
- Inicialmente se guarda en `Vocabulary` la frecuencia de la palabra y la posición en el documento, comparando dicha frecuencia con la frecuencia máxima del documento. 
- Una vez terminado este proceso se calcula el *TF-IDF* de las palabras del corpus, mediante el método `TfIdfDoc` de la clase `Document` y se almacenan estos valores en `Vocabulary`.

### Procesamiento de la Query 

Cuando el usuario introduce una nueva *Query* se crea un objeto de la clase `QueryClass` y en dicha clase se extraen las palabras de la *Query*.

### Operadores

Dentro de la clase `QueryClass`, se toma el string que contiene a la *Query* y se procede a separar por espacios y a eliminar los signos de puntuación que no pertenezcan a la identificación de un operador:
- Se identifican los operadores de búsqueda mediante el método `Operators`. 
- Se agrega al proyecto un nuevo operador de búsqueda: *Búsqueda Literal*, cuya explicación aparece en la descripción del proyecto.
- Para el operador de cercanía se considera la distancia entre `A~B~C` como la mínima ventana del texto que contiene a `A`, `B` y `C` en cualquier orden.
- Una vez identificados los operadores se procede a comprobar la existencia de las palabras de la *Query* en el corpus, en caso contrario, se llama al método `Suggestion`. 

### Sugerencia

- En el método `Suggestion` se llama al método `SuggestionWord`, donde se busca la palabra del corpus con la mínima cantidad de cambios con respecto a la palabra para la cual se quiere obtener la sugerencia, para ello se emplea la *Distancia de Levenstein*, la cual consiste en dar un costo a las operaciones que permiten convertir una palabra en otra: *Eliminación*, *Inserción* y *Sustitución* de una letra. 
- La implementación de la *Distancia de Levenstein* está basada en un algoritmo de programación dinámica (donde en cada estado se decide entre la operación con menor costo), además tiene un grupo de modificaciones que otorgan un menor costo a los errores ortográficos más comunes.  
- En caso de obtener dos palabras con la misma cantidad de cambios, se guarda la que mayor peso tenga entre todos los documentos del Corpus.
- Una vez identificada la palabra, en el método `Suggestion` se busca la porción del string `SuggestionQuery` (inicialmente tiene el mismo valor que el string de la *Query*), que contiene la palabra para la cual queremos dar la sugerencia y se sustituye por la nueva palabra.

### Raíces y Sinónimos

Para obtener mejores resultados en la búsqueda se identifican las palabras que posean las mismas raíces o el mismo significado que las de la *Query*:
- En la clase `Snowball`, está implementado un algoritmo que se encarga de realizar el stemming en español, el cual se apoya en tres reglas y un conjunto de pasos y sufijos  mediante los cuales se obtiene el lexema aproximado de la palabra.
- Para hallar los sinónimos se emplea un diccionario de sinónimos para el español, estructurado en la lista `Synonymous` de la clase `CorpusData`, la cual tiene una lista de arreglos de string, donde cada arreglo contiene un grupo de palabras con similar significado.

### TF-IDF Query

En la clase `QueryClass` tenemos los diccionarios `WordsQuery` y `WordsStemmingSyn`, los cuales guardan las palabras de la *Query* y las palabras que resultan de la búsqueda de las palabras con la misma raíz y el mismo significado que las de la *Query*, ambos grupos de palabras con su frecuencia respectivamente.
- Se procede a calcular el *TF-IDF* de las palabras de `WordsQuery` y `WordsStemmingSyn` (en el caso de los sinónimos se divide su frecuencia entre dos, para que posean menos peso las palabras que tienen la misma raíz) y almacenar el resultado en el propio diccionario respectivamente.
Nota: Aquí se hace una diferenciación entre dos *Querys*, una con las palabras originales y otra para las palabras con la misma raíz y el mismo significado. 
- En el caso del operador *Mayor Relevancia*, a la hora de calcular el *TF-IDF*, de las palabras que poseen dicho operador, se multiplica la frecuencia de la palabra por el número e, elevado a la cantidad de asteriscos más uno que posea la palabra.

### Resultados de la Búsqueda

Por cada documento se crea un objeto `DocumentResult` y se analizan los requisitos de este con respecto a la búsqueda.

### Ranking de los Documentos

Para dar el ranking de los documentos se emplea *Similitud del Coseno*, mencionada en la descripción del proyecto:
- En el método `SimVectors` se comparan los datos del vector documento (correspondiente al documento analizado) almacenados en el diccionario `Vocabulary` de la clase `CorpusData`, con los valores del vector consulta almacenados en  el diccionario `WordsQuery` de la clase `QueryClass`. 
- En caso de que el documento no contenga ninguna de las palabras de `WordsQuery`, se realiza el procedimiento anterior pero ahora se emplea los datos del diccionario `WordsStemmingSyn`, si los resultados son positivos se suma al *Score* del documento el mínimo valor de double (con esto se garantiza que los documentos que solo poseen palabras con la misma raíz y el mismo significado que las de la *Query* siempre sean devueltos por debajo de los que contienen al menos una palabra de la *Query*).
- Luego se calcula el *Score* del documento.

### Influencia de los Operadores en la Búsqueda

Se determina si el documento cumple con los parámetros de los operadores mediante el método `ResultSearch`.
- Para tener en cuenta las condiciones de los operadores `Close` y `SearchLiteral`, se emplea la clase `DistanceWord` donde está el método `DistanceClose` que devuelve la mínima distancia entre una lista de palabras en un determinado documento y el método `DistanceLiteral` que se encarga de buscar en un documento y determinar la posición de las palabras que están especificadas en el operador `SearchLiteral`.
- En el caso del operador `Close` se le suma al *Score* del documento *100/n* donde *n* es la mínima distancia entre el grupo de palabras que conforman el operador.

### Snippet

- Se construye el *Snippet* del documento mediante el método `Snippet`, si hay resultados del operador `SearchLiteral` se muestra una línea por cada grupo de palabras de dicho operador. Por otro lado se define un tamaño máximo de 20 palabras para cada línea, luego se llama al método `DistanceSnippet` de la clase `DistanceWord`, el cual determina el máximo número de palabras resultantes de la búsqueda que ocupan una ventana del texto de tamaño 20 y las posiciones en que estas se encuentran, si todas estas palabras no fueron contenidas en dicha ventana se realiza el mismo procedimiento con las restantes, hasta obtener como máximo 5 *Snippets* por documento.
- Con las posiciones obtenidas en el método `Snippet`, se lee el documento y se guarda el texto contenido en dichas posiciones mediante el método `BuildSinipped`.

### Resultados Obtenidos

- Una vez concluida la búsqueda, se comprueba que la sugerencia hecha al usuario es válida y se construye el objeto `SearchResult` que devuelve el método `Query` de la clase `Moogle`, mediante una lista de objetos `SearchItem`, que contiene un arreglo con las líneas del *Snippet*, las posiciones de dichas líneas en el documento y la lista de palabras de la *Query* que no fueron encontradas en el documento.

### Implementación para la mínima distancia entre un grupo de palabras

Para determinar la mínima distancia entre una lista de palabras se utiliza el algoritmo *Sliding Window*, empleando las posiciones de las palabras en el documento, guardadas en el diccionario `Vocabulary` de la clase `CorpusData`, además de los métodos agrupados en la clase `DistanceWord`.
- En el método `ListPosWords` se crea un arreglo de tuplas por cada palabra que contiene el índice de la palabra en la lista de palabras y la posición del documento, mediante el método `BuildTuple`, luego se ordena dicho arreglo por el valor de las posiciones de las palabras mediante el método `Sorted` y se determina la cantidad de ocurrencias de la palabra en la lista de palabras, mediante el método `OcurrenceWord`.
- En el método `SearchDistanceWords` se utiliza el arreglo de tuplas obtenido en el paso anterior y se emplea una cola en la que se van introduciendo los elementos del array hasta que todas las palabras estén contenidas en la cola, una vez hecho esto se intenta sacar de la cola sin que se deje de cumplir que todas las palabras están contenidas, terminado este proceso se calcula la distancia entre las palabras que se encuentran en los extremos de la cola, se compara el valor con el que se tenía calculado, se guarda el mínimo y se vuelve a repetir el procedimiento.

```cs
//Recorremos el array buscando la minima ventana q contenga a todas las palabras
for (int i = 0; i < PosWordsSorted.Length; i++)
{
    searchMinDist.Enqueue(PosWordsSorted[i]);
    posList[PosWordsSorted[i].Item1]++;
    if(posList[PosWordsSorted[i].Item1]==ocurrence[PosWordsSorted[i].Item1]) contains++;
    if (contains == minWords)
    {
    //Si la cantidad de palabras correcta esta en la cola tratamos de ver cuantas podemos sacar
        (int, int) eliminate;
        while(true)
        {
            //Buscamos la posible palabra a eliminar de la cola
            eliminate = searchMinDist.Peek();
            if(posList[eliminate.Item1]==ocurrence[eliminate.Item1]) break;
            else
            {
                searchMinDist.Dequeue();
                posList[eliminate.Item1]--;
            }
        }
        //Comprobamos si la distancia obtenida es menor que la q teniamos
        if (PosWordsSorted[i].Item2 - searchMinDist.Peek().Item2 + 1 < minDist)
        {
            WordsNotRange(words,posList,ocurrence,wordsNotRange);
            pos = searchMinDist.Peek().Item2;
            minDist = PosWordsSorted[i].Item2 - searchMinDist.Peek().Item2 + 1;
        }
        else if (PosWordsSorted[i].Item2 - searchMinDist.Peek().Item2 + 1 == minDist)
        {
            Random random = new Random();
            if (random.Next(2) == 0)
            {
                WordsNotRange(words,posList,ocurrence,wordsNotRange);
                pos = searchMinDist.Peek().Item2;
            }
        }
        //Sacamos de la cola
        contains--;
        posList[eliminate.Item1]--;
        searchMinDist.Dequeue();
    }
}
```

### Implementación para la Búsqueda Literal

Para determinar si en el documento existe una ventana de texto exactamente con las palabras de la lista, se utilizan los métodos de la clase `DistanceWord`.
- Se emplea el método `ListPosWords`, descrito anteriormente, pero en este caso se tiene en cuenta la posible presencia de los comodines `?` en la lista de palabras, se recorre la lista de palabras y se lleva un contador con la cantidad de comodines, ahora en cada momento de formar la tupla se resta la cantidad de comodines al índice y a la posición de la palabra.
- Se recorre el arreglo de tuplas, mediante el método `DistanceLiteral`, llevando un contador que muestra el índice de la palabra que corresponde en cada momento. Mientras la posición sea la misma significa que la palabra es igual a la anterior, por lo que se almacenan los índices en una lista, si se encuentra una posición diferente se comprueba que la nueva posición sea consecutiva con la anterior y que alguno de los índices almacenados en la lista coincida con el de la palabra que corresponde, en caso afirmativo se aumenta en uno el índice de la palabra a buscar y se repite el procedimiento, en caso contrario se reinicia el índice de la palabra a buscar.
- Si en algún momento el índice de la palabra a buscar coincide con la última palabra se guarda la posición donde se encontró la porción de texto correspondiente.

```cs
pubic static int DistanceLiteral(List<string> words,Document document)
{
    ((int,int)[],int) aux = ListPosWords(words,document);
    (int,int)[] PosWordsSorted = aux.Item1;
    int posRnd=aux.Item2;
    int ind=0;
    int pos=PosWordsSorted[0].Item2;
    int posLiteral=-1;
    List<int> indWord=new List<int>();
    bool literal=false;
    //Recorremos la posiciones de la palabras
    for(int i=0;i<PosWordsSorted.Length;i++)
    {
        //Si encontramos dos indices iguales estamos en presencia de la misma palabra
        if(pos==PosWordsSorted[i].Item2) indWord.Add(PosWordsSorted[i].Item1);
        else
        {
            //Si encontramos una posicion diferente revisamos si es correcto el indice de las palabras que son iguales
            if(LiteralIndex(words,indWord,ref posRnd,ref ind,ref pos,ref posLiteral,ref literal) )
            {
                //Si la posicion actual es el sucesor de la anterior avanzamos un indice
                if(PosWordsSorted[i].Item2==pos+1) ind++;
                else ind=0;
            }
            else ind=0; 
            //Como hemos encontrado una palabra diferente creamos una nueva lista de indices
            indWord=new List<int>();
            indWord.Add(PosWordsSorted[i].Item1);
            pos=PosWordsSorted[i].Item2;
        }
    }
    LiteralIndex(words,indWord,ref posRnd,ref ind,ref pos,ref posLiteral,ref literal);
    //Si hemos encontrado una posicion correcta la devolvemos de lo contrario devlovemos -1
    if(literal) return posLiteral;
    return -1;
}
```

## Implementación MoogleServer 

Se añade al proyecto una nueva página `Doc.razor`, donde se le brinda al usuario la opción de poder visualizar el documento directamente desde el navegador.

### AutoCompletar

- Para el autocompletamiento se añade el evento `bind:event="oninput"` el cual permite actualizar el valor del string `query` cada vez que el usuario teclea o borra un nuevo carácter
- Se emplea el evento `onkeyup` que llama al método `Press`, el cual identifica la última porción de palabra tecleada por el usuario y llama al método `AutoComplete` de la clase `Server` el cual devuelve como máximo las 5 palabras del corpus más cercanas a completar el texto escrito por el usuario.
- Una vez obtenidas las palabras para autocompletar se utiliza un `datalist` para mostrárselas al usuario.

### Suggestion

- Se utiliza el evento `onclick` que llama al método `Suggestion` el cual permite realizar una nueva consulta con la *Query* `suggestion`.

### Visualizar el Documento

- Cada etiqueta `Tittle` y `Snippet` contiene un enlace a la página `Doc.razor`, la cual recibe como parámetros el título del documento, la posición de la línea y la página donde se encuentra el *Snippet*.
- La página `Doc.razor` llama al método `Read` de la clase `Server` el cual devuelve las 100 líneas de la página del documento que se quiere mostrar. Además está implementada la posibilidad de visualizar la página anterior, la siguiente y cualquier página del documento a la que quiera acceder el usuario.