using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Threading.Tasks;
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
    [SerializeField] GameUiView gameUiView = null;

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
        playerData.AddBuff(BuffFactory.CreateBuff(6, 0, 1));
        playerData.AddBuff(BuffFactory.CreateBuff(7, 0, 3));

        playerData.wantUseSkill = playerData.skillData[0];//自動選擇第一個技能

        playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
        enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
        gameUiView.UpdateBlood(true, playerData.currentBlood, playerData.maxBlood);
        gameUiView.UpdateBlood(false, enemyData.currentBlood, enemyData.maxBlood);

        manaRoller.SetAllSkill(playerData.skillData);
        AddEvent();
        LoadData();
    }
    async void LoadData()
    {
        //載入遊戲數據
        Sprite enemySprite = await AddressableManager.LoadAssetAsync<Sprite>("enemy_" + enemyData.enemyId);
        enemyView.SetEnemySprite(enemySprite);
        await AddressableManager.PreloadAssetAsync<GameObject>("buffCard");
        //生成初始buff
        UpdateBuffEvent(null);
        gameUiView.UpdateDiceCount(playerData.diceCount, enemyData.diceCount);
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
        EventCenter.AddListener(GameEvent.EVENT_ADD_POWER_DICE, AddPowerDiceEvent);
        EventCenter.AddListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, ClearChooseSkill);

        EventCenter.AddListener(GameEvent.EVENT_SKILL_ATTACK, OnSkillAttack);
        EventCenter.AddListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);
        EventCenter.AddListener(GameEvent.EVENT_SKILL_HEAL, OnSkillHeal);
        EventCenter.AddListener(GameEvent.EVENT_PLAYER_USE_SKILL, OnPlayerUseSkill);
        EventCenter.AddListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.AddListener(GameEvent.EVENT_ADD_BUFF, AddBuffEvent);
        EventCenter.AddListener(GameEvent.EVENT_UPDATE_BUFF, UpdateBuffEvent);
        EventCenter.AddListener(GameEvent.EVENT_UPDATE_MANA_DICE, UpdateManaDiceEvent);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_ROLL, RollBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_ADD_POWER_DICE, AddPowerDiceEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, ClearChooseSkill);

        EventCenter.RemoveListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.RemoveListener(GameEvent.EVENT_SKILL_ATTACK, OnSkillAttack);
        EventCenter.RemoveListener(GameEvent.EVENT_SKILL_HEAL, OnSkillHeal);
        EventCenter.RemoveListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);
        EventCenter.RemoveListener(GameEvent.EVENT_PLAYER_USE_SKILL, OnPlayerUseSkill);
        EventCenter.RemoveListener(GameEvent.EVENT_ADD_BUFF, AddBuffEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_UPDATE_BUFF, UpdateBuffEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_UPDATE_MANA_DICE, UpdateManaDiceEvent);

        AddressableManager.ReleaseAsset("enemy_" + enemyData.enemyId);
    }
    void ChangeState(TurnState newState)
    {
        if (currentState == newState) return;
        playerData.UpdateBuff();
        enemyData.UpdateBuff();
        switch (newState)
        {
            case TurnState.roundStart:
                // 在這裡處理回合開始的邏輯
                round++;
                //round廣播事件
                //EventCenter.Dispatch(GameEvent.EVENT_ROUND_START, round);
                Debug.Log("Round " + round + " Start");
                List<int> playerRoll = playerData.RollDice();
                if (playerRoll.Count == 0)
                {
                    //跳過玩家回合
                    Debug.Log("玩家因狀態無法行動，跳過回合");
                    ChangeState(TurnState.enemyTurn);
                    playerData.TurnEndBuffDecrease();
                    return;
                }
                StartCoroutine(playerView.ShowRollAnimation(playerData.RollDice(), () =>
                {
                    ChangeState(TurnState.playerTurn);
                }));
                break;
            case TurnState.playerTurn:
                UpdateBlood();
                manaRoller.SetDice(playerData.rollDiceResult, playerData.keepDiceCount, playerData.maxRollCount);
                // 在這裡處理玩家回合的邏輯
                Debug.Log("Player's Turn");
                playerData.TurnStartBuffEffect();
                manaRoller.BtnMode(manaRollerMode.Idle);
                break;
            case TurnState.enemyTurn:
                UpdateBlood();
                List<int> enemyRoll = enemyData.RollDice();
                if (enemyRoll.Count == 0)
                {
                    //跳過玩家回合
                    Debug.Log("敵人因狀態無法行動，跳過回合");
                    ChangeState(TurnState.roundEnd);
                    enemyData.TurnEndBuffDecrease();
                    return;
                }
                manaRoller.BtnMode(manaRollerMode.Off);
                // 在這裡處理敵人回合的邏輯
                Debug.Log("Enemy's Turn");
                enemyData.TurnStartBuffEffect();
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
    void OnSkillAttack(object[] args)//todo 改成玩家或怪物受傷
    {
        float damage = (float)args[0];
        bool isPlayer = (bool)args[1];

        //先做攻擊buff計算
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
            gameUiView.UpdateBlood(true, playerData.currentBlood, playerData.maxBlood);
        }
        else
        {
            enemyData.TakeDamage(damage);
            playerView.PlayAnim("atk");
            enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
            gameUiView.UpdateBlood(false, enemyData.currentBlood, enemyData.maxBlood);
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
        if (currentState == TurnState.roundEnd) return;
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
            ChangeState(TurnState.roundEnd);
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
    void UpdateBlood()
    {
        gameUiView.UpdateBlood(true, playerData.currentBlood, playerData.maxBlood);
        gameUiView.UpdateBlood(false, enemyData.currentBlood, enemyData.maxBlood);
        playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
        enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
        Invoke("CheckLive", 0.5f);
    }
    void AddBuffEvent(object[] args)
    {
        Debug.Log("DiceGame AddBuffEvent");
        IBuffData buff = (IBuffData)args[0];
        bool isPlayer = (bool)args[1];
        //加入延遲避免計算中增加buff
        StartCoroutine(AddBuffDelay(buff, isPlayer));
    }
    IEnumerator AddBuffDelay(IBuffData buff, bool isPlayer)
    {
        yield return new WaitForSeconds(0.2f);
        if (isPlayer)
            playerData.AddBuff(buff);
        else
            enemyData.AddBuff(buff);
        Debug.Log($"Added buff {buff.buffName} to {(isPlayer ? "player" : "enemy")}");
        UpdateBuffEvent(null);
    }
    void UpdateBuffEvent(object[] args)
    {
        gameUiView.UpdateBuffs(true, playerData.buffData.ToArray());
        gameUiView.UpdateBuffs(false, enemyData.buffData.ToArray());
    }
    void RemoveBuffEvent(object[] args)
    {
        string debuffName = (string)args[0];
        //移除玩家debuff
        //技能卡
    }
    void UpdateManaDiceEvent(object[] args)
    {
        int playerDiceCount = playerData.diceCount;
        int enemyDiceCount = enemyData.diceCount;
        if (playerData.limitDiceCount > 0)
        {
            playerDiceCount = playerData.limitDiceCount;
        }
        if (enemyData.limitDiceCount > 0)
        {
            enemyDiceCount = enemyData.limitDiceCount;
        }
        gameUiView.UpdateDiceCount(playerDiceCount, enemyDiceCount);
    }
}
