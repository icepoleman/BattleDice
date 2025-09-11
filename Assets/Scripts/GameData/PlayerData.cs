public interface IPlayerData
{
    float Health { get; set; }
    int[] DiceSides { get; set; }
}

public class BasePlayerData : IPlayerData
{
    public float Health { get; set; } = 100f;
    public int[] DiceSides { get; set; } = { 1, 2, 3, 4, 5, 6 };
}

