namespace MoogleEngine;

public class SearchResult
{
    private List<SearchItem> _items;

    public SearchResult(List<SearchItem> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }

        items.Sort((o1, o2) => o2.Score.CompareTo(o1.Score));
        this._items = items;
        this.Suggestion = "";
        this.QuerySuggestion = null!;
    }

    public SearchResult(List<SearchItem> items, List<SearchItem> sugestionItems, string suggestion)
    {
        if (items == null!)
        {
            throw new ArgumentNullException("items");
        }

        items.Sort((o1, o2) => o2.Score.CompareTo(o1.Score));
        this._items = items;
        //Comrobamos si la sugerencia es coreecta
        if (sugestionItems.Count != 0)
        {
            this.QuerySuggestion = new SearchResult(sugestionItems);
        }
        else
        {
            suggestion = "";
            this.QuerySuggestion = null!;
        }

        this.Suggestion = suggestion;
    }

    public SearchResult() : this(new List<SearchItem>())
    {
    }

    public string Suggestion { get; private set; }

    public IEnumerable<SearchItem> Items()
    {
        return this._items;
    }

    public int Count
    {
        get { return this._items.Count; }
    }

    public SearchResult QuerySuggestion { get; set; }
}