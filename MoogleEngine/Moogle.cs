using System.Text.Json;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda
        QueryClass query1 = new QueryClass(query);
        //QueryIndex(query1);
        if(query1.no_results)
        {
            return new SearchResult(new SearchItem[0],query1.txt);
        }
        //Creamos un array y pasamos los resultados de la busqueda
        SearchItem[] items = new SearchItem[query1.Score.Count];
        for (int i = 0; i < query1.Score.Count; i++)
        {
            items[i] = new SearchItem(query1.resultsearchDoc[i].title, query1.SnippetResult[i], query1.Pos_SnippetResult[i], query1.Score[i], query1.Words_not_result[i]);
        }
        string suggestion="";
        if(query1.txt==query1.original) suggestion="";
        else suggestion=query1.txt; 
        return new SearchResult(items, suggestion);    
    }
    /// <summary>Metodo para indexar nuestro corpus</summary>
    public static void Indexar()
    {
        var list = Directory.EnumerateFiles("..//Content", "*.txt");
        Document.documents = new List<Document>();
        int q = 0;
        //Contamos la cantidad de documentos
        foreach (var i in list)
        {
            q++;
        }
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
        Synonymous sin = JsonSerializer.Deserialize<Synonymous>(jsonstring);
        Corpus_Data.synonymous = sin.synonymous;
        Document.Tf_IdfDoc();
    } 
}
