namespace MoogleEngine;

public class SearchResult
{
    private List<SearchItem> items;

    public SearchResult(List<SearchItem> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }
        items.Sort((o1,o2)=>o2.Score.CompareTo(o1.Score));
        this.items = items;
        this.Suggestion = "";
        this.QuerySuggestion=null!;
    }

    public SearchResult(List<SearchItem> items, List<SearchItem> SugestionItems ,string suggestion)
    {
        if (items == null!)
        {
            throw new ArgumentNullException("items");
        }
        items.Sort((o1,o2)=>o2.Score.CompareTo(o1.Score));
        this.items = items;
        //Comrobamos si la sugerencia es coreecta
        if(SugestionItems.Count != 0)
        {
            this.QuerySuggestion=new SearchResult(SugestionItems);
        }
        else
        {
            suggestion="";
            this.QuerySuggestion=null!;
        } 
        this.Suggestion = suggestion;
    }
    public SearchResult() : this(new List<SearchItem>())
    {

    }

    public string Suggestion { get; private set; }

    public IEnumerable<SearchItem> Items()
    {
        return this.items;
    }

    public int Count { get { return this.items.Count; } }
    public SearchResult QuerySuggestion {get; set;}
}
