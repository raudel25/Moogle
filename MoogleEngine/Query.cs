namespace MoogleEngine;

public class Sinonimo
{
    public List<string[]> sinonimos { get; set; }
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
    public List<string> Excluir = new List<string>();
    //Guardar las palabras del operador Incluir
    public List<string> Incluir = new List<string>();
    //Guardar las palabras del operador Cercania por cada grupo de palabras cercanas
    public List<List<string>> Cercanas = new List<List<string>>();
    //Guardar las palabras del operador Relevancia y su respectiva relevancia
    public Dictionary<string, int> MayorRelevancia = new Dictionary<string, int>();
    public bool SearchLiteral;
    public List<List<string>> SearchLiteral_words=new List<List<string>>();
    public List<List<int>> Pos_SearchLiteral=new List<List<int>>();
    //Guardar las palabras con la misma raiz q las de nuestra query
    public List<string> wordsRaices = new List<string>();
    //Guardar los sinonimos de las palabras de nustra query
    public List<string> wordsSinonimo = new List<string>();
    //Guardar los sinonimos cargados de nuestro json
    public static List<string[]> sinonimos;
    //Guardar el texto de nustra query
    public string txt;
    public QueryClass(string s)
    {
        this.txt = s;
        //Quitamos los esapcios y los signos de puntiucion
        Document.Token(new string[] { s }, Document.cantdoc, this);
        //Creamos un nuevo arreglo para guardar los pesos de la query
        this.vectorC = new double[Document.sistema.cantwords];

    }
    /// <summary>Metodo para los operadores de busqueda</summary>
    /// <param name="word">String que contiene los operadores</param>
    public void Operators(string word, int index, int pos)
    {
        bool operadores = false;
        if(!operadores && SearchLiteral_Operator(word,index,pos))
        {
            operadores = true;
        }
        if (!operadores && Cercania_Operator(word, index, pos))
        {
            operadores = true;
        }
        if (!operadores && Excluir_Operator(word, index, pos))
        {
            operadores = true;
        }
        if (!operadores && Incluir_Operator(word, index, pos))
        {
            operadores = true;
        }
        if (!operadores && MayorRelevancia_Operator(word, index, pos))
        {
            operadores = true;
        }
        //Si no hemos encontrado un operador procedemos la busqueda de la palabra
        if (!operadores)
        {
            //Comprobamos si la palabra a buscar existe en nuestro sistema
            if (Document.sistema.dic.ContainsKey(word))
            {
                Document.sistema.InsertWord(word, index, pos);
            }
            else
            {
                //Si la palabra no existe procedemos a dar una recomndacion
                word=similar(word, index, pos);
            }
            //Buscamos los sinonimos y las raices de la palabra
            Search_Raices(word, index, pos);
            Search_Sinonimos(word, index, pos);
        }
    }
    private bool SearchLiteral_Operator(string word,int index,int pos)
    {
        if(word[0]=='"'&&!SearchLiteral)
        {
            SearchLiteral_words.Add(new List<string>());
            SearchLiteral=true;
        }
        if(SearchLiteral)
        {
            if(word[word.Length-1]=='"') SearchLiteral=false;
            word=Document.SignosPuntuacion(word);
            if(word=="") return true;
            if (Document.sistema.dic.ContainsKey(word))
            {
                SearchLiteral_words[SearchLiteral_words.Count-1].Add(word);
                Document.sistema.InsertWord(word, index, pos);
            }
            else
            {
                string a = similar(word, index, pos);
                if (a != "")
                {
                    SearchLiteral_words[SearchLiteral_words.Count-1].Add(a);
                }
            }
            return true;
        }
        return false;
    }
    /// <summary>Metodo para el operador cercania</summary>
    /// <param name="word">String que contiene los operadores</param>
    private bool Cercania_Operator(string word, int index, int pos)
    {
        List<string> cerca = new List<string>();
        string[] p = word.Split('~');
        //Si nuestro arreglo tienen mas de dos elementos estamos en presencia del operador
        if (p.Length > 1)
        {
            for (int m = 0; m < p.Length; m++)
            {
                if(p[m]=="") continue;
                p[m] = Document.SignosPuntuacion(p[m]);
                //Comprobamos si la palabra esta en nuestro sistema
                if (Document.sistema.dic.ContainsKey(p[m]))
                {
                    Document.sistema.InsertWord(p[m], index, pos);
                }
                else
                {
                    //Si la palabra no existe procedemos a dar una recomendacion
                    p[m] = similar(p[m], index, pos);
                }
                if (!cerca.Contains(p[m]))
                {
                    cerca.Add(p[m]);
                }
            }
            if (cerca.Count != 0)
            {
                Cercanas.Add(cerca);
            }
            return true;
        }
        return false;
    }
    private bool Excluir_Operator(string word, int index, int pos)
    {
        if (word[0] == '!')
        {
            word = Document.SignosPuntuacion(word);
            if(word=="") return true;
            if (Document.sistema.dic.ContainsKey(word))
            {
                Excluir.Add(word);
                Document.sistema.InsertWord(word, index, pos);
            }
            else
            {
                string a = similar(word, index, pos);
                if (a != "")
                {
                    Excluir.Add(a);
                }
            }
            return true;
        }
        return false;
    }
    private bool Incluir_Operator(string word, int index, int pos)
    {
        if (word[0] == '^')
        {
            word = Document.SignosPuntuacion(word);
            if(word=="") return true;
            if (Document.sistema.dic.ContainsKey(word))
            {
                Incluir.Add(word);
                Document.sistema.InsertWord(word, index, pos);
            }
            else
            {
                string a = similar(word, index, pos);
                if (a != "")
                {
                    Incluir.Add(a);
                }
            }
            return true;
        }
        return false;
    }
    private bool MayorRelevancia_Operator(string word, int index, int pos)
    {
        if (word[0] == '*')
        {
            int a = 0;
            while (word[a] == '*')
            {
                a++;
                if(a==word.Length) break;
            }
            word = Document.SignosPuntuacion(word);
            if(word=="") return true;
            if (Document.sistema.dic.ContainsKey(word))
            {
                MayorRelevancia.Add(word, a + 1);
                Document.sistema.InsertWord(word, index, pos);
            }
            else
            {
                string b = similar(word, index, pos);
                if (b != "")
                {
                    MayorRelevancia.Add(b, a);
                }
            }
            return true;
        }
        return false;
    }
    private void Search_Raices(string word, int index, int pos)
    {
        string stemmer = Snowball.Stemmer(word);
        foreach (var i in Document.sistema.dic)
        {
            if(stemmer=="") continue;
            if (i.Key[0] == stemmer[0] && word != i.Key)
            {
                if (Snowball.Stemmer(i.Key) == stemmer)
                {
                    Document.sistema.InsertWord(i.Key, index, pos);
                    wordsRaices.Add(i.Key);
                }
            }
        }
    }
    private void Search_Sinonimos(string word, int index, int pos)
    {
        foreach (var line in QueryClass.sinonimos)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == word)
                {
                    for (int m = 0; m < line.Length; m++)
                    {
                        if (line[m] != word && Document.sistema.dic.ContainsKey(line[m]))
                        {
                            Document.sistema.InsertWord(line[m], index, pos);
                            wordsSinonimo.Add(line[m]);
                        }
                    }
                    break;
                }
            }
        }
    }
    private string similar(string word, int index, int pos)
    {
        string a = simlitudword(word);
        Document.sistema.InsertWord(a, index, pos);
            for (int m = 0; m <= this.txt.Length - word.Length; m++)
            {
                if (word == txt.Substring(m, word.Length).ToLower())
                {
                    txt = txt.Substring(0, m) + a + txt.Substring(m + word.Length, txt.Length - word.Length - m);
                    break;
                }
            }
        return a;
    }
    private string simlitudword(string query)
    {
        string similar = "";
        double similarTF_IDF = 0;
        double cambios = int.MaxValue;
        foreach (var i in Document.sistema.dic)
        {
            double dist = LevenshteinDistance(query, i.Key);
            if (dist < cambios)
            {
                similar = i.Key;
                cambios = dist;
                double suma = 0;
                for (int j = 0; j < Document.cantdoc; j++)
                {
                    suma += i.Value.Item1[j];
                }
                similarTF_IDF = suma;
            }
            if (dist == cambios)
            {
                double suma = 0;
                for (int j = 0; j < Document.cantdoc; j++)
                {
                    suma += i.Value.Item1[j];
                }
                if (suma > similarTF_IDF)
                {
                    similarTF_IDF = suma;
                    similar = i.Key;
                }
            }
        }
        return similar;
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