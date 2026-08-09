using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Threading.Tasks;
public enum manaRollerMode
{
    Off,
    Idle,
    UseDice
}
public class ManaRoller : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    manaRollerMode currentMode = manaRollerMode.Off;
    [SerializeField] Button btn_roll = null;//擲骰子按鈕
    [SerializeField] GameObject obj_vfx_roll = null;//擲骰子特效
    float rollAnimationDuration = 0.5f; // 擲骰子動畫總時長
    Button btn_turnEnd = null;//結束回合按鈕
    [Header("鎖骰數量")]
    [SerializeField] Text text_freezeCount;
    Transform rollDiceParent;    //骰子生成位置
    int maxFreezeCount; //最大凍結數量
    int rollCount = 0; //最大擲骰次數
    int freezeCount = 0; //凍結數量
    [Header("骰子")]
    [SerializeField] GameObject dicePrefab = null;
    [SerializeField] Image img_rollCount = null;//擲骰次數顯示
    RectTransform rect_rollCount;
    bool isOpen = false;
    List<ManaRollerDice> manaDiceList = new List<ManaRollerDice>();
    int maxDiceCount = 8;//最大存放骰子數量
    [Header("技能按鈕")]
    [SerializeField] ToggleGroup skillToggleGroup;
    [SerializeField] List<SkillCard> skillCardList = new List<SkillCard>();
    [Header("技能提示")]
    [SerializeField] TextMeshProUGUI text_skillHint;
    [SerializeField] GameObject obj_buffTip;
    [SerializeField] TextMeshProUGUI text_buffTip;
    List<Sprite> diceSprites = new List<Sprite>();
    [SerializeField] Material diceMtl;

    void Awake()
    {
        //監聽技能選取事件
        EventCenter.AddListener(GameEvent.EVENT_SELECT_SKILL, OnSkillSelected);
    }
    public async Task Init()
    {
        if (isOpen) return;

        diceSprites.Clear();
        for (int i = 0; i <= 6; i++)
        {
            diceSprites.Add(AtlasLoader.Instance.GetDiceSprite(i));
        }

        //尋找物件
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        btn_turnEnd = GameObject.Find("btn_turnEnd").GetComponent<Button>();
        rect_rollCount = img_rollCount.GetComponent<RectTransform>();
        rect_rollCount.anchoredPosition = new Vector2(rect_rollCount.anchoredPosition.x, -200f);

        btn_roll.onClick.AddListener(() => { RollDices(); });//擲骰子
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });//結束回合

        BtnMode(manaRollerMode.Off);
        isOpen = true;
    }

    void OnDestroy()
    {
        rect_rollCount?.DOKill();
        EventCenter.RemoveListener(GameEvent.EVENT_SELECT_SKILL, OnSkillSelected);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TrySelectSkillCard(0);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            TrySelectSkillCard(1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TrySelectSkillCard(2);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            TrySelectSkillCard(3);
        }
    }

    void TrySelectSkillCard(int index)
    {
        if (index < 0 || index >= skillCardList.Count)
        {
            return;
        }

        SkillCard card = skillCardList[index];
        if (card == null || !card.CanBeSelected)
        {
            return;
        }

        card.SelectByShortcut();
    }

    /// <summary>
    /// 開啟擲骰次數顯示（從-200移動到0，完成後漂浮）
    /// </summary>
    public void OpenRollCount()
    {
        rect_rollCount.DOKill();
        rect_rollCount.DOAnchorPosY(-10f, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // 漂浮動畫
                rect_rollCount.DOAnchorPosY(10f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            });
    }

    /// <summary>
    /// 關閉擲骰次數顯示（移動到-200）
    /// </summary>
    public void CloseRollCount()
    {
        rect_rollCount.DOKill();
        rect_rollCount.DOAnchorPosY(-200f, 0.3f)
            .SetEase(Ease.InBack);
    }

    void OnSkillSelected(object[] args)
    {
        ISkillData selectedSkill = args[0] as ISkillData;
        //chooseSkillCard.SetData(selectedSkill);
        text_skillHint.text = selectedSkill.conditionText + "\n" + selectedSkill.effectText;
        // 切換技能時清除所有骰子選取狀態
        ClearAllSelections();
    }

    //獲取初始骰子
    public async void SetDice(List<int> _dices, int _keepDiceCount, int _maxRollCount)
    {
        if (_maxRollCount > 6)
        {
            _maxRollCount = 6;//最大擲骰次數限制為6次，避免UI顯示問題
        }
        rollCount = _maxRollCount;
        img_rollCount.sprite = diceSprites[rollCount];

        OpenRollCount();
        maxFreezeCount = _keepDiceCount;
        text_freezeCount.text = maxFreezeCount.ToString();
        maxDiceCount = GameDataManager.PlayerData.manaRollerMaxDiceCount;

        foreach (var sideNum in _dices)
        {
            if (manaDiceList.Count >= maxDiceCount)
            {
                break;
            }
            burnRollDice(sideNum);
            await System.Threading.Tasks.Task.Delay(100);
        }
    }
    public manaRollerMode GetCurrentMode()
    {
        return currentMode;
    }
    public void BtnMode(manaRollerMode mode)
    {
        switch (mode)
        {
            case manaRollerMode.Off:
                btn_roll.interactable = false;
                btn_turnEnd.interactable = false;
                break;
            case manaRollerMode.Idle:
                btn_roll.interactable = rollCount > 0 && manaDiceList.Count > 0;
                btn_turnEnd.interactable = true;
                break;
            case manaRollerMode.UseDice:
                btn_roll.interactable = false;
                btn_turnEnd.interactable = false;
                break;
        }
        currentMode = mode;
    }

    bool firstSetSkill = false;
    //生成技能卡
    public void SetAllSkill(List<ISkillData> iskList)
    {
        for (int i = 0; i < skillCardList.Count; i++)
        {
            if (i < iskList.Count)
            {
                skillCardList[i].SetData(iskList[i], skillToggleGroup);
                if (!firstSetSkill)
                {
                    EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, iskList[i]);
                    firstSetSkill = true;
                }
            }
            else
            {
                skillCardList[i].SetInteractable(false);
            }
        }
    }
    public async void RollDices()
    {
        rollCount--;
        img_rollCount.sprite = diceSprites[rollCount];
        btn_roll.interactable = false; // 立即禁用按鈕
        obj_vfx_roll.SetActive(false); // 先關閉特效以重置動畫
        obj_vfx_roll.SetActive(true); // 重新啟用特效以播放

        for (int i = 0; i < manaDiceList.Count; i++)
        {
            float rollAnimTime = rollAnimationDuration + UnityEngine.Random.Range(-0.2f, 0.2f); // 隨機化動畫時間，增加自然感
            int side = UnityEngine.Random.Range(1, 7); //假設骰子面數為6
            manaDiceList[i].RollDice(side, rollAnimTime);
        }

        // 等待動畫結束
        await Task.Delay((int)(rollAnimationDuration * 1000));

        // 動畫結束後，如果還有次數且在 Idle 模式才重新啟用
        if (rollCount > 0 && currentMode == manaRollerMode.Idle && manaDiceList.Count > 0)
        {
            btn_roll.interactable = true;
        }
        else
        {
            CloseRollCount();
        }
    }
    public void ClearAllRollDices()
    {
        manaDiceList.Clear();
        foreach (Transform child in rollDiceParent)
        {
            Destroy(child.gameObject);
        }
    }
    bool CanFreezeDice()
    {
        return freezeCount < maxFreezeCount;
    }

    /// <summary>
    /// 取得所有已選取的骰子
    /// </summary>
    public List<ManaRollerDice> GetSelectedDices()
    {
        List<ManaRollerDice> selected = new List<ManaRollerDice>();
        foreach (var dice in manaDiceList)
        {
            if (dice.isSelected)
            {
                selected.Add(dice);
            }
        }
        return selected;
    }

    /// <summary>
    /// 取得已選取骰子的點數列表
    /// </summary>
    public List<int> GetSelectedDiceValues()
    {
        List<int> values = new List<int>();
        foreach (var dice in manaDiceList)
        {
            if (dice.isSelected)
            {
                values.Add(dice.sideNum);
            }
        }
        return values;
    }

    /// <summary>
    /// 消耗已選取的骰子
    /// </summary>
    public void ConsumeSelectedDices()
    {
        List<ManaRollerDice> toRemove = new List<ManaRollerDice>();
        foreach (var dice in manaDiceList)
        {
            if (dice.isSelected)
            {
                if (dice.IsFrozen())
                {
                    freezeCount--;
                }
                toRemove.Add(dice);
            }
        }

        foreach (var dice in toRemove)
        {
            manaDiceList.Remove(dice);
            Destroy(dice.gameObject);
        }

        text_freezeCount.text = (maxFreezeCount - freezeCount).ToString();
    }

    /// <summary>
    /// 清除所有骰子的選取狀態
    /// </summary>
    public void ClearAllSelections()
    {
        foreach (var dice in manaDiceList)
        {
            dice.SetSelected(false);
        }
    }

    public void burnRollDice(int _sideNum)
    {
        GameObject dice = Instantiate(dicePrefab, rollDiceParent);
        ManaRollerDice diceScript = dice.GetComponent<ManaRollerDice>();
        diceScript.SetDice(_sideNum,
            // 左鍵點擊 - 切換選取狀態
            (clickedDice) =>
            {
                if (currentMode == manaRollerMode.Off) return;
                if (clickedDice.IsFrozen())
                {
                    clickedDice.SetFrozen(false);
                    freezeCount--;
                }
                text_freezeCount.text = (maxFreezeCount - freezeCount).ToString();
                clickedDice.ToggleSelect();
                // 通知選取狀態變更
                EventCenter.Dispatch(GameEvent.EVENT_DICE_SELECTION_CHANGED, GetSelectedDiceValues());
            },
            // 右鍵點擊 - 凍結/解凍骰子
            (diceObj) =>
            {
                if (currentMode == manaRollerMode.Off) return;
                if (diceObj.IsFrozen())
                {
                    diceObj.SetFrozen(false);
                    freezeCount--;
                }
                else
                {
                    if (CanFreezeDice())
                    {
                        freezeCount++;
                        diceObj.SetFrozen(true);
                    }
                    else
                    {

                        UIManager.ShowHintBubble(LanguageManager.GetText("T_ManaRoller_FreezeLimitReached"));
                    }
                }
                text_freezeCount.text = (maxFreezeCount - freezeCount).ToString();
            }
            , diceSprites.ToArray()
            , diceMtl
        );
        manaDiceList.Add(diceScript);
    }
    #region BUFF說明的Tooltip
    private int currentLinkIndex = -1;
    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            text_skillHint,
            eventData.position,
            eventData.enterEventCamera
        );

        if (linkIndex != -1)
        {
            if (linkIndex != currentLinkIndex)
            {
                currentLinkIndex = linkIndex;
                TMP_LinkInfo linkInfo = text_skillHint.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();

                // 取得 link 在文字中的位置（使用第一個字符的位置）
                int firstCharIndex = linkInfo.linkTextfirstCharacterIndex;
                TMP_CharacterInfo charInfo = text_skillHint.textInfo.characterInfo[firstCharIndex];
                Vector3 charWorldPos = text_skillHint.transform.TransformPoint(charInfo.topLeft);

                obj_buffTip.SetActive(true);
                BuffConfigData buffData = BuffDatabase.GetBuffConfig(int.Parse(linkId));
                text_buffTip.text = buffData.describe;
            }
        }
        else
        {
            if (currentLinkIndex != -1)
            {
                currentLinkIndex = -1;
                obj_buffTip.SetActive(false);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        currentLinkIndex = -1;
        obj_buffTip.SetActive(false);
    }
    #endregion

    public bool CheckHasDice()
    {
        return manaDiceList.Count > 0;
    }
}
