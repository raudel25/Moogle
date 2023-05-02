namespace MoogleEngine;

public class QueryClass
{
    //Guardar la maxima frecuencia de la query
    private int _max = 1;

    //Guardar la maxima frecuencia de la query con los sinonimos y las raices
    private int _maxStemmingSyn = 1;

    //Para determinar si no hay resultados
    private bool _noResults;

    //Bool para la presencia de busqueda literal
    private bool _searchLiteral;

    public QueryClass(string query)
    {
        SuggestionQuery = query;
        WordsQuery = new Dictionary<string, double>();
        WordsStemmingSyn = new Dictionary<string, double[]>();
        Exclude = new List<string>();
        Include = new List<string>();
        CloseWords = new List<List<string>>();
        SearchLiteralWords = new List<List<string>>();
        HighestRelevance = new Dictionary<string, int>();
        Operators(query);
        if (_noResults) return;
        TfIdfC();
        //Comprobamos la sugerencia
        if (SuggestionQuery == query) SuggestionQuery = "";
    }

    //Guardar la sugerencia para el usuario
    public string SuggestionQuery { get; private set; }

    //Guardar los pesos de las palabras de la query
    public Dictionary<string, double> WordsQuery { get; }

    //Guardar los sinonimos y las palabras con la misma raiz q las de nuestra query
    public Dictionary<string, double[]> WordsStemmingSyn { get; }
    public double Norma { get; private set; }

    public double NormaStemmingSyn { get; private set; }

    //Guardar las palabras del operador Excluir
    public List<string> Exclude { get; }

    //Guardar las palabras del operador Incluir
    public List<string> Include { get; }

    //Guardar las palabras del operador Cercania por cada grupo de palabras cercanas
    public List<List<string>> CloseWords { get; }

    //Guardar las palabras del operador Relevancia y su respectiva relevancia
    private Dictionary<string, int> HighestRelevance { get; }

    //Guardar las palabras de la busqueda literal
    public List<List<string>> SearchLiteralWords { get; }

    #region TfIdf

    /// <summary>Metodo para calcular el TfIdf de nuestra query</summary>
    private void TfIdfC()
    {
        foreach (var word in WordsQuery)
        {
            //Factor para modificar el peso de la palabra
            double a = 0;
            if (HighestRelevance.ContainsKey(word.Key)) a = HighestRelevance[word.Key];
            WordsQuery[word.Key] = Math.Pow(Math.E, a) * word.Value / _max *
                                   Math.Log(Document.CantDoc / CorpusData.Vocabulary[word.Key].WordCantDoc);
            Norma += WordsQuery[word.Key] * WordsQuery[word.Key];
        }

        foreach (var word in WordsStemmingSyn)
        {
            //Comprobamos si la palabra es raiz o sinonimo
            if (word.Value[0] != 0)
                WordsStemmingSyn[word.Key][0] = (word.Value[0] + word.Value[1]) / _maxStemmingSyn *
                                                Math.Log(Document.CantDoc /
                                                         CorpusData.Vocabulary[word.Key].WordCantDoc);
            else
                WordsStemmingSyn[word.Key][0] = (word.Value[0] + word.Value[1]) / (2 * (double)_maxStemmingSyn) *
                                                Math.Log(Document.CantDoc /
                                                         CorpusData.Vocabulary[word.Key].WordCantDoc);

            NormaStemmingSyn += WordsStemmingSyn[word.Key][0];
        }
    }

    #endregion

    #region FrecuencyQuery

    /// <summary>Metodo para calcular la frecuencia de la query</summary>
    /// <param name="word">String que contien la palabra</param>
    private void FrecuencyQuery(string word)
    {
        if (!WordsQuery.ContainsKey(word)) WordsQuery.Add(word, 0);
        WordsQuery[word]++;
        if (WordsQuery[word] > _max) _max = (int)WordsQuery[word];
    }

