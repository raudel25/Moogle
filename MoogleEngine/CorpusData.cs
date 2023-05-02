namespace MoogleEngine;

public static class CorpusData
{
    //Guardamos las palabras de nuestro corpus
    public static readonly Dictionary<string, DataStructure> Vocabulary = new();

    //Guardar los sinonimos cargados de nuestro json
    public static List<string[]>? Synonymous { get; set; }

    /// <summary>Metodo para el indexar los terminos en el corpus</summary>
    /// <param name="word">Palabra a indexar</param>
    /// <param name="document">Documento donde encontramos la palabra</param>
    /// <param name="pos">Posicion de la palabra en el documento</param>
    public static void InsertWord(string word, Document document, int pos)
    {
        if (!Vocabulary.ContainsKey(word))
        {
            var data = new DataStructure(Document.CantDoc);
            Vocabulary.Add(word, data);
        }

        if (Vocabulary[word].WeightDoc[document.Index] == 0) Vocabulary[word].PosDoc[document.Index] = new List<int>();

        Vocabulary[word].WeightDoc[document.Index]++;
        Vocabulary[word].PosDoc[document.Index]!.Add(pos);
        //Llevamos la cuenta de la maxima frecuencia en el documento
        if (Vocabulary[word].WeightDoc[document.Index] > document.Max)
            document.Max = (int)Vocabulary[word].WeightDoc[document.Index];
    }
}

public class DataStructure
{
    public DataStructure(int n)
    {
        WeightDoc = new double[n];
        PosDoc = new List<int>[n];
    }

    //Guardamos el peso de la palabra en cada documento
    public double[] WeightDoc { get; set; }

    //Guardamos la candtidad de documentos en los que aparece la palabra
    public double WordCantDoc { get; set; }

    //Guardamos las posiciones de la palabra en cada documento
    public List<int>?[] PosDoc { get; set; }
}

public class Synonymous
{
    public List<string[]>? synonymous { get; set; }
}