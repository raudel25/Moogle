namespace MoogleEngine;
using System.IO;

public static class Server
{
    /// <summary>Renderizar el documento</summary>
    /// <param name="path">Ruta del documento</param>
    /// <param name="line">Linea donde se encuentra el snippet</param>
    public static List<string> leer(string path, int line)
    {
        string[] doc;
        doc = File.ReadAllLines("..//Content//" + path + ".txt");
        if(line<0) line=0;
        if(line>doc.Length-1) line=doc.Length-1-(doc.Length-1)%100;
        List<string> words = new List<string>();
        for (int i = line; i < doc.Length; i++)
        {
            words.Add(doc[i]);
            if (i == line + 100) break;
        }
        return words;
    }
    /// <summary>Metodo para Autocompletar la query</summary>
    /// <param name="word">Query</param>
    public static List<string> AutoCompletar(string word)
    {
        List<string> auto=new List<string>();
        if(word=="") 
        {
            return auto;
        }
        //Recorremos nuestro corpus
        foreach(var i in BuildIndex.dic)
        {
            if(i.Key.Length>=word.Length)
            {
                if(i.Key.Substring(0,word.Length)==word)
                {
                    if(auto.Count<5)
                    {
                        if(!auto.Contains(i.Key)) auto.Add(i.Key);
                    }
                    else
                    {
                        //Nos quedamos con las palabras de menor longitud
                        for(int j=0;j<auto.Count;j++)
                        {
                            if(i.Key.Length<auto[j].Length) 
                            {
                                auto[j]=i.Key;
                                break;
                            }
                        }
                    }
                }
            }
        }
        return auto;
    }
}