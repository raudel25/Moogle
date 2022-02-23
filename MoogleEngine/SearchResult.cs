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
        this.Query_Suggestion=null!;
    }

    public SearchResult(List<SearchItem> items, List<SearchItem> Sugestion_Items ,string suggestion)
    {
        if (items == null!)
        {
            throw new ArgumentNullException("items");
        }
        items.Sort((o1,o2)=>o2.Score.CompareTo(o1.Score));
        this.items = items;
        //Comrobamos si la sugerencia es coreecta
        if(Sugestion_Items.Count != 0)
        {
            this.Query_Suggestion=new SearchResult(Sugestion_Items);
        }
        else
        {
            suggestion="";
            this.Query_Suggestion=null!;
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
    public SearchResult Query_Suggestion {get; set;}
}
