namespace MoogleEngine;

public class Synonymous
{
    public List<string[]> synonymous { get; set; }
}
public class QueryClass
{
    //Guardar los pesos de las palabras de la query
    public double[] vectorC;
    //Guardar los documentos que resultan de la busqueda
    public List<Document> resultsearchDoc = new List<Document>();
    //Guardar el score de cada documento que resulta de la busqueda
    public List<double> Score = new List<double>();
    //Guardar con las palabras encontradas en cada documento
    public List<List<string>> Snippet = new List<List<string>>();
    //Guardar los Snippets resultantes de cada documento
    public List<string[]> SnippetResult = new List<string[]>();
    //Guardar la linea correspondiente a los Snippets en cada documento resultante de la busqueda
    public List<int[]> Pos_SnippetResult = new List<int[]>();
    //Guardar las palabras del operador Excluir
    public List<string> Exclude = new List<string>();
    //Guardar las palabras del operador Incluir
    public List<string> Include = new List<string>();
    //Guardar las palabras del operador Cercania por cada grupo de palabras cercanas
    public List<List<string>> Close = new List<List<string>>();
    //Guardar las palabras del operador Relevancia y su respectiva relevancia
    public Dictionary<string, int> highest_relevance = new Dictionary<string, int>();
    //Bool para la presencia de busqueda literal
    public bool SearchLiteral;
    //Guardar las palabras de la busqueda literal
    public List<List<string>> SearchLiteral_words = new List<List<string>>();
    //Guardar la posicion de las palabras de la busqueda literal
    public List<List<int>> Pos_SearchLiteral = new List<List<int>>();
    //Guardar las palabras con la misma raiz q las de nuestra query
    public List<string> words_Stemming = new List<string>();
    //Guardar los sinonimos de las palabras de nustra query
    public List<string> words_Synonymous = new List<string>();
    //Guardar los sinonimos cargados de nuestro json
    public static List<string[]> synonymous;
    //Guardar el texto de nustra query
    public string txt;
    //Guardar la maxima frecuencia de la query
    public double max = 1;
    public QueryClass(string s)
    {
        this.txt = s;
        //Quitamos los esapcios y los signos de puntiucion
        Document.Token(new string[] { s }, Document.cantdoc, this);
        //Creamos un nuevo arreglo para guardar los pesos de la query
        this.vectorC = new double[BuildIndex.cantwords];

    }
    /// <summary>Metodo para los operadores de busqueda</summary>
    /// <param name="word">String que contiene los operadores</param>
    public void Operators(string word)
    {
        bool operators = false;
        if (!operators && SearchLiteral_Operator(word))
        {
            operators = true;
        }
        if (!operators && Close_Operator(word))
        {
            operators = true;
        }
        if (!operators && Exclude_Operator(word))
        {
            operators = true;
        }
        if (!operators && Include_Operator(word))
        {
            operators = true;
        }
        if (!operators && highest_relevance_Operator(word))
        {
            operators = true;
        }
        //Si no hemos encontrado un operador procedemos la busqueda de la palabra
        if (!operators)
        {
            //Comprobamos si la palabra a buscar existe en nuestro sistema
            if (BuildIndex.dic.ContainsKey(word))
            {
                BuildIndex.InsertWord(word, 0, 0, this);
            }
            else
            {
                //Si la palabra no existe procedemos a dar una recomendacion
                word = suggestion(word);
            }
            //Buscamos los sinonimos y las raices de la palabra
            Search_Stemming(word);
            Search_Synonymous(word);
        }
    }
    /// <summary>Metodo para el operador de busqueda literal</summary>
    /// <param name="word">String que contiene los operadores</param>
    private bool SearchLiteral_Operator(string word)
    {
        if (word[0] == '"' && !SearchLiteral)
        {
            //Agregamos una nueva lista de busqueda literal
            SearchLiteral_words.Add(new List<string>());
            //Activamos la condicion de busqueda literal
            SearchLiteral = true;
        }
        if (SearchLiteral)
        {
            //Comprobamos si las palabras pertenecientes a la busqueda han terminado
            if (word[word.Length - 1] == '"') SearchLiteral = false;
            word = Document.Sign_Puntuation(word);
            if (word == "") return true;
            if (BuildIndex.dic.ContainsKey(word))
            {
                SearchLiteral_words[SearchLiteral_words.Count - 1].Add(word);
                BuildIndex.InsertWord(word, 0, 0, this);
            }
            else
            {
                string a = suggestion(word);
                SearchLiteral_words[SearchLiteral_words.Count - 1].Add(a);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador cercania</summary>
    /// <param name="word">String que contiene los operadores</param>
    private bool Close_Operator(string word)
    {
        List<string> close = new List<string>();
        string[] close_words = word.Split('~');
        //Si nuestro arreglo tienen mas de dos elementos estamos en presencia del operador
        if (close_words.Length > 1)
        {
            for (int m = 0; m < close_words.Length; m++)
            {
                if (close_words[m] == "") continue;
                close_words[m] = Document.Sign_Puntuation(close_words[m]);
                if (BuildIndex.dic.ContainsKey(close_words[m]))
                {
                    BuildIndex.InsertWord(close_words[m], 0, 0, this);
                }
                else
                {
                    close_words[m] = suggestion(close_words[m]);
                }
                if (!close.Contains(close_words[m]))
                {
                    close.Add(close_words[m]);
                }
            }
            if (close.Count != 0)
            {
                Close.Add(close);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador exclusion</summary>
    /// <param name="word">String que contiene los operadores</param>
    private bool Exclude_Operator(string word)
    {
        if (word[0] == '!')
        {
            word = Document.Sign_Puntuation(word);
            if (word == "") return true;
            if (BuildIndex.dic.ContainsKey(word))
            {
                Exclude.Add(word);
                BuildIndex.InsertWord(word, 0, 0, this);
            }
            else
            {
                string a = suggestion(word);
                Exclude.Add(a);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador inclusion</summary>
    /// <param name="word">String que contiene los operadores</param>
    private bool Include_Operator(string word)
    {
        if (word[0] == '^')
        {
            word = Document.Sign_Puntuation(word);
            if (word == "") return true;
            if (BuildIndex.dic.ContainsKey(word))
            {
                Include.Add(word);
                BuildIndex.InsertWord(word, 0, 0, this);
            }
            else
            {
                string a = suggestion(word);
                Include.Add(a);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador MayorRelevancia</summary>
    /// <param name="word">String que contiene los operadores</param>
    private bool highest_relevance_Operator(string word)
    {
        if (word[0] == '*')
        {
            //Buscamos la cantidad de *
            int a = 0;
            while (word[a] == '*')
            {
                a++;
                if (a == word.Length) break;
            }
            word = Document.Sign_Puntuation(word);
            if (word == "") return true;
            if (BuildIndex.dic.ContainsKey(word))
            {
                highest_relevance.Add(word, a + 1);
                BuildIndex.InsertWord(word, 0, 0, this);
            }
            else
            {
                string b = suggestion(word);
                highest_relevance.Add(b, a + 1);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para buscar las raices</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void Search_Stemming(string word)
    {
        //Hallamos la raiz de la palabra
        string stemmer = Snowball.Stemmer(word);
        if (stemmer == "") return;
        foreach (var i in BuildIndex.dic)
        {
            //Comprobamos q las primeras letras sean iguales
            if (i.Key[0] == stemmer[0] && word != i.Key)
            {
                if (Snowball.Stemmer(i.Key) == stemmer)
                {
                    BuildIndex.InsertWord(i.Key, 0, 0, this);
                    words_Stemming.Add(i.Key);
                }
            }
        }
    }
    /// <summary>Metodo para buscar los sinonimos</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void Search_Synonymous(string word)
    {
        //Recorremos la lista de los sinonimos
        foreach (var line in QueryClass.synonymous)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == word)
                {
                    //Si nos encontramos una palabra igual a word todas las palabras del arreglo seran sus sinonimos 
                    for (int m = 0; m < line.Length; m++)
                    {
                        if (line[m] != word && BuildIndex.dic.ContainsKey(line[m]))
                        {
                            BuildIndex.InsertWord(line[m], 0, 0, this);
                            words_Synonymous.Add(line[m]);
                        }
                    }
                    break;
                }
            }
        }
    }
    /// <summary>Metodo para dar las recomendaciones</summary>
    /// <param name="word">String q contiene la palabra</param>
    private string suggestion(string word)
    {
        string a = suggestion_word(word);
        BuildIndex.InsertWord(a, 0, 0, this);
        //MOdifiquemos nuestro txt de la query por la palabra recomendada
        for (int m = 0; m <= txt.Length - word.Length; m++)
        {
            if (word == txt.Substring(m, word.Length).ToLower())
            {
                txt = txt.Substring(0, m) + a + txt.Substring(m + word.Length, txt.Length - word.Length - m);
                break;
            }
        }
        return a;
    }
    /// <summary>Metodo para encontrar la palabra mas cercana</summary>
    /// <param name="word">String q contiene la palabra</param>
    private string suggestion_word(string word)
    {
        string suggestion = "";
        double suggestionTF_IDF = 0;
        double changes = int.MaxValue;
        foreach (var i in BuildIndex.dic)
        {
            double dist = LevenshteinDistance(word, i.Key);
            //Nos quedamos con la palabra que posea menos cambios
            if (dist < changes)
            {
                suggestion = i.Key;
                changes = dist;
                double sum = 0;
                for (int j = 0; j < Document.cantdoc; j++)
                {
                    sum += i.Value.weight_doc[j];
                }
                suggestionTF_IDF = sum;
            }
            //Si las palabras poseen la misma cantidad de cambios recomendamos la q mas peso tenga en el corpus
            if (dist == changes)
            {
                double sum = 0;
                for (int j = 0; j < Document.cantdoc; j++)
                {
                    sum += i.Value.weight_doc[j];
                }
                if (sum > suggestionTF_IDF)
                {
                    suggestionTF_IDF = sum;
                    suggestion = i.Key;
                }
            }
        }
        return suggestion;
    }
    private static double LevenshteinDistance(string s, string t)
    {
        double porcentaje = 0;

        // d es una tabla con m+1 renglones y n+1 columnas
        int costo = 0;
        int m = s.Length;
        int n = t.Length;
        double[,] d = new double[m + 1, n + 1];

        // Verifica que exista algo que comparar
        if (n == 0) return m;
        if (m == 0) return n;

        // Llena la primera columna y la primera fila.
        for (int i = 0; i <= m; d[i, 0] = i++) ;
        for (int j = 0; j <= n; d[0, j] = j++) ;


        /// recorre la matriz llenando cada unos de los pesos.
        /// i columnas, j renglones
        for (int i = 1; i <= m; i++)
        {
            // recorre para j
            for (int j = 1; j <= n; j++)
            {
                /// si son iguales en posiciones equidistantes el peso es 0
                /// de lo contrario el peso suma a uno.
                costo = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1,  //Eliminacion
                            d[i, j - 1] + 1),                             //Insercion 
                            d[i - 1, j - 1] + costo);                     //Sustitucion
            }
        }

        /// Calculamos el porcentaje de cambios en la palabra.
        if (s.Length > t.Length)
            porcentaje = (1 - (d[m, n] / (double)s.Length)) * 100;
        else
            porcentaje = (1 - (d[m, n] / (double)t.Length)) * 100;
        //return porcentaje;
        return d[m, n];
    }
}