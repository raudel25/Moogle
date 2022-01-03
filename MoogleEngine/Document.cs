namespace MoogleEngine;

public class Document
{
    string[] doc;
    public static double time1;
    public static int[] max;
    //public static List<Document> sistemaDoc;
    public int index;
    public static int cantdoc;
    public string title;
    public string ruta;
    public string score;
    public static Suffix_Tree sistema;
    public Document(string[] doc, string title, int q)
    {
        this.doc = doc;
        this.ruta = title;
        this.title = title.Substring(12, title.Length - 5 - 12 + 1);
        this.index = q;
        BuildHash(doc, q);
    }
    public static void BuildHash(string[] doc, int index, Consulta busqueda = null)
    {
        int cant = 0;
        for (int i = 0; i < doc.Length; i++)
        {
            string[] s = doc[i].Split(' ');
            for (int j = 0; j < s.Length; j++)
            {
                string change = s[j];
                if (change == "")
                {
                    cant++;
                    continue;
                }
                change = SignosPuntuacion(change, busqueda);
                if (change == "")
                {
                    cant++;
                    continue;
                }
                change = change.ToLower();
                if (busqueda != null)
                {
                    busqueda.Busqueda(change, index, i, cant);
                }
                else
                {
                    sistema.InsertWord(change, index, /*new Tuple<int, int>(i, j)*/ cant);
                }
                cant++;
            }
        }
    }
    public static string SignosPuntuacion(string s, Consulta busqueda)
    {
        //char[] signos = { '!', '*', '^', '…', '«', '—', '»', '\'', '\"', ',', '.', ';', ':', '(', ')', '[', ']', '{', '}', '?', '¿', '¡', '-' };
        int start = 0; int stop = 0;
        for (int i = 0; i < s.Length; i++)
        {
            bool sig = false;
            bool operadores = false;
            if (busqueda != null)
            {
                if (s[i] == '!' || s[i] == '*' || s[i] == '^') operadores = true;
            }
            /*for (int j = x; j < signos.Length; j++)
            {
                /*if (s[i] == signos[j])
                {
                    sig = true;
                    break;
                }
                if (char.IsPunctuation(s[i]))
                {
                    sig = true;
                    break;
                }
            }*/
            if (!operadores && char.IsPunctuation(s[i]))
            {
                sig = true;
            }
            if (!sig)
            {
                start = i; break;
            }
            if (i == s.Length - 1) return "";
        }
        for (int i = 0; i < s.Length; i++)
        {
            bool sig = false;
            /*for (int j = 0; j < signos.Length; j++)
            {
                if (s[s.Length - 1 - i] == signos[j])
                {
                    sig = true;
                    break;
                }
                if (char.IsPunctuation(s[s.Length - 1 - i]))
                {
                    sig = true;
                    break;
                }
            }*/
            if (char.IsPunctuation(s[s.Length - 1 - i]))
            {
                sig = true;
            }
            if (!sig)
            {
                stop = s.Length - 1 - i; break;
            }
        }
        return s.Substring(start, stop - start + 1);
    }
}