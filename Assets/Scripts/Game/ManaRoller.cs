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
    KeepDice,
}
public class ManaRoller : MonoBehaviour
{
    manaRollerMode currentMode = manaRollerMode.Off;
    Button btn_roll = null;//擲骰子按鈕
    Button btn_turnEnd = null;//結束回合按鈕
    [SerializeField] Button btn_keep = null;//保留骰子按鈕
    Transform rollDiceParent;    //骰子生成位置
    Transform keepDiceParent;    //保留骰子生成位置
    List<int> rollDices = new List<int>();  //所有骰子
    List<int> keepDices = new List<int>();    //保留骰子
    int maxkeepCount; //最大保留數量
    int rollCount = 0; //最大擲骰次數
    [SerializeField] GameObject dicePrefab = null;
    [SerializeField] GameObject skillCardPrefab = null;
    [SerializeField] GameObject diceOFF;
    Transform skillCardParent;    //技能生成位置
    TextMeshProUGUI txt_rollCount = null;//擲骰次數顯示
    bool isOpen = false;
    public void Init()
    {
        if (isOpen) return;
        //尋找物件
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        keepDiceParent = GameObject.Find("keep").transform;
        skillCardParent = GameObject.Find("skillBox").transform;
        btn_roll = GameObject.Find("rollerBtns/btn_roll").GetComponent<Button>();
        btn_turnEnd = GameObject.Find("btn_turnEnd").GetComponent<Button>();
        txt_rollCount = GameObject.Find("rollerBtns/btn_roll/txt_rollCount").GetComponent<TextMeshProUGUI>();

        //按鈕事件
        btn_roll.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_ROLL); });//擲骰子
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });//結束回合
        btn_keep.onClick.AddListener(() => { BtnMode(manaRollerMode.KeepDice); });//保留骰子
        //btn_cancelFight.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_CANCEL_SKILL); });//取消戰鬥


        BtnMode(manaRollerMode.Off);
        isOpen = true;
    }

    int maxRollCount = 6;//測試用 test 
    //獲取初始骰子
    public void SetDice(List<int> _dices, int _keepDiceCount, int _maxRollCount)
    {
        rollCount = _maxRollCount;
        txt_rollCount.text = "重骰次數：" + rollCount.ToString();
        maxkeepCount = _keepDiceCount;
        //rollDices.Clear();
        foreach (var sideNum in _dices)
        {
            if (rollDices.Count >= maxRollCount)
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
                btn_keep.interactable = false;
                break;
            case manaRollerMode.Idle:
                btn_roll.interactable = rollCount > 0 && rollDices.Count > 0;
                btn_turnEnd.interactable = true;
                btn_keep.interactable = true;
                btn_keep.image.color = new Color32(255, 255, 255, 255);
                keepDiceParent.localScale = new Vector3(0.3f, 0.3f, 1);
                break;
            case manaRollerMode.UseDice:
                btn_roll.interactable = false;
                btn_turnEnd.interactable = false;
                btn_keep.interactable = false;
                btn_keep.image.color = new Color32(255, 255, 255, 255);
                keepDiceParent.localScale = new Vector3(0.3f, 0.3f, 1);
                break;
            case manaRollerMode.KeepDice:
                btn_keep.image.color = new Color32(255, 255, 0, 255);
                keepDiceParent.localScale = new Vector3(1f, 1f, 1);
                EventCenter.Dispatch(GameEvent.EVENT_CLEAR_CHOOSE_SKILL);
                break;
        }
        currentMode = mode;
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
            skillCard.SetData(isk, () =>
            {
                if (currentMode == manaRollerMode.UseDice || currentMode == manaRollerMode.Off)
                    return;
                EventCenter.Dispatch(GameEvent.EVENT_CLEAR_CHOOSE_SKILL);
                EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, isk);
                BtnMode(manaRollerMode.Idle);
                skillCard.SkillChoosenEvent();
            });
            if (!firstSetSkill)
            {
                EventCenter.Dispatch(GameEvent.EVENT_SELECT_SKILL, isk);
                skillCard.SkillChoosenEvent();
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
        int totalDice = rollDices.Count;
        ClearAllRollDices();
        for (int i = 0; i < totalDice; i++)
        {
            int side = UnityEngine.Random.Range(1, 7); //假設骰子面數為6
            burnRollDice(side);
        }
        if (rollCount <= 0)
        {
            btn_roll.interactable = false;
        }
    }
    public void ClearAllRollDices()
    {
        rollDices.Clear();
        foreach (Transform child in rollDiceParent)
        {
            Destroy(child.gameObject);
        }
    }
    bool CanKeepDice()
    {
        return keepDices.Count < maxkeepCount;
    }
    void burnKeepDice(int _sideNum)
    {
        keepDices.Add(_sideNum);
        GameObject dice = Instantiate(dicePrefab, keepDiceParent);
        ManaRollerDice diceScript = dice.GetComponent<ManaRollerDice>();
        diceScript.SetDice(_sideNum, (sideNum) =>
        {
            switch (currentMode)
            {
                case manaRollerMode.KeepDice://保留骰子模式下點擊保留骰子，將其丟回擲骰區
                    Destroy(dice);
                    keepDices.Remove(sideNum);
                    burnRollDice(sideNum);
                    break;
            }
        });
    }

    void burnRollDice(int _sideNum)
    {
        rollDices.Add(_sideNum);
        GameObject dice = Instantiate(dicePrefab, rollDiceParent);
        ManaRollerDice diceScript = dice.GetComponent<ManaRollerDice>();
        diceScript.SetDice(_sideNum, (sideNum) =>
        {
            switch (currentMode)
            {
                case manaRollerMode.UseDice:
                case manaRollerMode.Idle:
                    Destroy(dice);
                    rollDices.Remove(sideNum);
                    EventCenter.Dispatch(GameEvent.EVENT_ADD_POWER_DICE, sideNum);
                    break;
                case manaRollerMode.KeepDice:
                    if (CanKeepDice())
                    {
                        Destroy(dice);
                        rollDices.Remove(sideNum);
                        burnKeepDice(sideNum);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("已達到最大保留骰子數量");
                    }
                    break;
            }
        });
    }
}
