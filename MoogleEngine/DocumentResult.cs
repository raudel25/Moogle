using System.Text;

namespace MoogleEngine;
public class DocumentResult
{
    public SearchItem? Item {get; set;}
    public static int SnippetLen=20;
    public DocumentResult(Document document,QueryClass query)
    { 
        //Lista para las palabras de la query presentes en el documento
        List<string> wordsDoc = new List<string>();
        //Lista para las palabras de la busqueda literal presentes en el documneto
        List<string> wordsDocLiteral = new List<string>();
        //Lista para las palabras que no estan en el documento
        List<string> wordsNoDoc = new List<string>();
        //Lista de posiciones para la busqueda literal
        List<int> PosSearchLiteral = new List<int>();
        double score= SimVectors(query, document, wordsDoc, wordsNoDoc, wordsDocLiteral);
        //Modificacion del Score por el operador cercania
        score += Close(wordsDocLiteral,document,query);
        if(!ResultSearch(wordsDoc, wordsNoDoc, wordsDocLiteral, PosSearchLiteral, score, document, query)) return;
        //Buscamos los Snippets
        (string[],int[]) aux=Snippet(wordsDoc, PosSearchLiteral, document, query);
        Item = new SearchItem(document.Title,aux.Item1,aux.Item2,score,wordsNoDoc);
    }
    #region SearchResult
    /// <summary>Metodo para calcular la similitud del coseno</summary>
    /// <param name="wordsDoc">Lista para las palabras de la query presentes en el documento</param>
    /// <param name="wordsDocLiteral">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="wordsNoDoc">Lista para las palabras de la query q no estan en el documento</param>
    /// <param name="document">Documento</param>
    /// <param name="query">Query</param>
    /// <returns>Score del Documento</returns>
    private static double SimVectors(QueryClass query,Document document,List<string> wordsDoc, List<string> wordsNoDoc, List<string> wordsDocLiteral)
    {
        double score = 0;
        double modQuery = 0, queryXdoc = 0, modDoc = 0;
        queryXdoc = 0; modDoc = 0;
            
            foreach (var wordDic in query.WordsQuery)
            {
                //Calculamos el producto de los 2 vectores
                queryXdoc += CorpusData.Vocabulary[wordDic.Key].WeightDoc[document.Index] * wordDic.Value;
                if (CorpusData.Vocabulary[wordDic.Key].WeightDoc[document.Index] * wordDic.Value != 0)
                {
                    wordsDoc.Add(wordDic.Key);
                }
                if (CorpusData.Vocabulary[wordDic.Key].PosDoc[document.Index] != null)
                {
                    wordsDocLiteral.Add(wordDic.Key);
                }
                else wordsNoDoc.Add(wordDic.Key);
            }
            double factor = 0;
            if (wordsDoc.Count == 0)
            {
                //Si no tenemos resultados buscamos las raices y los sinonimos
                foreach (var wordDic in query.WordsStemmingSyn)
                {
                    queryXdoc += CorpusData.Vocabulary[wordDic.Key].WeightDoc[document.Index] * wordDic.Value[0];
                    if (CorpusData.Vocabulary[wordDic.Key].WeightDoc[document.Index] * wordDic.Value[0] != 0)
                    {
                        wordsDoc.Add(wordDic.Key);
                    }
                }
                modQuery = query.NormaStemmingSyn;
                if (queryXdoc != 0) 
                {
                    factor = double.MinValue;
                }
            }
            else modQuery = query.Norma;
            modDoc = Document.Documents![document.Index].Norma;
            if (modQuery * modDoc != 0)
            {
                //Calculamos la similitud del coseno
                score = factor + queryXdoc / Math.Sqrt(modQuery * modDoc);
            }
            return score;
    }
    /// <summary>Metodo para determinar si el documento puede ser resultado de la busqueda</summary>
    /// <param name="wordsDoc">Lista para las palabras de la query presentes en el documento</param>
    /// <param name="wordsDocLiteral">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="score">Score del documento</param>
    /// <param name="doc">Documento</param>
    /// <returns>Si el documento debe ser devuelto</returns>
    private static bool ResultSearch(List<string> wordsDoc,List<string> wordsNoDoc, List<string> wordsDocLiteral, List<int> PosSearchLiteral, double score, Document doc,QueryClass query)
    {
        if(score==0) return false;
        //Comprobamos el operador de Exclusion
        for (int m = 0; m < query.Exclude.Count; m++)
        {
            if (CorpusData.Vocabulary[query.Exclude[m]].PosDoc[doc.Index] != null)
            {
                return false;
            }
        }
        //Comprobamos el operador de Inclusion
        for (int m = 0; m < query.Include.Count; m++)
        {
            if (CorpusData.Vocabulary[query.Include[m]].PosDoc[doc.Index] == null)
            {
                return false;
            }
        }
        //Comprobamos el operador de busqueda literal
        return SearchLiteralOperator(wordsDocLiteral, doc,query, PosSearchLiteral);
    }
    /// <summary>Metodo para comprobar la busqueda literal</summary>
    /// <param name="wordsDoc">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="doc">Documento</param>
    /// <returns>Bool indicando si el documento es valido para el operador busqueda literal</returns>
    private static bool SearchLiteralOperator(List<string> wordsDoc, Document doc,QueryClass query, List<int> PosSearchLiteral)
    {
        for (int i = 0; i < query.SearchLiteralWords.Count; i++)
        {
            //Evaluamos los parametros para la busqueda literal
            for (int j = 0; j < query.SearchLiteralWords[i].Count; j++)
            {
                if(query.SearchLiteralWords[i][j]=="?") continue;
                if (CorpusData.Vocabulary[query.SearchLiteralWords[i][j]].PosDoc[doc.Index] == null) return false;
            }
            int posLiteral = DistanceWord.DistanceLiteral(query.SearchLiteralWords[i], doc);
            if (posLiteral == -1) return false;
            //Guardamos la posicion de la busqueda literal encontrada
            PosSearchLiteral.Add(posLiteral);
        }
        return true;
    }
    /// <summary>Metodo obtener el Ranking de la cercania</summary>
    /// <param name="words">Lista de palabras</param>
    /// <param name="document">Documento</param>
    /// <returns>Cantidad a incrementar en el score por el operador cercania</returns>
    private static double Close(List<string> words, Document document,QueryClass query)
    {
        double sumScore = 0;
        foreach (var wordList in query.CloseWords)
        {
            bool close=true;
            //Comprobamos que las palabras del operador cercania esten en nuestra lista de palabras
            for (int j = 0; j < wordList.Count; j++)
            {
                if (!words.Contains(wordList[j]))
                {
                    close=false;
                    break;
                }              
            }
            //Si hemos encontrado todas las palabras de nuestro operador cercania buscamos la minima distancia
            if (close)
            {
                double n = (double)DistanceWord.DistanceClose(wordList, document);
                if(n == int.MaxValue) continue;
                sumScore += 100/(n-1);
            }
        }
        return sumScore;
    }
    #endregion
    
