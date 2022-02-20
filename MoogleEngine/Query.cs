using System.Text;

namespace MoogleEngine;

public class QueryClass
{
    #region Public_Property
    //Guardar los documentos que resultan de la busqueda
    public List<Document> resultsearchDoc {get; private set;}
    //Guardar los Snippets resultantes de cada documento
    public List<string[]> SnippetResult {get; private set;}
    //Guardar la linea correspondiente a los Snippets en cada documento resultante de la busqueda
    public List<int[]> Pos_SnippetResult {get; private set;}
    //Guardar el score de cada documento que resulta de la busqueda
    public List<double> Score {get; private set;}
    //Guardar las palabras que no estan en el documento que vamos a devolver
    public List<List<string>> Words_not_result {get; private set;}
    //Guardar el texto de nustra query para dar la sugerencia
    public string txt {get; private set;}
    //Guardar el texto de la query original
    public string original {get; private set;}
    //Para determinar si no hay resultados
    public bool no_results = false;
    public static int Snippet_len = 20;
    #endregion 
    #region Private_Property
    //Guardar los pesos de las palabras de la query
    private Dictionary<string, double> words_query = new Dictionary<string, double>();
     //Guardar los sinonimos y las palabras con la misma raiz q las de nuestra query
    private Dictionary<string, double[]> words_Stemming_Syn = new Dictionary<string, double[]>();
    private double norma {get; set;}
    private double norma_Stemming_Syn {get; set;}
    //Guardar con las palabras encontradas en cada documento
    private List<List<string>> Snippet_words = new List<List<string>>();
    //Guardar las palabras del operador Excluir
    private List<string> Exclude = new List<string>();
    //Guardar las palabras del operador Incluir
    private List<string> Include = new List<string>();
    //Guardar las palabras del operador Cercania por cada grupo de palabras cercanas
    private List<List<string>> Close_Words = new List<List<string>>();
    //Guardar las palabras del operador Relevancia y su respectiva relevancia
    private Dictionary<string, int> highest_relevance = new Dictionary<string, int>();
    //Bool para la presencia de busqueda literal
    private bool SearchLiteral;
    //Guardar las palabras de la busqueda literal
    private List<List<string>> SearchLiteral_words = new List<List<string>>();
    //Guardar la posicion de las palabras de la busqueda literal
    private List<List<int>> Pos_SearchLiteral = new List<List<int>>();
    //Guardar la maxima frecuencia de la query
    private int max = 1;
    //Guardar la maxima frecuencia de la query con los sinonimos y las raices
    private int max_Stemming_Syn = 1;
    #endregion
    public QueryClass(string s)
    {
        this.txt = s;
        this.original = s;
        this.resultsearchDoc = new List<Document>();
        this.Score= new List<double>();
        this.SnippetResult = new List<string[]>();
        this.Pos_SnippetResult = new List<int[]>();
        this.Words_not_result=new List<List<string>>();
        Operators();
        if(no_results) return;
        TF_idfC();
        SimVectors();
        Snippet();
    }
    #region Frecuency_Query
    /// <summary>Metodo para calcular la frecuencia de la query</summary>
    /// <param name="word">String que contien la palabra</param>
    private void Frecuency_Query(string word)
    {
        if (!words_query.ContainsKey(word)) words_query.Add(word, 0);
        words_query[word]++;
        if (words_query[word] > max) max = (int)words_query[word];
    }
    /// <summary>Metodo para calcular la frecuencia de la query con las raices y los sinonimos</summary>
    /// <param name="word">String que contien la palabra</param>
    /// <param name="stemming">Sinonimos o raices</param>
    private void Frecuency_Query_Stemming_Syn(string word, bool stemming)
    {
        if (!words_Stemming_Syn.ContainsKey(word)) words_Stemming_Syn.Add(word, new double[2]);
        if (stemming) words_Stemming_Syn[word][0]++;
        else words_Stemming_Syn[word][1]++;
        if (words_Stemming_Syn[word][0] + words_Stemming_Syn[word][1] > max_Stemming_Syn) max_Stemming_Syn = (int)(words_Stemming_Syn[word][0] + words_Stemming_Syn[word][1]);
    }
    #endregion

