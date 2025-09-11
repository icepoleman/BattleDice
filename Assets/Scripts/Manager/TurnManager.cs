using UnityEngine;
public enum TurnState
{
    RollDice,
    Skill,
    Energy,
    Card,
    EndTurn
}
public class TurnManager : MonoBehaviour
{
    int round = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IPlayerData playerData = new BasePlayerData();
        Debug.Log($"Player Health: {playerData.Health}");
        Debug.Log($"Player Dice Sides: {string.Join(", ", playerData.DiceSides)}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
