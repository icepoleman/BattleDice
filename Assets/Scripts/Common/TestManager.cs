using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class TestManager : MonoBehaviour
{
    [SerializeField] GameObject testPanel;
    [SerializeField] Toggle tog_testMode; // 是否啟用測試模式
    [SerializeField] TMP_InputField inputField_map;
    [SerializeField] TMP_InputField inputField_Money;
    [SerializeField] TMP_InputField inputField_Gear;
    [SerializeField] TMP_InputField inputField_Blood;

    [SerializeField] Button[] btn_H_eventList;
    [SerializeField] Button btn_check;
    [SerializeField] Button btn_close;
    [SerializeField] float f1HoldDuration = 3f;
    [SerializeField] Button btn_hRoom;

    private float f1HoldTimer;
    private bool f1HoldTriggered;
    private string[] hEventNames = new string[]
        {
        "H_Witch_1",
        "H_Wolfgirl_1",
        };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputField_map.text = GameDataManager.CurrentMap.ToString();
        inputField_Money.text = GameDataManager.Gold.ToString();
        inputField_Gear.text = GameDataManager.Gear.ToString();
        inputField_Blood.text = 100.ToString();

        btn_check.onClick.AddListener(OnCheckButtonClicked);

        for (int i = 0; i < btn_H_eventList.Length; i++)
        {
            int index = i; // 捕获当前索引
            btn_H_eventList[i].onClick.AddListener(() => EnterHEvent(index));
        }
        tog_testMode.onValueChanged.AddListener((isOn) =>
        {
            GameDataManager.TestMode = isOn;
            EventCenter.Dispatch(MapEvent.EVENT_OPEN_ALL_STAGE_NODE);
            Debug.Log($"測試模式已設置為: {isOn}");
        });
        GameDataManager.TestMode = tog_testMode.isOn;
        btn_close.onClick.AddListener(() =>
        {
            testPanel.SetActive(false);
        });
        btn_hRoom.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_H_ROOM);
        });
        //GameDataManager.TmpEnemyID = testEnemyId;
        //GameDataManager.TestMode

    }
    private void OnCheckButtonClicked()
    {
        if (tog_testMode.isOn)
        {
            for (int i = 1; i <= 34; i++)
            {
                GameDataManager.HasSkillIDs.Add(i);
            }
        }
        // 讀取輸入框的值並更新 GameDataManager
        if (int.TryParse(inputField_map.text, out int mapValue))
        {
            GameDataManager.CurrentMap = mapValue;
            Debug.Log($"已更新地圖索引為: {mapValue}");
        }

        if (int.TryParse(inputField_Money.text, out int moneyValue))
        {
            GameDataManager.Gold = moneyValue;
            Debug.Log($"已更新金幣數量為: {moneyValue}");
        }

        if (int.TryParse(inputField_Gear.text, out int gearValue))
        {
            GameDataManager.Gear = gearValue;
            Debug.Log($"已更新齒輪數量為: {gearValue}");
        }
        if (int.TryParse(inputField_Blood.text, out int bloodValue))
        {
            GameDataManager.PlayerData.maxBlood = bloodValue;
            GameDataManager.PlayerData.currentBlood = bloodValue;
            Debug.Log($"已更新最大血量為: {bloodValue}");
        }
        GameDataManager.PlayerData.skillIDs = new List<int>() { 1, 2 }; //預設技能
        GameDataManager.PlayerData.diceCount = 6;
        GameDataManager.PlayerData.maxRollCount = 5;
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
    }
    private void EnterHEvent(int index)
    {
        if (index < 0 || index >= hEventNames.Length)
        {
            Debug.LogError($"無效的事件索引: {index}");
            return;
        }
        string eventName = hEventNames[index];
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, eventName);
    }

    // Update is called once per frame
    void Update()
    {
        // 長按 F1 達指定秒數後顯示測試面板
        if (Input.GetKeyDown(KeyCode.F1))
        {
            testPanel.SetActive(true);
        }
    }
}
