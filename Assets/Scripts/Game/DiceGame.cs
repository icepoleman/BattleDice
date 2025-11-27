using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
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
    EnemyView enemyView = null;
    PlayerData playerData = new PlayerData();//藉由GameDataManager取得
    EnemyData enemyData;//藉由GameDataManager取得
    ManaRoller manaRoller = null;
    bool isOpen = false;
    [SerializeField] Text txt_enemySkill = null;//測試用
    [SerializeField] Text txt_enemyDescription = null;//測試用
    [SerializeField] Text txt_enemyDiceCount = null;//測試用

    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        manaRoller = GameObject.Find("ManaRoller").GetComponent<ManaRoller>();
        manaRoller.Init();
        // 生成角色實例
        CreateCharacter("character/jailerGirl", "playerPos", true);
        CreateCharacter("character/enemy", "enemyPos", false);
        playerData = GameDataManager.PlayerData;
        enemyData = GameDataManager.TmpEnemyData;

        //test
        enemyData = EnemyFactory.CreateEnemy(1);
        playerData = new PlayerData();
        //txt_enemySkill.text = enemyData.skillData[0].cardTitle; //測試用
        txt_enemyDescription.text = enemyData.description; //測試用

        playerData.wantUseSkill = playerData.skillData[0];//自動選擇第一個技能

        playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
        enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);

        manaRoller.SetAllSkill(playerData.skillData);
        AddEvent();
        LoadData();
    }
    async void LoadData()
    {
        //載入遊戲數據
        await EnemyPortraitManager.LoadEnemyIfNeeded(enemyData.spriteLabel);
        enemyView.SetEnemyLabel(enemyData.spriteLabel);
        ChangeState(TurnState.roundStart);
    }
    // 通用角色生成方法
    void CreateCharacter(string prefabPath, string positionName, bool isPlayer)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject characterObj = Instantiate(prefab, GameObject.Find(positionName).transform);
        characterObj.transform.localPosition = Vector3.zero;
        if (isPlayer)
        {
            playerView = characterObj.AddComponent<CharacterView>();
            playerView.Init();
        }
        else
        {
            enemyView = characterObj.AddComponent<EnemyView>();
            enemyView.Init();
        }
    }
    void AddEvent()
    {
        EventCenter.AddListener(GameEvent.EVENT_CLICK_ROLL, RollBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_CHANGE_STATE, ChangeStateEvent);
        EventCenter.AddListener(GameEvent.EVENT_ADD_POWER_DICE, AddPowerDiceEvent);
        EventCenter.AddListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.AddListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, ClearChooseSkill);

        EventCenter.AddListener(GameEvent.EVENT_SKILL_ATTACK, OnSkillAttack);
        EventCenter.AddListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);
        EventCenter.AddListener(GameEvent.EVENT_SKILL_HEAL, OnSkillHeal);
        EventCenter.AddListener(GameEvent.EVENT_PLAYER_USE_SKILL, OnPlayerUseSkill);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_ROLL, RollBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CHANGE_STATE, ChangeStateEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_ADD_POWER_DICE, AddPowerDiceEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, ClearChooseSkill);

        EventCenter.RemoveListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.RemoveListener(GameEvent.EVENT_SKILL_ATTACK, OnSkillAttack);
        EventCenter.RemoveListener(GameEvent.EVENT_SKILL_HEAL, OnSkillHeal);
        EventCenter.RemoveListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);
        EventCenter.RemoveListener(GameEvent.EVENT_PLAYER_USE_SKILL, OnPlayerUseSkill);

        EnemyPortraitManager.UnloadAllEnemies();
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
                manaRoller.BtnMode(manaRollerMode.Idle);
                break;
            case TurnState.enemyTurn:
                manaRoller.BtnMode(manaRollerMode.Off);
                // 在這裡處理敵人回合的邏輯
                Debug.Log("Enemy's Turn");
                List<int> enemyRoll = enemyData.RollDice();
                StartCoroutine(enemyView.ShowRollAnimation(enemyRoll, () =>
                {
                    //敵人使用技能;
                    enemyData.UseSkill();
                    ChangeState(TurnState.roundEnd);
                    enemyData.TurnEndBuffDecrease();
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
                    GameObject winlosePanel = Instantiate(Resources.Load<GameObject>("UI/winLosePanel"), transform);
                    winlosePanel.GetComponent<WinLoseView>().PlayWinAnimation(enemyData.IsDead(), () =>
                    {
                        // 在這裡處理遊戲結束的邏輯
                        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
                    });
                }
                else
                    ChangeState(TurnState.roundStart);
                break;
            default:
                break;
        }
        currentState = newState;
    }
    bool onPlayerPowerCharge = false;
    //玩家選擇使用技能需要骰子
    void AddPowerDiceEvent(object[] args)
    {
        int sideNum = (int)args[0];
        autoUseSkillDelay = 2f;
        onPlayerPowerCharge = true;
        playerView.PlayAnim("charge");
        manaRoller.BtnMode(manaRollerMode.UseDice);

        playerData.AddPowerDice(sideNum);//一定要放最後面
    }
    float autoUseSkillDelay = 0f;
    void OnPlayerUseSkill(object[] args)
    {
        onPlayerPowerCharge = false;
        autoUseSkillDelay = 100f;
        Debug.Log(playerData.wantUseSkill.diceBox);
        if (playerData.wantUseSkill.canUseSkill())
        {
            playerData.UseSkill();
            playerView.PlayAnim("fight");
        }
        else
        {
            playerData.wantUseSkill.diceBox.Clear();
            playerView.PlayAnim("fail");
            Debug.Log("Skill cannot be used yet.");
        }
        manaRoller.BtnMode(manaRollerMode.Idle);
    }

    private void Update()
    {
        if (onPlayerPowerCharge)
        {
            if (autoUseSkillDelay > 0f)
            {
                autoUseSkillDelay -= Time.deltaTime;
                if (autoUseSkillDelay <= 0f)
                {
                    OnPlayerUseSkill(null);
                }
            }
        }
    }

    void RollBtnClick(object[] args)
    {
        if (currentState != TurnState.playerTurn) return;
        Debug.Log("Roll button clicked");
        manaRoller.RollDices();
    }
    void TurnEndBtnClick(object[] args)
    {
        playerData.TurnEndBuffDecrease();
        Debug.Log("Turn End button clicked");
        // 在這裡處理結束回合的邏輯
        ChangeState(TurnState.enemyTurn);
        // 這裡可以加入切換到敵人回合的邏輯
        manaRoller.ClearAllRollDices();
        manaRoller.BtnMode(manaRollerMode.Off);
    }
    void SkillCardClick(object[] args)
    {
        ISkillData _skill = (ISkillData)args[0];
        if (currentState != TurnState.playerTurn) return;
        playerData.wantUseSkill = _skill;
        Debug.Log("Skill Card clicked" + _skill.skillName);
    }
    void ClearChooseSkill(object[] args)
    {
        playerData.wantUseSkill = null;
    }
    //todo keep點下
    void OnSkillAttack(object[] args)//todo 改成玩家或怪物受傷
    {
        float damage = (float)args[0];
        bool isPlayer = (bool)args[1];

        if (isPlayer)
        {
            playerData.Attack(damage);
        }
        else
        {
            enemyData.Attack(damage);
        }
        //todo 結算回合
        //if (playerData.IsDead() || enemyData.IsDead())
        // EventCenter.Dispatch(GameEvent.EVENT_CHANGE_STATE, TurnState.roundEnd);
        Debug.Log($" 造成 {damage} 點傷害");
        //等一秒CheckLive
        Invoke("CheckLive", 1f);
    }
    void OnSkillHeal(object[] args)
    {
        float healAmount = (float)args[0];
        bool isPlayer = (bool)args[1];//治療對象
        if (isPlayer)
        {
            playerData.Heal(healAmount);
        }
        else
        {
            enemyData.Heal(healAmount);
        }
        Debug.Log($" 恢復 {healAmount} 點血量");
        Invoke("CheckLive", 1f);
    }
    void OnAttackCharacter(object[] args)
    {
        float damage = (float)args[0];
        bool isPlayer = (bool)args[1];//攻擊對象

        // 根據當前回合狀態判斷攻擊目標
        if (isPlayer)
        {
            playerData.TakeDamage(damage);
            enemyView.PlayAnim("atk");
            playerView.PlayAnim("hurt");
            playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
            playerView.CreateFlyText(damage);
        }
        else
        {
            enemyData.TakeDamage(damage);
            playerView.PlayAnim("atk");
            enemyView.PlayAnim("hurt");
            enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
            enemyView.CreateFlyText(damage);
        }
        //todo 結算回合
        //if (playerData.IsDead() || enemyData.IsDead())
        // EventCenter.Dispatch(GameEvent.EVENT_CHANGE_STATE, TurnState.roundEnd);
        Debug.Log($" 造成 {damage} 點傷害");
        //等一秒CheckLive
        Invoke("CheckLive", 1f);
    }
    void CheckLive()
    {
        if (playerData.IsDead() || enemyData.IsDead())
        {
            if (playerData.IsDead())
            {
                playerView.PlayAnim("defeat");
            }
            if (enemyData.IsDead())
            {
                enemyView.PlayAnim("defeat");
            }
            Debug.Log("Game Over");
            EventCenter.Dispatch(GameEvent.EVENT_CHANGE_STATE, TurnState.roundEnd);
        }
        else
        {
            playerView.PlayAnim("idle");
            enemyView.PlayAnim("idle");
        }
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
