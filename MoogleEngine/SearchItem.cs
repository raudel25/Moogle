namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string[] snippet, int[] posSnippet, double score, List<string> wordsNoDoc)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.PosSnippet = posSnippet;
        this.WordNoDoc = wordsNoDoc;
    }

    public string Title { get; private set; }

    public string[] Snippet { get; private set; }
    public int[] PosSnippet { get; private set; }
    public List<string> WordNoDoc { get; private set; }

    public double Score { get; set; }
}