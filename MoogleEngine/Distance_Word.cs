namespace MoogleEngine;

public static class Distance_Word
{
    /// <summary>Metodo para determinar la busqueda literal</summary>
    /// <param name="words">Lista de palabras para busqueda literal</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <returns>Posicion de la busqueda literal</returns>
    public static int Distance_Literal(List<string> words,Document document)
    {
        ((int,int)[],int) aux = List_Pos_Words(words,document);
        (int,int)[] Pos_words_Sorted = aux.Item1;
        int pos_rnd=aux.Item2;
        int ind=0;
        int pos=Pos_words_Sorted[0].Item2;
        int pos_literal=-1;
        List<int> ind_word=new List<int>();
        bool literal=false;
        //Recorremos la posiciones de la palabras
        for(int i=0;i<Pos_words_Sorted.Length;i++)
        {
            //Si encontramos dos indices iguales estamos en presencia de la misma palabra
            if(pos==Pos_words_Sorted[i].Item2) ind_word.Add(Pos_words_Sorted[i].Item1);
            else
            {
                //Si encontramos una posicion diferente revisamos si es correcto el indice de las palabras que son iguales
                if(Literal_Index(words,ind_word,ref pos_rnd,ref ind,ref pos,ref pos_literal,ref literal) )
                {
                    //Si la posicion actual es el sucesor de la anterior avanzamos un indice
                    if(Pos_words_Sorted[i].Item2==pos+1) ind++;
                    else ind=0;
                }
                else ind=0; 
                //Como hemos encontrado una palabra diferente creamos una nueva lista de indices
                ind_word=new List<int>();
                ind_word.Add(Pos_words_Sorted[i].Item1);
                pos=Pos_words_Sorted[i].Item2;
            }
        }
        Literal_Index(words,ind_word,ref pos_rnd,ref ind,ref pos,ref pos_literal,ref literal);
        //Si hemos encontrado una posicion correcta la devolvemos de lo contrario devlovemos -1
        if(literal) return pos_literal;
        return -1;
    }
    /// <summary>Distancia para el operador de Cercania</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <returns>Minima distancia entre las palabras del operador</returns>
    public static int Distance_Close(List<string> words,Document document)
    {
        int[] ocurrence=new int[words.Count];
        (int, int)[] Pos_words_Sorted = List_Pos_Words(words,document,ocurrence).Item1;
        return Search_Distance_Words(words,Pos_words_Sorted,ocurrence,words.Count).Item1;
    } 
    /// <summary>Metodo para encontrar el Snippet mas preciso, con las palabras de la query</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <returns>Tupla con la posicion y las palabras que no fueron contenidas</returns>
    public static (int, List<string>) Distance_Snippet(List<string> words, Document document)
    {
        List<string> words_not_range = new List<string>();
        int[] ocurrence=new int[words.Count];
        (int, int)[] Pos_words_Sorted = List_Pos_Words(words,document,ocurrence).Item1;
        int minDist = int.MaxValue;
        int pos = 0;
        int[] posList = new int[words.Count];
        int start = Math.Min(words.Count,Document_Result.Snippet_len);
        //Buscamos la maxima cantidad de palabras a contener en la ventana de texto
        for (int j=start; j >= 1; j--)
        {      
            (int,int) aux = Search_Distance_Words(words,Pos_words_Sorted,ocurrence,j,words_not_range);
            minDist=aux.Item1;
            pos=aux.Item2;
            //Si la minima distancia enontrada es menor a la esperada paramos
            if (minDist <= Document_Result.Snippet_len) break;
        }
        //Retornamos una tupla con la posicion encontrada, la menor distancia y las palabras que no fueron contenidas
        return (pos, words_not_range);
    }
    /// <summary>Metodo para encontrar la minima distancia de una lista de palabras</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="Pos_words_Sorted">Lista de indices de las palabras ya ordendas</param>
    /// <param name="ocurrence">Cantidad minima de apariciones de la palabra</param>
    /// <param name="min_words">Minima cantidad de palabras que debe tener la ventana de texto</param>
    /// <param name="words_not_range">Palabras q no fueron encontradas en la ventana del texto</param>
    /// <returns>Tupla con la minima distancia entre las palabras y la posicion</returns>
    private static (int,int) Search_Distance_Words(List<string> words,(int,int)[] Pos_words_Sorted,int[] ocurrence, int min_words, List<string> words_not_range = null!)
    {
        int minDist=int.MaxValue;
        int pos=0;
        int contains=0;
        Queue<(int, int)> search_min_dist = new Queue<(int, int)>();
        int[] posList = new int[words.Count];
        //Recorremos el array buscando la minima ventana q contenga a todas las palabras
        for (int i = 0; i < Pos_words_Sorted.Length; i++)
        {
            search_min_dist.Enqueue(Pos_words_Sorted[i]);
            posList[Pos_words_Sorted[i].Item1]++;
            if(posList[Pos_words_Sorted[i].Item1]==ocurrence[Pos_words_Sorted[i].Item1]) contains++;
            if (contains == min_words)
            {
                //Si la cantidad de palabras correcta esta en la cola tratamos de ver cuantas podemos sacar
                (int, int) eliminate;
                while(true)
                {
                    //Buscamos la posible palabra a eliminar de la cola
                    eliminate = search_min_dist.Peek();
                    if(posList[eliminate.Item1]==ocurrence[eliminate.Item1]) break;
                    else
                    {
                        search_min_dist.Dequeue();
                        posList[eliminate.Item1]--;
                    }
                }
                //Comprobamos si la distancia obtenida es menor que la q teniamos
                if (Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1 < minDist)
                {
                    Words_Not_Range(words,posList,ocurrence,words_not_range);
                    pos = search_min_dist.Peek().Item2;
                    minDist = Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1;
                }
                else if (Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1 == minDist)
                {
                    Random random = new Random();
                    if (random.Next(2) == 0)
                    {
                        Words_Not_Range(words,posList,ocurrence,words_not_range);
                        pos = search_min_dist.Peek().Item2;
                    }
                }
                //Sacamos de la cola
                contains--;
                posList[eliminate.Item1]--;
                search_min_dist.Dequeue();
            }
        }
        return (minDist,pos);
    }
    /// <summary>Crear un array ordenado con las posiciones de la palabra</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <param name="ocurrence">Cantidad minima de apariciones de la palabra</param>
    /// <returns>Arreglo de tuplas ordenadas con el indice y la posicion de la palabra</returns>
    private static ((int,int)[],int) List_Pos_Words(List<string> words,Document document, int[] ocurrence = null!)
    {
        int pos_rnd=0;
        if(ocurrence != null) Ocurrence_word(words,ocurrence);
        //Guardamos en un arreglo de tuplas las posiciones de las palabras
        //Ordenamos los elementos de los arrays de tuplas por el numero de la posicion de la palabra
        (int, int)[] Pos_words_Sorted = new (int, int)[0];
        for (int i = 0; i < words.Count; i++)
        {
            if(ocurrence == null!)
            {
                //Comprobamos la presencia de un comodin
                if(words[i]=="?")
                {
                    pos_rnd++;
                    continue;
                }
            }
            Pos_words_Sorted = Sorted(Pos_words_Sorted, BuildTuple(Corpus_Data.Vocabulary[words[i]].Pos_Doc[document.Index], i,pos_rnd));
        }
        return (Pos_words_Sorted,pos_rnd);
    }
    /// <summary>Determinar si los indices de la busqueda literal son correctos</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="ind_word">Lista de indices de las palabras</param>
    /// <param name="cant_rnd">Cantidad de comodines</param>
    /// <param name="ind">Indice actual</param>
    /// <param name="pos">Posicion actual</param>
    /// <param name="pos_literal">Posicion a devolver de la Busqueda Literal</param>
    /// <param name="literal">Condiciones de Busqueda literal</param>
    /// <returns>Comprueba si esta contenido el indice que buscabamos</returns>
    private static bool Literal_Index(List<string> words,List<int> ind_word,ref int cant_rnd,ref int ind,ref int pos,ref int pos_literal,ref bool literal)
    {
        if(ind_word.Contains(ind))
        {
            //Si llegamos al ultimo indice encontramos una posicion correcta
            if(ind==words.Count-1-cant_rnd)
            {
                if(pos-words.Count+1+cant_rnd >= 0)
                {
                    if(pos_literal == -1) pos_literal=pos-words.Count+1+cant_rnd;
                    else
                    {
                        Random rnd=new Random();
                        if(rnd.Next(2)==0) pos_literal=pos-words.Count+1+cant_rnd;
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
    /// <param name="words_not_range">Palabras q no fueron encontradas en la ventana del texto</param>
    private static void Words_Not_Range(List<string> words,int[] posList, int[] ocurrence, List<string> words_not_range)
    {
        if(words_not_range==null!) return;
        words_not_range.Clear();
        for(int u=0;u<words.Count;u++)
        {
            if(posList[u] < ocurrence[u]) words_not_range.Add(words[u]);
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
    /// <param name="words_pos">Lista de posiciones de la palabra</param>
    /// <param name="index">Indice de la palabra</param>
    /// <param name="pos_rnd">Ajuste para la posicion y el indice en caso de existir un comodin</param>
    /// <returns>Tuplas con el indice de la palabra en la lista y la posiciion de la palabra</returns>
    private static (int, int)[] BuildTuple(List<int> words_pos, int index,int pos_rnd=0)
    {
        (int, int)[] tuple = new (int, int)[words_pos.Count];
        for (int i = 0; i < tuple.Length; i++)
        {
            tuple[i] = (index-pos_rnd, words_pos[i]-pos_rnd);
        }
        return tuple;
    }
    /// <summary>Metodo para determinar la cantidad de ocurrencia de las palabras<summary>
    /// <param name="words">Lista de palabras</param>
    /// <param name="ocurrence">Cantidad de ocurrencias de la palabra</param>
    private static void Ocurrence_word(List<string> words, int[] ocurrence)
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