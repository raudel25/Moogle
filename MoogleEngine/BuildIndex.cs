namespace MoogleEngine;

public static class BuildIndex
{
    //Guardamos las palabras de nuestro corpus
    public static Dictionary<string, DataStructure> dic = new Dictionary<string, DataStructure>();
    public static int cantwords;
    /// <summary>Metodo para el indexar los terminos en el corpus</summary>
    /// <param name="word">Palabra a indexar</param>
    /// <param name="index">Indice del documento</param>
    /// <param name="pos">Posicion de la palabra en el documento</param>
    /// <param name="query">Query</param>
    public static void InsertWord(string word, int index, int pos, QueryClass query = null)
    {
        if (!dic.ContainsKey(word))
        {
            DataStructure data = new DataStructure();
            data.weight_doc = new double[Document.cantdoc];
            data.weigth_query = 0;
            data.Pos_doc = new List<int>[Document.cantdoc];
            dic.Add(word, data);
            cantwords++;
        }
        if (query == null)
        {
            if (dic[word].weight_doc[index] == 0)
            {
                dic[word].Pos_doc[index] = new List<int>();
            }
            dic[word].weight_doc[index]++;
            dic[word].Pos_doc[index].Add(pos);
            //Llevamos la cuenta de la maxima frecuencia en el documento
            if (dic[word].weight_doc[index] > Document.max[index]) Document.max[index] = dic[word].weight_doc[index];
        }
        else
        {
            dic[word].weigth_query++;
            //Llevamos la cuenta de la maxima frecuencia en el documento
            if (dic[word].weigth_query > query.max) query.max = dic[word].weigth_query;
        }

    }

}
public class DataStructure
{
    //Guardamos el peso de la palabra en cada documento
    public double[] weight_doc { get; set; }
    //Guardamos el peso de la palabra en la query
    public double weigth_query { get; set; }
    //Guardamos la candtidad de documentos en los que aparece la palabra
    public double word_cant_doc { get; set; }
    //Guardamos las posiciones de la palabra en cada documento
    public List<int>[] Pos_doc { get; set; }
}