    #region Operators
    /// <summary>Metodo para los operadores de busqueda</summary>
    public void Operators()
    {
        //Tokenizamos nuestro query
        string[] s=original.Split(' ');
        for(int i=0;i<s.Length;i++)
        {
            string word = s[i];
            if(word == "") continue;
            word = Document.Sign_Puntuation(word,true);
            if(word == "") continue;
            word = word.ToLower();
            if (SearchLiteral_Operator(word)) continue;
            if (Close_Operator(word)) continue;
            if (Exclude_Operator(word)) continue;
            if (Include_Operator(word)) continue;
            if (highest_relevance_Operator(word)) continue;
            word=Document.Sign_Puntuation(word);
            if(word=="") continue;
            //Comprobamos si la palabra a buscar existe en nuestro sistema
            if (Corpus_Data.vocabulary.ContainsKey(word))
            {
                Frecuency_Query(word);
            }
            else
            {
                //Si la palabra no existe procedemos a dar una recomendacion
                suggestion(word);
            }
            //Buscamos los sinonimos y las raices de la palabra
            Search_Stemming(word);
            Search_Synonymous(word);
        }
    }
    /// <summary>Metodo para el operador de busqueda literal</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
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
            if(word=="\"") return true;
            //Comprobamos si las palabras pertenecientes a la busqueda han terminado
            if (word[word.Length - 1] == '"') SearchLiteral = false;
            //Comprobamos si estamos en presencia de un comodin
            if(word=="\"?"||word=="?\"") word="?";
            if(word !="?") word = Document.Sign_Puntuation(word);
            if (word == "") return true;
            if (Corpus_Data.vocabulary.ContainsKey(word)||word=="?")
            {
                SearchLiteral_words[SearchLiteral_words.Count - 1].Add(word);
                if(word !="?") Frecuency_Query(word);
            }
            else
            {
                //Si no esta la palabra en nuestro corpus no hay resultados
                no_results=true;
                suggestion(word);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador cercania</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
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
                if (Corpus_Data.vocabulary.ContainsKey(close_words[m]))
                {
                    Frecuency_Query(close_words[m]);
                }
                else
                {   
                    suggestion(close_words[m]);
                }
                close.Add(close_words[m]);
            }
            if (close.Count != 0)
            {
                Close_Words.Add(close);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador exclusion</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
    private bool Exclude_Operator(string word)
    {
        if (word[0] == '!')
        {
            word = Document.Sign_Puntuation(word);
            if (word == "") return true;
            if (Corpus_Data.vocabulary.ContainsKey(word))
            {
                Exclude.Add(word);
                Frecuency_Query(word);
            }
            else
            {
                suggestion(word);
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador inclusion</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
    private bool Include_Operator(string word)
    {
        if (word[0] == '^')
        {
            word = Document.Sign_Puntuation(word);
            if (word == "") return true;
            if (Corpus_Data.vocabulary.ContainsKey(word))
            {
                Include.Add(word);
                Frecuency_Query(word);
            }
            else
            {
                suggestion(word);
                //Si no esta la palabra en nuestro corpus no hay resultados
                no_results=true;
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador MayorRelevancia</summary>
    /// <param name="word">String que contiene los operadores</param>
    /// <returns>Bool indicando si el operador fue encontrado</returns>
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
            if (Corpus_Data.vocabulary.ContainsKey(word))
            {
                highest_relevance.Add(word, a + 1);
                Frecuency_Query(word);
            }
            else
            {
                suggestion(word);
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Stemming_Synonymous
    /// <summary>Metodo para buscar las raices</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void Search_Stemming(string word)
    {
        //Hallamos la raiz de la palabra
        string stemmer = Snowball.Stemmer(word);
        if (stemmer == "") return;
        foreach (var word_dic in Corpus_Data.vocabulary)
        {
            //Comprobamos q las primeras letras sean iguales
            if (word_dic.Key[0] == stemmer[0] && word != word_dic.Key)
            {
                if (Snowball.Stemmer(word_dic.Key) == stemmer)
                {
                    Frecuency_Query_Stemming_Syn(word_dic.Key, true);
                }
            }
        }
    }
    /// <summary>Metodo para buscar los sinonimos</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void Search_Synonymous(string word)
    {
        //Recorremos la lista de los sinonimos
        foreach (var line in Corpus_Data.synonymous)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == word)
                {
                    //Si nos encontramos una palabra igual a word todas las palabras del arreglo seran sus sinonimos 
                    for (int m = 0; m < line.Length; m++)
                    {
                        if (line[m] != word && Corpus_Data.vocabulary.ContainsKey(line[m]))
                        {
                            Frecuency_Query_Stemming_Syn(line[m], false); ;
                        }
                    }
                    break;
                }
            }
        }
    }
    #endregion

    #region Suggestion
    /// <summary>Metodo para dar las recomendaciones</summary>
    /// <param name="word">String q contiene la palabra</param>
    private void suggestion(string word)
    {
        string a = suggestion_word(word);
        //Modifiquemos nuestro txt de la query por la palabra recomendada
        for (int m = 0; m <= txt.Length - word.Length; m++)
        {
            if (word == txt.Substring(m, word.Length).ToLower())
            {
                txt = txt.Substring(0, m) + a + txt.Substring(m + word.Length, txt.Length - word.Length - m);
                break;
            }
        }
    }
    /// <summary>Metodo para encontrar la palabra mas cercana</summary>
    /// <param name="word">String q contiene la palabra</param>
    /// <returns>Palabra con la sugerencia a la busqueda</returns>
    private string suggestion_word(string word)
    {
        string suggestion = "";
        double suggestionTF_IDF = 0;
        int changes = int.MaxValue;
        foreach (var word_dic in Corpus_Data.vocabulary)
        {
            int dist = Levenshtein_Distance(word, word_dic.Key);
            //Nos quedamos con la palabra que posea menos cambios
            if (dist < changes)
            {
                suggestion = word_dic.Key;
                changes = dist;
                double sum = 0;
                for (int j = 0; j < Document.cantdoc; j++)
                {
                    sum += word_dic.Value.weight_doc[j];
                }
                suggestionTF_IDF = sum;
            }
            //Si las palabras poseen la misma cantidad de cambios recomendamos la q mas peso tenga en el corpus
            if (dist == changes)
            {
                double sum = 0;
                for (int j = 0; j < Document.cantdoc; j++)
                {
                    sum += word_dic.Value.weight_doc[j];
                }
                if (sum > suggestionTF_IDF)
                {
                    suggestionTF_IDF = sum;
                    suggestion = word_dic.Key;
                }
            }
        }
        return suggestion;
    }
    /// <summary>Metodo para calcular la similitud entre dos palabras</summary>
    /// <param name="a">Palabra para realizar los cambios</param>
    /// <param name="b">Palabra original</param>
    /// <returns>Cantidad de cambios entre una palabra y otra</returns>
    private static int Levenshtein_Distance(string a, string b)
    {
        int cost = 0;
        int m = a.Length;
        int n = b.Length;
        int[,] change = new int[m + 1, n + 1];
        if (n == 0) return m;
        if (m == 0) return n;
        // Llenamos la primera columna y la primera fila.
        for (int i = 0; i <= m; i++)
        {
            change[i, 0] = 4*i;
            if(i>0)
            {
                if(a[i-1]=='h') change[i,0]-=2;
            }
        }
        for (int j = 0; j <= n; j++)
        {
            change[0, j] = 4*j;
            if(j>0)
            {
                if(b[j-1]=='h') change[0,j]-=2;
            }
        }
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                //Damos menos peso a los errores ortograficos
                cost = (a[i - 1] == b[j - 1]) ? 0 : OrtograficRule(a[i-1],b[j-1]);
                change[i, j] = Math.Min(Math.Min(change[i - 1, j] + ((a[i - 1]=='h') ? 2 : 4),  //Eliminacion
                            change[i, j - 1] + ((b[j - 1]=='h') ? 2 : 4)),                             //Insercion 
                            change[i - 1, j - 1] + cost);                     //Sustitucion
            }
        }
        return change[m, n];
    }
    /// <summary>Determinar los errores ortograficos mas comunes</summary>
    /// <param name="a">Caracter a cambiar</param>
    /// <param name="b">Caraacter original</param>
    /// <returns>Peso reducido segun la regla</returns>
    private static int OrtograficRule(char a,char b)
    {
        int min; int max;
        if((int)a>(int)b)
        {
            min=(int)b; max=(int)a;
        }
        else
        {
            min=(int)a; max=(int)b;
        }
        //Vocales con tilde
        if(min==97 && 224<=max && max<=229) return 1;
        if(min==101 && 232<=max && max<=235) return 1;
        if(min==105 && 236<=max && max<=239) return 1;
        if(min==111 && 242<=max && max<=246) return 1;
        if(min==117 && 249<=max && max<=252) return 1;
        //c-s c-z j-g v-b
        if(min==99 && max==115) return 2;
        if(min==115 && max==122) return 2;
        if(min==103 && max==106) return 2;
        if(min==98 && max==118) return 2;
        //m-n ñ-n x-c x-s l-r
        if(min==109 && max==110) return 3;
        if(min==110 && max==241) return 3;
        if(min==99 && max==120) return 3;
        if(min==115 && max==120) return 3;
        if(min==108 && max==114) return 3;
        return 4;
    }
    #endregion

    #region TF_IDF
    /// <summary>Metodo para calcular el Tf_idf de nuestra query</summary>
    private void TF_idfC()
    {
        foreach (var word in words_query)
        {
            //Factor para modificar el peso de la palabra
            double a = 1;
            if (highest_relevance.ContainsKey(word.Key)) a = highest_relevance[word.Key];
            words_query[word.Key] = (a * word.Value / (double)max) * Math.Log((double)Document.cantdoc / (double)Corpus_Data.vocabulary[word.Key].word_cant_doc);
            norma += words_query[word.Key] * words_query[word.Key];
        }
        foreach (var word in words_Stemming_Syn)
        {
            //Comprobamos si la palabra es raiz o sinonimo
            if (word.Value[0] != 0)
            {
                words_Stemming_Syn[word.Key][0] = ((word.Value[0] + word.Value[1]) / (double)max_Stemming_Syn) * Math.Log((double)Document.cantdoc / (double)Corpus_Data.vocabulary[word.Key].word_cant_doc);
            }
            else
            {
                words_Stemming_Syn[word.Key][0] = ((word.Value[0] + word.Value[1]) / (2 * (double)max_Stemming_Syn)) * Math.Log((double)Document.cantdoc / (double)Corpus_Data.vocabulary[word.Key].word_cant_doc);
            }
            norma_Stemming_Syn += words_Stemming_Syn[word.Key][0];
        }
    }
    #endregion

    #region Search_Result
    /// <summary>Metodo para calcular la similitud del coseno</summary>
    private void SimVectors()
    {
        double[] score = new double[Document.cantdoc];
        double mod_query = 0, query_x_doc = 0, mod_doc = 0;
        for (int i = 0; i < Document.cantdoc; i++)
        {
            query_x_doc = 0; mod_doc = 0;
            //Lista para las palabras de la query presentes en el documento
            List<string> words_doc = new List<string>();
            //Lista para las palabras de la busqueda literal presentes en el documneto
            List<string> words_docLiteral = new List<string>();
            //Lista para las palabras que no estan en el documento
            List<string> words_no_doc =new List<string>();
            foreach (var word_dic in words_query)
            {
                //Calculamos el producto de los 2 vectores
                query_x_doc += Corpus_Data.vocabulary[word_dic.Key].weight_doc[i] * word_dic.Value;
                if (Corpus_Data.vocabulary[word_dic.Key].weight_doc[i] * word_dic.Value != 0)
                {
                    words_doc.Add(word_dic.Key);
                }
                if (Corpus_Data.vocabulary[word_dic.Key].Pos_doc[i] != null)
                {
                    words_docLiteral.Add(word_dic.Key);
                }
                else words_no_doc.Add(word_dic.Key);
            }
            double factor = 0;
            if (words_doc.Count == 0)
            {
                //Si no tenemos resultados buscamos las raices y los sinonimos
                foreach (var word_dic in words_Stemming_Syn)
                {
                    query_x_doc += Corpus_Data.vocabulary[word_dic.Key].weight_doc[i] * word_dic.Value[0];
                    if (Corpus_Data.vocabulary[word_dic.Key].weight_doc[i] * word_dic.Value[0] != 0)
                    {
                        words_doc.Add(word_dic.Key);
                    }
                }
                mod_query = norma_Stemming_Syn;
                if (query_x_doc != 0) 
                {
                    factor = double.MinValue;
                }
            }
            else mod_query = norma;
            mod_doc = Document.documents[i].norma;
            if (mod_query * mod_doc != 0)
            {
                //Calculamos la similitud del coseno
                score[i] = factor + query_x_doc / Math.Sqrt(mod_query * mod_doc);
            }
            if (score[i] != 0)
            {
                ResultSearch(words_doc, words_no_doc, words_docLiteral, score[i], Document.documents[i]);
            }
        }
    }
    /// <summary>Metodo para determinar si el documento puede ser resultado de la busqueda</summary>
    /// <param name="words_doc">Lista para las palabras de la query presentes en el documento</param>
    /// <param name="words_docLiteral">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="score">Score del documento</param>
    /// <param name="doc">Documento</param>
    private void ResultSearch(List<string> words_doc,List<string> words_no_doc, List<string> words_docLiteral, double score, Document doc)
    {
        bool result = true;
        //Comprobamos el operador de Exclusion
        for (int m = 0; m < Exclude.Count; m++)
        {
            if (Corpus_Data.vocabulary[Exclude[m]].Pos_doc[doc.index] != null)
            {
                result = false;
            }
        }
        //Comprobamos el operador de Inclusion
        for (int m = 0; m < Include.Count; m++)
        {
            if (Corpus_Data.vocabulary[Include[m]].Pos_doc[doc.index] == null)
            {
                result = false;
            }
        }
        if (result) result = SearchLiteral_Operator(words_docLiteral, doc);
        if (result)
        {
            //Guardamos los resultados de nuetra busqueda
            resultsearchDoc.Add(doc);
            Snippet_words.Add(words_doc);
            score = score + Close(words_doc, doc);
            Score.Add(score);
            Words_not_result.Add(words_no_doc);
        }
    }
    /// <summary>Metodo para comprobar la busqueda literal</summary>
    /// <param name="words_doc">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="doc">Documento</param>
    /// <returns>Bool indicando si el documento es valido para el operador busqueda literal</returns>
    private bool SearchLiteral_Operator(List<string> words_doc, Document doc)
    {
        List<int> literal = new List<int>();
        //System.Console.WriteLine(SearchLiteral_words.Count);
        for (int i = 0; i < SearchLiteral_words.Count; i++)
        {
            //Evaluamos los parametros para la busqueda literal
            for (int j = 0; j < SearchLiteral_words[i].Count; j++)
            {
                if(SearchLiteral_words[i][j]=="?") continue;
                if (Corpus_Data.vocabulary[SearchLiteral_words[i][j]].Pos_doc[doc.index] == null) return false;
            }
            Tuple<int, int, List<string>> t = Distance_Word.Search_Distance(SearchLiteral_words[i], doc,Distance_Word.Distance.SearchLiteral);
            if (t.Item1 == -1) return false;
            //Guardamos la posicion de la busqueda literal encontrada
            literal.Add(t.Item1);
        }
        //Guardamos las posiciones de la busqueda literal encontrada
        Pos_SearchLiteral.Add(literal);
        return true;
    }
    /// <summary>Metodo obtener el Ranking de la cercania</summary>
    /// <param name="words">Lista de palabras</param>
    /// <param name="document">Documento</param>
    /// <returns>Cantidad a incrementar en el score por el operador cercania</returns>
    private double Close(List<string> words, Document document)
    {
        double sumScore = 0;
        foreach (var word_list in Close_Words)
        {
            bool close=true;
            //Comprobamos que las palabras del operador cercania esten en nuestra lista de palabras
            List<string> words_searchDist = new List<string>();
            for (int j = 0; j < word_list.Count; j++)
            {
                if (!words.Contains(word_list[j]))
                {
                    close=false;
                    break;
                }              
            }
            //Si hemos encontrado todas las palabras de nuestro operador cercania buscamos la minima distancia
            if (close)
            {
                double n = (double)Distance_Word.Search_Distance(word_list, document, Distance_Word.Distance.Close).Item2;
                sumScore += 100/(n-1);
            }
        }
        return sumScore;
    }
    #endregion
    
    #region Snippet
    /// <summary>Metodo para buscar los snippets</summary>
    private void Snippet()
    {
        for (int i = 0; i < resultsearchDoc.Count; i++)
        {
            List<int> words_list = new List<int>();
            //Comprobamos si tenemos resultados de busqueda literal
            if (SearchLiteral_words.Count > 0)
            {
                for (int x = 0; x < SearchLiteral_words.Count; x++)
                {
                    //Removemos la palabra de la busqueda literal, de la lista de palabras del snippet
                    foreach (var y in SearchLiteral_words[x])
                    {
                        if (Snippet_words[i].Contains(y)) Snippet_words[i].Remove(y);
                    }
                    //Guardamos la posicion en la lista de posiciones de los snippets
                    words_list.Add(Pos_SearchLiteral[i][x]);
                }
            }
            //Buscamos la mayor cantidad de palabras de nuestro resultado que esten contenidas en una ventana de tamaño Snippet_len
            while (Snippet_words[i].Count != 0)
            {
                Tuple<int, int, List<string>> tuple = Distance_Word.Search_Distance(Snippet_words[i], resultsearchDoc[i],Distance_Word.Distance.Snippet);
                words_list.Add(tuple.Item1);
                //Actualizamos nuestra lista de palabras
                Snippet_words[i] = tuple.Item3;
            }
            BuildSinipped(resultsearchDoc[i], words_list);
        }
    }
    /// <summary>Metodo para construir los snippets</summary>
    /// <param name="document">Documento</param>
    /// <param name="Snippetwords">Lista de las posiciones de las palabras</param>
    private void BuildSinipped(Document document, List<int> Snippetwords)
    {
        Snippetwords.Sort();
        string[] doc = File.ReadAllLines(document.path);
        string[] addSnippet = new string[Snippetwords.Count];
        int[] addposSnippet = new int[Snippetwords.Count];
        //Recorremos el documento
        int i=0;
        int cant = 0;
        for (int linea_ind = 0; linea_ind < doc.Length; linea_ind++)
        {
            string[] linea = doc[linea_ind].Split(' ');
            for(int j=0;j<linea.Length;j++)
            {
                if(cant==Snippetwords[i])
                {
                    int cant1 = 0;
                    StringBuilder sb = new StringBuilder();
                    for (int w = j; w < linea.Length; w++)
                    {
                        sb.Append(linea[w] + " ");
                        cant1++;
                        if (cant1 == Snippet_len) break;
                    }
                    //Si no hemos alcanzado la longitud de nuestro snippet vamos a la linea siguiente
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
                    //Comprobamos el proximi indice
                    i++;
                }
                if(i==Snippetwords.Count) break; 
                string word = linea[j];
                //Si nos encontramos una linea vacia seguimos
                if (word == "") continue;
                //Quitamos los signos de puntuacion
                word = Document.Sign_Puntuation(word);
                //Si solo es un signo de puntuacion seguimos
                if (word == "") continue;
                cant++;                    
            }
            if(i==Snippetwords.Count) break;                
        }
        //Guardamos lo snippets del documento
        SnippetResult.Add(addSnippet);
        Pos_SnippetResult.Add(addposSnippet);
    }
    #endregion
}