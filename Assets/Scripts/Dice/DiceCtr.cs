using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceCtr : MonoBehaviour
{
    [SerializeField] List<Roll3D_Dice> diceList;
    [SerializeField] Button rollButton;
    [SerializeField] float rollInterval = 0.3f;
    
    void Start()
    {
        rollButton.onClick.AddListener(() =>
        {
            StartCoroutine(RollDiceWithInterval());
        });
    }
    
    IEnumerator RollDiceWithInterval()
    {
        foreach (var dice in diceList)
        {
            dice.RollDice();
            yield return new WaitForSeconds(rollInterval);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
