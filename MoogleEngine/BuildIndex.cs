namespace MoogleEngine;

public class BuildIndex
{
    //Para indexar las palabras en el sistema usaremos un diccionario que tiene como Key el string con la palabra a indexar
    //y como valor una tupla q tiene en el 1er item 
    public Dictionary<string, Tuple<double[], List<int>[]>> dic = new Dictionary<string, Tuple<double[], List<int>[]>>();
    public int cantwords;
    public void InsertWord(string word, int index, int pos)
    {
        if (!this.dic.ContainsKey(word))
        {
            double[] n = new double[Document.cantdoc + 2];
            List<int>[] l = new List<int>[Document.cantdoc];
            this.dic.Add(word, new Tuple<double[], List<int>[]>(n, l));
            this.cantwords++;
        }
        if (this.dic[word].Item1[index] == 0 && index < Document.cantdoc)
        {
            this.dic[word].Item2[index] = new List<int>();
        }
        this.dic[word].Item1[index]++;
        if (index < Document.cantdoc)
        {
            this.dic[word].Item2[index].Add(pos);
        }
        if (this.dic[word].Item1[index] > Document.max[index]) Document.max[index] = Convert.ToInt32(this.dic[word].Item1[index]);
    }
}