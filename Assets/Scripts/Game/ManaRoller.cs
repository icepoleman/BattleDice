using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum manaRollerMode
{
    Off,
    Idle,
    UseDice
}
public class ManaRoller : MonoBehaviour
{
    manaRollerMode currentMode = manaRollerMode.Off;
    Button btn_roll = null;//擲骰子按鈕
    Button btn_turnEnd = null;//結束回合按鈕
    [SerializeField] Text text_freezeCount;
    Transform rollDiceParent;    //骰子生成位置
    int maxFreezeCount; //最大凍結數量
    int rollCount = 0; //最大擲骰次數
    int freezeCount = 0; //凍結數量
    [SerializeField] GameObject dicePrefab = null;
    [SerializeField] GameObject skillCardPrefab = null;
    Text txt_rollCount = null;//擲骰次數顯示
    bool isOpen = false;
    List<ManaRollerDice> manaDiceList = new List<ManaRollerDice>();
    int maxDiceCount = 8;//最大存放骰子數量
    [SerializeField] ToggleGroup skillToggleGroup;
    [SerializeField] List<SkillCard> skillCardList;
    [SerializeField] SkillCard chooseSkillCard;
    [SerializeField] Button btn_changeSkill;
    [SerializeField] Animator anim_skillBox;
    Sprite[] diceSprites; // 骰子圖集
    [SerializeField] Material diceMtl;
    public async void Init()
    {
        if (isOpen) return;
        var spriteList = await AddressableManager.LoadLabelAsync<Sprite>("Dice");
        // 依照名稱排序 (dice_0, dice_1, dice_2...)
        spriteList.Sort((a, b) =>
        {   
            int aNum = int.Parse(a.name.Replace("dice_", ""));
            int bNum = int.Parse(b.name.Replace("dice_", ""));
            return aNum.CompareTo(bNum);
        });
        diceSprites = spriteList.ToArray(); 
        //尋找物件
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        btn_roll = GameObject.Find("rollerBtns/btn_roll").GetComponent<Button>();
        btn_turnEnd = GameObject.Find("btn_turnEnd").GetComponent<Button>();
        txt_rollCount = GameObject.Find("rollerBtns/btn_roll/txt_rollCount").GetComponent<Text>();

        //按鈕事件
        btn_changeSkill.onClick.AddListener(() =>
        {
            bool isActive = anim_skillBox.GetBool("isOpen");
            anim_skillBox.SetBool("isOpen", !isActive);
        });
        btn_roll.onClick.AddListener(() => { RollDices(); });//擲骰子
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });//結束回合

        chooseSkillCard.SetInteractable(false);

        //監聽技能選取事件
        EventCenter.AddListener(GameEvent.EVENT_SELECT_SKILL, OnSkillSelected);

        BtnMode(manaRollerMode.Off);
        isOpen = true;
    }

    void OnDestroy()
    {
        EventCenter.RemoveListener(GameEvent.EVENT_SELECT_SKILL, OnSkillSelected);
    }

    void OnSkillSelected(object[] args)
    {
        anim_skillBox.SetBool("isOpen", false);
        ISkillData selectedSkill = args[0] as ISkillData;
        chooseSkillCard.SetData(selectedSkill);
        // 切換技能時清除所有骰子選取狀態
        ClearAllSelections();
    }

    //獲取初始骰子
    public async void SetDice(List<int> _dices, int _keepDiceCount, int _maxRollCount)
    {
        rollCount = _maxRollCount;
        txt_rollCount.text = "重骰次數：" + rollCount.ToString();
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
        for (int i = 0; i < iskList.Count; i++)
        {
            skillCardList[i].SetData(iskList[i], skillToggleGroup);
            if (!firstSetSkill)
            {
                EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, iskList[i]);
                firstSetSkill = true;
            }
        }
    }
    public void AddSkill(ISkillData isk)
    {
        //新增技能
    }
    public void RollDices()
    {
        rollCount--;
        txt_rollCount.text = "重骰次數：" + rollCount.ToString();
        for (int i = 0; i < manaDiceList.Count; i++)
        {
            int side = UnityEngine.Random.Range(1, 7); //假設骰子面數為6
                                                       // burnRollDice(side);
            manaDiceList[i].RollDice(side);
        }
        if (rollCount <= 0)
        {
            btn_roll.interactable = false;
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
                        UnityEngine.Debug.Log("已達到最大凍結骰子數量");
                    }
                }
                text_freezeCount.text = (maxFreezeCount - freezeCount).ToString();
            }
            , diceSprites
            , diceMtl
        );
        manaDiceList.Add(diceScript);
    }
}
