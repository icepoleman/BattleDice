using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private const string RESOLUTION_KEY = "Resolution";
    private const string SCREEN_MODE_KEY = "ScreenMode";
    private const string TEXT_SPEED_KEY = "TextSpeed";

    private readonly (int width, int height)[] resolutions16_9 = new[]
    {
        (1280, 720),
        (1366, 768),
        (1600, 900),
        (1920, 1080),
        (2560, 1440)
    };

    private readonly float[] textSpeedValues = { 0.1f, 0.05f, 0.01f };

    [SerializeField] Button btn_newGame;
    [SerializeField] Button btn_continue;
    [SerializeField] Button btn_setting;
    [SerializeField] Button btn_creator;
    [SerializeField] Button btn_exitGame;

    [SerializeField] Button btn_test;
    [Header("開發人員名單")]
    [SerializeField] creatorPanelPopup creatorPanelPopup;
    [Header("表演用")]
    [SerializeField] Animator anim_menu;
    void Awake()
    {
        LoadSavedSettingsIfHasSave();
        AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "SaveLoadPanel" + ".prefab");
        AddressableManager.PreloadAssetAsync<GameObject>(ABconfig.COMMON_PREFABS + "SetPanel" + ".prefab");
        AtlasLoader.Instance.Init();
    }
    private void LoadSavedSettingsIfHasSave()
    {
        if (GameDataManager.isFirstOpen)
        {
            GameDataManager.isFirstOpen = false;
            return; // 已經是第一次開啟遊戲，不需要再次調整解析度
        }
        bool hasSave = SaveManager.HasAnySave();

        if (!hasSave)
        {
            EnsureWindowedIfNoResolutionSave();
            return;
        }

        ApplySavedScreenMode();
        ApplySavedResolution();
        ApplySavedTextSpeed();

        // AudioManager 初始化時會自行載入音量/靜音設定，這裡只確保初始化已完成。
        _ = AudioManager.Instance;
    }

    private void EnsureWindowedIfNoResolutionSave()
    {
        if (!PlayerPrefs.HasKey(RESOLUTION_KEY) && !PlayerPrefs.HasKey(SCREEN_MODE_KEY))
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.Save();
        }
    }

    private void ApplySavedScreenMode()
    {
        if (!PlayerPrefs.HasKey(SCREEN_MODE_KEY))
        {
            return;
        }

        int savedMode = Mathf.Clamp(PlayerPrefs.GetInt(SCREEN_MODE_KEY, 2), 0, 2);
        switch (savedMode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }

    private void ApplySavedResolution()
    {
        int savedIndex = PlayerPrefs.GetInt(RESOLUTION_KEY, -1);
        if (savedIndex < 0 || savedIndex >= resolutions16_9.Length)
        {
            return;
        }

        var res = resolutions16_9[savedIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    private void ApplySavedTextSpeed()
    {
        int savedIndex = Mathf.Clamp(PlayerPrefs.GetInt(TEXT_SPEED_KEY, 1), 0, textSpeedValues.Length - 1);
        ChatWindow.TypingSpeed = textSpeedValues[savedIndex];
    }

    private void Start()
    {
        btn_newGame.onClick.AddListener(OnNewGameClicked);
        btn_exitGame.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("Sound_Click1");
            Application.Quit();
        });
        btn_continue.onClick.AddListener(async () =>
        {
            GameObject loadPanel = await UIManager.ShowCommonPanel("SaveLoadPanel");
            loadPanel.GetComponent<SaveLoadPanel>().SetUp(SaveLoadPanel.PanelType.Load);
            AudioManager.Instance.PlaySFX("Sound_Click1");
        });
        btn_setting.onClick.AddListener(async () =>
        {
            AudioManager.Instance.PlaySFX("Sound_Click1");
            await UIManager.ShowCommonPanel("SetPanel");
        });
        btn_creator.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("Sound_Click1");
            creatorPanelPopup.OpenPanel();
        });

        btn_continue.interactable = SaveManager.HasAnySave();

        // TEST
        BuffDatabase.LoadFromCSV();
        btn_test.onClick.AddListener(OnTestClicked);

        AudioManager.Instance.PlayBGM("Bgm_Menu");
        SceneLoader.HideLoadingScreen();
    }

    void OnTestClicked()
    {
        //測試用
        GameDataManager.PreparationRoomStage = "0";//初始整備室
        GameDataManager.CurrentMap = 0;
        GameDataManager.CurrentStage = "0";
        GameDataManager.PlayerData.currentBlood = 100;
        GameDataManager.PlayerData.maxBlood = 100;
        GameDataManager.Gold = 10000;
        GameDataManager.Gear = 1000;
        GameDataManager.PlayerData.skillIDs = new List<int>() { 1, 2 }; //預設技能
        GameDataManager.HasSkillIDs.UnionWith(new HashSet<int> { 1, 2 }); //預設擁有技能ID
        for (int i = 3; i <= 45; i++)
        {
            GameDataManager.HasSkillIDs.Add(i);
        }

        GameDataManager.PlayerData.diceCount = 6;
        GameDataManager.PlayerData.maxRollCount = 5;
        GameDataManager.PlayerData.keepDiceCount = 4;
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
    }

    async void OnNewGameClicked()
    {
        anim_menu.Play("fadeOut");
        AudioManager.Instance.PlaySFX("Sound_Click1");
        //等一秒
        await Task.Delay(1000);
        //Todo 加入載入存檔
        GameDataManager.PreparationRoomStage = "StartPoint";//初始整備室
        GameDataManager.CurrentMap = 1;
        GameDataManager.CurrentStage = "StartPoint";
        GameDataManager.PlayerData.currentBlood = 100;
        GameDataManager.PlayerData.maxBlood = 100;
        GameDataManager.Gold = 0;
        GameDataManager.Gear = 0;
        GameDataManager.PlayerData.keepDiceCount = 1;
        GameDataManager.PlayerData.skillIDs = new List<int>() { 1, 5 }; //預設技能
        GameDataManager.HasSkillIDs = new HashSet<int> { 1, 5 }; //預設擁有技能ID
        GameDataManager.PlayerData.diceCount = 3;
        GameDataManager.PlayerData.maxRollCount = 1;
        GameDataManager.charactersAffinity = new int[] { 0, 0, 0, 0 };//角色親密度歸零
        GameDataManager.SocialPoint = 0;//社交點數歸零
        GameDataManager.unlockedAffinityStages.Clear();//解鎖的親密度關卡
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "Prologue1_1");
        Debug.Log("Start Game Clicked");
    }
}
