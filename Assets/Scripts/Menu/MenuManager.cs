using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button btn_newGame;
    [SerializeField] Button btn_exitGame;
    [SerializeField] Button btn_continue;
    [SerializeField] Button btn_setting;
    [SerializeField] Button btn_test;
    [SerializeField] Text text_TestMap;

    async void Start()
    {
        btn_newGame.onClick.AddListener(OnNewGameClicked);
        btn_exitGame.onClick.AddListener(() => { Application.Quit(); });
        btn_continue.onClick.AddListener(async () =>
        {
            await UIManager.ShowCommonPanel("LoadPanel");
            AudioManager.Instance.PlaySFX("Sound_Click1");
        });
        btn_setting.onClick.AddListener(async () =>
        {
            await UIManager.ShowCommonPanel("SetPanel");
        });

        btn_continue.interactable = SaveManager.HasAutoSave();
        //TEST
        BuffDatabase.LoadFromCSV();
        btn_test.onClick.AddListener(OnTestClicked);

        AudioManager.Instance.PlayBGM("Bgm_Menu");

       // Assets/DiceGame_ab/Music/BGM/
    }
    void OnTestClicked()
    {
        GameDataManager.TestMode = true;
        //測試用
        GameDataManager.PreparationRoomStage = "0";//初始整備室
        GameDataManager.CurrentMap = int.Parse(text_TestMap.text);
        GameDataManager.CurrentStage = "0";
        GameDataManager.PlayerData.currentBlood = 100;
        GameDataManager.PlayerData.maxBlood = 100;
        GameDataManager.Gold = 10000;
        GameDataManager.Gear = 1000;
        GameDataManager.PlayerData.keepDiceCount = 1;
        GameDataManager.PlayerData.skillIDs = new List<int>() { 1, 2 }; //預設技能
        GameDataManager.HasSkillIDs = new List<int>() { 1, 2 }; //預設擁有技能ID
        for (int i = 3; i <= 25; i++)
        {
            GameDataManager.HasSkillIDs.Add(i);
        }

        GameDataManager.PlayerData.diceCount = 3;
        GameDataManager.PlayerData.maxRollCount = 1;
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
    }

    void OnNewGameClicked()
    {
        //Todo 加入載入存檔
        GameDataManager.PreparationRoomStage = "0";//初始整備室
        GameDataManager.CurrentMap = 1;
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
