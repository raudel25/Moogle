using System.Text;

namespace MoogleEngine;

public class QueryClass
{
    //Guardar la sugerencia para el usuario
    public string suggestion_query {get; private set;}
    //Guardar los pesos de las palabras de la query
    public Dictionary<string, double> words_query {get; private set;}
    //Guardar los sinonimos y las palabras con la misma raiz q las de nuestra query
    public Dictionary<string, double[]> words_Stemming_Syn {get; private set;}
    public double norma {get; private set;}
    public double norma_Stemming_Syn {get; private set;}
    //Guardar las palabras del operador Excluir
    public List<string> Exclude {get; private set;}
    //Guardar las palabras del operador Incluir
    public List<string> Include {get; private set;}
    //Guardar las palabras del operador Cercania por cada grupo de palabras cercanas
    public List<List<string>> Close_Words {get; private set;}
    //Guardar las palabras del operador Relevancia y su respectiva relevancia
    public Dictionary<string, int> highest_relevance {get; private set;}
    //Guardar las palabras de la busqueda literal
    public List<List<string>> SearchLiteral_words {get; private set;}
    //Bool para la presencia de busqueda literal
    private bool SearchLiteral;
    //Guardar la maxima frecuencia de la query
    private int max = 1;
    //Guardar la maxima frecuencia de la query con los sinonimos y las raices
    private int max_Stemming_Syn = 1;
    //Para determinar si no hay resultados
    private bool no_results = false;
    public QueryClass(string query)
    {
        this.suggestion_query = query;
        this.words_query = new Dictionary<string, double>();
        this.words_Stemming_Syn = new Dictionary<string, double[]>();
        this.Exclude = new List<string>();
        this.Include = new List<string>();
        this.Close_Words = new List<List<string>>();
        this.SearchLiteral_words = new List<List<string>>();
        this.highest_relevance = new Dictionary<string, int>();
        Operators(query);
        if(no_results) return;
        TF_IDFC();
        //Comprobamos la sugerencia
        if(this.suggestion_query==query) this.suggestion_query="";
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
    /// <param name="query">Texto de la Query</param>
    public void Operators(string query)
    {
        //Tokenizamos nuestro query
        string[] s=query.Split();
        for(int i=0;i<s.Length;i++)
        {
            string word = s[i];
            word = Document.Sign_Puntuation(word,true);
            if(word == "") continue;
            word = word.ToLower();
            if (SearchLiteral_Operator(word)) continue;
            if (Close_Operator(word)) continue;
            if (Exclude_Operator(word)) continue;
            if (Include_Operator(word)) continue;
            if (Highest_relevance_Operator(word)) continue;
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
                Suggestion(word);
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
                Suggestion(word);
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
                close_words[m] = Document.Sign_Puntuation(close_words[m]);
                if(close_words[m]=="") continue;
                if (Corpus_Data.vocabulary.ContainsKey(close_words[m]))
                {
                    Frecuency_Query(close_words[m]);
                }
                else
                {   
                    Suggestion(close_words[m]);
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
                Suggestion(word);
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
                Suggestion(word);
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
    private bool Highest_relevance_Operator(string word)
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
                highest_relevance.Add(word, a + 1 );
                Frecuency_Query(word);
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
        foreach (var line in Corpus_Data.synonymous!)
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
    private void Suggestion(string word)
    {
        string a = Suggestion_word(word);
        //Modifiquemos nuestro txt de la query por la palabra recomendada
        for (int m = 0; m <= suggestion_query.Length - word.Length; m++)
        {
            if (word == suggestion_query.Substring(m, word.Length).ToLower())
            {
                suggestion_query = suggestion_query.Substring(0, m) + a + suggestion_query.Substring(m + word.Length, suggestion_query.Length - word.Length - m);
                break;
            }
        }
    }
    /// <summary>Metodo para encontrar la palabra mas cercana</summary>
    /// <param name="word">String q contiene la palabra</param>
    /// <returns>Palabra con la sugerencia a la busqueda</returns>
    private static string Suggestion_word(string word)
    {
        string suggestion = "";
        double suggestionTF_IDF = 0;
        int changes = int.MaxValue;
        double len=word.Length;
        foreach (var word_dic in Corpus_Data.vocabulary)
        {
            if(Math.Abs(word_dic.Key.Length-len)>changes) continue;
            int dist = Levenshtein_Distance(word, word_dic.Key,changes);
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
    /// <param name="actchange">Cantidad actual de cambios</param>
    /// <returns>Cantidad de cambios entre una palabra y otra</returns>
    private static int Levenshtein_Distance(string a, string b,int actchange)
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
        for (int i = 1; i <= Math.Min(m,n); i++)
        {
            int min=int.MaxValue;
            for (int j = i; j <= n; j++)
            {
                //Damos menos peso a los errores ortograficos
                cost = (a[i - 1] == b[j - 1]) ? 0 : OrtograficRule(a[i-1],b[j-1]);
                change[i, j] = Math.Min(Math.Min(change[i - 1, j] + ((a[i - 1]=='h') ? 2 : 4),  //Eliminacion
                            change[i, j - 1] + ((b[j - 1]=='h') ? 2 : 4)),                             //Insercion 
                            change[i - 1, j - 1] + cost);                     //Sustitucion
                min=Math.Min(min,change[i,j]);         
            }
            for (int j = i+1; j <= m; j++)
            {
                //Damos menos peso a los errores ortograficos
                cost = (a[j - 1] == b[i - 1]) ? 0 : OrtograficRule(a[j-1],b[i-1]);
                change[j, i] = Math.Min(Math.Min(change[j - 1, i] + ((a[j - 1]=='h') ? 2 : 4),  //Eliminacion
                            change[j, i - 1] + ((b[i - 1]=='h') ? 2 : 4)),                             //Insercion 
                            change[j - 1, i - 1] + cost);                     //Sustitucion  
                min=Math.Min(min,change[j,i]);          
            }
            //Comprobamos si la cantidad de cambios que llevamos es mayor que la que ya teniamos como minima
            if(min>actchange) return int.MaxValue;
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
    private void TF_IDFC()
    {
        foreach (var word in words_query)
        {
            //Factor para modificar el peso de la palabra
            double a = 0;
            if (highest_relevance.ContainsKey(word.Key)) a = highest_relevance[word.Key];
            words_query[word.Key] = (Math.Pow(Math.E, a) * word.Value / (double)max) * Math.Log((double)Document.cantdoc / (double)Corpus_Data.vocabulary[word.Key].word_cant_doc);
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
}