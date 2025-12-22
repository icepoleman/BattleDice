using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum manaRollerMode
{
    Off,
    Idle,
    UseDice,
    FreezeDice
}
public class ManaRoller : MonoBehaviour
{
    manaRollerMode currentMode = manaRollerMode.Off;
    Button btn_roll = null;//擲骰子按鈕
    Button btn_turnEnd = null;//結束回合按鈕
    [SerializeField] Toggle tog_freeze = null;//凍結骰子按鈕
    [SerializeField] Text text_freezeCount;
    Transform rollDiceParent;    //骰子生成位置
    int maxFreezeCount; //最大凍結數量
    int rollCount = 0; //最大擲骰次數
    int freezeCount = 0; //凍結數量
    [SerializeField] GameObject dicePrefab = null;
    [SerializeField] GameObject skillCardPrefab = null;
    [SerializeField] GameObject diceOFF;
    Transform skillCardParent;    //技能生成位置
    Text txt_rollCount = null;//擲骰次數顯示
    bool isOpen = false;
    List<ManaRollerDice> manaDiceList = new List<ManaRollerDice>();
    int maxDiceCount = 8;//最大存放骰子數量
    [SerializeField] ToggleGroup skillToggleGroup;
    List<SkillCard> skillCardList = new List<SkillCard>();
    public void Init()
    {
        if (isOpen) return;
        //尋找物件
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        skillCardParent = GameObject.Find("skillBox").transform;
        btn_roll = GameObject.Find("rollerBtns/btn_roll").GetComponent<Button>();
        btn_turnEnd = GameObject.Find("btn_turnEnd").GetComponent<Button>();
        txt_rollCount = GameObject.Find("rollerBtns/btn_roll/txt_rollCount").GetComponent<Text>();

        //按鈕事件
        btn_roll.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_ROLL); });//擲骰子
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });//結束回合
        tog_freeze.onValueChanged.AddListener((isOn) => { BtnMode(isOn ? manaRollerMode.FreezeDice : manaRollerMode.Idle); });//凍結骰子

        BtnMode(manaRollerMode.Off);
        isOpen = true;
    }
    //獲取初始骰子
    public void SetDice(List<int> _dices, int _keepDiceCount, int _maxRollCount)
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
        }
    }
    public manaRollerMode GetCurrentMode()
    {
        return currentMode;
    }
    public void BtnMode(manaRollerMode mode)
    {
        diceOFF.SetActive(mode == manaRollerMode.Off);
        switch (mode)
        {
            case manaRollerMode.Off:
                btn_roll.interactable = false;
                btn_turnEnd.interactable = false;
                tog_freeze.interactable = false;
                SetSkillCardInteractable(false);
                break;
            case manaRollerMode.Idle:
                btn_roll.interactable = rollCount > 0 && manaDiceList.Count > 0;
                btn_turnEnd.interactable = true;
                tog_freeze.interactable = true;
                SetSkillCardInteractable(true);
                break;
            case manaRollerMode.UseDice:
                btn_roll.interactable = false;
                btn_turnEnd.interactable = false;
                tog_freeze.interactable = false;
                SetSkillCardInteractable(false);
                break;
            case manaRollerMode.FreezeDice:
                //EventCenter.Dispatch(GameEvent.EVENT_CLEAR_CHOOSE_SKILL);
                break;
        }
        currentMode = mode;
    }
    public void SetSkillCardInteractable(bool _isInteractable)
    {
        foreach (var skillCard in skillCardList)
        {
            skillCard.SetInteractable(_isInteractable);
        }
    }
    bool firstSetSkill = false;
    //生成技能卡
    public void SetAllSkill(List<ISkillData> iskList)
    {
        foreach (var isk in iskList)
        {
            //生成技能物件
            GameObject skillObj = Instantiate(skillCardPrefab, skillCardParent);
            SkillCard skillCard = skillObj.GetComponent<SkillCard>();
            skillCard.SetData(isk, skillToggleGroup);
            skillCardList.Add(skillCard);
            if (!firstSetSkill)
            {
                EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, isk);
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
    void burnRollDice(int _sideNum)
    {
        GameObject dice = Instantiate(dicePrefab, rollDiceParent);
        ManaRollerDice diceScript = dice.GetComponent<ManaRollerDice>();
        diceScript.SetDice(_sideNum, (sideNum) =>
        {
            switch (currentMode)
            {
                case manaRollerMode.UseDice:
                case manaRollerMode.Idle:
                    if (diceScript.IsFrozen())
                    {
                        freezeCount--;
                        text_freezeCount.text = (maxFreezeCount - freezeCount).ToString();
                    }
                    Destroy(dice);
                    manaDiceList.Remove(diceScript);
                    EventCenter.Dispatch(GameEvent.EVENT_ADD_POWER_DICE, sideNum);
                    break;
                case manaRollerMode.FreezeDice:
                    //freezeDices.Add(sideNum);
                    if (diceScript.IsFrozen())
                    {
                        diceScript.SetFrozen(false);
                        freezeCount--;
                    }
                    else
                    {
                        if (CanFreezeDice())
                        {
                            freezeCount++;
                            diceScript.SetFrozen(true);
                        }
                        else
                        {
                            UnityEngine.Debug.Log("已達到最大凍結骰子數量");
                        }
                    }
                    text_freezeCount.text = (maxFreezeCount - freezeCount).ToString();
                    UnityEngine.Debug.Log(maxFreezeCount + " " + freezeCount);
                    break;
            }
        });
        manaDiceList.Add(diceScript);
    }
}
