namespace MoogleEngine;

public class SearchResult
{
    private SearchItem[] items;

    public SearchResult(SearchItem[] items, string suggestion = "")
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }
        Array.Sort(items,(o1,o2)=>o2.Score.CompareTo(o1.Score));
        this.items = items;
        this.Suggestion = suggestion;
    }

    public SearchResult() : this(new SearchItem[0])
    {

    }

    public string Suggestion { get; private set; }

    public IEnumerable<SearchItem> Items()
    {
        return this.items;
    }

    public int Count { get { return this.items.Length; } }
}
