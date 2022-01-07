namespace MoogleEngine;

public class Document
{
    string[] doc;
    public static double time1;
    public static int[] max;
    public int index;
    public static int cantdoc;
    public string title;
    public string ruta;
    public string score;
    public static BuildIndex sistema;
    public Document(string[] doc, string title, int q)
    {
        this.doc = doc;
        this.ruta = title;
        this.title = title.Substring(12, title.Length - 5 - 12 + 1);
        this.index = q;
        BuildHash(doc, q);
    }
    public static void BuildHash(string[] doc, int index, QueryClass busqueda = null)
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
                    sistema.InsertWord(change, index, cant);
                }
                cant++;
            }
        }
    }
    public static string SignosPuntuacion(string s, QueryClass query)
    {
        int start = 0; int stop = 0;
        for (int i = 0; i < s.Length; i++)
        {
            bool sig = false;
            bool operadores = false;
            if (query != null)
            {
                if (s[i] == '!' || s[i] == '*' || s[i] == '^') operadores = true;
            }
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