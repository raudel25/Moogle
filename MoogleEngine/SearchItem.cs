namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string[] snippet, int[] Pos_Snippet, float score)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.Pos_Snippet = Pos_Snippet;
    }
    
    public string Title { get; private set; }

    public string[] Snippet { get; private set; }
    public int[] Pos_Snippet { get; private set; }

    public float Score { get; set; }
}
