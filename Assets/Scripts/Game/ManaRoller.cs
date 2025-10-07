using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaRoller : MonoBehaviour
{
    public Button btn_roll = null;//擲骰子按鈕
    public Button btn_turnEnd = null;//結束回合按鈕
    public Button btn_fight = null;//戰鬥按鈕
    Transform rollDiceParent;    //骰子生成位置
    Transform keepDiceParent;    //保留骰子生成位置
    List<int> rollDices = new List<int>();  //所有骰子
    List<int> keepDices = new List<int>();    //保留骰子
    int maxkeepCount; //最大保留數量
    bool isOpen = false;
    [SerializeField] GameObject dicePrefab = null;
    Sprite[] diceSprites;
    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        btn_roll.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_ROLL); });
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });
        btn_fight.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_FIGHT); });
        rollDiceParent = GameObject.Find("diceBox/dices").transform;
        keepDiceParent = GameObject.Find("diceBox/keep").transform;
        diceSprites = Resources.LoadAll<Sprite>("dice/dice_base");
    }
    public void SetDice(List<int> _dices, int _keepDiceCount)
    {
        maxkeepCount = _keepDiceCount;
        rollDices.Clear();
        //使用_dices來生成骰子（避免在遍歷時修改集合）
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
    // Update is called once per frame
    void Update()
    {

    }
    bool CanKeepDice()
    {
        return keepDices.Count < maxkeepCount;
    }
    void burnKeepDice(int _sideNum)
    {
        keepDices.Add(_sideNum);
        GameObject dice = Instantiate(dicePrefab, keepDiceParent);
        dice.GetComponent<Image>().sprite = diceSprites[_sideNum-1];
        dice.GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(dice);
            keepDices.Remove(_sideNum); // 直接移除數值，不依賴索引
            burnRollDice(_sideNum);
        });
    }
    void burnRollDice(int _sideNum)
    {
        rollDices.Add(_sideNum);
        GameObject dice = Instantiate(dicePrefab, rollDiceParent);
        dice.GetComponent<Image>().sprite = diceSprites[_sideNum-1];
        dice.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (CanKeepDice())
            {
                Destroy(dice);
                rollDices.Remove(_sideNum);
                burnKeepDice(_sideNum);
            }
            else
            {
                Debug.Log("不能再保留骰子了");
            }
        });
    }
    public void ClearAllDices()
    {
        rollDices.Clear();
        foreach (Transform child in rollDiceParent)
        {
            Destroy(child.gameObject);
        }
    }
}
