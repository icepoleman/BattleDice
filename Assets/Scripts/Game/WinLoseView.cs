using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinLoseView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt_title;
    [SerializeField] Button btn_check;
    [SerializeField] Button btn_escape;
    [SerializeField] Button btn_restart;
    [SerializeField] TextMeshProUGUI txt_gold;
    [SerializeField] TextMeshProUGUI txt_gear;
    public void SetData(bool isWin, string goldReward, string gearReward)
    {
        txt_gold.text = isWin ? goldReward : "";
        txt_gear.text = isWin ? gearReward : "";
        txt_title.text = isWin ? "Win!!!" : "You Lose!";
        
        btn_escape.gameObject.SetActive(!isWin);
        btn_restart.gameObject.SetActive(!isWin);
        btn_check.gameObject.SetActive(isWin);

        btn_check.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(GameDataManager.TmpCompletedStory))
            {
                string tmpStory = GameDataManager.TmpCompletedStory;
                GameDataManager.TmpCompletedStory = "";
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, tmpStory);
            }
            else
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
        });
        btn_escape.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(GameEvent.EVENT_ESCAPE_BATTLE);
        });
        btn_restart.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(GameEvent.EVENT_RESTART_GAME);
        });
    }
}
