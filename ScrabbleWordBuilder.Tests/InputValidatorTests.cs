using ScrabbleWordBuilder.Models;
using ScrabbleWordBuilder.Services;

namespace ScrabbleWordBuilder.Tests;

[TestFixture]
public class InputValidatorTests
{
    private InputValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        string dataPath = TestHelpers.GetDataPath();
        var letterData = new LetterDataService(dataPath).LoadLetterData();
        _validator = new InputValidator(letterData);
    }

    [Test]
    public void Validate_EmptyRack_ReturnsInvalid()
    {
        var result = _validator.Validate("", null);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("empty").IgnoreCase);
    }

    [Test]
    public void Validate_RackTooLong_ReturnsInvalid()
    {
        var result = _validator.Validate("ABCDEFGH", null); // 8 letters
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("7").Or.Contain("8"));
    }

    [Test]
    public void Validate_RackWithNonLetters_ReturnsInvalid()
    {
        var result = _validator.Validate("A1B", null);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("A-Z").IgnoreCase);
    }

    [Test]
    public void Validate_BoardWordWithNonLetters_ReturnsInvalid()
    {
        var result = _validator.Validate("ABCDE", "WI3");
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("A-Z").IgnoreCase);
    }

    [Test]
    public void Validate_ValidRackNoBoardWord_ReturnsValid()
    {
        var result = _validator.Validate("AIDOORW", null);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_ValidRackAndBoardWord_ReturnsValid()
    {
        var result = _validator.Validate("AIDOORW", "WIZ");
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_ZInRackAndBoardWord_ExceedsTileLimit_ReturnsInvalid()
    {
        // Only 1 Z tile available in the game
        var result = _validator.Validate("AIDOORZ", "QUIZ");
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Z"));
    }

    [Test]
    public void Validate_BoardWordWithSpecialChars_ReturnsInvalid()
    {
        var result = _validator.Validate("ABCDE", "WI-Z");
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_SingleLetterRack_ReturnsValid()
    {
        var result = _validator.Validate("A", null);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_SevenLetterRack_ReturnsValid()
    {
        var result = _validator.Validate("ABCDEFG", null);
        Assert.That(result.IsValid, Is.True);
    }
}
