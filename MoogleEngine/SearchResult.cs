namespace MoogleEngine;

public class SearchResult
{
    private SearchItem[] items;

    public SearchResult(SearchItem[] items, string suggestion = "")
    {
        if (items == null){
            throw new ArgumentNullException("items");
        }
        if(items.Length!=0) items=MergeSort(items,0,items.Length-1);
        this.items = items;
        this.Suggestion = suggestion;
    }

    public SearchResult() : this(new SearchItem[0]){
 
    }

    public string Suggestion { get; private set; }

    public IEnumerable<SearchItem> Items()
    {
        return this.items;
    }

    public int Count { get { return this.items.Length; } }

    static SearchItem[] MergeSort(SearchItem[] a, int b, int c)
    {
        if (b - c == 0) return new SearchItem[] { a[c] };
        return Merge(MergeSort(a, b, (b + c) / 2), MergeSort(a, (b + c) / 2 + 1, c));
    }
    static SearchItem[] Merge(SearchItem[] a, SearchItem[] b)
    {
        SearchItem[] c = new SearchItem[a.Length + b.Length];
        int i = 0; int j = 0;
        while(i<a.Length&&j<b.Length)
        {
            if(a[i].Score>b[j].Score)
            {
                c[i+j]=a[i];
                i++;
            }
            else
            {
                c[i+j]=b[j];
                j++;
            }
        }
        while(i<a.Length)
        {
            c[i+j]=a[i];
            i++;
        }
        while(j<b.Length)
        {
            c[i+j]=b[j];
            j++;
        }
        return c;
    }
}
