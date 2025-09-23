public interface ICharacterData
{
    float blood { get; set; }
    int[] diceSides { get; set; }
    int diceCount { get; set; }    
    int keepDiceCount { get; set; }    
}

public class BaseCharacterData : ICharacterData
{
    public float blood { get; set; } = 30f;
    public int[] diceSides { get; set; } = { 1, 2, 3, 4, 5, 6 };
    public int diceCount { get; set; } = 5;
    public int keepDiceCount { get; set; } = 2;
}

