using System.Text.Json;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda
        QueryClass query_object = new QueryClass(query);
        //Comprobando la sugerencia
        string suggestion="";
        if(query_object.txt!=query_object.original) suggestion=query_object.txt;
        if(query_object.txt=="") return new SearchResult(BuildItem(query_object));
        else
        {
            QueryClass suggestion_object=new QueryClass(suggestion);
            return new SearchResult(BuildItem(query_object),BuildItem(suggestion_object),suggestion);
        }
    }
    /// <summary>Construyendo el arreglo de SearchItem</summary>
    /// <param name="query">Query</param>
    /// <returns>Arreglo de SearchItem correspondiente a la query</returns>

    public static SearchItem[] BuildItem(QueryClass query)
    {
        SearchItem[] items = new SearchItem[query.Score.Count];
        for (int i = 0; i < query.Score.Count; i++)
        {
            items[i] = new SearchItem(query.resultsearchDoc[i].title, query.SnippetResult[i], query.Pos_SnippetResult[i], query.Score[i], query.Words_not_result[i]);
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
        Document.Tf_IdfDoc();
    } 
}
