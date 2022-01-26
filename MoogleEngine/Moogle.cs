namespace MoogleEngine;
using System.IO;
using System.Text.Json;

public static class Moogle
{
    static int Snippet_len=20;
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda
        QueryClass query1 = new QueryClass(query);
        QueryIndex(query1);
        SearchItem[] items = new SearchItem[query1.Score.Count];
        for (int i = 0; i < query1.Score.Count; i++)
        {
            items[i] = new SearchItem(query1.resultsearchDoc[i].title, query1.SnippetResult[i], query1.Pos_SnippetResult[i], (float)query1.Score[i]);
        }
        return new SearchResult(items, query1.txt);
    }
    public static void Indexar()
    {
        Document.sistema = new BuildIndex();
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        Document.documents = new List<Document>();
        int q = 0;
        foreach (var i in list)
        {
            q++;
        }
        Document.cantdoc = q;
        Document.max = new int[q + 1];
        for(int i=0;i<Document.max.Length;i++)
        {
            Document.max[i]=1;
        }
        q = 0;
        foreach (var i in list)
        {
            Document d1 = new Document(File.ReadAllLines(i), i, q);
            Document.documents.Add(d1);
            q++;
        }
        string jsonstring = File.ReadAllText("..//sinonimos.json");
        Sinonimo sin = JsonSerializer.Deserialize<Sinonimo>(jsonstring);
        QueryClass.sinonimos = sin.sinonimos;
        Tf_IdfDoc();
    }
    static void Tf_IdfDoc()
    {
        foreach (var i in Document.sistema.dic)
        {
            i.Value.Item1[Document.cantdoc + 1] = word_cantdoc(i.Value.Item1);
            for (int j = 0; j < Document.cantdoc; j++)
            {
                i.Value.Item1[j] = (i.Value.Item1[j] / Convert.ToDouble(Document.max[j])) * Math.Log(Convert.ToDouble(Document.cantdoc) / Convert.ToDouble(i.Value.Item1[Document.cantdoc + 1]));
            }
        }
    }
    public static int word_cantdoc(double[] a)
    {
        int cant = 0;
        for (int i = 0; i < Document.cantdoc; i++)
        {
            if (a[i] != 0) cant++;
        }
        return cant;
    }
    public static void QueryIndex(QueryClass query)
    {
        TF_idfC(query);
        SimVectors(query);
        Snippet(query);
    }
    public static void TF_idfC(QueryClass query)
    {
        int j = 0;
        foreach (var i in Document.sistema.dic)
        {
            double a = 1;
            if (query.MayorRelevancia.ContainsKey(i.Key)) a = query.MayorRelevancia[i.Key];
            if (query.wordsSinonimo.IndexOf(i.Key) >= 0) a = 0.10;
            if (query.wordsRaices.IndexOf(i.Key) >= 0) a = 0.20;
            query.vectorC[j] = (a * i.Value.Item1[Document.cantdoc] / Convert.ToDouble(Document.max[Document.cantdoc])) * Math.Log(Convert.ToDouble(Document.cantdoc) / Convert.ToDouble(i.Value.Item1[Document.cantdoc + 1]));
            i.Value.Item1[Document.cantdoc] = 0;
            j++;
        }
        Document.max[Document.cantdoc] = 1;
    }
    static void SimVectors(QueryClass query)
    {
        double[] score = new double[Document.cantdoc];
        double mod_query = 0, query_x_doc = 0, mod_doc = 0;
        for (int i = 0; i < Document.sistema.cantwords; i++)
        {
            mod_query += query.vectorC[i] * query.vectorC[i];
        }
        for (int i = 0; i < Document.cantdoc; i++)
        {
            query_x_doc = 0; mod_doc = 0;
            int ind_query = 0;
            List<string> words_doc = new List<string>();
            List<string> words_docLiteral=new List<string>();
            List<string> words_docSin_Raices = new List<string>();
            foreach (var j in Document.sistema.dic)
            {
                query_x_doc += j.Value.Item1[i] * query.vectorC[ind_query];
                mod_doc += j.Value.Item1[i] * j.Value.Item1[i];
                if (j.Value.Item1[i] * query.vectorC[ind_query] != 0)
                {
                    if (!query.wordsSinonimo.Contains(j.Key) && !query.wordsRaices.Contains(j.Key))
                    {
                        words_doc.Add(j.Key);
                    }
                    else
                    {
                        words_docSin_Raices.Add(j.Key);
                    }
                }
                ind_query++;
                if (j.Value.Item2 != null)
                {
                    words_docLiteral.Add(j.Key);
                }
            }
            if (words_doc.Count == 0) words_doc = words_docSin_Raices;
            if (mod_query * mod_doc != 0)
            {
                score[i] = (query_x_doc) / Math.Sqrt(mod_doc * mod_query);
            }
            else score[i] = 0;
            if (score[i] != 0)
            {
                ResultSearch(query, words_doc,words_docLiteral, score[i], Document.documents[i]);
            }
        }
    }
    static void ResultSearch(QueryClass query, List<string> words_doc,List<string> words_docLiteral, double score, Document doc)
    {
        bool result = true;
        for (int m = 0; m < query.Excluir.Count; m++)
        {
            if (Document.sistema.dic[query.Excluir[m]].Item1[doc.index] != 0)
            {
                result = false;
            }
        }
        for (int m = 0; m < query.Incluir.Count; m++)
        {
            if (Document.sistema.dic[query.Incluir[m]].Item1[doc.index] == 0)
            {
                result = false;
            }
        }
        if(result) result=SearchLiteral_Operator(query,words_docLiteral,doc);
        if (result)
        {
            query.resultsearchDoc.Add(doc);
            query.Snippet.Add(words_doc);
            score = score * Cercania(words_doc, query, doc);
            query.Score.Add(score);
        }
    }
    static bool SearchLiteral_Operator(QueryClass query, List<string> words_doc, Document doc)
    {
        List<int> l=new List<int>();
        for(int i=0;i<query.SearchLiteral_words.Count;i++)
        {
            for(int j=0;j<query.SearchLiteral_words[i].Count;j++)
            {
                if (Document.sistema.dic[query.SearchLiteral_words[i][j]].Item2[doc.index] == null)
                {
                    return false;
                }
            }
            Tuple<int,int,List<string>> t=Menor_DistanciaWord(query.SearchLiteral_words[i],doc,query.SearchLiteral_words[i].Count,10,true);
            if(t.Item2>query.SearchLiteral_words[i].Count)
            {
                return false;
            }
            l.Add(t.Item1);
        }
        query.Pos_SearchLiteral.Add(l);
        return true;
    }
    static void Snippet(QueryClass query)
    {
        for (int i = 0; i < query.resultsearchDoc.Count; i++)
        {
            List<int> Snippet_words = new List<int>();
            if(query.SearchLiteral_words.Count>0)
            {
                for(int x=0;x<query.SearchLiteral_words.Count;x++)
                {
                    foreach(var y in query.SearchLiteral_words[x])
                    {
                        if(query.Snippet[i].Contains(y)) query.Snippet[i].Remove(y);
                    }
                    Snippet_words.Add(query.Pos_SearchLiteral[i][x]);
                }
            }
            while(query.Snippet[i].Count!=0)
            {
                Tuple<int, int,List<string>> tuple = Menor_DistanciaWord(query.Snippet[i], query.resultsearchDoc[i],1,Snippet_len);
                Snippet_words.Add(tuple.Item1);
                query.Snippet[i]=tuple.Item3;
            }              
            BuildSinipped(query, i, Snippet_words);
        }
    }
    static void BuildSinipped(QueryClass query, int indexdoc, List<int> Snippetwords)
    {
        string[] doc = File.ReadAllLines(query.resultsearchDoc[indexdoc].ruta);
        string[] addSnippet = new string[Snippetwords.Count];
        int[] addposSnippet = new int[Snippetwords.Count];
        for (int i = 0; i < Snippetwords.Count; i++)
        {
            int cant = 0;
            for (int linea_ind = 0; linea_ind < doc.Length; linea_ind++)
            {
                string[] linea = doc[linea_ind].Split(' ');
                if (Snippetwords[i] <= cant + linea.Length - 1)
                {
                    int j = Snippetwords[i] - cant;
                    string n = "";
                    int cant1 = 0;
                    for (int w = j; w < linea.Length; w++)
                    {
                        n = n + linea[w] + " ";
                        cant1++;
                        if (cant1 == Snippet_len) break;
                    }
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
                            n = n + newlinea[w] + " ";
                            cant1++;
                            if (cant1 == Snippet_len) break;
                        }
                    }
                    addSnippet[i] = n;
                    addposSnippet[i] = linea_ind;
                    break;
                }
                cant += linea.Length;
            }
        }
        query.SnippetResult.Add(addSnippet);
        query.Pos_SnippetResult.Add(addposSnippet);
        doc=null;
    }
    public static double Cercania(List<string> words, QueryClass query, Document document)
    {
        int cant = 0;
        double sumaScore = 0;
        foreach (var i in query.Cercanas)
        {
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
                double n = (double)Menor_DistanciaWord(words_searchDist, document,words_searchDist.Count,10).Item2;
                if (n <= 100)
                {
                    n=n*words_searchDist.Count/i.Count;
                    sumaScore += ((double)cant / (double)i.Count) * (100 / (n - 1));
                }
                else sumaScore = 0;
            }
        }
        if (sumaScore > 0) return sumaScore;
        return 1;
    }
    static Tuple<int, int,List<string>> Menor_DistanciaWord(List<string> words, Document document,int cantmin,int cota,bool orden=false)
    {
        List<int> l=new List<int>();
        List<int> index_words_not_range=new List<int>();
        Tuple<int, int>[] t = BuildTuple(Document.sistema.dic[words[0]].Item2[document.index], 0);
        for (int i = 1; i < words.Count; i++)
        {
            t = Sorted(t, BuildTuple(Document.sistema.dic[words[i]].Item2[document.index], i));
        }
        int menorDist = int.MaxValue;
        int pos = 0;
        Queue<Tuple<int, int>> search_min_dist;
        int[] posList = new int[words.Count];
        for(int j=words.Count;j>=cantmin;j--)
        {
            search_min_dist=new Queue<Tuple<int, int>>();
            posList=new int[words.Count];
            for (int i = 0; i < t.Length; i++)
            {
                search_min_dist.Enqueue(t[i]);
                posList[t[i].Item1]++;
                Tuple<bool,List<int>> all=AllContains(posList,j);
                if (all.Item1)
                {
                    l=all.Item2;
                    while (true)
                    {
                        Tuple<int, int> eliminate = search_min_dist.Peek();
                        posList[eliminate.Item1]--;
                        Tuple<bool,List<int>> tuple=AllContains(posList,j);
                        if (tuple.Item1)
                        {
                            l=tuple.Item2;
                            search_min_dist.Dequeue();
                        }
                        else
                        {
                            posList[eliminate.Item1]++;
                            break;
                        }
                    }
                    bool orden1=true;
                    if(orden)
                    {
                        int comp=-1;
                        if(search_min_dist.Count==words.Count)
                        {
                            foreach(var m in search_min_dist)
                            {
                                if(m.Item1<=comp) 
                                {
                                    orden1=false;
                                    break;
                                }
                                comp=m.Item1;
                            }
                        }
                    }
                    if (orden1&&t[i].Item2 - search_min_dist.Peek().Item2 + 1 < menorDist)
                    {
                        index_words_not_range=l;
                        menorDist = t[i].Item2 - search_min_dist.Peek().Item2 + 1;
                        pos = search_min_dist.Peek().Item2;
                    }
                    if (orden1&&t[i].Item2 - search_min_dist.Peek().Item2 + 1 == menorDist)
                    {
                        Random random=new Random();
                        if(random.Next(2)==0)
                        {
                            index_words_not_range=l;
                            pos = search_min_dist.Peek().Item2;
                        }
                    }
                }
                
            }
            if(menorDist<=cota) break; 
        }
        List<string> words_not_range=new List<string>();
        for(int i=0;i<words.Count;i++)
        {
            if(!index_words_not_range.Contains(i))
            {
                words_not_range.Add(words[i]);
            }
        }
        return new Tuple<int, int,List<string>>(pos, menorDist,words_not_range);
    }
    static Tuple<int, int>[] Sorted(Tuple<int, int>[] a, Tuple<int, int>[] b)
    {
        Tuple<int, int>[] c = new Tuple<int, int>[a.Length + b.Length];
        int i = 0; int j = 0;
         while(i<a.Length&&j<b.Length)
        {
            if(a[i].Item2<b[j].Item2)
            {
                c[i+j]=a[i];
                i++;
            }
            else
            {
                c[i+j]=b[j];
                j++;
            }
        }
        while(i<a.Length)
        {
            c[i+j]=a[i];
            i++;
        }
        while(j<b.Length)
        {
            c[i+j]=b[j];
            j++;
        }
        return c;
    }
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
    static Tuple<bool,List<int>> AllContains(int[] posList,int cant)
    {
        List<int> words_not_range=new List<int>();
        for (int i = 0; i < posList.Length; i++)
        {
            if (posList[i] != 0)
            {
                words_not_range.Add(i);
            }
        }
        if(words_not_range.Count<cant) return new Tuple<bool,List<int>>(false,words_not_range);
        return new Tuple<bool,List<int>>(true,words_not_range);
    }
}
