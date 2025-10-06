using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
public enum TurnState
{
    roundStart,
    playerTurn,
    enemyTurn,
    roundEnd,
}
public class DiceGame : MonoBehaviour
{
    //目前狀態
    TurnState currentState;
    int round = 0;
    //保留骰子
    List<int> keepDice = new List<int>();
    List<int> rollDiceResult = new List<int>();//擲骰子結果
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    GameObject jailerGirl_prefab = null;


    GameObject playerBurnPos = null;
    Transform rollDiceParent;
    Transform keepDiceParent;


    JailerGirl jailerGirl = new JailerGirl();
    ICharacterData characterData = new PlayerData();//之後會視實際情況改成讀取json
    //test
    ISkillData skillData = new FireBall();
    bool isOpen = false;
    void Awake()
    {
        playerBurnPos = GameObject.Find("playerPos");
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        keepDiceParent = GameObject.Find("diceBox/keep").transform;

        Debug.Log(keepDice.Count);
    }
    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        //生成玩家角色實例
        GameObject playerObj = Instantiate(jailerGirl_prefab, playerBurnPos.transform);
        jailerGirl = playerObj.GetComponent<JailerGirl>();
        jailerGirl.SetData(characterData);

        //初始化擲骰子結果和保留骰子列表
        rollDiceResult = new List<int>();
        keepDice = new List<int>();


        skillData.AddDiceData(6);

        Debug.Log(skillData.canUseSkill());
        AddEvent();
        ChangeState( TurnState.roundStart);
    }
    void AddEvent()
    {
        EventCenter.AddListener(GameEvent.EVENT_CLICK_ROLL, RollBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_FIGHT, FightBtnClick);
    }
    void Init()
    {

    }

    void ChangeState(TurnState newState)
    {
        switch (newState)
        {
            case TurnState.roundStart:
                // 在這裡處理回合開始的邏輯
                round++;
                //round廣播事件
                //EventCenter.Dispatch(GameEvent.EVENT_ROUND_START, round);
                Debug.Log("Round " + round + " Start");
                StartCoroutine(jailerGirl.Roll(characterData.diceCount));
                currentState = TurnState.playerTurn;
                break;
            case TurnState.playerTurn:
                // 在這裡處理玩家回合的邏輯
                Debug.Log("Player's Turn");
                break;
            case TurnState.enemyTurn:
                // 在這裡處理敵人回合的邏輯
                Debug.Log("Enemy's Turn");
                break;
            case TurnState.roundEnd:
                // 在這裡處理回合結束的邏輯
                Debug.Log("Round " + round + " End");
                break;
            default:
                break;
        }
        currentState = newState;
    }

    void RollBtnClick(object[] args)
    {
        if (currentState != TurnState.playerTurn) return;
        Debug.Log("Roll button clicked");
        rollDiceResult.Clear();
        // 清除所有子物件
        foreach (Transform child in rollDiceParent)
        {
            Destroy(child.gameObject);
        }
        // 在這裡處理擲骰子的邏輯
       // player.Roll(characterData.diceCount - keepDice.Count);


        //currentState = TurnState.chooseKeepDice;  
    }
    void TurnEndBtnClick(object[] args)
    {
        if (currentState != TurnState.playerTurn) return;
        Debug.Log("Turn End button clicked");
        // 在這裡處理結束回合的邏輯
        currentState = TurnState.enemyTurn;
        // 這裡可以加入切換到敵人回合的邏輯
    }
    void FightBtnClick(object[] args)
    {
        
    }
    //新增event事件
    private void OnEnable()
    {
        //EventCenter.AddListener(GameEvent.EVENT_KEEP_DICE, OnKeepDice);
    }
    //骰子邏輯
  /*  bool CanKeepDice()
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
        dice.GetComponent<Image>().sprite = rollDiceSprites[_sideNum];
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
        dice.GetComponent<Image>().sprite = rollDiceSprites[_sideNum];
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
    }*/
    // Update is called once per frame
    void Update()
    {

    }
    void OnDisable()
    {
        //rollButton.onClick.RemoveAllListeners();
    }

}
