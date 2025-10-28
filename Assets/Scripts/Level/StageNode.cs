using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageNode : MonoBehaviour
{
    public enum StageState
    {
        Locked,    // 鎖定
        Unlocked,  // 解鎖但未完成
        Completed  // 已完成
    }

    public enum StageType
    {
        Story,     // 劇情關卡
        Battle,    // 戰鬥關卡
        Shop,      // 商店
        Boss,       // 頭目關卡
        SavePoint,  // 整備室
        Item       // 道具關卡
    }
    [Header("關卡設定")]
    public string stageID;              // 關卡唯一ID //物件名稱
    public StageType stageType;         // 關卡類型
    //敵人設定? 測試用
    public int enemyId;

    [Header("連接設定")]
    public List<string> nextNodes = new List<string>(); // 完成後解鎖的節點

    [Header("視覺設定")]
    public GameObject lockedVisual;     // 鎖定時的視覺
    public GameObject unlockedVisual;   // 解鎖時的視覺
    public GameObject completedVisual;  // 完成時的視覺

    private Button nodeButton;
    private StageState currentState;



    void Awake()
    {
        nodeButton = GetComponent<Button>();
        nodeButton.onClick.AddListener(OnNodeClick);
    }

    public void Initialize(StageState state)
    {
        SetState(state);
    }

    public void SetState(StageState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // 重設所有視覺
        lockedVisual?.SetActive(false);
        unlockedVisual?.SetActive(false);
        completedVisual?.SetActive(false);

        // 設定按鈕互動性
        // nodeButton.interactable = currentState != StageState.Locked;

        // 根據狀態顯示對應視覺
        switch (currentState)
        {
            case StageState.Locked:
                lockedVisual?.SetActive(true);
                break;
            case StageState.Unlocked:
                unlockedVisual?.SetActive(true);
                break;
            case StageState.Completed:
                completedVisual?.SetActive(true);
                break;
        }
    }

    void OnNodeClick()
    {
        //測試用
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, enemyId);
        if (currentState == StageState.Unlocked)
        {
            //  StageMapManager.Instance.SelectStage(this);
        }
        else if (currentState == StageState.Completed)
        {
            // 已完成的關卡可以重複挑戰
            // StageMapManager.Instance.SelectStage(this);
        }
    }

    public void CompleteStage()
    {
        SetState(StageState.Completed);

        // 儲存進度
        //StageMapManager.Instance.SaveProgress();
    }



    public StageState GetState() => currentState;
}