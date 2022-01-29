namespace MoogleEngine;
using System.IO;

public static class Server
{
    public static List<string> leer(string rute,int pos)
    {
        string[] s;
        int line=pos;
        s = File.ReadAllLines("..//Content//" + rute + ".txt");
        List<string> l = new List<string>();
        for (int i = line; i < s.Length; i++)
        {
            l.Add(s[i]);
            if (i == line + 100) break;
        }
        return l;
    }
    public static List<string> AutoCompletar(string s)
    {
         List<string> l=new List<string>();
        if(s=="") 
        {
            return l;
        }
        string[] a=s.Split(' ');
        s=a[a.Length-1];
        foreach(var i in BuildIndex.dic)
        {
            if(i.Key.Length>=s.Length)
            {
                if(i.Key.Substring(0,s.Length)==s)
                {
                    if(l.Count<5)
                    {
                        if(!l.Contains(i.Key)) l.Add(i.Key);
                    }
                    else
                    {
                        for(int j=0;j<l.Count;j++)
                        {
                            if(i.Key.Length<l[j].Length) 
                            {
                                l[j]=i.Key;
                                break;
                            }
                        }
                    }
                }
            }
        }
        return l;
    }
}