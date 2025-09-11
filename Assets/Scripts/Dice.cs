using UnityEngine;

public class Dice
{
    int[] sides = null;
    int rollResult;

    public void SetSides(int[] newSides)
    {
        sides = newSides;
    }
    public int Roll()
    {
        if (sides == null || sides.Length == 0)
        {
            Debug.LogError("Sides not set or empty.");
            return -1; // Indicate an error
        }
        rollResult = sides[Random.Range(0, sides.Length)];
        return rollResult;
    }
}
