namespace MoogleEngine;

public class SearchResult
{
    private SearchItem[] items;

    public SearchResult(SearchItem[] items, SearchItem[] Sugestion_Items=null,string suggestion = "")
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }
        Array.Sort(items,(o1,o2)=>o2.Score.CompareTo(o1.Score));
        this.items = items;
        //Comrobamos si la sugerencia es coreecta
        if(suggestion!="")
        {
            if(Sugestion_Items.Length!=0)
            {
                this.Query_Suggestion=new SearchResult(Sugestion_Items);
            }
            else suggestion="";
        }
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
    public SearchResult Query_Suggestion {get; set;}
}
