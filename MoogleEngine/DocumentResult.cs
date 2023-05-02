using System.Text;

namespace MoogleEngine;

public class DocumentResult
{
    public static readonly int SnippetLen = 20;

    public DocumentResult(Document document, QueryClass query)
    {
        //Lista para las palabras de la query presentes en el documento
        var wordsDoc = new List<string>();
        //Lista para las palabras de la busqueda literal presentes en el documneto
        var wordsDocLiteral = new List<string>();
        //Lista para las palabras que no estan en el documento
        var wordsNoDoc = new List<string>();
        //Lista de posiciones para la busqueda literal
        var posSearchLiteral = new List<int>();
        var score = SimVectors(query, document, wordsDoc, wordsNoDoc, wordsDocLiteral);
        //Modificacion del Score por el operador cercania
        score += Close(wordsDocLiteral, document, query);
        if (!ResultSearch(posSearchLiteral, score, document, query)) return;
        //Buscamos los Snippets
        var aux = Snippet(wordsDoc, posSearchLiteral, document, query);
        Item = new SearchItem(document.Title, aux.Item1, aux.Item2, score, wordsNoDoc);
    }

    public SearchItem? Item { get; set; }

    #region SearchResult

    /// <summary>Metodo para calcular la similitud del coseno</summary>
    /// <param name="wordsDoc">Lista para las palabras de la query presentes en el documento</param>
    /// <param name="wordsDocLiteral">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="wordsNoDoc">Lista para las palabras de la query q no estan en el documento</param>
    /// <param name="document">Documento</param>
    /// <param name="query">Query</param>
    /// <returns>Score del Documento</returns>
    private static double SimVectors(QueryClass query, Document document, List<string> wordsDoc,
        List<string> wordsNoDoc, List<string> wordsDocLiteral)
    {
        double score = 0;
        double modQuery, queryXdoc;
        queryXdoc = 0;

        foreach (var wordDic in query.WordsQuery)
        {
            //Calculamos el producto de los 2 vectores
            queryXdoc += CorpusData.Vocabulary[wordDic.Key].WeightDoc[document.Index] * wordDic.Value;
            if (CorpusData.Vocabulary[wordDic.Key].WeightDoc[document.Index] * wordDic.Value != 0)
                wordsDoc.Add(wordDic.Key);

            if (CorpusData.Vocabulary[wordDic.Key].PosDoc[document.Index] != null)
                wordsDocLiteral.Add(wordDic.Key);
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
                    wordsDoc.Add(wordDic.Key);
            }

            modQuery = query.NormaStemmingSyn;
            if (queryXdoc != 0) factor = double.MinValue;
        }
        else
        {
            modQuery = query.Norma;
        }

        var modDoc = Document.Documents![document.Index].Norma;
        if (modQuery * modDoc != 0)
            //Calculamos la similitud del coseno
            score = factor + queryXdoc / Math.Sqrt(modQuery * modDoc);

