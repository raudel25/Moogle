namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string[] snippet, int[] Pos_Snippet, double score, List<string> words_no_doc)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.Pos_Snippet = Pos_Snippet;
        this.Word_no_doc = words_no_doc;
    }
    
    public string Title { get; private set; }

    public string[] Snippet { get; private set; }
    public int[] Pos_Snippet { get; private set; }
    public List<string> Word_no_doc {get; private set;}

    public double Score { get; set; }
}
