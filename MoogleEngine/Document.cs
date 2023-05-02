namespace MoogleEngine;

public class Document
{
    public Document(string[] doc, string title, int q)
    {
        Max = 1;
        Path = title;
        Title = title.Substring(12, title.Length - 5 - 12 + 1);
        Index = q;
        Norma = 0;
        Token(doc);
    }

    //Guardar una lista con todos los documentos del corpus
    public static List<Document>? Documents { get; set; }

    //Guardar la cantidad de documentos
    public static int CantDoc { get; set; }

    //Guardar la frecuencia de la palabra que mas se repite por documento
    public int Max { get; set; }

    //Guardar el indice del documento
    public int Index { get; }

    //Guardar el titulo del documento
    public string Title { get; }

    //Guardar la ruta del documento
    public string Path { get; }
    public double Norma { get; private set; }

    #region Token

    /// <summary>Metodo para eliminar todo lo q no sea alfanumerico</summary>
    /// <param name="doc">Texto del documento</param>
    private void Token(string[] doc)
    {
        var cant = 0;
        //Recorremos cada linea del documento y llevamos un contadorcon la posicion de la palabra
        foreach (var t in doc)
        {
            //Separamos por espacios
            var s = t.Split();
            foreach (var t1 in s)
            {
                var word = t1;
                //Quitamos los signos de puntuacion
                word = SignPunctuation(word);
                //Si solo es un signo de puntuacion seguimos
                if (word == "") continue;
                word = word.ToLower();
                //Insertamos la palabra en el sistema
                CorpusData.InsertWord(word, this, cant);
                cant++;
            }
        }
    }

    /// <summary>Metodo para eliminar los signos de puntuacion</summary>
    /// <param name="s">Texto del documento</param>
    /// <param name="query">Query</param>
    /// <returns>Una palabra tras eliminar los extremos no alfanumericos</returns>
    public static string SignPunctuation(string s, bool query = false)
    {
        if (s == "") return s;
        var start = 0;
        var stop = 0;
        //Recorremos la palabra de izqueierda a derecha y paramos cuando hallemos una letra
        for (var i = 0; i < s.Length; i++)
        {
            var next = false;
            var operators = false;
            //Si la palabra es parte de la query excluimos los signos de los operadores
            if (query)
                if (s[i] == '!' || s[i] == '*' || s[i] == '^' || s[i] == '"' || s[i] == '?')
                    operators = true;

            //Si nos encontramos una letra paramos y guardamos la posicion
            if (!operators && !char.IsLetterOrDigit(s[i])) next = true;
            if (!next)
            {
                start = i;
                break;
            }

            //Si hemos llegado al final de la palabra y no hemos encontrado un letra devolvemos un string vacio 
            if (i == s.Length - 1) return "";
        }

        //Recorremos la palabra de derecha a izquierda y paramos cuando hallemos una letra
        for (var i = 0; i < s.Length; i++)
        {
            var next = false;
            var operators = false;
            if (query)
                if (s[s.Length - 1 - i] == '"' || s[s.Length - 1 - i] == '?')
                    operators = true;

            //Si nos encontramos una letra paramos y guardamos la posicion
            if (!operators && !char.IsLetterOrDigit(s[s.Length - 1 - i])) next = true;
            if (!next)
            {
                stop = s.Length - 1 - i;
                break;
            }
        }

        //Devolvemos el substring que no contiene signos de puntuacion
        return s.Substring(start, stop - start + 1);
    }

    #endregion

    #region TFIDF

    /// <summary>Metodo para calcular el TFIdf de los documentos</summary>
    public static void TfIdfDoc()
    {
        foreach (var word in CorpusData.Vocabulary)
        {
            word.Value.WordCantDoc = WordCantDoc(word.Value.WeightDoc);
            for (var j = 0; j < CantDoc; j++)
            {
                word.Value.WeightDoc[j] = word.Value.WeightDoc[j] / Documents![j].Max *
                                          Math.Log(CantDoc / word.Value.WordCantDoc);
                Documents[j].Norma += word.Value.WeightDoc[j] * word.Value.WeightDoc[j];
            }
        }
    }

    /// <summary>Metodo para determinar la cantidad de documentos que contienen a una palabra</summary>
    /// <param name="words">Arreglo con la frecuencia de la palabra en los documentos</param>
    private static int WordCantDoc(double[] words)
    {
        var cant = 0;
        for (var i = 0; i < CantDoc; i++)
            if (words[i] != 0)
                cant++;

        return cant;
    }

    #endregion
}