using System.Text.Json;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        QueryClass query_object = new QueryClass(query);
        //Comprobamos la sugerencia
        if(query_object.suggestion_query=="") 
        {
            return new SearchResult(BuildItem(query_object));
        }
        else
        {        
            QueryClass suggestion_object=new QueryClass(query_object.suggestion_query);
            return new SearchResult(BuildItem(query_object),BuildItem(suggestion_object),query_object.suggestion_query);
        }
    }
    /// <summary>Construyendo el arreglo de SearchItem</summary>
    /// <param name="query">Query</param>
    /// <returns>Arreglo de SearchItem correspondiente a la query</returns>
    public static List<SearchItem> BuildItem(QueryClass query)
    {
        List<SearchItem> items = new List<SearchItem>();
        for (int i = 0; i < Document.documents!.Count; i++)
        {
            Document_Result d=new Document_Result(Document.documents[i],query);
            if(d.item!=null) items.Add(d.item);
        }
        return items;
    }

    /// <summary>Metodo para indexar nuestro corpus</summary>
    public static void Index_Corpus()
    {
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        Document.documents = new List<Document>();
        int q = 0;
        //Contamos la cantidad de documentos
        foreach (var i in list) q++;
        Document.cantdoc = q;
        q = 0;
        foreach (var i in list)
        {
            Document d1 = new Document(File.ReadAllLines(i), i, q);
            Document.documents.Add(d1);
            q++;
        }
        //Deserializamos nuestra base de datos de sinonimos
        string jsonstring = File.ReadAllText("..//synonymous.json");
        Synonymous sin = JsonSerializer.Deserialize<Synonymous>(jsonstring)!;
        Corpus_Data.synonymous = sin!.synonymous;
        Document.Tf_IDFDoc();
    } 
}
