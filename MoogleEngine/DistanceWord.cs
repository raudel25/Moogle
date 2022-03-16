namespace MoogleEngine;

public static class DistanceWord
{
    /// <summary>Metodo para determinar la busqueda literal</summary>
    /// <param name="words">Lista de palabras para busqueda literal</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <returns>Posicion de la busqueda literal</returns>
    public static int DistanceLiteral(List<string> words,Document document)
    {
        ((int,int)[],int) aux = ListPosWords(words,document);
        (int,int)[] PoswordsSorted = aux.Item1;
        int posRnd=aux.Item2;
        int ind=0;
        int pos=PoswordsSorted[0].Item2;
        int posLiteral=-1;
        List<int> indWord=new List<int>();
        bool literal=false;
        //Recorremos la posiciones de la palabras
        for(int i=0;i<PoswordsSorted.Length;i++)
        {
            //Si encontramos dos indices iguales estamos en presencia de la misma palabra
            if(pos==PoswordsSorted[i].Item2) indWord.Add(PoswordsSorted[i].Item1);
            else
            {
                //Si encontramos una posicion diferente revisamos si es correcto el indice de las palabras que son iguales
                if(LiteralIndex(words,indWord,ref posRnd,ref ind,ref pos,ref posLiteral,ref literal) )
                {
                    //Si la posicion actual es el sucesor de la anterior avanzamos un indice
                    if(PoswordsSorted[i].Item2==pos+1) ind++;
                    else ind=0;
                }
                else ind=0; 
                //Como hemos encontrado una palabra diferente creamos una nueva lista de indices
                indWord=new List<int>();
                indWord.Add(PoswordsSorted[i].Item1);
                pos=PoswordsSorted[i].Item2;
            }
        }
        LiteralIndex(words,indWord,ref posRnd,ref ind,ref pos,ref posLiteral,ref literal);
        //Si hemos encontrado una posicion correcta la devolvemos de lo contrario devlovemos -1
        if(literal) return posLiteral;
        return -1;
    }
    /// <summary>Distancia para el operador de Cercania</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <returns>Minima distancia entre las palabras del operador</returns>
    public static int DistanceClose(List<string> words,Document document)
    {
        int[] ocurrence=new int[words.Count];
        (int, int)[] PoswordsSorted = ListPosWords(words,document,ocurrence).Item1;
        return SearchDistanceWords(words,PoswordsSorted,ocurrence,words.Count).Item1;
    } 
    /// <summary>Metodo para encontrar el Snippet mas preciso, con las palabras de la query</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <returns>Tupla con la posicion y las palabras que no fueron contenidas</returns>
    public static (int, List<string>) DistanceSnippet(List<string> words, Document document)
    {
        List<string> wordsNotRange = new List<string>();
        int[] ocurrence=new int[words.Count];
        (int, int)[] PoswordsSorted = ListPosWords(words,document,ocurrence).Item1;
        int minDist = int.MaxValue;
        int pos = 0;
        int[] posList = new int[words.Count];
        int start = Math.Min(words.Count,DocumentResult.SnippetLen);
        //Buscamos la maxima cantidad de palabras a contener en la ventana de texto
        for (int j=start; j >= 1; j--)
        {      
            (int,int) aux = SearchDistanceWords(words,PoswordsSorted,ocurrence,j,wordsNotRange);
            minDist=aux.Item1;
            pos=aux.Item2;
            //Si la minima distancia enontrada es menor a la esperada paramos
            if (minDist <= DocumentResult.SnippetLen) break;
        }
        //Retornamos una tupla con la posicion encontrada, la menor distancia y las palabras que no fueron contenidas
        return (pos, wordsNotRange);
    }
    /// <summary>Metodo para encontrar la minima distancia de una lista de palabras</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="PoswordsSorted">Lista de indices de las palabras ya ordendas</param>
    /// <param name="ocurrence">Cantidad minima de apariciones de la palabra</param>
    /// <param name="minWords">Minima cantidad de palabras que debe tener la ventana de texto</param>
    /// <param name="wordsNotRange">Palabras q no fueron encontradas en la ventana del texto</param>
    /// <returns>Tupla con la minima distancia entre las palabras y la posicion</returns>
    private static (int,int) SearchDistanceWords(List<string> words,(int,int)[] PoswordsSorted,int[] ocurrence, int minWords, List<string> wordsNotRange = null!)
    {
        int minDist=int.MaxValue;
        int pos=0;
        int contains=0;
        Queue<(int, int)> searchMinDist = new Queue<(int, int)>();
        int[] posList = new int[words.Count];
        //Recorremos el array buscando la minima ventana q contenga a todas las palabras
        for (int i = 0; i < PoswordsSorted.Length; i++)
        {
            searchMinDist.Enqueue(PoswordsSorted[i]);
            posList[PoswordsSorted[i].Item1]++;
            if(posList[PoswordsSorted[i].Item1]==ocurrence[PoswordsSorted[i].Item1]) contains++;
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
                if (PoswordsSorted[i].Item2 - searchMinDist.Peek().Item2 + 1 < minDist)
                {
                    WordsNotRange(words,posList,ocurrence,wordsNotRange);
                    pos = searchMinDist.Peek().Item2;
                    minDist = PoswordsSorted[i].Item2 - searchMinDist.Peek().Item2 + 1;
                }
                else if (PoswordsSorted[i].Item2 - searchMinDist.Peek().Item2 + 1 == minDist)
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
        return (minDist,pos);
    }
    /// <summary>Crear un array ordenado con las posiciones de la palabra</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <param name="ocurrence">Cantidad minima de apariciones de la palabra</param>
    /// <returns>Arreglo de tuplas ordenadas con el indice y la posicion de la palabra</returns>
    private static ((int,int)[],int) ListPosWords(List<string> words,Document document, int[] ocurrence = null!)
    {
        int posRnd=0;
        if(ocurrence != null) OcurrenceWord(words,ocurrence);
        //Guardamos en un arreglo de tuplas las posiciones de las palabras
        //Ordenamos los elementos de los arrays de tuplas por el numero de la posicion de la palabra
        (int, int)[] PoswordsSorted = new (int, int)[0];
        for (int i = 0; i < words.Count; i++)
        {
            if(ocurrence == null!)
            {
                //Comprobamos la presencia de un comodin
                if(words[i]=="?")
                {
                    posRnd++;
                    continue;
                }
            }
            PoswordsSorted = Sorted(PoswordsSorted, BuildTuple(CorpusData.Vocabulary[words[i]].PosDoc[document.Index], i,posRnd));
        }
        return (PoswordsSorted,posRnd);
    }
    /// <summary>Determinar si los indices de la busqueda literal son correctos</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="indWord">Lista de indices de las palabras</param>
    /// <param name="cantRnd">Cantidad de comodines</param>
    /// <param name="ind">Indice actual</param>
    /// <param name="pos">Posicion actual</param>
    /// <param name="posLiteral">Posicion a devolver de la Busqueda Literal</param>
    /// <param name="literal">Condiciones de Busqueda literal</param>
    /// <returns>Comprueba si esta contenido el indice que buscabamos</returns>
    private static bool LiteralIndex(List<string> words,List<int> indWord,ref int cantRnd,ref int ind,ref int pos,ref int posLiteral,ref bool literal)
    {
        if(indWord.Contains(ind))
        {
            //Si llegamos al ultimo indice encontramos una posicion correcta
            if(ind==words.Count-1-cantRnd)
            {
                if(pos-words.Count+1+cantRnd >= 0)
                {
                    if(posLiteral == -1) posLiteral=pos-words.Count+1+cantRnd;
                    else
                    {
                        Random rnd=new Random();
                        if(rnd.Next(2)==0) posLiteral=pos-words.Count+1+cantRnd;
                    }
                    literal=true;
                }
                ind=0;
            }
            return true;
        }
        return false;
    }
    /// <summary>Hallar las palabras que no fueron contenidas en la ventana de texto</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="posList">Cantidad de apariciones de la palabra</param>
    /// <param name="ocurrence">Cantidad minima de apariciones de la palabra</param>
    /// <param name="wordsNotRange">Palabras q no fueron encontradas en la ventana del texto</param>
    private static void WordsNotRange(List<string> words,int[] posList, int[] ocurrence, List<string> wordsNotRange)
    {
        if(wordsNotRange==null!) return;
        wordsNotRange.Clear();
        for(int u=0;u<words.Count;u++)
        {
            if(posList[u] < ocurrence[u]) wordsNotRange.Add(words[u]);
        }
    }
    ///<sumary>Mezcla ordenada de las tuplas correspondientes a las posiciones de las palabras</summary> 
    /// <paam name="words1">Array de tuplas</param>
    /// <paam name="words2">Array de tuplas</param>
    /// <returns>Array ordenado</returns>
    private static (int, int)[] Sorted((int, int)[] words1, (int, int)[] words2)
    {
        (int, int)[] words3 = new (int, int)[words1.Length + words2.Length];
        int i = 0; int j = 0;
        while (i < words1.Length && j < words2.Length)
        {
            if (words1[i].Item2 <= words2[j].Item2)
            {
                words3[i + j] = words1[i];
                i++;
            }
            else
            {
                words3[i + j] = words2[j];
                j++;
            }
        }
        while (i < words1.Length)
        {
            words3[i + j] = words1[i];
            i++;
        }
        while (j < words2.Length)
        {
            words3[i + j] = words2[j];
            j++;
        }
        return words3;
    }
    /// <summary>Metodo para crear las tuplas de las posiciones de las palabras</summary>
    /// <param name="wordsPos">Lista de posiciones de la palabra</param>
    /// <param name="index">Indice de la palabra</param>
    /// <param name="posRnd">Ajuste para la posicion y el indice en caso de existir un comodin</param>
    /// <returns>Tuplas con el indice de la palabra en la lista y la posiciion de la palabra</returns>
    private static (int, int)[] BuildTuple(List<int> wordsPos, int index,int posRnd=0)
    {
        (int, int)[] tuple = new (int, int)[wordsPos.Count];
        for (int i = 0; i < tuple.Length; i++)
        {
            tuple[i] = (index-posRnd, wordsPos[i]-posRnd);
        }
        return tuple;
    }
    /// <summary>Metodo para determinar la cantidad de ocurrencia de las palabras<summary>
    /// <param name="words">Lista de palabras</param>
    /// <param name="ocurrence">Cantidad de ocurrencias de la palabra</param>
    private static void OcurrenceWord(List<string> words, int[] ocurrence)
    {
        for(int i=0;i<words.Count;i++)
        {
            if(ocurrence[i]!=0) continue;
            int cant=0;
            //Contamos la cantidad de palabras iguales
            for(int j=i;j<words.Count;j++)
            {
                if(words[i]==words[j]) cant++;
            }
            //Asignamos esa cantidad a cada posicion donde esten las palabras iguales
            for(int j=i;j<words.Count;j++)
            {
                if(words[i]==words[j]) ocurrence[j]=cant;
            }
        }
    }
}