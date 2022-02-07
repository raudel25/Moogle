namespace MoogleEngine;

public class Document
{
    //Guardar una lista con todos los documentos del corpus
    public static List<Document> documents {get; set;}
    //Guardar la frecuencia de la palabra que mas se repite por documento
    public static int[] max {get; set;}
    //Guardar el indice del documento
    public int index {get; private set;}
    //Guardar la cantidad de documentos
    public static int cantdoc {get; set;}
    //Guardar el titulo del documento
    public string title {get; private set;}
    //Guardar la ruta del documento
    public string path {get; private set;}
    public double norma = 0;
    public Document(string[] doc, string title, int q)
    {
        this.path = title;
        this.title = title.Substring(12, title.Length - 5 - 12 + 1);
        this.index = q;
        Token(doc, q);
    }
    #region Token
    /// <summary>Metodo para eliminar todo lo q no sea alfanumerico</summary>
    /// <param name="doc">Texto del documento</param>
    /// <param name="index">Indice del documento</param>
    /// <param name="query">Query</param>
    public static void Token(string[] doc, int index, QueryClass query = null)
    {
        int cant = 0;
        //Recorremos cada linea del documento y llevamos un contadorcon la posicion de la palabra
        for (int i = 0; i < doc.Length; i++)
        {
            //Separamos por espacios
            string[] s = doc[i].Split(' ');
            for (int j = 0; j < s.Length; j++)
            {
                string word = s[j];
                //Si nos encontramos una linea vacia seguimos
                if (word == "")
                {
                    cant++;
                    continue;
                }
                //Quitamos los signos de puntuacion
                word = Sign_Puntuation(word, query);
                //Si solo es un signo de puntuacion seguimos
                if (word == "")
                {
                    cant++;
                    continue;
                }
                word = word.ToLower();
                //Si la palabra es del query vamos al metodo de los operadores de busqueda
                if (query != null)
                {
                    query.Operators(word);
                }
                else
                {
                    //Insertamos la palabra en el sistema
                    BuildIndex.InsertWord(word, index, cant);
                }
                cant++;
            }
        }
    }
    /// <summary>Metodo para eliminar los signos de puntuacion</summary>
    /// <param name="s">Texto del documento</param>
    /// <param name="query">Query</param>
    public static string Sign_Puntuation(string s, QueryClass query = null)   
    {
        int start = 0; int stop = 0;
        //Recorremos la palabra de izqueierda a derecha y paramos cuando hallemos una letra
        for (int i = 0; i < s.Length; i++)
        {
            bool sig = false;
            bool operators = false;
            //Si la palabra es parte de la query excluimos los signos de los operadores
            if (query != null)
            {
                if (s[i] == '!' || s[i] == '*' || s[i] == '^' || s[i] == '"') operators = true;
            }
            //Si nos encontramos una letra paramos y guardamos la posicion
            if (!operators && !char.IsLetterOrDigit(s[i]))
            {
                sig = true;
            }
            if (!sig)
            {
                start = i; break;
            }
            //Si hemos llegado al final de la palabra y no hemos encontrado un letra devolvemos un string vacio 
            if (i == s.Length - 1) return "";
        }
        //Recorremos la palabra de derecha a izquierda y paramos cuando hallemos una letra
        for (int i = 0; i < s.Length; i++)
        {
            bool sig = false;
            bool operadores = false;
            if (query != null)
            {
                if (s[s.Length - 1 - i] == '"') operadores = true;
            }
            //Si nos encontramos una letra paramos y guardamos la posicion
            if (!operadores && !char.IsLetterOrDigit(s[s.Length - 1 - i]))
            {
                sig = true;
            }
            if (!sig)
            {
                stop = s.Length - 1 - i; break;
            }
        }
        //Devolvemos el substring que no contiene signos de puntuacion
        return s.Substring(start, stop - start + 1);
    }
    #endregion
    #region TF_IDF
    /// <summary>Metodo para calcular el TF_idf de los documentos</summary>
    public static void Tf_IdfDoc()
    {
        foreach (var word in BuildIndex.dic)
        {
            word.Value.word_cant_doc = word_cantdoc(word.Value.weight_doc);
            for (int j = 0; j < Document.cantdoc; j++)
            {
                word.Value.weight_doc[j] = (word.Value.weight_doc[j] / Document.max[j]) * Math.Log((double)Document.cantdoc / (double)word.Value.word_cant_doc);
                Document.documents[j].norma += word.Value.weight_doc[j] * word.Value.weight_doc[j];
            }
        }
    }
    /// <summary>Metodo para determinar la cantidad de documentos que contienen a una palabra</summary>
    /// <param name="a">Arreglo con la frecuencia de la palabra en los documentos</param>
    private static int word_cantdoc(double[] a)
    {
        int cant = 0;
        for (int i = 0; i < Document.cantdoc; i++)
        {
            if (a[i] != 0) cant++;
        }
        return cant;
    }
    #endregion
}