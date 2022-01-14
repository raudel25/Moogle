namespace MoogleEngine;

public class Sinonimo
{
    public List<string[]> sinonimos { get; set; }
}
public class QueryClass
{
    public double[] vectorC;
    public List<Document> resultsearchDoc = new List<Document>();
    public int cantresult;
    public List<double> Score = new List<double>();
    public List<List<string>> Snipped = new List<List<string>>();
    public List<string[]> SnippedResult = new List<string[]>();
    public List<string> Excluir = new List<string>();
    public List<string> Incluir = new List<string>();
    public List<List<string>> Cercanas = new List<List<string>>();
    public Dictionary<string, int> MayorRelevancia = new Dictionary<string, int>();
    public List<int> CantRelevancia = new List<int>();
    public List<string> wordsRaices = new List<string>();
    public List<string> wordsSinonimo = new List<string>();
    public static List<string[]> sinonimos;
    public string txt;
    public QueryClass(string s)
    {
        this.txt = s;
        Document.BuildHash(new string[] { s }, Document.cantdoc, this);
        this.vectorC = new double[Document.sistema.cantwords];

    }
    public void Busqueda(string change, int index, int i, int pos)
    {
        List<string> cerca = new List<string>();
        string[] p = change.Split('~');
        if (p.Length > 1)
        {
            bool agregar = true;
            for (int m = 0; m < p.Length; m++)
            {
                agregar = true;
                if (Document.sistema.searchTree(p[m]))
                {
                    Document.sistema.InsertWord(p[m], index, pos);
                }
                else
                {
                    p[m] = similar(p[m], index, i, pos);
                    if (p[m] == "") agregar = false;
                }
                if (Moogle.word_cantdoc(Document.sistema.dic[p[m]].Item1) == 0) agregar = false;
                if (agregar)
                {
                    if (!cerca.Contains(p[m]))
                    {
                        cerca.Add(p[m]);
                    }
                }
            }
            if (cerca.Count != 0)
            {
                Cercanas.Add(cerca);
            }
        }
        else if (change[0] == '!')
        {
            change = change.Substring(1);
            if (Document.sistema.searchTree(change))
            {
                Excluir.Add(change);
                Document.sistema.InsertWord(change, index, pos);
            }
            else
            {
                string a = similar(change, index, i, pos);
                if (a != "")
                {
                    Excluir.Add(a);
                }
            }
        }
        else if (change[0] == '^')
        {
            change = change.Substring(1);
            if (Document.sistema.searchTree(change))
            {
                Incluir.Add(change);
                Document.sistema.InsertWord(change, index, pos);
            }
            else
            {
                string a = similar(change, index, i, pos);
                if (a != "")
                {
                    Incluir.Add(a);
                }
            }
        }
        else if (change[0] == '*')
        {
            int a = 0;
            while (change[a] == '*')
            {
                a++;
            }
            change = change.Substring(a);
            if (Document.sistema.searchTree(change))
            {
                MayorRelevancia.Add(change, a);
                Document.sistema.InsertWord(change, index, pos);
            }
            else
            {
                string b = similar(change, index, i, pos);
                if (b != "")
                {
                    MayorRelevancia.Add(b, a);
                }
            }
        }
        else if (Document.sistema.searchTree(change))
        {
            Document.sistema.InsertWord(change, index, pos);
            Search_Raices(change, index, pos);
            Search_Sinonimos(change, index, pos);
        }
        else
        {
            similar(change, index, i, pos);
        }
        Document.max[Document.cantdoc] = 1;
    }
    public void Search_Raices(string word, int index, int pos)
    {
        string stemmer = Snowball.Stemmer(word);
        foreach (var i in Document.sistema.dic)
        {
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
    public void Search_Sinonimos(string word, int index, int pos)
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
    public string similar(string change, int index, int i, int pos)
    {
        string a = simlitudword(change);
        if (a != "")
        {
            Document.sistema.InsertWord(a, index, pos);
            for (int m = 0; m <= this.txt.Length - change.Length; m++)
            {
                if (change == txt.Substring(m, change.Length).ToLower())
                {
                    txt = txt.Substring(0, m) + a + txt.Substring(m + change.Length, txt.Length - change.Length - m);
                    break;
                }
            }
        }
        return a;
    }
    public string simlitudword(string query)
    {
        string similar = "";
        double similarTF_IDF = 0;
        //List<string> similar=new List<string>();
        double porcentaje = int.MaxValue;
        foreach (var i in Document.sistema.dic)
        {
            double dist = LevenshteinDistance(query, i.Key);
            if (/*dist >= 60*/true)
            {
                /*double suma = 0;
                for (int j = 0; j < Document.cantdoc; j++)
                {
                    suma += i.Value.Item1[j];
                }*/
                //dist = dist * suma;
                if (dist < porcentaje)
                {
                    similar = i.Key;
                    //similar=new List<string>(){i.Key};
                    porcentaje = dist;
                    double suma = 0;
                    for (int j = 0; j < Document.cantdoc; j++)
                    {
                        suma += i.Value.Item1[j];
                    }
                    similarTF_IDF = suma;
                }
                if (dist == porcentaje)
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
        }
        return similar;
    }
    public static double LevenshteinDistance(string s, string t)
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