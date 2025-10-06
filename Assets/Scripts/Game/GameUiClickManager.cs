using UnityEngine;
using UnityEngine.UI;

public class GameUiClickManager : MonoBehaviour
{
    public Button btn_roll = null;//擲骰子按鈕
    public Button btn_turnEnd = null;//結束回合按鈕
    public Button btn_fight = null;//戰鬥按鈕
    bool isOpen = false;
    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        btn_roll.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_ROLL); });
        btn_turnEnd.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_TURN_END); });
        btn_fight.onClick.AddListener(() => { EventCenter.Dispatch(GameEvent.EVENT_CLICK_FIGHT); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
