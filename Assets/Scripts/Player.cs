using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    GameObject dicePrefab = null;
    GameObject diceBox = null;
    ICharacterData playerData = new BaseCharacterData();
    public List<int> diceResults;
    public void SetData(ICharacterData _playerData, GameObject _dicePrefab)
    {
        playerData = _playerData;
        dicePrefab = _dicePrefab;
        diceBox = gameObject.transform.Find("diceBox").gameObject;
        diceResults = new List<int>();
    }

    void OnDisable()
    {
        //rollButton.onClick.RemoveAllListeners();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Roll(int diceCount)
    {
        diceResults.Clear();
        for (int i = 0; i < diceCount; i++)
        {
            diceResults.Add(playerData.diceSides[Random.Range(0, playerData.diceSides.Length)]);
        }
    }


}