        return score;
    }

    /// <summary>Metodo para determinar si el documento puede ser resultado de la busqueda</summary>
    /// <param name="score">Score del documento</param>
    /// <param name="posSearchLiteral">Lista de posiciones de la busqueda literal</param>
    /// <param name="query">Query</param>
    /// <param name="doc">Documento</param>
    /// <returns>Si el documento debe ser devuelto</returns>
    private static bool ResultSearch(List<int> posSearchLiteral, double score, Document doc, QueryClass query)
    {
        if (score == 0) return false;
        //Comprobamos el operador de Exclusion
        if (query.Exclude.Any(t => CorpusData.Vocabulary[t].PosDoc[doc.Index] != null))
        {
            return false;
        }

        //Comprobamos el operador de Inclusion
        foreach (var t in query.Include)
            if (CorpusData.Vocabulary[t].PosDoc[doc.Index] == null)
                return false;

        //Comprobamos el operador de busqueda literal
        return SearchLiteralOperator(doc, query, posSearchLiteral);
    }

    /// <summary>Metodo para comprobar la busqueda literal</summary>
    /// <param name="doc">Documento</param>
    /// <param name="posSearchLiteral">Lista de posiciones de la busqueda literal</param>
    /// <param name="query">Query</param>
    /// <returns>Bool indicando si el documento es valido para el operador busqueda literal</returns>
    private static bool SearchLiteralOperator(Document doc, QueryClass query, List<int> posSearchLiteral)
    {
        foreach (var t in query.SearchLiteralWords)
        {
            //Evaluamos los parametros para la busqueda literal
            foreach (var t1 in t)
            {
                if (t1 == "?") continue;
                if (CorpusData.Vocabulary[t1].PosDoc[doc.Index] == null) return false;
            }

            var posLiteral = DistanceWord.DistanceLiteral(t, doc);
            if (posLiteral == -1) return false;
            //Guardamos la posicion de la busqueda literal encontrada
            posSearchLiteral.Add(posLiteral);
        }

        return true;
    }

    /// <summary>Metodo obtener el Ranking de la cercania</summary>
    /// <param name="words">Lista de palabras</param>
    /// <param name="document">Documento</param>
    /// <param name="query">Query</param>
    /// <returns>Cantidad a incrementar en el score por el operador cercania</returns>
    private static double Close(List<string> words, Document document, QueryClass query)
    {
        double sumScore = 0;
        foreach (var wordList in query.CloseWords)
        {
            var close = true;
            //Comprobamos que las palabras del operador cercania esten en nuestra lista de palabras
            foreach (var t in wordList)
                if (!words.Contains(t))
                {
                    close = false;
                    break;
                }

            //Si hemos encontrado todas las palabras de nuestro operador cercania buscamos la minima distancia
            if (close)
            {
                var n = DistanceWord.DistanceClose(wordList, document);
                if (n == int.MaxValue) continue;
                sumScore += 100 / ((double)n - 1);
            }
        }

        return sumScore;
    }

    #endregion

    #region Snippet

    /// <summary>Metodo para buscar los snippets</summary>
    /// <param name="document">Documento</param>
    /// <param name="query">Query</param>
    /// <param name="posSearchLiteral">Lista de posiciones de la busqueda literal</param>
    /// <param name="snippetWords">Lista de las posiciones de las palabras</param>
    /// <returns>Snippets y posiciones en las que de encuentran</returns>
    private static (string[], int[]) Snippet(List<string> snippetWords, List<int> posSearchLiteral, Document document,
        QueryClass query)
    {
        var wordsList = new List<int>();
        //Comprobamos si tenemos resultados de busqueda literal
        if (query.SearchLiteralWords.Count > 0)
            for (var x = 0; x < query.SearchLiteralWords.Count; x++)
            {
                //Removemos la palabra de la busqueda literal, de la lista de palabras del snippet
                foreach (var y in query.SearchLiteralWords[x])
                    if (snippetWords.Contains(y))
                        snippetWords.Remove(y);

                //Guardamos la posicion del snippet
                wordsList.Add(posSearchLiteral[x]);
            }

        //Buscamos la mayor cantidad de palabras de la query que esten contenidas en una ventana de tamaño SnippetLen
        while (snippetWords.Count != 0 && wordsList.Count < 5)
        {
            var tuple = DistanceWord.DistanceSnippet(snippetWords, document);
            wordsList.Add(tuple.Item1);
            //Actualizamos la lista de palabras
            snippetWords = tuple.Item2;
        }

        return BuildSinipped(document, wordsList);
    }

    /// <summary>Metodo para construir los snippets</summary>
    /// <param name="document">Documento</param>
    /// <param name="snippetWords">Lista de las posiciones de las palabras</param>
    /// <returns>Snippets y posiciones en las que de encuentran</returns>
    private static (string[], int[]) BuildSinipped(Document document, List<int> snippetWords)
    {
        snippetWords.Sort();
        var doc = File.ReadAllLines(document.Path);
        var addSnippet = new string[snippetWords.Count];
        var addposSnippet = new int[snippetWords.Count];
        //Recorremos el documento
        var i = 0;
        var cant = 0;
        for (var lineaInd = 0; lineaInd < doc.Length; lineaInd++)
        {
            var linea = doc[lineaInd].Split();
            for (var j = 0; j < linea.Length; j++)
            {
                if (cant == snippetWords[i])
                {
                    var cantWordsSnippet = 0;
                    var sb = new StringBuilder();
                    var nextline = lineaInd < doc.Length - 1;
                    var pos = j;
                    //Determinamos si es necesario coger palabras anteriores a la posicion indicada
                    if (nextline)
                    {
                        if (doc[lineaInd + 1] == "" && linea.Length - j < 20) pos = j < 5 ? 0 : j - 5;
                    }
                    else if (linea.Length - j < 20)
                    {
                        pos = j < 5 ? 0 : j - 5;
                    }

                    for (var w = pos; w < linea.Length; w++)
                    {
                        sb.Append(linea[w] + " ");
                        cantWordsSnippet++;
                        if (cantWordsSnippet == SnippetLen) break;
                    }

                    //Si no hemos alcanzado la longitud de nuestro snippet vamos a la linea siguiente
                    if (cantWordsSnippet < SnippetLen && nextline)
                    {
                        var ind = lineaInd + 1;
                        if (doc[lineaInd + 1] == "" && lineaInd < doc.Length - 2) ind = lineaInd + 2;

                        var newline = doc[ind].Split();
                        foreach (var t in newline)
                        {
                            sb.Append(t + " ");
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

                if (i == snippetWords.Count) break;
                var word = linea[j];
                //Quitamos los signos de puntuacion
                word = Document.SignPunctuation(word);
                //Si solo es un signo de puntuacion seguimos
                if (word == "") continue;
                cant++;
            }

            if (i == snippetWords.Count) break;
        }

        return (addSnippet, addposSnippet);
    }

    #endregion
}