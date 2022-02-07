namespace MoogleEngine;

public static class Distance_Word
{
    public enum Distance
    {
        Snippet, 
        SearchLiteral,
        Close,
    }
    /// <summary>Metodo para buscar distancia entre palabras</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <param name="d">Referencia a la estructura donde se busca la cercania</param>
    public static Tuple<int, int, List<string>> Search_Distance(List<string> words,Document document,Distance d)
    {
        switch(d)
        {
            case Distance.Snippet: 
                return Shortest_Distance_Word(words, document, 1, QueryClass.Snippet_len,false);
            case Distance.SearchLiteral:
                return Shortest_Distance_Word(words, document, words.Count, 10, true);
            default:
                return Shortest_Distance_Word(words, document, words.Count, 10, false);       
        }
    }
    /// <summary>Metodo para encontrar la minima distancia de una lista de palabras</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <param name="cantmin">Cantidad minima de palabras de la lista en las q queremos buscar la minima distancia</param>
    /// <param name="cota">Maxima distancia que podemos encontrar entre las palabras</param>
    /// <param name="SearchLiteral">Presencia de la busqueda literal</param>
    private static Tuple<int, int, List<string>> Shortest_Distance_Word(List<string> words, Document document, int cantmin, int cota, bool SearchLiteral)
    {
        List<int> l = new List<int>();
        List<int> index_words_in_range = new List<int>();
        //Guardamos en un arreglo de tuplas las posiciones de las palabras donde el primer indice corresponde al indice de la palabra en la lista 
        //Ordenamos los elementos de los arrays de tuplas por el numero de la posicion de la palabra
        Tuple<int, int>[] Pos_words_Sorted = new Tuple<int, int>[0];
        for (int i = 0; i < words.Count; i++)
        {
            Pos_words_Sorted = Sorted(Pos_words_Sorted, BuildTuple(BuildIndex.dic[words[i]].Pos_doc[document.index], i));
        }
        int minDist = int.MaxValue;
        int pos = 0;
        Queue<Tuple<int, int>> search_min_dist;
        int[] posList = new int[words.Count];
        //Recorremos la cantidad de palabras en la que queremos encontrar la minima distancia
        for (int j = words.Count; j >= cantmin; j--)
        {
            search_min_dist = new Queue<Tuple<int, int>>();
            posList = new int[words.Count];
            for (int i = 0; i < Pos_words_Sorted.Length; i++)
            {
                search_min_dist.Enqueue(Pos_words_Sorted[i]);
                posList[Pos_words_Sorted[i].Item1]++;
                Tuple<bool, List<int>> all = AllContains(posList, j);
                if (all.Item1)
                {
                    //Si la cantidad de palabras correcta esta en la cola tratamos de ver cuantas podemos sacar
                    l = all.Item2;
                    while (true)
                    {
                        //Buscamos la posible palabra a eliminar de la cola
                        Tuple<int, int> eliminate = search_min_dist.Peek();
                        posList[eliminate.Item1]--;
                        Tuple<bool, List<int>> tuple = AllContains(posList, j);
                        if (tuple.Item1)
                        {
                            //Si la cantidad de palabras correctas en la cola no se altera sacamos la palabra de la cola
                            l = tuple.Item2;
                            search_min_dist.Dequeue();
                        }
                        else
                        {
                            posList[eliminate.Item1]++;
                            break;
                        }
                    }
                    //Si estamos en precencia de la busqueda literal comprobamos el orden de las palabras en la cola
                    bool orden = true;
                    if (SearchLiteral)
                    {
                        int comp = -1;
                        if (search_min_dist.Count == words.Count)
                        {
                            foreach (var m in search_min_dist)
                            {
                                if (m.Item1 <= comp)
                                {
                                    orden = false;
                                    break;
                                }
                                comp = m.Item1;
                            }
                        }
                    }
                    //Comprobamos si la distancia obtenida es menor que la q teniamos
                    if (orden && Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1 < minDist)
                    {
                        index_words_in_range = l;
                        minDist = Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1;
                        pos = search_min_dist.Peek().Item2;
                    }
                    if (orden && Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1 == minDist)
                    {
                        Random random = new Random();
                        if (random.Next(2) == 0)
                        {
                            index_words_in_range = l;
                            pos = search_min_dist.Peek().Item2;
                        }
                    }
                }
            }
            //Si la minima distancia enontrada es menor a la esperada paramos
            if (minDist <= cota) break;
        }
        //Guardamos los indices de las palabras q no fueron contenidas
        List<string> words_not_range = new List<string>();
        for (int i = 0; i < words.Count; i++)
        {
            if (!index_words_in_range.Contains(i))
            {
                words_not_range.Add(words[i]);
            }
        }
        //Retornamos una tupla con la posicion encontrada, la menor distancia y las palabras que no fueron contenidas
        return new Tuple<int, int, List<string>>(pos, minDist, words_not_range);
    }
    ///<sumary>Mezcla ordenada de las tuplas correspondientes a las posiciones de las palabras</sumary>
    /// <param name="a">Array de tuplas</param>
    /// <param name="b">Array de tuplas</param>
    private static Tuple<int, int>[] Sorted(Tuple<int, int>[] a, Tuple<int, int>[] b)
    {
        Tuple<int, int>[] c = new Tuple<int, int>[a.Length + b.Length];
        int i = 0; int j = 0;
        while (i < a.Length && j < b.Length)
        {
            if (a[i].Item2 < b[j].Item2)
            {
                c[i + j] = a[i];
                i++;
            }
            else
            {
                c[i + j] = b[j];
                j++;
            }
        }
        while (i < a.Length)
        {
            c[i + j] = a[i];
            i++;
        }
        while (j < b.Length)
        {
            c[i + j] = b[j];
            j++;
        }
        return c;
    }
    ///<sumary>Metodo para crear las tuplas de las posiciones de las palabras</sumary>
    /// <param name="words_pos">Lista de posiciones de la palabra</param>
    /// <param name="index">Indice de la palabra</param>
    private static Tuple<int, int>[] BuildTuple(List<int> words_pos, int index)
    {
        Tuple<int, int>[] tuple = new Tuple<int, int>[words_pos.Count];
        for (int i = 0; i < tuple.Length; i++)
        {
            Tuple<int, int> t1 = new Tuple<int, int>(index, words_pos[i]);
            tuple[i] = t1;
        }
        return tuple;
    }
    ///<sumary>Metodo para determinar si la cantidad de palabras correcta esta contenida en la cola</sumary>
    /// <param name="posList">Frecuencia de cada palabra en la cola</param>
    /// <param name="cant">Frecuencia de cada palabra en la cola</param>
    private static Tuple<bool, List<int>> AllContains(int[] posList, int cant)
    {
        List<int> words_in_range = new List<int>();
        for (int i = 0; i < posList.Length; i++)
        {
            //Contamos la cantidad de palabras cuya frecuencia es diferente de 0
            if (posList[i] != 0)
            {
                //Lista de indice de las palabras contenidas en la cola
                words_in_range.Add(i);
            }
        }
        if (words_in_range.Count < cant) return new Tuple<bool, List<int>>(false, words_in_range);
        return new Tuple<bool, List<int>>(true, words_in_range);
    }
}