    #region Snippet
    /// <summary>Metodo para buscar los snippets</summary>
    /// <param name="document">Documento</param>
    /// <param name="query">Query</param>
    /// <param name="Snippetwords">Lista de las posiciones de las palabras</param>
    /// <returns>Snippets y posiciones en las que de encuentran</returns>
    private static (string[],int[]) Snippet(List<string> Snippetwords, List<int> PosSearchLiteral, Document document, QueryClass query)
    {
        List<int> wordsList = new List<int>();
        //Comprobamos si tenemos resultados de busqueda literal
        if (query.SearchLiteralWords.Count > 0)
            {
                for (int x = 0; x < query.SearchLiteralWords.Count; x++)
                {
                    //Removemos la palabra de la busqueda literal, de la lista de palabras del snippet
                    foreach (var y in query.SearchLiteralWords[x])
                    {
                        if (Snippetwords.Contains(y)) Snippetwords.Remove(y);
                    }
                    //Guardamos la posicion del snippet
                    wordsList.Add(PosSearchLiteral[x]);
                }
            }
            //Buscamos la mayor cantidad de palabras de la query que esten contenidas en una ventana de tamaño SnippetLen
            while (Snippetwords.Count != 0 && wordsList.Count < 5)
            {
                (int, List<string>) tuple = DistanceWord.DistanceSnippet(Snippetwords, document);
                wordsList.Add(tuple.Item1);
                //Actualizamos la lista de palabras
                Snippetwords = tuple.Item2;
            }
            return BuildSinipped(document, wordsList);
    }
    /// <summary>Metodo para construir los snippets</summary>
    /// <param name="document">Documento</param>
    /// <param name="Snippetwords">Lista de las posiciones de las palabras</param>
    /// <returns>Snippets y posiciones en las que de encuentran</returns>
    private static (string[],int[]) BuildSinipped(Document document, List<int> Snippetwords)
    {
        Snippetwords.Sort();
        string[] doc = File.ReadAllLines(document.Path);
        string[] addSnippet = new string[Snippetwords.Count];
        int[] addposSnippet = new int[Snippetwords.Count];
        //Recorremos el documento
        int i=0;
        int cant = 0;
        for (int lineaInd = 0; lineaInd < doc.Length; lineaInd++)
        {
            string[] linea = doc[lineaInd].Split();
            for(int j=0;j<linea.Length;j++)
            {
                if(cant==Snippetwords[i])
                {
                    int cantWordsSnippet = 0;
                    StringBuilder sb = new StringBuilder();
                    bool nextline = lineaInd < doc.Length - 1;
                    int pos = j;
                    //Determinamos si es necesario coger palabras anteriores a la posicion indicada
                    if(nextline)
                    {
                        if(doc[lineaInd+1]=="" && linea.Length - j < 20) 
                        {
                            pos=(j < 5) ? 0 : j - 5;
                        }
                    }
                    else if(linea.Length - j < 20)
                    {
                        pos=(j < 5) ? 0 : j-5;
                    }
                    for (int w = pos; w < linea.Length; w++)
                    {
                        sb.Append(linea[w] + " ");
                        cantWordsSnippet++;
                        if (cantWordsSnippet == SnippetLen) break;
                    }
                    //Si no hemos alcanzado la longitud de nuestro snippet vamos a la linea siguiente
                    if (cantWordsSnippet < SnippetLen && nextline)
                    {
                        int ind = lineaInd + 1;
                        if (doc[lineaInd + 1] == "" && lineaInd < doc.Length - 2)
                        {
                            ind = lineaInd + 2;
                        }
                        string[] newlinea = doc[ind].Split();
                        for (int w = 0; w < newlinea.Length; w++)
                        {
                            sb.Append(newlinea[w] + " ");
                            cantWordsSnippet++;
                            if (cantWordsSnippet == SnippetLen) break;
                        }
                    }
                    //Guardamos el snippet
                    addSnippet[i] = sb.ToString();
                    addposSnippet[i] = lineaInd;
                    //Comprobamos el proximo indice
                    i++;
                }
                if(i==Snippetwords.Count) break; 
                string word = linea[j];
                //Quitamos los signos de puntuacion
                word = Document.SignPuntuation(word);
                //Si solo es un signo de puntuacion seguimos
                if (word == "") continue;
                cant++;                    
            }
            if(i == Snippetwords.Count) break;                
        }
        return (addSnippet,addposSnippet);
    }
    #endregion
}