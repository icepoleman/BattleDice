using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
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

    Transform enemyPos = null;//敵人位置
    Transform playerPos = null;//玩家位置
    [SerializeField] Transform powerDiceBurnPos = null;//骰子燃燒位置
    [SerializeField] GameObject diceTrailPrefab = null;//骰子特效預置物

    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        manaRoller = GameObject.Find("ManaRoller").GetComponent<ManaRoller>();
        manaRoller.Init();
        enemyPos = GameObject.Find("enemyPos").transform;
        playerPos = GameObject.Find("playerPos").transform;
        // 生成角色實例
        CreateCharacter("character/jailerGirl", playerPos, true);
        CreateCharacter("character/enemy", enemyPos, false);
        playerData = GameDataManager.PlayerData;
        enemyData = GameDataManager.TmpEnemyData;

        //test
        enemyData = EnemyFactory.CreateEnemy(1);
        playerData = new PlayerData();
        playerData.AddBuff(new ShieldBuff(), 0, 2);
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
        Sprite enemySprite = await AddressableManager.LoadAssetAsync<Sprite>("enemy_" + enemyData.enemyId);
        enemyView.SetEnemySprite(enemySprite);
        ChangeState(TurnState.roundStart);
    }
    // 通用角色生成方法
    void CreateCharacter(string prefabPath, Transform positionTransform, bool isPlayer)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject characterObj = Instantiate(prefab, positionTransform);
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

        AddressableManager.ReleaseAsset("enemy_" + enemyData.enemyId);
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
    float playerPowerChargeTime = 1.5f;
    //玩家選擇使用技能需要骰子
    void AddPowerDiceEvent(object[] args)
    {
        BurnDiceTrail();
        int sideNum = (int)args[0];
        playerView.BurnDice(sideNum);
        if (!playerData.wantUseSkill.acceptMoreDice || manaRoller.GetCurrentMode() != manaRollerMode.UseDice)
        {
            autoUseSkillDelay = playerPowerChargeTime;
        }
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
        playerView.UpdateCD(0f);
        Debug.Log(playerData.wantUseSkill.diceBox);
        if (playerData.wantUseSkill.canUseSkill())
        {
            StartCoroutine(PlayerFight());
        }
        else
        {
            playerData.wantUseSkill.diceBox.Clear();
            playerView.PlayAnim("fail");
            Debug.Log("Skill cannot be used yet.");
            manaRoller.BtnMode(manaRollerMode.Idle);
            playerView.ClearDiceBox();
        }
    }
    IEnumerator PlayerFight()
    {
        manaRoller.BtnMode(manaRollerMode.Off);
        yield return new WaitForSeconds(0.2f);
        playerView.CreateFlyText(playerData.wantUseSkill.skillName, Color.white, 0.5f, Ease.OutBack);
        // 等待動畫播放完成後切換回合
        yield return new WaitForSeconds(1f);
        playerData.UseSkill();
        playerView.PlayAnim("fight");
        manaRoller.BtnMode(manaRollerMode.Idle);
        playerView.ClearDiceBox();
    }
    private void Update()
    {
        if (onPlayerPowerCharge)
        {
            if (autoUseSkillDelay > 0f)
            {
                autoUseSkillDelay -= Time.deltaTime;
                playerView.UpdateCD(autoUseSkillDelay / playerPowerChargeTime);
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
        //manaRoller.ClearAllRollDices();
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
            playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
        }
        else
        {
            enemyData.TakeDamage(damage);
            playerView.PlayAnim("atk");
            enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
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
    void BurnDiceTrail()
    {
        if (diceTrailPrefab != null)
        {
            GameObject trail = Instantiate(diceTrailPrefab, powerDiceBurnPos.position, Quaternion.identity);
            //DOTween移動trail position _burnPos to playerPos
            trail.transform.DOMove(playerPos.position, 0.1f).SetEase(Ease.Linear);
            Destroy(trail, 1f); // 假設特效持續2秒
        }
    }
    void AddBuffEvent(object[] args)
    {
        string debuffName = (string)args[0];
        //給予玩家debuff
        //技能卡
    }
    void RemoveBuffEvent(object[] args)
    {
        string debuffName = (string)args[0];
        //移除玩家debuff
        //技能卡
    }
}
