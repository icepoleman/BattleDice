using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class AffinityItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt_storyName;
    [SerializeField] TextMeshProUGUI txt_unlockHint;
    [SerializeField] Button btn_enterStage;
    public void SetUp(AffinityStoryData storyData, int index)
    {
        string storyID = "Affinity_" + storyData.role + "_" + (index + 1);
        btn_enterStage.onClick.AddListener(() =>
        {
            // 進入親密度關卡的事件
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, storyID);
        });
        // 根據 storyData 設置親密度項目的顯示內容
        // 例如：顯示故事名稱、解鎖提示等
        if (GameDataManager.unlockedAffinityStages.Contains(storyID))
        {
            txt_unlockHint.text = "";
            txt_storyName.text = storyData.storyName;
            // 已解鎖，按鈕可用
            btn_enterStage.interactable = true;
        }
        else
        {
            txt_unlockHint.text = storyData.unlockHint;
            txt_storyName.text = "?????";
            // 未解鎖，按鈕不可用
            btn_enterStage.interactable = false;
        }
    }
}
