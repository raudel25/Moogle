namespace MoogleEngine;
using System.IO;
using System.Text;
using System.Text.Json;

public static class Moogle
{
    static int Snippet_len = 20;
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda
        QueryClass query1 = new QueryClass(query);
        QueryIndex(query1);
        //Creamos un array y pasamos los resultados de la busqueda
        SearchItem[] items = new SearchItem[query1.Score.Count];
        for (int i = 0; i < query1.Score.Count; i++)
        {
            items[i] = new SearchItem(query1.resultsearchDoc[i].title, query1.SnippetResult[i], query1.Pos_SnippetResult[i], (float)query1.Score[i]);
        }
        return new SearchResult(items, query1.txt);
    }
    /// <summary>Metodo para indexar nuestro corpus</summary>
    public static void Indexar()
    {
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        Document.documents = new List<Document>();
        int q = 0;
        //Contamos la cantidad de documentos
        foreach (var i in list)
        {
            q++;
        }
        Document.cantdoc = q;
        Document.max = new double[q];
        //Inicializamos la frecuencia maximas de los documentos en 1
        for (int i = 0; i < Document.max.Length; i++)
        {
            Document.max[i] = 1;
        }
        q = 0;
        foreach (var i in list)
        {
            Document d1 = new Document(File.ReadAllLines(i), i, q);
            Document.documents.Add(d1);
            q++;
        }
        //Deserializamos nuestra base de datos de sinonimos
        string jsonstring = File.ReadAllText("..//synonymous.json");
        Synonymous sin = JsonSerializer.Deserialize<Synonymous>(jsonstring);
        QueryClass.synonymous = sin.synonymous;
        Tf_IdfDoc();
    }
    /// <summary>Metodo para calcular el TF_idf de los documentos</summary>
    static void Tf_IdfDoc()
    {
        foreach (var word in BuildIndex.dic)
        {
            word.Value.word_cant_doc = word_cantdoc(word.Value.weight_doc);
            for (int j = 0; j < Document.cantdoc; j++)
            {
                word.Value.weight_doc[j] = (word.Value.weight_doc[j] / Document.max[j]) * Math.Log(Convert.ToDouble(Document.cantdoc) / word.Value.word_cant_doc);
            }
        }
    }
    /// <summary>Metodo para determinar la cantidad de documentos que contienen a una palabra</summary>
    /// <param name="a">Arreglo con la frecuencia de la palabra en los documentos</param>
    public static double word_cantdoc(double[] a)
    {
        double cant = 0;
        for (int i = 0; i < Document.cantdoc; i++)
        {
            if (a[i] != 0) cant++;
        }
        return cant;
    }
    /// <summary>Metodo para indexar nuestra query</summary>
    /// <param name="query">Query</param>
    public static void QueryIndex(QueryClass query)
    {
        TF_idfC(query);
        SimVectors(query);
        Snippet(query);
    }
    /// <summary>Metodo para calcular el Tf_idf de nuestra query</summary>
    /// <param name="query">Query</param>
    public static void TF_idfC(QueryClass query)
    {
        int j = 0;
        foreach (var word in BuildIndex.dic)
        {
            //Factor para modificar el peso de la palabra
            double a = 1;
            if (query.highest_relevance.ContainsKey(word.Key)) a = query.highest_relevance[word.Key];
            if (query.words_Synonymous.IndexOf(word.Key) >= 0) a = 0.10;
            if (query.words_Stemming.IndexOf(word.Key) >= 0) a = 0.20;
            query.vectorC[j] = (a * word.Value.weigth_query / query.max) * Math.Log(Convert.ToDouble(Document.cantdoc) / word.Value.word_cant_doc);
            word.Value.weigth_query = 0;
            j++;
        }
        query.max = 1;
    }
    /// <summary>Metodo para calcular la similitud del coseno</summary>
    /// <param name="query">Query</param>
    static void SimVectors(QueryClass query)
    {
        double[] score = new double[Document.cantdoc];
        double mod_query = 0, query_x_doc = 0, mod_doc = 0;
        //Calculamos la normal de la query
        for (int i = 0; i < BuildIndex.cantwords; i++)
        {
            mod_query += query.vectorC[i] * query.vectorC[i];
        }
        for (int i = 0; i < Document.cantdoc; i++)
        {
            query_x_doc = 0; mod_doc = 0;
            int ind_query = 0;
            //Lista para las palabras de la query presentes en el documento
            List<string> words_doc = new List<string>();
            //Lista para las palabras de la busqueda literal presentes en el documneto
            List<string> words_docLiteral = new List<string>();
            //Lista para los sinonimos y raices de la query en el documento
            List<string> words_doc_Syn_Stemming = new List<string>();
            foreach (var word_dic in BuildIndex.dic)
            {
                //Calculamos el producto de los 2 vecotes y la norma del documento
                query_x_doc += word_dic.Value.weight_doc[i] * query.vectorC[ind_query];
                mod_doc += word_dic.Value.weight_doc[i] * word_dic.Value.weight_doc[i];
                if (word_dic.Value.weight_doc[i] * query.vectorC[ind_query] != 0)
                {
                    if (!query.words_Synonymous.Contains(word_dic.Key) && !query.words_Stemming.Contains(word_dic.Key))
                    {
                        words_doc.Add(word_dic.Key);
                    }
                    else
                    {
                        words_doc_Syn_Stemming.Add(word_dic.Key);
                    }
                }
                ind_query++;
                if (word_dic.Value.Pos_doc != null)
                {
                    words_docLiteral.Add(word_dic.Key);
                }
            }
            if (words_doc.Count == 0) words_doc = words_doc_Syn_Stemming;
            if (mod_query * mod_doc != 0)
            {
                //Calculamos la similitud del coseno
                score[i] = (query_x_doc) / Math.Sqrt(mod_doc * mod_query);
            }
            else score[i] = 0;
            if (score[i] != 0)
            {
                ResultSearch(query, words_doc, words_docLiteral, score[i], Document.documents[i]);
            }
        }
    }
    /// <summary>Metodo para determinar si el documento puede ser resultado de la busqueda</summary>
    /// <param name="query">Query</param>
    /// <param name="words_doc">Lista para las palabras de la query presentes en el documento</param>
    /// <param name="words_docLiteral">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="score">Score del documento</param>
    /// <param name="doc">Documento</param>
    static void ResultSearch(QueryClass query, List<string> words_doc, List<string> words_docLiteral, double score, Document doc)
    {
        bool result = true;
        //Comprobamos el operador de Exclusion
        for (int m = 0; m < query.Exclude.Count; m++)
        {
            if (BuildIndex.dic[query.Exclude[m]].weight_doc[doc.index] != 0)
            {
                result = false;
            }
        }
        //Comprobamos el operador de Inclusion
        for (int m = 0; m < query.Include.Count; m++)
        {
            if (BuildIndex.dic[query.Include[m]].weight_doc[doc.index] == 0)
            {
                result = false;
            }
        }
        if (result) result = SearchLiteral_Operator(query, words_docLiteral, doc);
        if (result)
        {
            //Guardamos los resultados de nuetra busqueda
            query.resultsearchDoc.Add(doc);
            query.Snippet.Add(words_doc);
            score = score * Close(words_doc, query, doc);
            query.Score.Add(score);
        }
    }
    /// <summary>Metodo para comprobar la busqueda literal</summary>
    /// <param name="query">Query</param>
    /// <param name="words_doc">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="doc">Documento</param>
    static bool SearchLiteral_Operator(QueryClass query, List<string> words_doc, Document doc)
    {
        List<int> l = new List<int>();
        for (int i = 0; i < query.SearchLiteral_words.Count; i++)
        {
            //Evaluamos los parametros para la busqueda literal
            for (int j = 0; j < query.SearchLiteral_words[i].Count; j++)
            {
                if (BuildIndex.dic[query.SearchLiteral_words[i][j]].Pos_doc[doc.index] == null)
                {
                    return false;
                }
            }
            Tuple<int, int, List<string>> t = Shortest_Distance_Word(query.SearchLiteral_words[i], doc, query.SearchLiteral_words[i].Count, 10, true);
            if (t.Item2 > query.SearchLiteral_words[i].Count)
            {
                return false;
            }
            //Guardamos la posicion de la busqueda literal encontrada
            l.Add(t.Item1);
        }
        //Guardamos las posiciones de la busqueda literal encontrada
        query.Pos_SearchLiteral.Add(l);
        return true;
    }
    /// <summary>Metodo para buscar los snippets</summary>
    /// <param name="query">Query</param>
    static void Snippet(QueryClass query)
    {
        for (int i = 0; i < query.resultsearchDoc.Count; i++)
        {
            List<int> Snippet_words = new List<int>();
            //Comprobamos si tenemos resultados de busqueda literal
            if (query.SearchLiteral_words.Count > 0)
            {
                for (int x = 0; x < query.SearchLiteral_words.Count; x++)
                {
                    //Removemos la palabra de la busqueda literal, de la lista de palabras del snippet
                    foreach (var y in query.SearchLiteral_words[x])
                    {
                        if (query.Snippet[i].Contains(y)) query.Snippet[i].Remove(y);
                    }
                    //Guardamos la posicion en la lista de posiciones de los snippets
                    Snippet_words.Add(query.Pos_SearchLiteral[i][x]);
                }
            }
            //Buscamos la mayor cantidad de palabras de nuestro resultado que esten contenidas en una ventana de tamaño Snippet_len
            while (query.Snippet[i].Count != 0)
            {
                Tuple<int, int, List<string>> tuple = Shortest_Distance_Word(query.Snippet[i], query.resultsearchDoc[i], 1, Snippet_len);
                Snippet_words.Add(tuple.Item1);
                //Actualizamos nuestra lista de palabras
                query.Snippet[i] = tuple.Item3;
            }
            BuildSinipped(query, query.resultsearchDoc[i], Snippet_words);
        }
    }
    /// <summary>Metodo para construir los snippets</summary>
    /// <param name="query">Query</param>
    /// <param name="document">Documento</param>
    /// <param name="Snippetwords">Lista de las posiciones de las palabras</param>
    static void BuildSinipped(QueryClass query, Document document, List<int> Snippetwords)
    {
        string[] doc = File.ReadAllLines(document.path);
        string[] addSnippet = new string[Snippetwords.Count];
        int[] addposSnippet = new int[Snippetwords.Count];
        //Recorremos el documento
        for (int i = 0; i < Snippetwords.Count; i++)
        {
            int cant = 0;
            for (int linea_ind = 0; linea_ind < doc.Length; linea_ind++)
            {
                string[] linea = doc[linea_ind].Split(' ');
                if (Snippetwords[i] <= cant + linea.Length - 1)
                {
                    //Si nos encotramos la posicion de la palabra procedemos a construir el snippet
                    int j = Snippetwords[i] - cant;
                    int cant1 = 0;
                    StringBuilder sb = new StringBuilder();
                    for (int w = j; w < linea.Length; w++)
                    {
                        sb.Append(linea[w] + " ");
                        cant1++;
                        if (cant1 == Snippet_len) break;
                    }
                    //Si no hemos alcanzad la longitud de nuestro snippet vamos a la linea siguiente
                    if (cant1 < Snippet_len && linea_ind < doc.Length - 1)
                    {
                        int ind = linea_ind + 1;
                        if (doc[linea_ind + 1] == "" && linea_ind < doc.Length - 2)
                        {
                            ind = linea_ind + 2;
                        }
                        string[] newlinea = doc[ind].Split(' ');
                        for (int w = 0; w < newlinea.Length; w++)
                        {
                            sb.Append(newlinea[w] + " ");
                            cant1++;
                            if (cant1 == Snippet_len) break;
                        }
                    }
                    //Guardamos el snippet
                    addSnippet[i] = sb.ToString();
                    addposSnippet[i] = linea_ind;
                    break;
                }
                cant += linea.Length;
            }
        }
        //Guardamos lo snippets del documento
        query.SnippetResult.Add(addSnippet);
        query.Pos_SnippetResult.Add(addposSnippet);
    }
    /// <summary>Metodo obtener el Ranking de la cercania</summary>
    /// <param name="words">Lista de palabras</param>
    /// <param name="query">Query</param>
    /// <param name="document">Documento</param>
    public static double Close(List<string> words, QueryClass query, Document document)
    {
        double sumScore = 0;
        foreach (var i in query.Close)
        {
            //Comprobamos que las palabras del operador cercania esten en nuestra lista de palabras
            List<string> words_searchDist = new List<string>();
            for (int j = 0; j < i.Count; j++)
            {
                if (words.Contains(i[j]))
                {
                    words_searchDist.Add(i[j]);
                }
            }
            if (words_searchDist.Count > 1)
            {
                double n = (double)Shortest_Distance_Word(words_searchDist, document, words_searchDist.Count, 10).Item2;
                //Si la distancia es menor a 100 procedemos a rankiarla
                if (n <= 100)
                {
                    n = n * words_searchDist.Count / i.Count;
                    sumScore += ((double)words_searchDist.Count / (double)i.Count) * (100 / (n - 1));
                }
                else sumScore = 0;
            }
        }

        if (sumScore > 0) return sumScore;
        return 1;
    }
    /// <summary>Metodo para encontrar la minima distancia de una lista de palabras</summary>
    /// <param name="words">Lista de palabras para buscar la cercania</param>
    /// <param name="document">Documento donde estan las palabras</param>
    /// <param name="cantmin">Cantidad minima de palabras de la lista en las q queremos buscar la minima distancia</param>
    /// <param name="cota">Maxima distancia que podemos encontrar entre las palabras</param>
    /// <param name="SearchLiteral">Presencia de la busqueda literal</param>
    static Tuple<int, int, List<string>> Shortest_Distance_Word(List<string> words, Document document, int cantmin, int cota, bool SearchLiteral = false)
    {
        List<int> l = new List<int>();
        List<int> index_words_in_range = new List<int>();
        //Guardamos en un arreglo de tuplas las posiciones de las palabras donde el primer indice corresponde al indice de la palabra en la lista 
        //Ordenamos los elementos de los arrays de tuplas por el numero de la posicion de la palabra
        Tuple<int, int>[] Pos_words_Sorted = BuildTuple(BuildIndex.dic[words[0]].Pos_doc[document.index], 0);
        for (int i = 1; i < words.Count; i++)
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
    static Tuple<int, int>[] Sorted(Tuple<int, int>[] a, Tuple<int, int>[] b)
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
    static Tuple<int, int>[] BuildTuple(List<int> words_pos, int index)
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
    static Tuple<bool, List<int>> AllContains(int[] posList, int cant)
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
