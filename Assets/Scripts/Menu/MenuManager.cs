using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button btn_newGame;
    [SerializeField] Button btn_exitGame;
    [SerializeField] Button btn_continue;
    //測試用UI元件
    [SerializeField] TMP_InputField testMap;
    [SerializeField] TMP_InputField testStage;
    [SerializeField] TMP_InputField blood;
    [SerializeField] TMP_InputField freeze;
    [SerializeField] TMP_InputField maxDiceCount;
    [SerializeField] TMP_InputField burndiceCount;
    [SerializeField] TMP_InputField rollDiceCount;
    void Start()
    {
        btn_newGame.onClick.AddListener(OnNewGameClicked);
        btn_exitGame.onClick.AddListener(() => { Application.Quit(); });
        btn_continue.onClick.AddListener(() => { OnContinueClicked(); });

        btn_continue.interactable = SaveManager.HasAutoSave();
        //TEST
        BuffDatabase.LoadFromCSV();

        
    }

    //載入快速存檔
    void OnContinueClicked()
    {
        SaveManager.LoadAutoSave();//載入自動存檔
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
        Debug.Log("Continue Clicked");
        // 在這裡添加繼續遊戲的邏輯
    }

    void OnNewGameClicked()
    {
        //Todo 加入載入存檔
        GameDataManager.PreparationRoomStage = "0";//初始整備室
        GameDataManager.CurrentMap = 1;
        GameDataManager.CurrentStage = testStage.text;
        GameDataManager.PlayerData.currentBlood = int.Parse(blood.text);
        GameDataManager.PlayerData.maxBlood = int.Parse(blood.text);
        GameDataManager.PlayerData.keepDiceCount = int.Parse(freeze.text);
        GameDataManager.PlayerData.manaRollerMaxDiceCount = int.Parse(maxDiceCount.text);
        GameDataManager.PlayerData.diceCount = int.Parse(burndiceCount.text);
        GameDataManager.PlayerData.maxRollCount = int.Parse(rollDiceCount.text);
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "Prologue1_1");
        Debug.Log("Start Game Clicked");
    }
}
