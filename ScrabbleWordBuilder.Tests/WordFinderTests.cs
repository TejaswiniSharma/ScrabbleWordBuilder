using ScrabbleWordBuilder.Services;

namespace ScrabbleWordBuilder.Tests;

[TestFixture]
public class WordFinderTests
{
    private WordFinder _wordFinder = null!;

    [SetUp]
    public void SetUp()
    {
        string dataPath = TestHelpers.GetDataPath();
        var letterData = new LetterDataService(dataPath).LoadLetterData();
        _wordFinder = new WordFinder(dataPath, letterData);
    }

    [Test]
    public void FindBestWord_RackAndBoardWord_ReturnsWizard()
    {
        // WIZARD = W(4) + I(1) + Z(10) + A(1) + R(1) + D(2) = 19 pts
        var result = _wordFinder.FindBestWord("AIDOORW", "WIZ");
        Assert.That(result, Is.EqualTo("WIZARD"));
    }

    [Test]
    public void FindBestWord_RackOnly_ReturnsDraw()
    {
        // DRAW, WARD, WOOD all score 8 pts — DRAW wins alphabetically
        var result = _wordFinder.FindBestWord("AIDOORW", null);
        Assert.That(result, Is.EqualTo("DRAW"));
    }

    [Test]
    public void FindBestWord_QuizRack_ReturnsQuiz()
    {
        // QUIZ = Q(10) + U(1) + I(1) + Z(10) = 22 pts
        var result = _wordFinder.FindBestWord("QUIZ", null);
        Assert.That(result, Is.EqualTo("QUIZ"));
    }

    [Test]
    public void FindBestWord_NoFormableWord_ReturnsNull()
    {
        // ZZZZZZ — only 1 Z in game but this bypasses validator
        // use letters that exist but form no valid dictionary word
        var result = _wordFinder.FindBestWord("XJQ", null);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindBestWord_TieScore_ReturnsAlphabeticallyFirst()
    {
        // DRAW and WARD both score 8 from rack AIDOORW, DRAW comes first alphabetically
        var result = _wordFinder.FindBestWord("AIDOORW", null);
        Assert.That(result, Is.EqualTo("DRAW"));
    }

    [Test]
    public void FindBestWord_BoardWordLettersUsed_CanFormWord()
    {
        // Without the board word WIZ, WIZARD can't be formed from AIDOORW alone
        var withoutBoard = _wordFinder.FindBestWord("AIDOORW", null);
        var withBoard = _wordFinder.FindBestWord("AIDOORW", "WIZ");
        Assert.That(withBoard, Is.EqualTo("WIZARD"));
        Assert.That(withoutBoard, Is.Not.EqualTo("WIZARD"));
    }

    [Test]
    public void FindBestWord_HighValueLetters_ScoresCorrectly()
    {
        // Ensure high-value tiles (Q, Z, X, J) are scored properly
        var result = _wordFinder.FindBestWord("QUIZ", null);
        Assert.That(result, Is.Not.Null);
        // QUIZ should beat any word without Q/Z
        Assert.That(result, Is.EqualTo("QUIZ"));
    }

    [Test]
    public void FindBestWord_CaseInsensitiveRack_ReturnsResult()
    {
        // Lowercase rack should work the same as uppercase
        var lower = _wordFinder.FindBestWord("aidoorw", "wiz");
        var upper = _wordFinder.FindBestWord("AIDOORW", "WIZ");
        Assert.That(lower, Is.EqualTo(upper));
    }
}
