using UnityEngine;
using System.Collections.Generic;

public interface ICharacterData
{
    float maxBlood { get; set; }
    float currentBlood { get; set; }
    int[] diceSides { get; set; }
    int diceCount { get; set; }
    int keepDiceCount { get; set; }
}
public class PlayerData : ICharacterData
{
    public float maxBlood { get; set; } = 30f;
    public float currentBlood { get; set; } = 30f;
    public int[] diceSides { get; set; } = { 1, 2, 3, 4, 5, 6 };
    public int diceCount { get; set; } = 5;
    public int keepDiceCount { get; set; } = 2;

    public void Hurt(float damage)
    {
        currentBlood -= damage;
        if (currentBlood < 0) currentBlood = 0;
    }
    public void Heal(float heal)
    {
        currentBlood += heal;
        if (currentBlood > maxBlood) currentBlood = maxBlood;
    }
    public List<int> RollDice(int _rollCount)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < _rollCount; i++)
        {
            int side = diceSides[Random.Range(0, diceSides.Length)];
            results.Add(side);
        }
        return results;
    }
    public bool IsDead()
    {
        return currentBlood <= 0;
    }
}


