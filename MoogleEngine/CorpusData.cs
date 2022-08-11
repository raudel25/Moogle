namespace MoogleEngine;

public static class CorpusData
{
    //Guardar los sinonimos cargados de nuestro json
    public static List<string[]>? Synonymous { get; set; }

    //Guardamos las palabras de nuestro corpus
    public static Dictionary<string, DataStructure> Vocabulary = new Dictionary<string, DataStructure>();

    /// <summary>Metodo para el indexar los terminos en el corpus</summary>
    /// <param name="word">Palabra a indexar</param>
    /// <param name="document">Documento donde encontramos la palabra</param>
    /// <param name="pos">Posicion de la palabra en el documento</param>
    public static void InsertWord(string word, Document document, int pos)
    {
        if (!Vocabulary.ContainsKey(word))
        {
            DataStructure data = new DataStructure(Document.Cantdoc);
            Vocabulary.Add(word, data);
        }

        if (Vocabulary[word].WeightDoc[document.Index] == 0)
        {
            Vocabulary[word].PosDoc[document.Index] = new List<int>();
        }

        Vocabulary[word].WeightDoc[document.Index]++;
        Vocabulary[word].PosDoc[document.Index].Add(pos);
        //Llevamos la cuenta de la maxima frecuencia en el documento
        if (Vocabulary[word].WeightDoc[document.Index] > document.Max)
            document.Max = (int)Vocabulary[word].WeightDoc[document.Index];
    }
}

public class DataStructure
{
    //Guardamos el peso de la palabra en cada documento
    public double[] WeightDoc { get; set; }

    //Guardamos la candtidad de documentos en los que aparece la palabra
    public double WordCantDoc { get; set; }

    //Guardamos las posiciones de la palabra en cada documento
    public List<int>[] PosDoc { get; set; }

    public DataStructure(int n)
    {
        this.WeightDoc = new double[n];
        this.PosDoc = new List<int>[n];
    }
}

public class Synonymous
{
    public List<string[]>? synonymous { get; set; }
}