using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseView : MonoBehaviour
{
    [SerializeField] GameObject obj_win;
    [SerializeField] GameObject obj_lose;
    [SerializeField] Button btn_check;
    [SerializeField] Button btn_escape;
    [SerializeField] Button btn_restart;
    [SerializeField] Text txt_reward;
    public void SetData(bool isWin, string rewardText)
    {
        txt_reward.text = isWin ? rewardText : "";
        obj_win.SetActive(isWin);
        obj_lose.SetActive(!isWin);
        btn_escape.gameObject.SetActive(!isWin);
        btn_restart.gameObject.SetActive(!isWin);
        btn_check.gameObject.SetActive(isWin);

        btn_check.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(GameDataManager.CompletedStory))
            {
                string tmpStory = GameDataManager.CompletedStory;
                GameDataManager.CompletedStory = "";
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
