namespace MoogleEngine;
using System.IO;

public static class CargarDoc
{
    public static List<string> leer(string rute)
    {
        string[] s = rute.Split('_');
        int line = int.Parse(s[1]);
        s = File.ReadAllLines("..//Content//" + s[0] + ".txt");
        List<string> l = new List<string>();
        for (int i = line; i < s.Length; i++)
        {
            l.Add(s[i]);
            if (i == line + 100) break;
        }
        return l;
    }
}