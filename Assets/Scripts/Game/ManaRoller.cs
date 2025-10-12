using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public void Init()
    {
        if (isOpen) return;
        //尋找物件
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        keepDiceParent = GameObject.Find("diceBox/keep").transform;
        skillParent = GameObject.Find("skillBox").transform;
        btn_roll = GameObject.Find("btn_roll").GetComponent<Button>();
        btn_turnEnd = GameObject.Find("btn_turnEnd").GetComponent<Button>();
        btn_fight = GameObject.Find("btn_fight").GetComponent<Button>();
        btn_cancelFight = GameObject.Find("btn_cancelFight").GetComponent<Button>();
        //按鈕事件
        btn_roll.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_ROLL); });
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });
        btn_fight.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_FIGHT); });
        btn_cancelFight.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(GameEvent.EVENT_STOP_USE_DICE);
            chosenSkillData = null;
        });

        isOpen = true;
    }
    //獲取初始骰子
    public void SetDice(List<int> _dices, int _keepDiceCount)
    {
        maxkeepCount = _keepDiceCount;
        rollDices.Clear();
        foreach (var sideNum in _dices)
        {
            burnRollDice(sideNum);
        }
    }
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
        int totalDice = rollDices.Count;
        ClearAllDices();
        for (int i = 0; i < totalDice; i++)
        {
            int side = UnityEngine.Random.Range(1, 7); //假設骰子面數為6
            burnRollDice(side);
        }
    }
    public void ClearAllDices()
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
                Debug.Log("不能再保留骰子了");
            }
        });
    }
}
