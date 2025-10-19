using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum manaRollerMode
{
    Off,
    RollDice,
    UseDice,
    CanFight,
}
public class ManaRoller : MonoBehaviour
{
    Button btn_roll = null;//擲骰子按鈕
    Button btn_turnEnd = null;//結束回合按鈕
    Button btn_fight, btn_cancelFight;//戰鬥按鈕
    Transform rollDiceParent;    //骰子生成位置
    Transform keepDiceParent;    //保留骰子生成位置
    List<int> rollDices = new List<int>();  //所有骰子
    List<int> keepDices = new List<int>();    //保留骰子
    int maxkeepCount; //最大保留數量
    bool isOpen = false;
    [SerializeField] GameObject dicePrefab = null;
    [SerializeField] GameObject skillPrefab = null;
    Transform skillParent;    //技能生成位置
    public ISkillData chosenSkillData = null;
    public List<ISkillData> skillDeBuffList = new List<ISkillData>();
    TextMeshProUGUI txt_rollCount = null;//擲骰次數顯示
    int rollCount = 0; //最大擲骰次數
    public void Init()
    {
        if (isOpen) return;
        //尋找物件
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        keepDiceParent = GameObject.Find("diceBox/keep").transform;
        skillParent = GameObject.Find("skillBox").transform;
        btn_roll = GameObject.Find("rollerBtns/btn_roll").GetComponent<Button>();
        btn_fight = GameObject.Find("rollerBtns/btn_fight").GetComponent<Button>();
        btn_cancelFight = GameObject.Find("rollerBtns/btn_cancelFight").GetComponent<Button>();
        btn_turnEnd = GameObject.Find("btn_turnEnd").GetComponent<Button>();
        txt_rollCount = GameObject.Find("rollerBtns/btn_roll/txt_rollCount").GetComponent<TextMeshProUGUI>();
        //按鈕事件
        btn_roll.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_ROLL); });//擲骰子
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });//結束回合
        btn_fight.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_FIGHT); });//戰鬥
        btn_cancelFight.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_CANCEL_SKILL); });//取消戰鬥

        isOpen = true;
        BtnMode(manaRollerMode.Off);
    }
    //獲取初始骰子
    public void SetDice(List<int> _dices, int _keepDiceCount, int _maxRollCount)
    {
        btn_roll.interactable = true;
        rollCount = _maxRollCount;
        txt_rollCount.text = "擲骰次數：" + rollCount.ToString();
        maxkeepCount = _keepDiceCount;
        rollDices.Clear();
        foreach (var sideNum in _dices)
        {
            burnRollDice(sideNum);
        }
    }
    public void BtnMode(manaRollerMode mode)
    {
        switch (mode)
        {
            case manaRollerMode.Off:
                btn_roll.gameObject.SetActive(false);
                btn_turnEnd.gameObject.SetActive(false);
                btn_fight.gameObject.SetActive(false);
                btn_cancelFight.gameObject.SetActive(false);
                break;
            case manaRollerMode.RollDice:
                btn_roll.gameObject.SetActive(true);
                btn_turnEnd.gameObject.SetActive(true);
                btn_fight.gameObject.SetActive(false);
                btn_cancelFight.gameObject.SetActive(false);
                break;
            case manaRollerMode.UseDice:
                btn_roll.gameObject.SetActive(false);
                btn_turnEnd.gameObject.SetActive(true);
                btn_fight.gameObject.SetActive(false);
                btn_cancelFight.gameObject.SetActive(true);
                break;
            case manaRollerMode.CanFight:
                btn_roll.gameObject.SetActive(false);
                btn_turnEnd.gameObject.SetActive(true);
                btn_fight.gameObject.SetActive(true);
                btn_cancelFight.gameObject.SetActive(true);
                break;
        }
    }
    //生成技能卡
    public void SetAllSkill(List<ISkillData> iskList)
    {
        foreach (var isk in iskList)
        {
            //生成技能物件
            GameObject skillObj = Instantiate(skillPrefab, skillParent);
            SkillCard skillCard = skillObj.GetComponent<SkillCard>();
            skillCard.SetData(isk);
        }
    }
    public void AddSkill(ISkillData isk)
    {
        //新增技能
    }
    public void RollDices()
    {
        rollCount--;
        txt_rollCount.text = "擲骰次數：" + rollCount.ToString();
        int totalDice = rollDices.Count;
        ClearAllRollDices();
        for (int i = 0; i < totalDice; i++)
        {
            int side = UnityEngine.Random.Range(1, 7); //假設骰子面數為6
            burnRollDice(side);
        }
        if(rollCount <= 0)
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
            Destroy(dice);
            keepDices.Remove(sideNum);
            burnRollDice(sideNum);
        });
    }

    void burnRollDice(int _sideNum)
    {
        rollDices.Add(_sideNum);
        GameObject dice = Instantiate(dicePrefab, rollDiceParent);
        ManaRollerDice diceScript = dice.GetComponent<ManaRollerDice>();
        diceScript.SetDice(_sideNum, (sideNum) =>
        {
            if (CanKeepDice())
            {
                Destroy(dice);
                rollDices.Remove(sideNum);
                burnKeepDice(sideNum);
            }
            else
            {
                //Debug.Log("不能再保留骰子了");
            }
        });
    }
    public void UseSkill()//消耗使用的技能骰
    {
        UseDices(rollDiceParent);
        UseDices(keepDiceParent);
        chosenSkillData.Use();
        chosenSkillData = null;
    }
    public void CancelSkillUse()
    {
        if(chosenSkillData == null) return;
        chosenSkillData.diceBox.Clear();
        chosenSkillData = null;
    }
    void UseDices(Transform diceBox)
    {
        //使用選取的骰子
        foreach (Transform child in diceBox)
        {
            ManaRollerDice diceScript = child.GetComponent<ManaRollerDice>();
            if (diceScript != null && diceScript.isChosen)
            {
                rollDices.Remove(diceScript.GetSideNum());
                diceScript.UseDice();
            }
        }
    }
}
