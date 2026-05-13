using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button btn_newGame;
    [SerializeField] Button btn_continue;
    [SerializeField] Button btn_setting;
    [SerializeField] Button btn_creator;
    [SerializeField] Button btn_exitGame;

    [SerializeField] Button btn_test;
    [SerializeField] Text text_TestMap;
    void Awake()
    {
        AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "SaveLoadPanel" + ".prefab");
        AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "SetPanel" + ".prefab");
    }
    async void Start()
    {
        btn_newGame.onClick.AddListener(OnNewGameClicked);
        btn_exitGame.onClick.AddListener(() => { Application.Quit(); });
        btn_continue.onClick.AddListener(async () =>
        {
            GameObject loadPanel = await UIManager.ShowCommonPanel("SaveLoadPanel");
            loadPanel.GetComponent<SaveLoadPanel>().SetUp(SaveLoadPanel.PanelType.Load);
            AudioManager.Instance.PlaySFX("Sound_Click1");
        });
        btn_setting.onClick.AddListener(async () =>
        {
            await UIManager.ShowCommonPanel("SetPanel");
        });

        btn_continue.interactable = SaveManager.HasAutoSave();
        PlayButtonsEnterAnimation();

        //TEST
        BuffDatabase.LoadFromCSV();
        btn_test.onClick.AddListener(OnTestClicked);

        AudioManager.Instance.PlayBGM("Bgm_Menu");

        // Assets/DiceGame_ab/Music/BGM/
    }

    void PlayButtonsEnterAnimation()
    {
        const float targetLocalX = 0f;
        const float moveDuration = 0.35f;
        const float enterInterval = 0.1f;

        // Keep the current declaration order; btn_exitGame remains the last one.
        List<Button> orderedButtons = new List<Button>
        {
            btn_newGame,
            btn_continue,
            btn_setting,
            btn_creator,
            btn_exitGame
        };

        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < orderedButtons.Count; i++)
        {
            Button button = orderedButtons[i];
            if (button == null)
            {
                continue;
            }

            sequence.Insert(i * enterInterval, button.transform.DOLocalMoveX(targetLocalX, moveDuration).SetEase(Ease.OutCubic));
        }
    }

    void OnTestClicked()
    {
        StateManager.Instance.OpenTestMode();
        //測試用
        GameDataManager.PreparationRoomStage = "0";//初始整備室
        GameDataManager.CurrentMap = int.Parse(text_TestMap.text);
        GameDataManager.CurrentStage = "0";
        GameDataManager.PlayerData.currentBlood = 100;
        GameDataManager.PlayerData.maxBlood = 100;
        GameDataManager.Gold = 10000;
        GameDataManager.Gear = 1000;
        GameDataManager.PlayerData.skillIDs = new List<int>() { 1, 2 }; //預設技能
        GameDataManager.HasSkillIDs.UnionWith(new HashSet<int> { 1, 2 }); //預設擁有技能ID
        /*for (int i = 3; i <= 45; i++)
        {
            GameDataManager.HasSkillIDs.Add(i);
        }*/

        GameDataManager.PlayerData.diceCount = 6;
        GameDataManager.PlayerData.maxRollCount = 5;
        GameDataManager.PlayerData.keepDiceCount = 4;
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
        GameDataManager.HasSkillIDs.UnionWith(new HashSet<int> { 1, 2 }); //預設擁有技能ID
        GameDataManager.PlayerData.diceCount = 3;
        GameDataManager.PlayerData.maxRollCount = 1;
        GameDataManager.charactersAffinity = new int[] { 0, 0, 0, 0 };//角色親密度歸零
        GameDataManager.SocialPoint = 0;//社交點數歸零
        GameDataManager.unlockedAffinityStages.Clear();//解鎖的親密度關卡
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "Prologue1_1");
        Debug.Log("Start Game Clicked");
    }
}
