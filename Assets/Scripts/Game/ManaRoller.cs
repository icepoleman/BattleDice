using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaRoller : MonoBehaviour
{
    Button btn_roll = null;//擲骰子按鈕
    Button btn_turnEnd = null;//結束回合按鈕
    Button btn_fight = null;//戰鬥按鈕
    Transform rollDiceParent;    //骰子生成位置
    Transform keepDiceParent;    //保留骰子生成位置
    List<int> rollDices = new List<int>();  //所有骰子
    List<int> keepDices = new List<int>();    //保留骰子
    int maxkeepCount; //最大保留數量
    bool isOpen = false;
    [SerializeField] GameObject dicePrefab = null;
    public bool onUseDice = false;
    void Start()
    {
        if (isOpen) return;
        //尋找物件
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        keepDiceParent = GameObject.Find("diceBox/keep").transform;
        btn_roll = GameObject.Find("btn_roll").GetComponent<Button>();
        btn_turnEnd = GameObject.Find("btn_turnEnd").GetComponent<Button>();
        btn_fight = GameObject.Find("btn_fight").GetComponent<Button>();
        //按鈕事件
        btn_roll.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_ROLL); });
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });
        btn_fight.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_FIGHT); });

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
    public void RollDices()
    {
        int totalDice = rollDices.Count;
        ClearAllDices();
        for (int i = 0; i < totalDice; i++)
        {
            int side = Random.Range(1, 7); //假設骰子面數為6
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
    public void CheckCanUseDice(List<int> skillNeedDices)
    {
        if (skillNeedDices == null || skillNeedDices.Count == 0)
        {
            Debug.Log("技能不需要骰子");
            return;
        }

        //判斷是否是技能需要的骰子 不是的按鈕無法點擊 變灰
        foreach (Transform child in rollDiceParent)
        {
            ManaRollerDice diceScript = child.GetComponent<ManaRollerDice>();
            if (diceScript != null)
            {
                int sideNum = diceScript.GetSideNum();
                Debug.Log("檢查骰子面數: " + sideNum);
                //檢查這個骰子是否在技能需求中
                if (skillNeedDices.Contains(sideNum))
                {
                    //這個骰子是技能需求的一部分，可以使用
                    //可以在這裡添加額外的邏輯，例如改變骰子的外觀等
                    diceScript.IsSkillDice();
                }
                else
                {
                    //這個骰子不是技能需求的一部分，不能使用
                    //可以在這裡添加額外的邏輯，例如改變骰子的外觀等
                    diceScript.UnSkillDice();
                }
            }
        }
       
        //onUseDice = rollDices.Count > 0;
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
