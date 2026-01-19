using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button btn_newGame;
    [SerializeField] Button btn_exitGame;
    [SerializeField] Button btn_continue;

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
        GameDataManager.CurrentMap = 0;
        GameDataManager.CurrentStage = "0";
        GameDataManager.PlayerData.currentBlood = 100;
        GameDataManager.PlayerData.maxBlood = 100;
        GameDataManager.Gold = 0;
        GameDataManager.Gear = 0;
        GameDataManager.PlayerData.keepDiceCount = 1;
        GameDataManager.PlayerData.skillIDs = new List<int>() { 1, 2 }; //預設技能
        GameDataManager.HasSkillIDs = new List<int>() { 1, 2 }; //預設擁有技能ID
        GameDataManager.PlayerData.diceCount = 3;
        GameDataManager.PlayerData.maxRollCount = 1;
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "Prologue1_1");
        Debug.Log("Start Game Clicked");
    }
}
