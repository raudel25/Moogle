namespace MoogleEngine;

public class SearchResult
{
    private readonly List<SearchItem> _items;

    public SearchResult(List<SearchItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        items.Sort((o1, o2) => o2.Score.CompareTo(o1.Score));
        _items = items;
        Suggestion = "";
        QuerySuggestion = null!;
    }

    public SearchResult(List<SearchItem> items, List<SearchItem> sugestionItems, string suggestion)
    {
        if (items == null!) throw new ArgumentNullException(nameof(items));

        items.Sort((o1, o2) => o2.Score.CompareTo(o1.Score));
        _items = items;
        //Comrobamos si la sugerencia es coreecta
        if (sugestionItems.Count != 0)
        {
            QuerySuggestion = new SearchResult(sugestionItems);
        }
        else
        {
            suggestion = "";
            QuerySuggestion = null!;
        }

        Suggestion = suggestion;
    }

    public SearchResult() : this(new List<SearchItem>())
    {
    }

    public string Suggestion { get; }

    public int Count => _items.Count;

    public SearchResult QuerySuggestion { get; set; }

    public IEnumerable<SearchItem> Items()
    {
        return _items;
    }
}