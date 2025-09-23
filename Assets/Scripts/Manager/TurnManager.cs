using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
public enum TurnState
{
    rollDice,
    chooseKeepDice,
    calculate,//結算
    endTurn
}
public class TurnManager : MonoBehaviour
{
    TurnState currentState = TurnState.rollDice;
    public int round = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    Button btn_roll = null;//擲骰子按鈕
    [SerializeField]
    Button btn_check = null;//確認按鈕
    [SerializeField]
    Button btn_reRoll = null;//重擲按鈕
    [SerializeField]
    GameObject playerPrefab = null;
    [SerializeField]
    GameObject playerPos = null;
    [SerializeField]
    GameObject dicePrefab = null;

    int[] keepDice;
    int keepDiceCount = 0;
    List<int> rollDiceResult = new List<int>();
    [SerializeField]
    Transform rollDiceParent;
    [SerializeField]
    Transform keepDiceParent;
    [SerializeField]
    Sprite[] rollDiceSprites;
    Player player;
    ICharacterData characterData;
    void Start()
    {
        //生成玩家角色實例
        GameObject playerObj = Instantiate(playerPrefab, playerPos.transform);
        player = playerObj.AddComponent<Player>();
        characterData = new BaseCharacterData();
        player.SetData(characterData, dicePrefab);

        SetClickEvent();

        //初始化擲骰子結果和保留骰子列表
        rollDiceResult = new List<int>();
        keepDice = new int[characterData.keepDiceCount];
    }
    void Init()
    {

    }

    private void SetClickEvent()
    {
        btn_roll.onClick.AddListener(() => RollBtnClick());
    }

    void RollBtnClick()
    {
        if (currentState != TurnState.rollDice) return;
        Debug.Log("Roll button clicked");
        rollDiceResult.Clear();
        // 清除所有子物件
        foreach (Transform child in rollDiceParent)
        {
            Destroy(child.gameObject);
        }
        // 在這裡處理擲骰子的邏輯
        player.Roll(characterData.diceCount-keepDiceCount);

        for (int i = 0; i < player.diceResults.Count; i++)
        {
            burnRollDice(player.diceResults[i]);
        }
        //currentState = TurnState.chooseKeepDice;  
    }
    //新增event事件
    private void OnEnable()
    {
        //EventCenter.AddListener(GameEvent.EVENT_KEEP_DICE, OnKeepDice);
    }
    bool CanKeepDice()
    {
        return keepDiceCount < keepDice.Length;
    }
    void burnKeepDice(int _sideNum)
    {
        keepDiceCount++;
        int keepIndex = -1;
        for (int i = 0; i < keepDice.Length; i++)
        {
            if (keepDice[i] == 0)
            {
                keepDice[i] = _sideNum;
                keepIndex = i;
                break;
            }
        }
        GameObject dice = Instantiate(dicePrefab, keepDiceParent);
        dice.GetComponent<Image>().sprite = rollDiceSprites[_sideNum - 1];
        dice.GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(dice);
            keepDice[keepIndex] = 0;
            burnRollDice(_sideNum);
            keepDiceCount--;
        });
    }
    void burnRollDice(int _sideNum)
    {
        rollDiceResult.Add(_sideNum);
        GameObject dice = Instantiate(dicePrefab, rollDiceParent);
        dice.GetComponent<Image>().sprite = rollDiceSprites[_sideNum - 1];
        dice.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (CanKeepDice())
            {
                Destroy(dice);
                rollDiceResult.Remove(_sideNum);
                burnKeepDice(_sideNum);
            }
            else
            {
                Debug.Log("不能再保留骰子了");
            }
        });
    }
    // Update is called once per frame
    void Update()
    {

    }
    void OnDisable()
    {
        //rollButton.onClick.RemoveAllListeners();
    }

}
