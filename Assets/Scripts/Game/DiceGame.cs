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
    //紀錄玩家進入時血量
    float playerEnterBlood;
    PlayerData playerData = new PlayerData();//藉由GameDataManager取得
    EnemyData enemyData;//藉由GameDataManager取得
    ManaRoller manaRoller = null;
    bool isOpen = false;
    GameUiView gameUiView = null;
    [SerializeField] Animator turnAnim = null;

    // 技能排隊系統
    private Queue<SkillOrderData> skillOrderQueue = new Queue<SkillOrderData>();
    private bool isProcessingSkill = false; // 是否正在處理
    private float skillInterval = 0.5f; // 技能攻擊間隔時間

    // 敵人重骰計數器（等待所有重骰完成）
    private int enemyRerollPending = 0;

    //buff提示泡泡(因該放給UI管理比較好)
    [SerializeField] GameObject buffBubblePrefab = null;    //Buff使用提示泡泡
    Transform playerBuffBubblePos = null;    //玩家使用技能提示泡泡生成位置
    Transform enemyBuffBubblePos = null;    //敵人使用技能提示泡泡生成位置

    bool enemyEatBuff = false;

    [SerializeField] Button btn_skip;//測試用工具
    void Awake()
    {
        AddEvent();
    }
    async void Start()
    {
        if (isOpen) return;
        isOpen = true;
        AudioManager.Instance.PlayBGM("Bgm_Battle", true, 1.0f);
        gameUiView = GetComponent<GameUiView>();
        manaRoller = GameObject.Find("ManaRoller").GetComponent<ManaRoller>();
        await manaRoller.Init();
       // playerBuffBubblePos = GameObject.Find("BuffBubbles/player").transform;
      //  enemyBuffBubblePos = GameObject.Find("BuffBubbles/enemy").transform;
        playerData = GameDataManager.PlayerData;
        enemyData = new EnemyData(GameDataManager.TmpEnemyID);

        playerEnterBlood = playerData.currentBlood;//記錄進入時血量

        //  enemyData.AddBuff(new BaseBuff(10, 0, 0));
        //playerData.AddBuff(new BaseBuff(10, 0, 0));
        //   playerData.AddBuff(BuffFactory.CreateBuff(7, 0, 3));

        playerData.wantUseSkill = playerData.skillData[0];//自動選擇第一個技能

        manaRoller.SetAllSkill(playerData.skillData);

        await gameUiView.UpdatePlayerInfo(playerData, enemyData);

        //生成初始buff
        UpdateBuffUIEvent(null);
        ChangeState(TurnState.roundStart);

        //test
        btn_skip.onClick.AddListener(() =>
        {
            enemyData.currentBlood = 0;
        });
    }

    void AddEvent()
    {
        EventCenter.AddListener(GameEvent.EVENT_RESTART_GAME, RestartGame);
        EventCenter.AddListener(GameEvent.EVENT_ESCAPE_BATTLE, EscapeBattle);
        EventCenter.AddListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.AddListener(GameEvent.EVENT_DICE_SELECTION_CHANGED, OnDiceSelectionChanged);
        EventCenter.AddListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, ClearChooseSkill);

        EventCenter.AddListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);//攻擊角色
        EventCenter.AddListener(GameEvent.EVENT_BUFF_EFFECT_BLOOD, OnBuffEffectBlood);//Buff效果造成的血量變化
        EventCenter.AddListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);//選取技能
        EventCenter.AddListener(GameEvent.EVENT_ADD_BUFF, AddBuffEvent);//新增buff
        EventCenter.AddListener(GameEvent.EVENT_UPDATE_BUFF, UpdateBuffUIEvent);
        EventCenter.AddListener(GameEvent.EVENT_UPDATE_BLOOD_UI, UpdateBloodUI);
        EventCenter.AddListener(GameEvent.EVENT_DESTROY_ENEMY_DICE, OnDestroyEnemyDice);
        EventCenter.AddListener(GameEvent.EVENT_GENERATE_MANA_DICE, OnGenerateManaDice);//生成能量骰子給裝置
        EventCenter.AddListener(GameEvent.EVENT_ENEMY_REROLL, OnEnemyReroll);//敵人重新擲骰並再次攻擊
        EventCenter.AddListener(GameEvent.EVENT_CLEAR_NEGATIVE_BUFFS, OnClearNegativeBuffs);//清除所有負面 Buff

        EventCenter.AddListener(GameEvent.EVENT_USE_SKILL, OnSkillUse);//使用技能通知
        EventCenter.AddListener(GameEvent.EVENT_USE_BUFF, OnBuffUse);//使用buff通知
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_RESTART_GAME, RestartGame);
        EventCenter.RemoveListener(GameEvent.EVENT_ESCAPE_BATTLE, EscapeBattle);
        EventCenter.RemoveListener(GameEvent.EVENT_CLICK_TURN_END, TurnEndBtnClick);
        EventCenter.RemoveListener(GameEvent.EVENT_DICE_SELECTION_CHANGED, OnDiceSelectionChanged);
        EventCenter.RemoveListener(GameEvent.EVENT_CLEAR_CHOOSE_SKILL, ClearChooseSkill);

        EventCenter.RemoveListener(GameEvent.EVENT_SELECT_SKILL, SkillCardClick);
        EventCenter.RemoveListener(GameEvent.EVENT_ATTACK_CHARACTER, OnAttackCharacter);
        EventCenter.RemoveListener(GameEvent.EVENT_ADD_BUFF, AddBuffEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_UPDATE_BUFF, UpdateBuffUIEvent);
        EventCenter.RemoveListener(GameEvent.EVENT_UPDATE_BLOOD_UI, UpdateBloodUI);
        EventCenter.RemoveListener(GameEvent.EVENT_BUFF_EFFECT_BLOOD, OnBuffEffectBlood);
        EventCenter.RemoveListener(GameEvent.EVENT_DESTROY_ENEMY_DICE, OnDestroyEnemyDice);
        EventCenter.RemoveListener(GameEvent.EVENT_GENERATE_MANA_DICE, OnGenerateManaDice);
        EventCenter.RemoveListener(GameEvent.EVENT_ENEMY_REROLL, OnEnemyReroll);
        EventCenter.RemoveListener(GameEvent.EVENT_CLEAR_NEGATIVE_BUFFS, OnClearNegativeBuffs);
        EventCenter.RemoveListener(GameEvent.EVENT_USE_SKILL, OnSkillUse);
        EventCenter.RemoveListener(GameEvent.EVENT_USE_BUFF, OnBuffUse);

        AddressableManager.ReleaseAll();
    }
    bool gameOver = false;
    async void ChangeState(TurnState newState)
    {
        if (currentState == newState) return;
        playerData.RemoveInvalidBuffs();
        enemyData.RemoveInvalidBuffs();
        UpdateBloodUI(null);

        switch (newState)
        {
            case TurnState.roundStart:
                await HandleRoundStart();
                break;
            case TurnState.playerTurn:
                await HandlePlayerTurn();
                break;
            case TurnState.enemyTurn:
                await HandleEnemyTurn();
                break;
            case TurnState.roundEnd:
                await HandleRoundEnd();
                break;
        }
        if (enemyEatBuff)
        {
            enemyEatBuff = false;
            List<IBuffData> allBuffs = new List<IBuffData>(playerData.buffData);
            playerData.RemoveAllBuff(); // 移除被吞噬者的所有buff
            if (allBuffs != null)
            {
                // 處理吞噬效果
                foreach (IBuffData buff in allBuffs)
                {
                    buff.canRemove = false; // 重置標記，避免被誤移除
                    enemyData.AddBuff(buff); // 將被吞噬者的buff加到攻擊者身上
                }
            }
            UpdateBuffUIEvent(null);
        }
        currentState = newState;
    }

    #region Turn Handlers
    /// <summary>
    /// 回合開始：玩家擲骰
    /// </summary>
    async Task HandleRoundStart()
    {
        turnAnim.Play("turnStart");
        await Task.Delay(1000);

        List<int> playerDiceResult = new List<int>();
        if (playerData.state == CharacterState.Idle)
        {
            playerDiceResult = playerData.RollDice();
        }
        await gameUiView.ShowDice(playerDiceResult, true);

        manaRoller.ClearAllSelections();
        ChangeState(TurnState.playerTurn);
    }

    /// <summary>
    /// 玩家回合
    /// </summary>
    async Task HandlePlayerTurn()
    {
        turnAnim.Play("playerTurn");
        UpdateBloodUI(null);
        await Task.Delay(1000);
        manaRoller.SetDice(playerData.rollDiceResult, playerData.keepDiceCount, playerData.maxRollCount);
        Debug.Log("Player's Turn");
        playerData.TurnStartBuffEffect();
        manaRoller.BtnMode(manaRollerMode.Idle);

        switch (playerData.state)
        {
            case CharacterState.Stunned:
                await Task.Delay(1000);
                TurnEndBtnClick(null);
                break;
            case CharacterState.Sleep:
                await HandleSleepState(playerData, isPlayer: true);
                break;
            default:
                gameUiView.ClearDiceBox();
                break;
        }
    }

    /// <summary>
    /// 敵人回合
    /// </summary>
    async Task HandleEnemyTurn()
    {
        turnAnim.Play("enemyTurn");
        await Task.Delay(1000);
        UpdateBloodUI(null);
        manaRoller.BtnMode(manaRollerMode.Off);
        Debug.Log("Enemy's Turn");
        enemyData.TurnStartBuffEffect();
        gameUiView.ClearDiceBox();

        switch (enemyData.state)
        {
            case CharacterState.Stunned:
                enemyData.TurnEndBuffDecrease();
                await Task.Delay(500);
                ChangeState(TurnState.roundEnd);
                break;
            case CharacterState.Sleep:
                await HandleSleepState(enemyData, isPlayer: false);
                break;
            default:
                await EnemyAction();
                break;
        }
    }

    /// <summary>
    /// 回合結束：判定勝負或進入下一回合
    /// </summary>
    async Task HandleRoundEnd()
    {
        if (gameOver) return;

        if (playerData.IsDead() || enemyData.IsDead())
        {
            await HandleGameOver();
        }
        else
        {
            await Task.Delay(1000);
            ChangeState(TurnState.roundStart);
        }
    }

    /// <summary>
    /// 處理睡眠狀態：50% 機率醒來
    /// </summary>
    async Task HandleSleepState(BaseCharacterData character, bool isPlayer)
    {
        await Task.Delay(1000);

        System.Random rand = new System.Random();
        bool wakeUp = rand.Next(0, 100) >= 50;

        if (!wakeUp)
        {
            // 沒醒，跳過回合
            if (isPlayer)
            {
                TurnEndBtnClick(null);
            }
            else
            {
                character.TurnEndBuffDecrease();
                await Task.Delay(500);
                ChangeState(TurnState.roundEnd);
            }
        }
        else
        {
            // 醒了，可以行動
            character.RemoveSleepBuff();
            character.state = CharacterState.Idle;
            await Task.Delay(500);

            if (!isPlayer)
            {
                await EnemyAction();
            }
            // 玩家醒來後可正常操作，不需額外處理
        }
    }

    /// <summary>
    /// 遊戲結束處理
    /// </summary>
    async Task HandleGameOver()
    {
        gameOver = true;
        playerData.RemoveAllBuff();
        await Task.Delay(500);
        Debug.Log("Game Over");

        GameObject winlosePanelPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "winLosePanel" + ".prefab");
        GameObject winlosePanel = Instantiate(winlosePanelPrefab, transform);

        bool playerWon = enemyData.IsDead();
        GameDataManager.Gold += playerWon ? enemyData.goldReward : 0;
        GameDataManager.Gear += playerWon ? enemyData.gearReward : 0;

        //string winText = LanguageManager.GetFormat("T_WinReward", enemyData.goldReward, enemyData.gearReward);
        winlosePanel.GetComponent<WinLoseView>().SetData(playerWon, " x  " + enemyData.goldReward, " x  " + enemyData.gearReward);
    }
    #endregion

    #region Enemy Action
    async Task EnemyAction()
    {
        // 敵人擲骰
        List<int> enemyDiceResult = new List<int>();
        if (enemyData.state == CharacterState.Idle)
        {
            enemyDiceResult = enemyData.RollDice();
        }
        await gameUiView.ShowDice(enemyDiceResult, false);

        // 取得可發動的技能列表
        List<SkillUseInfo> usableSkills = enemyData.GetUsableSkills(enemyDiceResult);
        gameUiView.UpdateEnemySkillCards(usableSkills.ConvertAll(skillInfo => skillInfo.skill));
        await Task.Delay(1000);
        gameUiView.ClearDiceBox();
        // 敵人使用技能
        enemyRerollPending = 0; // 重置重骰計數
        enemyData.UseSkill();
        await Task.Delay(300);

        // 等待所有重骰完成
        while (enemyRerollPending > 0)
        {
            await Task.Delay(100);
        }

        enemyData.TurnEndBuffDecrease();
        await Task.Delay(200);
        gameUiView.ClearUsedEnemySkillCards();
        gameUiView.ClearDiceBox();
        ChangeState(TurnState.roundEnd);
    }
    #endregion

    void OnDestroyEnemyDice(object[] args)
    {
        Debug.Log("Destroy Enemy Dice Event Triggered");
        //依照breakNum刪除隨機骰子數量
        int breakNum = (int)args[0];
        List<int> diceToRemove = new List<int>();
        for (int i = 0; i < breakNum; i++)
        {
            if (enemyData.rollDiceResult.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, enemyData.rollDiceResult.Count);
                diceToRemove.Add(randomIndex);
            }
        }
        enemyData.DistroyTargetDice(diceToRemove);
        gameUiView.UpdateEnemySkillCards(enemyData.GetUsableSkills(enemyData.rollDiceResult).ConvertAll(skillInfo => skillInfo.skill));
    }
    void OnGenerateManaDice(object[] args)
    {
        //生成generateCount數量的骰子 數字從sideNumList中隨機選取
        int[] sideNumList = (int[])args[0];
        int generateCount = (int)args[1];
        List<int> burnManaDices = new List<int>();
        for (int i = 0; i < generateCount; i++)
        {
            int randomIndex = sideNumList[UnityEngine.Random.Range(0, sideNumList.Length)];
            burnManaDices.Add(randomIndex);
        }
        for (int i = 0; i < burnManaDices.Count; i++)
        {
            manaRoller.burnRollDice(burnManaDices[i]);
            Debug.Log("Generate Mana Dice: " + burnManaDices[i]);
        }
    }

    // 敵人重新擲骰並再次攻擊
    async void OnEnemyReroll(object[] args)
    {
        if (gameOver) return;
        Debug.Log("Enemy Reroll Event Triggered");
        enemyRerollPending++; // 開始重骰，增加計數

        try
        {
            // 重新擲骰
            List<int> newDiceResult = enemyData.RollDice();

            // 播放擲骰動畫
            await gameUiView.ShowDice(newDiceResult, false);

            // 更新技能卡顯示
            List<SkillUseInfo> usableSkills = enemyData.GetUsableSkills(newDiceResult);
            gameUiView.UpdateEnemySkillCards(usableSkills.ConvertAll(skillInfo => skillInfo.skill));

            // 再次使用技能
            await Task.Delay(500);
            enemyData.UseSkill();
            await Task.Delay(300); // 等待技能執行完畢
        }
        finally
        {
            enemyRerollPending--; // 完成重骰，減少計數
        }
    }

    /// <summary>
    /// 骰子選取狀態變更 - 當選取數量達到技能需求時自動嘗試使用技能
    /// </summary>
    void OnDiceSelectionChanged(object[] args)
    {
        if (currentState != TurnState.playerTurn) return;
        if (playerData.wantUseSkill == null) return;

        List<int> selectedDices = args[0] as List<int>;
        if (selectedDices == null) return;

        int needDiceNum = playerData.wantUseSkill.needDiceNum;
        if (needDiceNum <= 0) return; // 無法確定需求數量的技能不自動觸發

        // 當選取數量達到需求時，嘗試使用技能
        if (selectedDices.Count >= needDiceNum)
        {
            TryUseSkillWithSelectedDices(selectedDices);
        }
    }

    /// <summary>
    /// 嘗試使用技能 - 驗證條件並執行
    /// </summary>
    void TryUseSkillWithSelectedDices(List<int> selectedDices)
    {
        // 清空技能的 diceBox 並加入選取的骰子
        playerData.wantUseSkill.diceBox.Clear();
        foreach (int sideNum in selectedDices)
        {
            playerData.wantUseSkill.AddDiceData(sideNum);
        }

        // 檢查技能條件是否滿足
        if (playerData.wantUseSkill.canUseSkill())
        {
            // 條件滿足 - 消耗骰子並使用技能
            foreach (int sideNum in selectedDices)
            {
                gameUiView.BurnDice(sideNum, true);
            }
            manaRoller.ConsumeSelectedDices();

            manaRoller.BtnMode(manaRollerMode.UseDice);

            // 發動技能
            StartCoroutine(PlayerFight());
        }
        else
        {
            // 條件不滿足 - 清除選取狀態
            playerData.wantUseSkill.diceBox.Clear();
            manaRoller.ClearAllSelections();
            Debug.Log("技能條件不符，取消選取");
        }
    }

    IEnumerator PlayerFight()
    {
        manaRoller.BtnMode(manaRollerMode.Off);
        yield return new WaitForSeconds(1f);
        playerData.UseSkill();
        // gameUiView.PlayFightAnim("playerAtk");
        manaRoller.BtnMode(manaRollerMode.Idle);
        gameUiView.ClearDiceBox();

        // 清空技能的 diceBox
        playerData.wantUseSkill.diceBox.Clear();
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
        playerData.wantUseSkill = _skill;
        // manaRoller.BtnMode(manaRollerMode.Idle);
        Debug.Log("Skill Card clicked" + _skill.skillName);
    }
    void ClearChooseSkill(object[] args)
    {
        playerData.wantUseSkill = null;
    }
    void OnBuffUse(object[] args)//TODO 之後換成生成特效
    {
        string buffname = (string)args[0];
        bool isPlayer = (bool)args[1];

        //顯示提示泡泡
        /*Transform bubblePos = isPlayer ? playerBuffBubblePos : enemyBuffBubblePos;
        GameObject bubble = Instantiate(buffBubblePrefab, bubblePos);
        bubble.transform.GetComponentInChildren<Text>().text = buffname;
        string str_buffAnim = isPlayer ? "BornBuff_L" : "BornBuff_R";
        bubble.GetComponent<Animator>().Play(str_buffAnim);*/

        Debug.Log($"{(isPlayer ? "Player" : "Enemy")} 使用buff {buffname}");
     //   Destroy(bubble, 2f); // 假設提示泡泡持續2秒
    }
    //敵我雙方使用技能都經過這裡
    void OnSkillUse(object[] args)
    {
        int skillID = (int)args[0];
        string skillname = (string)args[1];
        SkillType skillType = (SkillType)args[2];
        List<int> values = (List<int>)args[3];
        bool isPlayer = (bool)args[4];

        // 加入排隊
        skillOrderQueue.Enqueue(new SkillOrderData(skillID, skillname, skillType, values, isPlayer));
        // 如果沒有正在處理，開始處理排隊
        if (!isProcessingSkill)
        {
            ProcessSkillQueue();
        }
    }
    void OnClearNegativeBuffs(object[] args)
    {
        bool isPlayer = (bool)args[0];
        if (isPlayer)
        {
            playerData.RemoveAllBuff();
            UpdateBuffUIEvent(null);
            Debug.Log($"消除玩家所有buff");
        }
        else
        {
            enemyData.RemoveAllBuff();
            UpdateBuffUIEvent(null);
            Debug.Log($"消除敵方所有buff");
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

            // 處理特殊技能效果 
            if (SkillDatabase.SpSkillIDs.Contains(skillOrder.skillID))
                SpSkillEffect(ref skillOrder);

            //角色喊技能
            //gameUiView.CreateFlyText(skillOrder.skillName);//之後分類
            switch (skillOrder.skillType)
            {
                case SkillType.Attack:
                    gameUiView.PlayFightAnim(skillOrder.isPlayerUse);
                    //先做攻擊buff計算 實際用OnAttackCharacter給予傷害
                    attacker.Attack(skillOrder.values[0]);
                    Debug.Log($"{(skillOrder.isPlayerUse ? "Player" : "Enemy")} 使用攻擊技能 {skillOrder.skillName}，造成 {skillOrder.values[0]} 點傷害");
                    break;
                case SkillType.Heal:
                    gameUiView.PlayBuffVfx(skillOrder.isPlayerUse);
                    attacker.Heal(skillOrder.values[0]);
                    UpdateBloodUI(null);
                    Debug.Log($"{(skillOrder.isPlayerUse ? "Player" : "Enemy")} 使用治療技能 {skillOrder.skillName}，恢復 {skillOrder.values[0]} 點血量");
                    break;
                case SkillType.Buff:
                    gameUiView.PlayBuffVfx(skillOrder.isPlayerUse);
                    //之後依照buff做對應特效
                    break;
                case SkillType.DeBuff://補上deuff特效
                    gameUiView.PlayDebuffVfx(!skillOrder.isPlayerUse);
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
    //處理特殊技能效果的函式，根據技能ID或其他條件來決定要執行什麼額外效果
    private void SpSkillEffect(ref SkillOrderData _skillOrder)
    {
        BaseCharacterData attacker = _skillOrder.isPlayerUse ? playerData : enemyData;
        BaseCharacterData defender = _skillOrder.isPlayerUse ? enemyData : playerData;
        switch (_skillOrder.skillID)
        {
            case 113:
            case 38: // 毒爆術 消耗敵方中毒狀態增加傷害
                //中毒狀態的buff ID為 1，每個中毒回合增加x倍傷害
                IBuffData poisonBuff = defender.buffData.Find(buff => buff.buffID == 1);

                if (poisonBuff != null)
                {
                    _skillOrder.values[0] = _skillOrder.values[0] * poisonBuff.duration; // 增加額外傷害
                    poisonBuff.duration = 0;//移除中毒狀態
                }
                break;
            case 116:
            case 33: // 弱點破壞 敵方撕裂(id 3)時傷害加倍
                IBuffData tearBuff = defender.buffData.Find(buff => buff.buffID == 3);
                if (tearBuff != null)
                {
                    int attackDiceCount = attacker.rollDiceResult.Count; // 假設攻擊骰子數量等於當前擲出的骰子數量
                    _skillOrder.values[0] = _skillOrder.values[0] * 2; // 增加額外傷害
                }
                break;
            case 128: // 吞噬
                enemyEatBuff = true;//標記敵人正在吞噬，結束回合時會有額外等待
                break;

            // 可以根據需要添加更多技能ID的處理
            default:
                break;
        }
    }
    void OnBuffEffectBlood(object[] args)
    {
        float value = (float)args[0];
        bool isPlayer = (bool)args[1];

        if (isPlayer)
        {
            if (value > 0) gameUiView.PlayBloodVfx(true);
            playerData.TakeDamage(value);
            gameUiView.CreateFlyBloodText((int)(value), true, false);
        }
        else
        {
            if (value > 0) gameUiView.PlayBloodVfx(false);
            enemyData.TakeDamage(value);
            gameUiView.CreateFlyBloodText((int)(value), false, false);
        }
        UpdateBloodUI(null);
        Debug.Log($"Buff效果造成 {(value > 0 ? "傷害" : "治療")} {Math.Abs(value)} 點");
    }
    async void OnAttackCharacter(object[] args)//技能打擊
    {
        float damage = (float)args[0];
        bool isPlayer = (bool)args[1];//攻擊對象

        if (isPlayer)
        {
            playerData.TakeDamage(damage);
            gameUiView.CreateFlyBloodText((int)damage, true, true);
        }
        else
        {
            enemyData.TakeDamage(damage);
            gameUiView.CreateFlyBloodText((int)damage, false, true);
        }
        await Task.Yield(); // 確保傷害計算先執行
        UpdateBloodUI(null);
        //todo 結算回合
        //if (playerData.IsDead() || enemyData.IsDead())
        // EventCenter.Dispatch(GameEvent.EVENT_CHANGE_STATE, TurnState.roundEnd);
        Debug.Log($" 造成 {damage} 點傷害");
        //等一秒CheckLive
        await Task.Delay(1000);
        CheckLive();
    }
    void CheckLive()
    {
        if (currentState == TurnState.roundEnd) return;
        if (playerData.IsDead() || enemyData.IsDead())
        {
            if (playerData.IsDead())
            {
                //playerView.PlayAnim("defeat");
            }
            if (enemyData.IsDead())
            {
                //enemyView.PlayAnim("defeat");
            }
            Debug.Log("Game Over");
            ChangeState(TurnState.roundEnd);
        }
    }
    void UpdateBloodUI(object[] args)
    {
        gameUiView.UpdateBlood(true, playerData.currentBlood, playerData.maxBlood);
        gameUiView.UpdateBlood(false, enemyData.currentBlood, enemyData.maxBlood);
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
    async void RestartGame(object[] args)
    {
        playerData.RemoveAllBuff();
        enemyData.RemoveAllBuff();
        playerData.currentBlood = playerEnterBlood;
        await Task.Delay(500);
        //重置遊戲
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    void EscapeBattle(object[] args)
    {
        //逃離戰鬥
        Debug.Log("Escape Battle Triggered");
        GameDataManager.PlayerData.RemoveAllBuff();
        GameDataManager.CurrentStage = GameDataManager.PreparationRoomStage;//傳回整備室
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_PREPARATION_ROOM);
    }
}
