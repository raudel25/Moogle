namespace MoogleEngine;

public static class BuildIndex
{
    //Guardar los sinonimos cargados de nuestro json
    public static List<string[]> synonymous {get; set;}
    //Guardamos las palabras de nuestro corpus
    public static Dictionary<string, DataStructure> dic = new Dictionary<string, DataStructure>();
    /// <summary>Metodo para el indexar los terminos en el corpus</summary>
    /// <param name="word">Palabra a indexar</param>
    /// <param name="index">Indice del documento</param>
    /// <param name="pos">Posicion de la palabra en el documento</param>
    public static void InsertWord(string word, int index, int pos)
    {
        if (!dic.ContainsKey(word))
        {
            DataStructure data = new DataStructure();
            data.weight_doc = new double[Document.cantdoc];
            data.Pos_doc = new List<int>[Document.cantdoc];
            dic.Add(word, data);
        }
        if (dic[word].weight_doc[index] == 0)
        {
            dic[word].Pos_doc[index] = new List<int>();
        }
        dic[word].weight_doc[index]++;
        dic[word].Pos_doc[index].Add(pos);
        //Llevamos la cuenta de la maxima frecuencia en el documento
        if (dic[word].weight_doc[index] > Document.max[index]) Document.max[index] = (int)dic[word].weight_doc[index];
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
}
public class Synonymous
{
    public List<string[]> synonymous { get; set; }
}
