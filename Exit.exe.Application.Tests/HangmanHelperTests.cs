using Exit.exe.Application.Features.Sessions;

namespace Exit.exe.Application.Tests;

public class HangmanHelperTests
{
    [Fact]
    public void MaskWord_NoGuesses_AllUnderscores()
    {
        var result = HangmanHelper.MaskWord("KAZIMIR", []);
        Assert.Equal("_ _ _ _ _ _ _", result);
    }

    [Fact]
    public void MaskWord_SomeGuesses_RevealsMatching()
    {
        var result = HangmanHelper.MaskWord("KAZIMIR", ["A", "I"]);
        Assert.Equal("_ A _ I _ I _", result);
    }

    [Fact]
    public void MaskWord_AllGuessed_FullyRevealed()
    {
        var result = HangmanHelper.MaskWord("KAZIMIR", ["K", "A", "Z", "I", "M", "R"]);
        Assert.Equal("K A Z I M I R", result);
    }

    [Fact]
    public void MaskWord_CaseInsensitive()
    {
        var result = HangmanHelper.MaskWord("KAZIMIR", ["k", "a"]);
        Assert.Equal("K A _ _ _ _ _", result);
    }

    [Fact]
    public void ParseGuessedLetters_Empty_ReturnsEmptyList()
    {
        var result = HangmanHelper.ParseGuessedLetters("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseGuessedLetters_Whitespace_ReturnsEmptyList()
    {
        var result = HangmanHelper.ParseGuessedLetters("   ");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseGuessedLetters_CommaSeparated_ReturnsList()
    {
        var result = HangmanHelper.ParseGuessedLetters("A,E,K");
        Assert.Equal(["A", "E", "K"], result);
    }

    [Fact]
    public void IsWordFullyGuessed_AllLettersGuessed_ReturnsTrue()
    {
        var result = HangmanHelper.IsWordFullyGuessed("KAZIMIR", ["K", "A", "Z", "I", "M", "R"]);
        Assert.True(result);
    }

    [Fact]
    public void IsWordFullyGuessed_MissingLetters_ReturnsFalse()
    {
        var result = HangmanHelper.IsWordFullyGuessed("KAZIMIR", ["K", "A"]);
        Assert.False(result);
    }

    [Fact]
    public void IsWordFullyGuessed_CaseInsensitive()
    {
        var result = HangmanHelper.IsWordFullyGuessed("KAZIMIR", ["k", "a", "z", "i", "m", "r"]);
        Assert.True(result);
    }
}
