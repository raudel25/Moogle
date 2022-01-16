namespace MoogleEngine;

public class Document
{
    string[] doc;
    public static string prueba;
    public static double time1;
    //Aqui guardamos la frecuencia de la palabra que mas se repite por documento
    public static int[] max;
    //Aqui guardamos el indice del documento
    public int index;
    //Aqui guardamos la cantidad de documentos
    public static int cantdoc;
    //Aqui guardamos el titulo del documento
    public string title;
    //Aqui guardamos la ruta del documento
    public string ruta;
    public static BuildIndex sistema;
    public Document(string[] doc, string title, int q)
    {
        this.doc = doc;
        this.ruta = title;
        this.title = title.Substring(12, title.Length - 5 - 12 + 1);
        this.index = q;
        //Quitamos los signos de puntuacion e indexamos el documento
        BuildHash(doc, q);
    }
    public static void BuildHash(string[] doc, int index, QueryClass query = null)
    {
        int cant = 0;
        //Recoremos cada linea del documento
        for (int i = 0; i < doc.Length; i++)
        {
            //Separamos por espacios
            string[] s = doc[i].Split(' ');
            for (int j = 0; j < s.Length; j++)
            {
                string change = s[j];
                //Si nos encontramos una linea vacia seguimos
                if (change == "")
                {
                    cant++;
                    continue;
                }
                //Quitamos los signos de puntuacion
                change = SignosPuntuacion(change, query);
                //Si solo es un signo de puntuacion seguimos
                if (change == "")
                {
                    cant++;
                    continue;
                }
                //Convertimos la palabra a minusculas
                change = change.ToLower();
                //Si la palabra es del query vamos al metodo de los operadores de busqueda
                if (query != null)
                {
                    query.Operators(change, index, i, cant);
                }
                else
                {
                    //Insertamos la palabra en el sistema
                    sistema.InsertWord(change, index, cant);
                }
                cant++;
            }
        }
    }
    //Metodo para eliminar los signos de puntuciaon
    public static string SignosPuntuacion(string s, QueryClass query = null)
    {
        int start = 0; int stop = 0;
        //Recorremos la palabra de izqueierda a derecha y paramos cuando hallemos una letra
        for (int i = 0; i < s.Length; i++)
        {
            bool sig = false;
            bool operadores = false;
            //Si la palabra es parte de la query excluimos los signos de los operadores
            if (query != null)
            {
                if (s[i] == '!' || s[i] == '*' || s[i] == '^') operadores = true;
            }
            //Si nos encontramos una letra paramos y guardamos la posicion
            if (!operadores && char.IsPunctuation(s[i]))
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
            //Si nos encontramos una letra paramos y guardamos la posicion
            if (char.IsPunctuation(s[s.Length - 1 - i]))
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
}