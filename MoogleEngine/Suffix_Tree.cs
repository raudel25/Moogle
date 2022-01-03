namespace MoogleEngine;

/*public class Suffix_Tree
{
    public List<double[]> vectors = new List<double[]>();
    public List<string[]> snipped = new List<string[]>();
    public int cantwords = 0;
    public Node init;
    public int searchTree(string txt)
    {
        Node a = init;
        for (int i = 0; i < txt.Length; i++)
        {
            Node b = Node.search(a, txt[i]);
            if (b == null) return -1;
            else a = b;
            if (i == txt.Length - 1 && b.vector == null) return -1;
        }
        return a.index;
    }
    public void InsertWord(string txt, int indexdoc, int cantdoc, string snippedtxt)
    {
        init.InsertNode(txt, 0, indexdoc, cantdoc, snippedtxt);
    }
    public Suffix_Tree()
    {
        this.init = new Node((char)0, this);
    }
}*/
public class Suffix_Tree
{
    public Dictionary<string, Tuple<double[], List</*Tuple<int, int>*/int>[]>> dic = new Dictionary<string, Tuple<double[], List</*Tuple<int, int>*/int>[]>>();
    //public Dictionary<string, int> dic1=new Dictionary<string, int>();
    //public List<double[]> vectors = new List<double[]>();
    public int cantwords;
    public void InsertWord(string word, int index, /*Tuple<int, int> t*/int t)
    {
        if (!this.dic.ContainsKey(word))
        {

            double[] n = new double[Document.cantdoc + 2];
            List</*Tuple<int, int>*/int>[] l = new List</*Tuple<int, int>*/int>[Document.cantdoc];
            /*for(int m=0;m<Document.cantdoc;m++)
            {
                l[m]=new List<Tuple<int, int>>();
            }*/
            this.dic.Add(word, new Tuple<double[], List</*Tuple<int, int>*/int>[]>(n, l));
            //this.vectors.Add(n);
            this.cantwords++;
        }
        //this.vectors[dic[word]][index]++;
        if (this.dic[word].Item1[index] == 0 && index < Document.cantdoc)
        {
            this.dic[word].Item2[index] = new List</*Tuple<int, int>*/int>();
        }
        this.dic[word].Item1[index]++;
        if (index < Document.cantdoc)
        {
            this.dic[word].Item2[index].Add(t);
        }
        //if (this.vectors[dic[word]][index] > Document.max[index]) Document.max[index] = Convert.ToInt32(this.vectors[dic[word]][index]);
        if (this.dic[word].Item1[index] > Document.max[index]) Document.max[index] = Convert.ToInt32(this.dic[word].Item1[index]);
    }
    public bool searchTree(string s)
    {
        if (this.dic.ContainsKey(s)) return true;
        return false;
    }
}