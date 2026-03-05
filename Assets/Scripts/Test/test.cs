using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    [Header("骰子設定")]
    [SerializeField] int diceCount = 3;      // 骰子數量
    [SerializeField] int rollTimes = 10;     // 投擲次數
    [ContextMenu("Roll Dice")]
    public void RollDice()
    {
        Debug.Log($"=== 投擲 {diceCount} 顆骰子，共 {rollTimes} 次 ===");
        
        for (int i = 0; i < rollTimes; i++)
        {
            int sum = 0;
            string details = "";
            
            for (int j = 0; j < diceCount; j++)
            {
                int roll = Random.Range(1, 7); // 1~6
                sum += roll;
                details += roll + (j < diceCount - 1 ? " + " : "");
            }
            
            Debug.Log($"第 {i + 1} 次: {details} = {sum}");
        }
    }
}
