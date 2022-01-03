namespace MoogleEngine;

/*public class Node
{
    public List<Node> children = new List<Node>();
    public int index = 0;
    public int cantchildren = 0;
    public char caracter;
    public double[] vector;
    public string[] snipped;
    public Suffix_Tree father;
    public Node(char caracter, Suffix_Tree father)
    {
        this.caracter = caracter;
        this.father = father;
    }
    public void InsertNode(string txt, int index, int indexdoc, int cantdoc, string snippedtxt)
    {
        Node a = search(this, txt[index]);
        if (index < txt.Length - 1)
        {
            if (a == null)
            {
                Node b = new Node(txt[index], father);
                children.Add(b);
                cantchildren++;
                b.InsertNode(txt, index + 1, indexdoc, cantdoc, snippedtxt);
            }
            else
            {
                a.InsertNode(txt, index + 1, indexdoc, cantdoc, snippedtxt);
            }
        }
        else
        {
            if (a == null)
            {
                a = new Node(txt[index], father);
                children.Add(a);
                cantchildren++;
            }
            if (a.vector == null)
            {
                a.vector = new double[cantdoc + 2];
                a.snipped = new string[cantdoc];
                father.vectors.Add(a.vector);
                //father.snipped.Add(a.snipped);
                a.index = father.cantwords;
                father.cantwords++;
            }
            if (indexdoc < cantdoc)
            {
                if (a.vector[indexdoc] == 0)
                {
                    a.snipped[indexdoc] = snippedtxt;
                }
                else
                {
                    Random random = new Random();
                    if (random.Next(2) == 1) a.snipped[indexdoc] = snippedtxt;
                }
            }
            a.vector[indexdoc]++;
            if (a.vector[indexdoc] > Document.max[indexdoc]) Document.max[indexdoc] = Convert.ToInt32(a.vector[indexdoc]);
        }
    }
    public static Node search(Node a, char n)
    {
        for (int i = 0; i < a.cantchildren; i++)
        {
            if (a.children[i].caracter == n) return a.children[i];
        }
        return null;
    }
}*/