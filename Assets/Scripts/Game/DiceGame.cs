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
    [SerializeField] GameObject littleSkillCardPrefab = null;//怪物技能卡預置物

    // 技能排隊系統
    private Queue<SkillOrderData> skillOrderQueue = new Queue<SkillOrderData>();
    private bool isProcessingSkill = false; // 是否正在處理
    private float skillInterval = 0.5f; // 技能攻擊間隔時間

    //buff提示泡泡
    [SerializeField] GameObject buffBubblePrefab = null;    //Buff使用提示泡泡
    Transform playerBuffBubblePos = null;    //玩家使用技能提示泡泡生成位置
    Transform enemyBuffBubblePos = null;    //敵人使用技能提示泡泡生成位置

    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        manaRoller = GameObject.Find("ManaRoller").GetComponent<ManaRoller>();
        manaRoller.Init();
        enemyPos = GameObject.Find("enemyPos").transform;
        playerPos = GameObject.Find("playerPos").transform;
        playerBuffBubblePos = GameObject.Find("BuffBubbles/player").transform;
        enemyBuffBubblePos = GameObject.Find("BuffBubbles/enemy").transform;
        // 生成角色實例
        CreateCharacter("character/jailerGirl", playerPos, true);
        CreateCharacter("character/enemy", enemyPos, false);
        playerData = GameDataManager.PlayerData;
        enemyData = GameDataManager.TmpEnemyData;

        //test
        enemyData = EnemyFactory.CreateEnemy(1);
        playerData = new PlayerData();

        //enemyData.AddBuff(new BaseBuff(9, 0, 3));
        // playerData.AddBuff(new BaseBuff(16, 0, 3));
        //   playerData.AddBuff(BuffFactory.CreateBuff(7, 0, 3));

        playerData.wantUseSkill = playerData.skillData[0];//自動選擇第一個技能

        playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
        enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
        gameUiView.UpdateBlood(true, playerData.currentBlood, playerData.maxBlood);
        gameUiView.UpdateBlood(false, enemyData.currentBlood, enemyData.maxBlood);

        enemyView.BornSkillCards(littleSkillCardPrefab, enemyData.skillData);

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
        UpdateBuffUIEvent(null);
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

        EventCenter.AddListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);//攻擊角色
        EventCenter.AddListener(GameEvent.EVENT_PLAYER_USE_SKILL, OnPlayerUseSkill);//玩家發動技能指令
        EventCenter.AddListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);//選取技能
        EventCenter.AddListener(GameEvent.EVENT_ADD_BUFF, AddBuffEvent);//新增buff
        EventCenter.AddListener(GameEvent.EVENT_UPDATE_BUFF, UpdateBuffUIEvent);
        EventCenter.AddListener(GameEvent.EVENT_UPDATE_MANA_DICE, UpdateManaDiceEvent);
        EventCenter.AddListener(GameEvent.EVENT_UPDATE_BLOOD_UI, UpdateBloodUI);

        EventCenter.AddListener(GameEvent.EVENT_USE_SKILL, OnSkillUse);//使用技能通知
        EventCenter.AddListener(GameEvent.EVENT_USE_BUFF, OnBuffUse);//使用buff通知
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_ROLL, RollBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_ADD_POWER_DICE, AddPowerDiceEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, ClearChooseSkill);

        EventCenter.RemoveListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.RemoveListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);
        EventCenter.RemoveListener(GameEvent.EVENT_PLAYER_USE_SKILL, OnPlayerUseSkill);
        EventCenter.RemoveListener(GameEvent.EVENT_ADD_BUFF, AddBuffEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_UPDATE_BUFF, UpdateBuffUIEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_UPDATE_MANA_DICE, UpdateManaDiceEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_UPDATE_BLOOD_UI, UpdateBloodUI);
        EventCenter.RemoveListener(GameEvent.EVENT_USE_SKILL, OnSkillUse);
        EventCenter.RemoveListener(GameEvent.EVENT_USE_BUFF, OnBuffUse);

        AddressableManager.ReleaseAsset("enemy_" + enemyData.enemyId);

        RemoveBuffEvent();
    }
    async void ChangeState(TurnState newState)
    {
        if (currentState == newState) return;
        playerData.RemoveInvalidBuffs();
        enemyData.RemoveInvalidBuffs();
        switch (newState)
        {
            case TurnState.roundStart:
                // 在這裡處理回合開始的邏輯
                round++;
                Debug.Log("Round " + round + " Start");
                //雙方同時骰
                List<int> enemyDiceResult = new List<int>();
                List<int> playerDiceResult = new List<int>();
                if (playerData.state == CharacterState.Idle)
                {
                    playerDiceResult = playerData.RollDice();
                    playerView.SetAnimBool("stun", false);
                    playerView.PlayAnim("charge");
                }
                if (enemyData.state == CharacterState.Idle)
                {
                    enemyDiceResult = enemyData.RollDice();
                }
                // 同時執行兩個動畫
                await Task.WhenAll(
                    playerView.ShowRollAnimation(playerDiceResult),
                    enemyView.ShowRollAnimation(enemyDiceResult)
                );
                // 兩個都完成後才繼續
                // 取得可發動的技能列表
                List<SkillUseInfo> usableSkills = enemyData.GetUsableSkills(enemyDiceResult);
                enemyView.UpdateSkillCards(usableSkills.ConvertAll(skillInfo => skillInfo.skill));

                ChangeState(TurnState.playerTurn);

                break;
            case TurnState.playerTurn:
                if (playerData.state == CharacterState.Stunned)
                {
                    playerView.SetAnimBool("stun", true);
                }
                else if (playerData.state == CharacterState.Sleep)
                {
                    playerView.PlayAnim("sleep");
                }
                else
                {
                    playerView.ClearDiceBox();
                    playerView.PlayAnim("idle");
                }
                UpdateBloodUI(null);
                manaRoller.SetDice(playerData.rollDiceResult, playerData.keepDiceCount, playerData.maxRollCount);
                // 在這裡處理玩家回合的邏輯
                Debug.Log("Player's Turn");
                playerData.TurnStartBuffEffect();
                manaRoller.BtnMode(manaRollerMode.Idle);
                break;
            case TurnState.enemyTurn:
                UpdateBloodUI(null);
                manaRoller.BtnMode(manaRollerMode.Off);
                // 在這裡處理敵人回合的邏輯
                Debug.Log("Enemy's Turn");
                enemyData.TurnStartBuffEffect();
                if (enemyData.state == CharacterState.Stunned)
                {
                    enemyData.TurnEndBuffDecrease();
                    await Task.Delay(500);
                    // playerView.SetAnimBool("stun", true);
                    ChangeState(TurnState.roundEnd);
                }
                else if (enemyData.state == CharacterState.Sleep)
                {
                    enemyData.TurnEndBuffDecrease();
                    await Task.Delay(500);
                    // playerView.PlayAnim("sleep");
                    ChangeState(TurnState.roundEnd);
                }
                else
                {
                    enemyView.ClearDiceBox();
                    //敵人使用技能;
                    enemyData.UseSkill();
                    await Task.Delay(500);
                    enemyData.TurnEndBuffDecrease();
                    await Task.Delay(500);
                    ChangeState(TurnState.roundEnd);
                }

                //enemy特寫擲骰 顯示使用技能
                break;
            case TurnState.roundEnd:
                // 在這裡處理回合結束的邏輯
                Debug.Log("Round " + round + " End");
                //任一方死亡 結束遊戲
                if (playerData.IsDead() || enemyData.IsDead())
                {
                    playerData.RemoveAllBuff();
                    await Task.Delay(500);
                    Debug.Log("Game Over");
                    GameObject winlosePanel = Instantiate(Resources.Load<GameObject>("UI/winLosePanel"), transform);
                    winlosePanel.GetComponent<WinLoseView>().PlayWinAnimation(enemyData.IsDead(), () =>
                    {
                        // 在這裡處理遊戲結束的邏輯
                        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
                    });
                }
                else
                {
                    await Task.Delay(1000);
                    ChangeState(TurnState.roundStart);
                }

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
    void OnBuffUse(object[] args)
    {
        string buffname = (string)args[0];
        bool isPlayer = (bool)args[1];

        //顯示提示泡泡
        Transform bubblePos = isPlayer ? playerBuffBubblePos : enemyBuffBubblePos;
        GameObject bubble = Instantiate(buffBubblePrefab, bubblePos);
        bubble.transform.GetComponentInChildren<Text>().text = buffname;
        string str_buffAnim = isPlayer ? "BornBuff_L" : "BornBuff_R";
        bubble.GetComponent<Animator>().Play(str_buffAnim);

        Debug.Log($"{(isPlayer ? "Player" : "Enemy")} 使用buff {buffname}");
        Destroy(bubble, 2f); // 假設提示泡泡持續2秒
    }
    //敵我雙方使用技能都經過這裡
    void OnSkillUse(object[] args)
    {
        string skillname = (string)args[0];
        SkillType skillType = (SkillType)args[1];
        List<int> values = (List<int>)args[2];
        bool isPlayer = (bool)args[3];

        // 加入排隊
        skillOrderQueue.Enqueue(new SkillOrderData(skillname, skillType, values, isPlayer));
        // 如果沒有正在處理，開始處理排隊
        if (!isProcessingSkill)
        {
            ProcessSkillQueue();
        }
    }

    // 處理技能排隊
    async void ProcessSkillQueue()
    {
        isProcessingSkill = true;

        while (skillOrderQueue.Count > 0)
        {
            SkillOrderData skillOrder = skillOrderQueue.Dequeue();
            BaseCharacterData attacker = skillOrder.isPlayerUse ? playerData : enemyData;
            CharacterView attackerView = skillOrder.isPlayerUse ? playerView : enemyView;
            //角色喊技能
            attackerView.CreateFlyText(skillOrder.skillName, Color.white, 0.5f, Ease.OutBack);
            switch (skillOrder.skillType)
            {
                case SkillType.Attack:
                    //先做攻擊buff計算 實際用OnAttackCharacter給予傷害
                    attacker.Attack(skillOrder.values[0]);
                    Debug.Log($"{(skillOrder.isPlayerUse ? "Player" : "Enemy")} 使用攻擊技能 {skillOrder.skillName}，造成 {skillOrder.values[0]} 點傷害");
                    break;
                case SkillType.Heal:
                    attacker.Heal(skillOrder.values[0]);
                    UpdateBloodUI(null);
                    Debug.Log($"{(skillOrder.isPlayerUse ? "Player" : "Enemy")} 使用治療技能 {skillOrder.skillName}，恢復 {skillOrder.values[0]} 點血量");
                    break;
                case SkillType.Buff://buff沒有名字代表是生成buff 有名稱只做喊招式
                    if (skillOrder.skillName == "")
                    {
                        attacker.AddBuff(new BaseBuff(skillOrder.values[0], skillOrder.values[1], skillOrder.values[2]));
                        UpdateBuffUIEvent(null);
                        Debug.Log($"{(skillOrder.isPlayerUse ? "Player" : "Enemy")} 使用增益技能 {skillOrder.skillName}");
                    }
                    break;
                default:
                    Debug.LogWarning("未知的技能類型");
                    break;
            }

            // 等待間隔時間
            await Task.Delay((int)(skillInterval * 300));
        }

        isProcessingSkill = false;
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
        }
        else
        {
            enemyData.TakeDamage(damage);
            playerView.PlayAnim("atk");
        }
        UpdateBloodUI(null);
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
    void UpdateBloodUI(object[] args)
    {
        gameUiView.UpdateBlood(true, playerData.currentBlood, playerData.maxBlood);
        gameUiView.UpdateBlood(false, enemyData.currentBlood, enemyData.maxBlood);
        playerView.UpdateBlood(playerData.currentBlood, playerData.maxBlood);
        enemyView.UpdateBlood(enemyData.currentBlood, enemyData.maxBlood);
        Invoke("CheckLive", 0.5f);
    }
    async void AddBuffEvent(object[] args)
    {
        Debug.Log("DiceGame AddBuffEvent");
        IBuffData buff = (IBuffData)args[0];
        bool isPlayer = (bool)args[1];
        //加入延遲避免計算中增加buff
        await Task.Delay(200);
        if (isPlayer)
            playerData.AddBuff(buff);
        else
            enemyData.AddBuff(buff);
        Debug.Log($"Added buff {buff.buffName} to {(isPlayer ? "player" : "enemy")}");
        UpdateBuffUIEvent(null);
    }
    void UpdateBuffUIEvent(object[] args)
    {
        gameUiView.UpdateBuffs(true, playerData.buffData.ToArray());
        gameUiView.UpdateBuffs(false, enemyData.buffData.ToArray());
    }
    void RemoveBuffEvent()//buff本身會自動移除
    {

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
