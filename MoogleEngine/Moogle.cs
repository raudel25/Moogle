using System.Text.Json;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        var queryObject = new QueryClass(query);
        //Comprobamos la sugerencia
        if (queryObject.SuggestionQuery == "")
        {
            return new SearchResult(BuildItem(queryObject));
        }

        var suggestionObject = new QueryClass(queryObject.SuggestionQuery);
        return new SearchResult(BuildItem(queryObject), BuildItem(suggestionObject), queryObject.SuggestionQuery);
    }

    /// <summary>Construyendo el arreglo de SearchItem</summary>
    /// <param name="query">Query</param>
    /// <returns>Arreglo de SearchItem correspondiente a la query</returns>
    private static List<SearchItem> BuildItem(QueryClass query)
    {
        var items = new List<SearchItem>();
        for (var i = 0; i < Document.Documents!.Count; i++)
        {
            var d = new DocumentResult(Document.Documents[i], query);
            if (d.Item != null) items.Add(d.Item);
        }

        return items;
    }

    /// <summary>Metodo para indexar nuestro corpus</summary>
    public static void IndexCorpus()
    {
        var list = Directory.EnumerateFiles("..//Content", "*.txt").ToList();

        Document.Documents = new List<Document>();

        Document.CantDoc = list.Count;

        var q = 0;
        foreach (var d1 in list.Select(i => new Document(File.ReadAllLines(i), i, q)))
        {
            Document.Documents.Add(d1);
            q++;
        }

        //Deserializamos nuestra base de datos de sinonimos
        var jsonstring = File.ReadAllText("..//synonymous.json");
        var sin = JsonSerializer.Deserialize<Synonymous>(jsonstring)!;
        CorpusData.Synonymous = sin.synonymous;
        Document.TfIdfDoc();
    }
}