    /// <summary>Metodo para calcular la frecuencia de la query con las raices y los sinonimos</summary>
    /// <param name="word">String que contien la palabra</param>
    /// <param name="stemming">Sinonimos o raices</param>
    private void FrecuencyQueryStemmingSyn(string word, bool stemming)
    {
        if (!WordsStemmingSyn.ContainsKey(word)) WordsStemmingSyn.Add(word, new double[2]);
        if (stemming) WordsStemmingSyn[word][0]++;
        else WordsStemmingSyn[word][1]++;
        if (WordsStemmingSyn[word][0] + WordsStemmingSyn[word][1] > _maxStemmingSyn)
            _maxStemmingSyn = (int)(WordsStemmingSyn[word][0] + WordsStemmingSyn[word][1]);
    }

    #endregion

    #region Operators

    /// <summary>Metodo para los operadores de busqueda</summary>
    /// <param name="query">Texto de la Query</param>
    private void Operators(string query)
    {
        //Tokenizamos nuestro query
        var s = query.Split();
        foreach (var t in s)
        {
            var word = t;
            word = Document.SignPunctuation(word, true);
            if (word == "") continue;
            word = word.ToLower();
            if (SearchLiteralOperator(word)) continue;
            if (CloseOperator(word)) continue;
            if (ExcludeOperator(word)) continue;
            if (IncludeOperator(word)) continue;
            if (HighestRelevanceOperator(word)) continue;
            word = Document.SignPunctuation(word);
            if (word == "") continue;
            //Comprobamos si la palabra a buscar existe en nuestro sistema
            if (CorpusData.Vocabulary.ContainsKey(word))
                FrecuencyQuery(word);
            else
                //Si la palabra no existe procedemos a dar una recomendacion
                Suggestion(word);

            //Buscamos los sinonimos y las raices de la palabra
            SearchStemming(word);
            SearchSynonymous(word);
        }
    }

    /// <summary>Metodo para el operador de busqueda literal</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
    private bool SearchLiteralOperator(string word)
    {
        if (word[0] == '"' && !_searchLiteral)
        {
            //Agregamos una nueva lista de busqueda literal
            SearchLiteralWords.Add(new List<string>());
            //Activamos la condicion de busqueda literal
            _searchLiteral = true;
        }

        if (_searchLiteral)
        {
            if (word == "\"") return true;
            //Comprobamos si las palabras pertenecientes a la busqueda han terminado
            if (word[^1] == '"') _searchLiteral = false;
            //Comprobamos si estamos en presencia de un comodin
            if (word == "\"?" || word == "?\"") word = "?";
            if (word != "?") word = Document.SignPunctuation(word);
            if (word == "") return true;
            if (CorpusData.Vocabulary.ContainsKey(word) || word == "?")
            {
                SearchLiteralWords[^1].Add(word);
                if (word != "?") FrecuencyQuery(word);
            }
            else
            {
                //Si no esta la palabra en nuestro corpus no hay resultados
                _noResults = true;
                Suggestion(word);
            }

            return true;
        }

        return false;
    }

    /// <summary>Metodo para el operador cercania</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
    private bool CloseOperator(string word)
    {
        var close = new List<string>();
        var closeWords = word.Split('~');
        //Si nuestro arreglo tienen mas de dos elementos estamos en presencia del operador
        if (closeWords.Length > 1)
        {
            for (var m = 0; m < closeWords.Length; m++)
            {
                closeWords[m] = Document.SignPunctuation(closeWords[m]);
                if (closeWords[m] == "") continue;
                if (CorpusData.Vocabulary.ContainsKey(closeWords[m]))
                    FrecuencyQuery(closeWords[m]);
                else
                    Suggestion(closeWords[m]);

                close.Add(closeWords[m]);
            }

            if (close.Count != 0) CloseWords.Add(close);

            return true;
        }

        return false;
    }

    /// <summary>Metodo para el operador exclusion</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
    private bool ExcludeOperator(string word)
    {
        if (word[0] == '!')
        {
            word = Document.SignPunctuation(word);
            if (word == "") return true;
            if (CorpusData.Vocabulary.ContainsKey(word))
            {
                Exclude.Add(word);
                FrecuencyQuery(word);
            }
            else
            {
                Suggestion(word);
            }

            return true;
        }

        return false;
    }

