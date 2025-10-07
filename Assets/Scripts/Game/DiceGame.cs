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
    [SerializeField] GameObject jailerGirl_prefab = null;
    GameObject playerBurnPos = null;
    JailerGirl jailerGirl = new JailerGirl();
    ICharacterData characterData = new PlayerData();//之後會視實際情況改成讀取json
    ManaRoller manaRoller = null;
    ISkillData skillData = new FireBall();    //test
    bool isOpen = false;

    void Awake()
    {
        playerBurnPos = GameObject.Find("playerPos");
        manaRoller = GameObject.Find("ManaRoller").GetComponent<ManaRoller>();
    }
    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        //生成玩家角色實例
        GameObject playerObj = Instantiate(jailerGirl_prefab, playerBurnPos.transform);
        jailerGirl = playerObj.GetComponent<JailerGirl>();
        jailerGirl.SetData(characterData);

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
        EventCenter.AddListener(GameEvent.EVENT_CHANGE_STATE, ChangeStateEvent);
    }
    void Init()
    {

    }
    void ChangeStateEvent(object[] args)
    {
        TurnState newState = (TurnState)args[0];
        ChangeState(newState);
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
                manaRoller.SetDice(jailerGirl.GetDiceResults(), characterData.keepDiceCount);
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
        manaRoller.RollDices();

   
        // 在這裡處理擲骰子的邏輯


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
}
