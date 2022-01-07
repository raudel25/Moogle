namespace MoogleEngine;

public class BuildIndex
{
    public Dictionary<string, Tuple<double[], List<int>[]>> dic = new Dictionary<string, Tuple<double[], List<int>[]>>();
    // public Dictionary<string, List<string>> raices = new Dictionary<string, List<string>>();
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
            /*string stemming = Snowball.Stemmer(word);
            if (this.raices.ContainsKey(stemming))
            {
                this.raices[stemming].Add(word);
            }
            else
            {
                this.raices.Add(stemming, new List<string>() { word });
            }*/

        }
        if (this.dic[word].Item1[index] > Document.max[index]) Document.max[index] = Convert.ToInt32(this.dic[word].Item1[index]);
    }
    public bool searchTree(string s)
    {
        if (this.dic.ContainsKey(s)) return true;
        return false;
    }
}