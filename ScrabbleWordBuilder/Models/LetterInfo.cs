namespace ScrabbleWordBuilder.Models;

public class LetterInfo
{
    public char Letter { get; set; }
    public int Score { get; set; }
    public int Count { get; set; }
}

public class LetterData
{
    public List<LetterInfo> Letters { get; set; } = new();
}