    /// <summary>Metodo para el operador inclusion</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
    private bool IncludeOperator(string word)
    {
        if (word[0] == '^')
        {
            word = Document.SignPunctuation(word);
            if (word == "") return true;
            if (CorpusData.Vocabulary.ContainsKey(word))
            {
                Include.Add(word);
                FrecuencyQuery(word);
            }
            else
            {
                Suggestion(word);
                //Si no esta la palabra en nuestro corpus no hay resultados
                _noResults = true;
            }

            return true;
        }

        return false;
    }

    /// <summary>Metodo para el operador MayorRelevancia</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
    private bool HighestRelevanceOperator(string word)
    {
        if (word[0] == '*')
        {
            //Buscamos la cantidad de *
            var a = 0;
            while (word[a] == '*')
            {
                a++;
                if (a == word.Length) break;
            }

            word = Document.SignPunctuation(word);
            if (word == "") return true;
            if (CorpusData.Vocabulary.ContainsKey(word))
            {
                HighestRelevance.Add(word, a + 1);
                FrecuencyQuery(word);
            }
            else
            {
                Suggestion(word);
            }

            return true;
        }

        return false;
    }

    #endregion

    #region StemmingSynonymous

    /// <summary>Metodo para buscar las raices</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void SearchStemming(string word)
    {
        //Hallamos la raiz de la palabra
        var stemmer = Snowball.Stemmer(word);
        if (stemmer == "") return;
        foreach (var wordDic in CorpusData.Vocabulary)
            //Comprobamos q las primeras letras sean iguales
            if (wordDic.Key[0] == stemmer[0] && word != wordDic.Key)
                if (Snowball.Stemmer(wordDic.Key) == stemmer)
                    FrecuencyQueryStemmingSyn(wordDic.Key, true);
    }

    /// <summary>Metodo para buscar los sinonimos</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void SearchSynonymous(string word)
    {
        //Recorremos la lista de los sinonimos
        foreach (var line in CorpusData.Synonymous!)
            foreach (var t in line)
                if (t == word)
                {
                    //Si nos encontramos una palabra igual a word todas las palabras del arreglo seran sus sinonimos 
                    foreach (var t1 in line)
                        if (t1 != word && CorpusData.Vocabulary.ContainsKey(t1))
                            FrecuencyQueryStemmingSyn(t1, false);

                    break;
                }
    }

    #endregion

    #region Suggestion

    /// <summary>Metodo para dar las recomendaciones</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void Suggestion(string word)
    {
        var a = SuggestionWord(word);
        //Modifiquemos nuestro txt de la query por la palabra recomendada
        for (var m = 0; m <= SuggestionQuery.Length - word.Length; m++)
            if (word == SuggestionQuery.Substring(m, word.Length).ToLower())
            {
                SuggestionQuery = SuggestionQuery.Substring(0, m) + a +
                                  SuggestionQuery.Substring(m + word.Length, SuggestionQuery.Length - word.Length - m);
                break;
            }
    }

    /// <summary>Metodo para encontrar la palabra mas cercana</summary>
    /// <param name="word">String q contiene la palabra</param>
    /// <returns>Palabra con la sugerencia a la busqueda</returns>
    private static string SuggestionWord(string word)
    {
        var suggestion = "";
        double suggestionTfIdf = 0;
        var changes = int.MaxValue;
        double len = word.Length;
        foreach (var wordDic in CorpusData.Vocabulary)
        {
            if (Math.Abs(wordDic.Key.Length - len) > changes) continue;
            var dist = LevenshteinDistance(word, wordDic.Key, changes);
            //Nos quedamos con la palabra que posea menos cambios
            if (dist < changes)
            {
                suggestion = wordDic.Key;
                changes = dist;
                double sum = 0;
                for (var j = 0; j < Document.CantDoc; j++) sum += wordDic.Value.WeightDoc[j];

                suggestionTfIdf = sum;
            }

            //Si las palabras poseen la misma cantidad de cambios recomendamos la q mas peso tenga en el corpus
            if (dist == changes)
            {
                double sum = 0;
                for (var j = 0; j < Document.CantDoc; j++) sum += wordDic.Value.WeightDoc[j];

                if (sum > suggestionTfIdf)
                {
                    suggestionTfIdf = sum;
                    suggestion = wordDic.Key;
                }
            }
        }

        return suggestion;
    }

