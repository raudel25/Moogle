namespace MoogleEngine;

public static class Corpus_Data
{
    //Guardar los sinonimos cargados de nuestro json
    public static List<string[]>? synonymous {get; set;}
    //Guardamos las palabras de nuestro corpus
    public static Dictionary<string, DataStructure> vocabulary = new Dictionary<string, DataStructure>();
    /// <summary>Metodo para el indexar los terminos en el corpus</summary>
    /// <param name="word">Palabra a indexar</param>
    /// <param name="document">Documento donde encontramos la palabra</param>
    /// <param name="pos">Posicion de la palabra en el documento</param>
    public static void InsertWord(string word, Document document, int pos)
    {
        if (!vocabulary.ContainsKey(word))
        {
            DataStructure data = new DataStructure(Document.cantdoc);
            vocabulary.Add(word, data);
        }
        if (vocabulary[word].weight_doc[document.index] == 0)
        {
            vocabulary[word].Pos_doc[document.index] = new List<int>();
        }
        vocabulary[word].weight_doc[document.index]++;
        vocabulary[word].Pos_doc[document.index].Add(pos);
        //Llevamos la cuenta de la maxima frecuencia en el documento
        if (vocabulary[word].weight_doc[document.index] > document.max) document.max = (int)vocabulary[word].weight_doc[document.index];
    }
}
public class DataStructure
{
    //Guardamos el peso de la palabra en cada documento
    public double[] weight_doc { get; set; }
    //Guardamos la candtidad de documentos en los que aparece la palabra
    public double word_cant_doc { get; set; }
    //Guardamos las posiciones de la palabra en cada documento
    public List<int>[] Pos_doc { get; set; }
    public DataStructure(int n)
    {
        this.weight_doc=new double[n];
        this.Pos_doc=new List<int>[n];
    }
}
public class Synonymous
{
    public List<string[]>? synonymous { get; set; }
}
