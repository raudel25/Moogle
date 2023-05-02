namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string[] snippet, int[] posSnippet, double score, List<string> wordsNoDoc)
    {
        Title = title;
        Snippet = snippet;
        Score = score;
        PosSnippet = posSnippet;
        WordNoDoc = wordsNoDoc;
    }

    public string Title { get; }

    public string[] Snippet { get; }
    public int[] PosSnippet { get; }
    public List<string> WordNoDoc { get; }

    public double Score { get; set; }
}