    /// <summary>Metodo para calcular la similitud entre dos palabras</summary>
    /// <param name="a">Palabra para realizar los cambios</param>
    /// <param name="b">Palabra original</param>
    /// <param name="actchange">Cantidad actual de cambios</param>
    /// <returns>Cantidad de cambios entre una palabra y otra</returns>
    private static int LevenshteinDistance(string a, string b, int actchange)
    {
        var m = a.Length;
        var n = b.Length;
        var change = new int[m + 1, n + 1];
        if (n == 0) return m;
        if (m == 0) return n;
        // Llenamos la primera columna y la primera fila.
        for (var i = 0; i <= m; i++)
        {
            change[i, 0] = 4 * i;
            if (i > 0)
                if (a[i - 1] == 'h')
                    change[i, 0] -= 2;
        }

        for (var j = 0; j <= n; j++)
        {
            change[0, j] = 4 * j;
            if (j > 0)
                if (b[j - 1] == 'h')
                    change[0, j] -= 2;
        }

        for (var i = 1; i <= Math.Min(m, n); i++)
        {
            var min = int.MaxValue;
            int cost;
            for (var j = i; j <= n; j++)
            {
                //Damos menos peso a los errores ortograficos
                cost = a[i - 1] == b[j - 1] ? 0 : OrtograficRule(a[i - 1], b[j - 1]);
                change[i, j] = Math.Min(Math.Min(change[i - 1, j] + (a[i - 1] == 'h' ? 2 : 4), //Eliminacion
                        change[i, j - 1] + (b[j - 1] == 'h' ? 2 : 4)), //Insercion 
                    change[i - 1, j - 1] + cost); //Sustitucion
                min = Math.Min(min, change[i, j]);
            }

            for (var j = i + 1; j <= m; j++)
            {
                //Damos menos peso a los errores ortograficos
                cost = a[j - 1] == b[i - 1] ? 0 : OrtograficRule(a[j - 1], b[i - 1]);
                change[j, i] = Math.Min(Math.Min(change[j - 1, i] + (a[j - 1] == 'h' ? 2 : 4), //Eliminacion
                        change[j, i - 1] + (b[i - 1] == 'h' ? 2 : 4)), //Insercion 
                    change[j - 1, i - 1] + cost); //Sustitucion  
                min = Math.Min(min, change[j, i]);
            }

            //Comprobamos si la cantidad de cambios que llevamos es mayor que la que ya teniamos como minima
            if (min > actchange) return int.MaxValue;
        }

        return change[m, n];
    }

    /// <summary>Determinar los errores ortograficos mas comunes</summary>
    /// <param name="a">Caracter a cambiar</param>
    /// <param name="b">Caraacter original</param>
    /// <returns>Peso reducido segun la regla</returns>
    private static int OrtograficRule(char a, char b)
    {
        int min;
        int max;
        if (a > b)
        {
            min = b;
            max = a;
        }
        else
        {
            min = a;
            max = b;
        }

        //Vocales con tilde
        if (min == 97 && max is >= 224 and <= 229) return 1;
        if (min == 101 && max is >= 232 and <= 235) return 1;
        if (min == 105 && max is >= 236 and <= 239) return 1;
        if (min == 111 && max is >= 242 and <= 246) return 1;
        if (min == 117 && max is >= 249 and <= 252) return 1;
        //c-s c-z j-g v-b
        if (min == 99 && max == 115) return 2;
        if (min == 115 && max == 122) return 2;
        if (min == 103 && max == 106) return 2;
        if (min == 98 && max == 118) return 2;
        //m-n ñ-n x-c x-s l-r
        if (min == 109 && max == 110) return 3;
        if (min == 110 && max == 241) return 3;
        if (min == 99 && max == 120) return 3;
        if (min == 115 && max == 120) return 3;
        if (min == 108 && max == 114) return 3;
        return 4;
    }

    #endregion
}