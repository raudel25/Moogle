namespace MoogleEngine;
using System.IO;
using System.Diagnostics;
using System.Text.Json;

public static class Moogle
{
    static string qwe;
    static string mostrar;
    static int cant2 = 0;
    static List<Document> documents = new List<Document>();
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda
        Consulta c1 = new Consulta(query);
        Consulta1(c1);
        SearchItem[] items = new SearchItem[c1.Score.Count];
        for (int i = 0; i < c1.Score.Count; i++)
        {
            double max = 0;
            int indicemax = 0;
            for (int j = 0; j < c1.Score.Count; j++)
            {
                if (c1.Score[j] > max)
                {
                    max = c1.Score[j];
                    indicemax = j;
                }
            }
            c1.Score[indicemax] = 0;
            items[i] = new SearchItem(c1.resultsearchDoc[indicemax].title, c1.SnippedResult[indicemax].Substring(0, c1.SnippedResult[indicemax].Length - 7), (float)c1.Score[i]);
        }
        SearchItem[] items1 = new SearchItem[1] { new SearchItem(/*c1.Cercanas[0][1]+*/ Consulta.sinonimos[0][0], Document.time1 + " ", 12) };
        return new SearchResult(items, c1.txt);
    }
    public static void Indexar()
    {
        Stopwatch crono = new Stopwatch();
        crono.Start();
        Document.sistema = new Suffix_Tree();
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        //List<Document> documents = new List<Document>();
        int q = 0;
        foreach (var i in list)
        {
            q++;
        }
        Document.cantdoc = q;
        Document.max = new int[q + 1];
        q = 0;
        foreach (var i in list)
        {
            Document d1 = new Document(File.ReadAllLines(i), i, q);
            documents.Add(d1);
            q++;
        }
        string jsonstring = File.ReadAllText("..//Content//sinonimos.json");
        Sinonimo sin = JsonSerializer.Deserialize<Sinonimo>(jsonstring);
        Consulta.sinonimos = sin.sinonimos;
        Tf_IdfDoc();
        crono.Stop();
        Document.time1 = crono.ElapsedMilliseconds;
    }
    static void Tf_IdfDoc()
    {
        //for (int i = 0; i < Document.sistema.cantwords; i++)
        foreach (var i in Document.sistema.dic)
        {

            //Document.sistema.vectors[i][Document.cantdoc + 1] = word_cantdoc(Document.sistema.vectors[i]);
            i.Value.Item1[Document.cantdoc + 1] = word_cantdoc(i.Value.Item1);
            for (int j = 0; j < Document.cantdoc; j++)
            {
                //Document.sistema.vectors[i][j] = (Document.sistema.vectors[i][j] / Convert.ToDouble(Document.max[j])) * Math.Log(Convert.ToDouble(Document.cantdoc) / Convert.ToDouble(Document.sistema.vectors[i][Document.cantdoc + 1]));
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
    public static void Consulta1(Consulta query)
    {
        TF_idfC(query);
        SimVectors(query);
        Snipped(query);
    }
    public static void TF_idfC(Consulta query)
    {
        //for (int i = 0; i < Document.sistema.cantwords; i++)
        int j = 0;
        foreach (var i in Document.sistema.dic)
        {
            //query.vectorC[i] = (Document.sistema.vectors[i][Document.cantdoc] / Convert.ToDouble(Document.max[Document.cantdoc])) * Math.Log(Convert.ToDouble(Document.cantdoc) / Convert.ToDouble(Document.sistema.vectors[i][Document.cantdoc + 1]));
            //Document.sistema.vectors[i][Document.cantdoc] = 0;
            double a = 1;
            if (query.MayorRelevancia.ContainsKey(i.Key)) a = query.MayorRelevancia[i.Key];
            if (query.wordsSinonimo.IndexOf(i.Key) >= 0) a = 0.10;
            query.vectorC[j] = (a * i.Value.Item1[Document.cantdoc] / Convert.ToDouble(Document.max[Document.cantdoc])) * Math.Log(Convert.ToDouble(Document.cantdoc) / Convert.ToDouble(i.Value.Item1[Document.cantdoc + 1]));
            i.Value.Item1[Document.cantdoc] = 0;
            j++;
        }
        Document.max[Document.cantdoc] = 0;
    }
    static void SimVectors(Consulta query)
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
            List<string> words_doc1 = new List<string>();
            //for (int j = 0; j < Document.sistema.cantwords; j++)
            bool search_natural = false;
            foreach (var j in Document.sistema.dic)
            {
                //e += Document.sistema.vectors[j][i] * query.vectorC[j];
                //r += Document.sistema.vectors[j][i] * Document.sistema.vectors[j][i];
                query_x_doc += j.Value.Item1[i] * query.vectorC[ind_query];
                mod_doc += j.Value.Item1[i] * j.Value.Item1[i];

                if (j.Value.Item1[i] * query.vectorC[ind_query] != 0)
                {

                    if (!query.wordsSinonimo.Contains(j.Key))
                    {
                        words_doc.Add(j.Key);
                    }
                    else
                    {
                        words_doc1.Add(j.Key);
                    }
                }
                ind_query++;
            }
            if (words_doc.Count == 0) words_doc = words_doc1;
            if (mod_query != 0)
            {
                //documents[i].score = Math.Sqrt(mod_doc) + "";
                score[i] = (query_x_doc) / Math.Sqrt(mod_doc * mod_query);

            }
            else score[i] = 0;
            if (score[i] != 0)
            {
                bool result = true;
                for (int m = 0; m < query.Excluir.Count; m++)
                {
                    if (Document.sistema.dic[query.Excluir[m]].Item1[i] != 0)
                    {
                        result = false;
                    }
                }
                for (int m = 0; m < query.Incluir.Count; m++)
                {
                    if (Document.sistema.dic[query.Incluir[m]].Item1[i] == 0)
                    {
                        result = false;
                    }
                }
                if (result)
                {
                    query.resultsearchDoc.Add(documents[i]);
                    query.Snipped.Add(words_doc);
                    query.cantresult++;
                    score[i] = score[i] * Cercania(words_doc, query, documents[i]);
                    query.Score.Add(score[i]);
                    //documents[i].score = score[i] + "";
                    //documents[i].title = documents[i].title + " " + Cercania(words_doc, query, documents[i]);
                }
            }
        }
    }
    static void Snipped(Consulta query)
    {
        for (int i = 0; i < query.cantresult; i++)
        {
            int[] Snipped_words = new int[query.Snipped[i].Count];
            for (int j = 0; j < Snipped_words.Length; j++)
            {
                List</*Tuple<int, int>*/int> l = Document.sistema.dic[query.Snipped[i][j]].Item2[query.resultsearchDoc[i].index];
                Random rnd = new Random();
                int ind = rnd.Next(l.Count);
                //linea[j] = l[ind].Item1;
                //pos[j] = l[ind].Item2;
                Snipped_words[j] = l[ind];

            }
            if (Snipped_words.Length > 1)
            {
                Tuple<int, int> tuple = Menor_DistanciaWord(query.Snipped[i], query.resultsearchDoc[i], 0, new int[Snipped_words.Length], int.MaxValue, int.MinValue, 10, 0);
                if (tuple.Item2 < 10)
                {
                    Snipped_words = new int[] { tuple.Item1 };
                }
            }
            BuildSinipped(query, i, Snipped_words);
        }
    }
    static void BuildSinipped(Consulta query, int indexdoc, int[] Snippedwords)
    {
        string[] doc = File.ReadAllLines(query.resultsearchDoc[indexdoc].ruta);
        string addSnipped = "";
        for (int i = 0; i < Snippedwords.Length; i++)
        {
            int cant = 0;
            for (int linea_ind = 0; linea_ind < doc.Length; linea_ind++)
            {
                string[] linea = doc[linea_ind].Split(' ');
                //cant = cant + s1.Length;
                //if(s1[q]=="") continue;
                if (Snippedwords[i] <= cant + linea.Length - 1)
                {
                    int start; int stop;
                    int j = Snippedwords[i] - cant;
                    /*int u = j;
                    if (s1.Length >= 10)
                    {
                        start = j - 5;
                        stop = 10;
                        if (j <= 4) start = 0;
                        if (j + 5 >= s1.Length) start = s1.Length - 10;
                    }
                    else
                    {
                        stop = s1.Length;
                        start = 0;
                    }
                    string n = "";
                    for (int m = 0; m < stop; m++)
                    {
                        if (m != stop - 1) n = n + s1[start + m] + " ";
                        else n = n + s1[start + m] + "";
                    }*/
                    string n = "";
                    int cant1 = 0;
                    for (int w = j; w < linea.Length; w++)
                    {
                        n = n + linea[w] + " ";
                        cant1++;
                        if (cant1 == 10) break;
                    }
                    if (cant1 < 10 && linea_ind < doc.Length - 1)
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
                            if (cant1 == 10) break;
                        }
                    }
                    addSnipped = addSnipped + n + "... ...";
                    break;
                }
                cant += linea.Length;
            }
        }
        query.SnippedResult.Add(addSnipped);
    }
    public static double Cercania(List<string> words, Consulta query, Document document)
    {
        int cant = 0;
        double sumaScore = 0;
        List<string> words_document = new List<string>();
        foreach (var i in query.Cercanas)
        {
            cant = 0;
            for (int j = 0; j < i.Count; j++)
            {
                if (words.Contains(i[j]))
                {
                    words_document.Add(i[j]);
                    cant++;
                }
            }

            if (cant > 1)
            {
                double n = (double)Menor_DistanciaWord(words, document, 0, new int[cant], int.MaxValue, int.MinValue, 100, 0).Item2;
                sumaScore += ((double)cant / (double)i.Count) * ((101 - n) / 10);
            }
        }
        /*if (cant > 1)
        {
            //double n = (double)Menor_DistanciaWord(words, document, 0, new int[cant], int.MaxValue, int.MinValue, 100, 0).Item2;
            return sumaScore;
        }*/
        if (sumaScore > 0) return sumaScore;
        return 1;
    }

    public static Tuple<int, int> Menor_DistanciaWord(List<string> words, Document document, int index, int[] distancia, int min, int max, int cota, int pos)
    {
        if (index == distancia.Length) return new Tuple<int, int>(min, max - min);
        int minDist = int.MaxValue;
        foreach (var i in Document.sistema.dic[words[index]].Item2[document.index])
        {
            int newmax = max;
            int newmin = min;
            distancia[index] = i;
            if (i < min) newmin = i;
            if (max < i) newmax = i;
            if (newmax - newmin < minDist && newmax - newmin < cota)
            {
                Tuple<int, int> t = Menor_DistanciaWord(words, document, index + 1, distancia, newmin, newmax, cota, pos);
                if (minDist > t.Item2)
                {
                    minDist = t.Item2; pos = t.Item1;
                }
                if (minDist == t.Item2)
                {
                    Random random = new Random();
                    if (random.Next(2) == 0) pos = t.Item1;
                }
            }
        }
        return new Tuple<int, int>(pos, minDist);
    }
}
