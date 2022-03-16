using System.Text.Json;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        QueryClass queryObject = new QueryClass(query);
        //Comprobamos la sugerencia
        if(queryObject.SuggestionQuery=="") 
        {
            return new SearchResult(BuildItem(queryObject));
        }
        else
        {        
            QueryClass suggestionObject=new QueryClass(queryObject.SuggestionQuery);
            return new SearchResult(BuildItem(queryObject),BuildItem(suggestionObject),queryObject.SuggestionQuery);
        }
    }
    /// <summary>Construyendo el arreglo de SearchItem</summary>
    /// <param name="query">Query</param>
    /// <returns>Arreglo de SearchItem correspondiente a la query</returns>
    public static List<SearchItem> BuildItem(QueryClass query)
    {
        List<SearchItem> items = new List<SearchItem>();
        for (int i = 0; i < Document.Documents!.Count; i++)
        {
            DocumentResult d=new DocumentResult(Document.Documents[i],query);
            if(d.Item!=null) items.Add(d.Item);
        }
        return items;
    }

    /// <summary>Metodo para indexar nuestro corpus</summary>
    public static void IndexCorpus()
    {
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        Document.Documents = new List<Document>();
        int q = 0;
        //Contamos la cantidad de documentos
        foreach (var i in list) q++;
        Document.Cantdoc = q;
        q = 0;
        foreach (var i in list)
        {
            Document d1 = new Document(File.ReadAllLines(i), i, q);
            Document.Documents.Add(d1);
            q++;
        }
        //Deserializamos nuestra base de datos de sinonimos
        string jsonstring = File.ReadAllText("..//synonymous.json");
        Synonymous sin = JsonSerializer.Deserialize<Synonymous>(jsonstring)!;
        CorpusData.Synonymous = sin!.synonymous;
        Document.TfIDFDoc();
    } 
}
