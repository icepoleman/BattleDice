using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
public enum TurnState
{
    gameStart,
    roundStart,
    playerTurn,
    enemyTurn,
    roundEnd,
}
public class DiceGame : MonoBehaviour
{
    TurnState currentState = TurnState.gameStart;//目前狀態
    int round = 0;
    CharacterView playerView = null;
    CharacterView enemyView = null;
    ICharacterData playerData = new PlayerData();//之後會視實際情況改成讀取json
    ICharacterData enemyData = new SlimeData();//之後會視實際情況改成讀取關卡資料
    ManaRoller manaRoller = null;
    List<int> onChooseSkillDice = new List<int>();//紀錄選取技能骰子
    bool isOpen = false;

    void Awake()
    {
        manaRoller = GameObject.Find("ManaRoller").GetComponent<ManaRoller>();
        manaRoller.Init();
    }
    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        
        // 生成角色實例
        playerView = CreateCharacter("character/jailerGirl", "playerPos");
        enemyView = CreateCharacter("character/slime", "enemyPos");
        
        manaRoller.SetAllSkill(playerData.skillData);
        AddEvent();
        ChangeState(TurnState.roundStart);
    }
    // 通用角色生成方法
    CharacterView CreateCharacter(string prefabPath, string positionName)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject characterObj = Instantiate(prefab, GameObject.Find(positionName).transform);
        CharacterView characterView = characterObj.AddComponent<CharacterView>();
        characterView.Init();
        return characterView;
    }
    void AddEvent()
    {
        EventCenter.AddListener(GameEvent.EVENT_CLICK_ROLL, RollBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_FIGHT, FightBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_CANCEL_SKILL, CancelFightClick);
        EventCenter.AddListener(GameEvent.EVENT_CHANGE_STATE, ChangeStateEvent);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_USE_DICE, UseDiceEvent);
        EventCenter.AddListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.AddListener(GameEvent.EVENT_SKILL_ATTACK, OnSkillAttack);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_ROLL, RollBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_FIGHT, FightBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_CANCEL_SKILL, CancelFightClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CHANGE_STATE, ChangeStateEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_USE_DICE, UseDiceEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.RemoveListener(GameEvent.EVENT_SKILL_ATTACK, OnSkillAttack);
    }
    //狀態改變事件
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
                StartCoroutine(playerView.ShowRollAnimation(playerData.RollDice(), () =>
                {
                    ChangeState(TurnState.playerTurn);
                }));
                break;
            case TurnState.playerTurn:
                manaRoller.SetDice(playerData.rollDiceResult, playerData.keepDiceCount, playerData.maxRollCount);
                // 在這裡處理玩家回合的邏輯
                Debug.Log("Player's Turn");
                manaRoller.BtnMode(manaRollerMode.RollDice);
                break;
            case TurnState.enemyTurn:
                // 在這裡處理敵人回合的邏輯
                Debug.Log("Enemy's Turn");
                List<int> enemyRoll = enemyData.RollDice();
                StartCoroutine(enemyView.ShowRollAnimation(enemyRoll, () =>
                {
                    //敵人使用技能;
                    enemyData.skillData[0].diceBox = enemyRoll;
                    enemyData.skillData[0].Use();
                }));
                //enemy特寫擲骰 顯示使用技能
                break;
            case TurnState.roundEnd:
                // 在這裡處理回合結束的邏輯
                Debug.Log("Round " + round + " End");
                //任一方死亡 結束遊戲
                if (playerData.IsDead() || enemyData.IsDead())
                {
                    Debug.Log("Game Over");
                    // 在這裡處理遊戲結束的邏輯
                }
                break;
            default:
                break;
        }
        currentState = newState;
    }
    //玩家選擇使用技能需要骰子
    void UseDiceEvent(object[] args)
    {
        int sideNum = (int)args[0];
        bool isChosen = (bool)args[1];

        if (isChosen && !manaRoller.chosenSkillData.canUseSkill())
        {
            onChooseSkillDice.Add(sideNum);
            manaRoller.chosenSkillData.AddDiceData(sideNum);
        }
         
        if (!isChosen)
        {
            onChooseSkillDice.Remove(sideNum);
            manaRoller.chosenSkillData.RemoveDiceData(sideNum);
        }            

        //放完骰子重新判斷是否能使用技能
        if (manaRoller.chosenSkillData.canUseSkill())
            manaRoller.BtnMode(manaRollerMode.CanFight);
        else
            manaRoller.BtnMode(manaRollerMode.UseDice);

        EventCenter.Dispatch(GameEvent.EVENT_CONFIRM_SELECT_SKILL, manaRoller.chosenSkillData);
    }
    void RollBtnClick(object[] args)
    {
        if (currentState != TurnState.playerTurn) return;
        Debug.Log("Roll button clicked");
        manaRoller.RollDices();
    }
    void FightBtnClick(object[] args)
    {
        Debug.Log("Fight button clicked");
        manaRoller.UseSkill();
        manaRoller.BtnMode(manaRollerMode.RollDice);
        EventCenter.Dispatch(GameEvent.EVENT_STOP_USE_DICE);
        //*Debug.Log(skillData.canUseSkill());
    }
    void TurnEndBtnClick(object[] args)
    {
        Debug.Log("Turn End button clicked");
        // 在這裡處理結束回合的邏輯
        ChangeState(TurnState.enemyTurn);
        // 這裡可以加入切換到敵人回合的邏輯
        manaRoller.ClearAllRollDices();
        manaRoller.BtnMode(manaRollerMode.Off);
    }
    void CancelFightClick(object[] args)
    {
        Debug.Log("Cancel Fight button clicked");
        manaRoller.CancelSkillUse();
        manaRoller.BtnMode(manaRollerMode.RollDice);
        EventCenter.Dispatch(GameEvent.EVENT_STOP_USE_DICE);
    }
    void SkillCardClick(object[] args)
    {
        ISkillData _skill = (ISkillData)args[0];
        if (currentState != TurnState.playerTurn || _skill == manaRoller.chosenSkillData) return;
        EventCenter.Dispatch(GameEvent.EVENT_STOP_USE_DICE);
        manaRoller.CancelSkillUse();
        Debug.Log("Skill Card clicked" + _skill.skillName);
        manaRoller.chosenSkillData = _skill;
        EventCenter.Dispatch(GameEvent.EVENT_CONFIRM_SELECT_SKILL, manaRoller.chosenSkillData);
        manaRoller.BtnMode(manaRollerMode.UseDice);
    }
    void OnSkillAttack(object[] args)
    {
        float damage = (float)args[0];
        
        // 根據當前回合狀態判斷攻擊目標
        if (currentState == TurnState.playerTurn)
        {
            // 玩家回合：攻擊敵人
            enemyData.TakeDamage(damage);
            enemyView.PlayAnim("Hurt");
            enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
            enemyView.CreateFlyText(damage);
        }
        else if (currentState == TurnState.enemyTurn)
        {
            // 敵人回合：攻擊玩家
            playerData.TakeDamage(damage);
            playerView.PlayAnim("Hurt");
            playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
            playerView.CreateFlyText(damage);
        }
        
        Debug.Log($" 造成 {damage} 點傷害");
    }
    void AddDeBuffEvent(object[] args)
    {
        string debuffName = (string)args[0];
        //給予玩家debuff
        //技能卡
    }
    void RemoveDeBuffEvent(object[] args)
    {
        string debuffName = (string)args[0];
        //移除玩家debuff
        //技能卡
    }
}
