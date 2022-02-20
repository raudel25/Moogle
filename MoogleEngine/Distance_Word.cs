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
    /// <returns>Tupla con la posicion, la minima distancia y las palabras que no fueron contenidas</returns>
    public static Tuple<int, int, List<string>> Search_Distance(List<string> words,Document document,Distance d)
    {
        switch(d)
        {
            case Distance.Snippet: 
                return Shortest_Distance_Word(words, document, 1, QueryClass.Snippet_len);
            case Distance.SearchLiteral:
                return Literal(words,document);
            default:
                return Shortest_Distance_Word(words, document, words.Count, 10);       
        }
    }   
    /// <summary>Metodo para determinar la busqueda literal</summary>
    /// <param name="words">Lista de palabras para busqueda literal</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <returns>Tupla con la posicion de la busqueda literal</returns>
    private static Tuple<int, int, List<string>> Literal(List<string> words,Document document)
    {
        //Guardamos en un arreglo de tuplas las posiciones de las palabras donde el primer indice corresponde al indice de la palabra en la lista 
        //Ordenamos los elementos de los arrays de tuplas por el numero de la posicion de la palabra
        Tuple<int, int>[] Pos_words_Sorted = new Tuple<int, int>[0];
        int pos_rnd=0;
        for (int i = 0; i < words.Count; i++)
        {
            //Comprobamos la presencia de un comodin
            if(words[i]=="?")
            {
                pos_rnd++;
                continue;
            }
            Pos_words_Sorted = Sorted(Pos_words_Sorted, BuildTuple(Corpus_Data.vocabulary[words[i]].Pos_doc[document.index], i,pos_rnd));
        }
        int ind=0;
        int pos=Pos_words_Sorted[0].Item2;
        int pos_literal=-1;
        List<int> ind_word=new List<int>();
        bool literal=false;
        //Recorrmos la posiciones de la palabras
        for(int i=0;i<Pos_words_Sorted.Length;i++)
        {
            //Si encontramos dos indices iguales estamos en presencia de la misma palabra
            if(pos==Pos_words_Sorted[i].Item2) ind_word.Add(Pos_words_Sorted[i].Item1);
            else
            {
                //Si encontramos un indice diferente revisamos si es correcto el indice de las palabras que sin iguales
                if(ind_word.Contains(ind) )
                {
                    //Si llegamos al ultimo indice encontramos una posicion correcta
                    if(ind==words.Count-1-pos_rnd)
                    {
                        if(pos_literal==-1) pos_literal=pos-words.Count+1+pos_rnd;
                        else
                        {
                            Random rnd=new Random();
                            if(rnd.Next(2)==0) pos_literal=pos-words.Count+1+pos_rnd;
                        }
                        literal=true;
                        ind=0;
                    }
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
        if(ind_word.Contains(ind) )
        {
            if(ind==words.Count-1-pos_rnd)
            {
                if(pos_literal==-1) pos_literal=pos-words.Count+1+pos_rnd;
                else
                {
                    Random rnd=new Random();
                    if(rnd.Next(2)==0) pos_literal=pos-words.Count+1+pos_rnd;
                }
                literal=true;
            }
        }
        //Si hemos encontrado una posicion correcta la devolvemos de lo contrario devlovemos -1
        if(literal) return new Tuple<int, int, List<string>>(pos_literal,words.Count,new List<string>());
        return new Tuple<int, int, List<string>>(-1,0,new List<string>());
    }
    /// <summary>Metodo para encontrar la minima distancia de una lista de palabras</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <param name="cantmin">Cantidad minima de palabras de la lista en las q queremos buscar la minima distancia</param>
    /// <param name="cota">Maxima distancia que podemos encontrar entre las palabras</param>
    /// <returns>Tupla con la posicion, la minima distancia y las palabras que no fueron contenidas</returns>
    private static Tuple<int, int, List<string>> Shortest_Distance_Word(List<string> words, Document document, int cantmin, int cota)
    {
        List<int> list_aux = new List<int>();
        List<int> index_words_in_range = new List<int>();
        int[] ocurrence=new int[words.Count];
        Ocurrence_word(words,ocurrence);
        //Guardamos en un arreglo de tuplas las posiciones de las palabras donde el primer indice corresponde al indice de la palabra en la lista 
        //Ordenamos los elementos de los arrays de tuplas por el numero de la posicion de la palabra
        Tuple<int, int>[] Pos_words_Sorted = new Tuple<int, int>[0];
        for (int i = 0; i < words.Count; i++)
        {
            Pos_words_Sorted = Sorted(Pos_words_Sorted, BuildTuple(Corpus_Data.vocabulary[words[i]].Pos_doc[document.index], i));
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
                Tuple<bool, List<int>> all = AllContains(posList, j,ocurrence);
                if (all.Item1)
                {
                    //Si la cantidad de palabras correcta esta en la cola tratamos de ver cuantas podemos sacar
                    list_aux = all.Item2;
                    while (true)
                    {
                        //Buscamos la posible palabra a eliminar de la cola
                        Tuple<int, int> eliminate = search_min_dist.Peek();
                        posList[eliminate.Item1]--;
                        Tuple<bool, List<int>> tuple = AllContains(posList, j, ocurrence);
                        if (tuple.Item1)
                        {
                            //Si la cantidad de palabras correctas en la cola no se altera sacamos la palabra de la cola
                            list_aux = tuple.Item2;
                            search_min_dist.Dequeue();
                        }
                        else
                        {
                            posList[eliminate.Item1]++;
                            break;
                        }
                    }
                    //Comprobamos si la distancia obtenida es menor que la q teniamos
                    if (Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1 < minDist)
                    {
                        index_words_in_range = list_aux;
                        minDist = Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1;
                        pos = search_min_dist.Peek().Item2;
                    }
                    if (Pos_words_Sorted[i].Item2 - search_min_dist.Peek().Item2 + 1 == minDist)
                    {
                        Random random = new Random();
                        if (random.Next(2) == 0)
                        {
                            index_words_in_range = list_aux;
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
    ///<summary>Mezcla ordenada de las tuplas correspondientes a las posiciones de las palabras</summary>
    /// <param name="words1">Array de tuplas</param>
    /// <param name="words2">Array de tuplas</param>
    /// <returns>Array ordenado</returns>
    private static Tuple<int, int>[] Sorted(Tuple<int, int>[] words1, Tuple<int, int>[] words2)
    {
        Tuple<int, int>[] words3 = new Tuple<int, int>[words1.Length + words2.Length];
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
    private static Tuple<int, int>[] BuildTuple(List<int> words_pos, int index,int pos_rnd=0)
    {
        Tuple<int, int>[] tuple = new Tuple<int, int>[words_pos.Count];
        for (int i = 0; i < tuple.Length; i++)
        {
            Tuple<int, int> t1 = new Tuple<int, int>(index-pos_rnd, words_pos[i]-pos_rnd);
            tuple[i] = t1;
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
            for(int j=i;j<words.Count;j++)
            {
                if(words[i]==words[j]) cant++;
            }
            for(int j=i;j<words.Count;j++)
            {
                if(words[i]==words[j]) ocurrence[j]=cant;
            }
        }
    }
    /// <summary>Metodo para determinar si la cantidad de palabras correcta esta contenida en la cola</summary>
    /// <param name="posList">Frecuencia de cada palabra en la cola</param>
    /// <param name="cant">Frecuencia de cada palabra en la cola</param>
    /// <returns>Un bool que indica si esta la minima cantidad de palabras contenidas y una lista con estas palabras</returns>
    private static Tuple<bool, List<int>> AllContains(int[] posList, int cant,int[] ocurrence)
    {
        List<int> words_in_range = new List<int>();
        for (int i = 0; i < posList.Length; i++)
        {
            //Contamos la cantidad de palabras cuya frecuencia es diferente de 0
            if (posList[i] >= ocurrence[i])
            {
                //Lista de indice de las palabras contenidas en la cola
                words_in_range.Add(i);
            }
        }
        if (words_in_range.Count < cant) return new Tuple<bool, List<int>>(false, words_in_range);
        return new Tuple<bool, List<int>>(true, words_in_range);
    }
}