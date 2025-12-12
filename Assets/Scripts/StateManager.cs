using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }
    public static CharacterSaveData playerData;

    [SerializeField]Image fadeImage;

    public enum GameState
    {
        MainMenu,
        AVG,
        DiceGame,
        Map,
        PreparationRoom,
        H_EVENT,
        Shop
    }

    [Header("當前遊戲狀態")]
    public GameState currentState = GameState.MainMenu;
    [SerializeField] bool testMode;
    //todo 轉場載入畫面黑幕
    
    void Awake()
    {
        GameDataManager.TestMode = testMode;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 設定 SceneLoader 的協程執行者
            SceneLoader.SetCoroutineRunner(this);
            
            AddEventListeners();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        RemoveEventListeners();
    }
    
    // 註冊事件監聽
    void AddEventListeners()
    {
        EventCenter.AddListener(StateEvent.EVENT_ENTER_DICEGAME, EnterDiceGame);
        EventCenter.AddListener(StateEvent.EVENT_ENTER_AVG, OnEnterAVG);
        EventCenter.AddListener(StateEvent.EVENT_ENTER_MAP, OnEnterMap);
        EventCenter.AddListener(StateEvent.EVENT_ENTER_PREPARATION_ROOM, OnEnterPreparationRoom);
        EventCenter.AddListener(StateEvent.EVENT_ENTER_SHOP, OnEnterShop);
        EventCenter.AddListener(StateEvent.EVENT_LOADING_SCREEN, OnLoadingScreen);
        
        // 添加其他事件監聽...
    }
    void RemoveEventListeners()
    {
        EventCenter.RemoveListener(StateEvent.EVENT_ENTER_DICEGAME, EnterDiceGame);
        EventCenter.RemoveListener(StateEvent.EVENT_ENTER_AVG, OnEnterAVG);
        EventCenter.RemoveListener(StateEvent.EVENT_ENTER_MAP, OnEnterMap);
        EventCenter.RemoveListener(StateEvent.EVENT_ENTER_PREPARATION_ROOM, OnEnterPreparationRoom);
        EventCenter.RemoveListener(StateEvent.EVENT_ENTER_SHOP, OnEnterShop);
        EventCenter.RemoveListener(StateEvent.EVENT_LOADING_SCREEN, OnLoadingScreen);
    }
    // 切換遊戲狀態
    void ChangeState(GameState newState)
    {
        if (currentState != newState)
        {
            GameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"遊戲狀態切換: {previousState} → {currentState}");
            OnStateChanged(previousState, currentState);
        }
    }
    
    void OnStateChanged(GameState from, GameState to)
    {
        // 只處理狀態切換的基本邏輯
        switch (to)
        {
            case GameState.MainMenu:
                Debug.Log("進入主選單狀態");
                break;
            case GameState.AVG:
                Debug.Log("進入 AVG 狀態");
                break;
            case GameState.DiceGame:
                Debug.Log("進入骰子遊戲狀態");
                break;
            case GameState.Map:
                Debug.Log("進入地圖狀態");
                break;
            case GameState.PreparationRoom:
                Debug.Log("進入準備房間狀態");
                break;
            case GameState.H_EVENT:
                Debug.Log("進入 H 事件狀態");
                break;
        }
        
        // 廣播狀態切換事件，讓其他系統響應
       // EventCenter.Dispatch(GameEvent.EVENT_GAME_STATE_CHANGED, from, to);
    }
    
    // 事件響應方法 - 只處理狀態切換邏輯
    void EnterDiceGame(object[] args)
    {
        //args不是int抱錯
        if (args.Length == 0 || !(args[0] is int))
        {
            Debug.LogError("EnterDiceGame: 無效的參數，預期為敵人ID的整數值");
            return;
        }
        int enemyId = (int)args[0];
        Debug.Log("StateManager: 進入 骰子遊戲 模式 Enemy" + enemyId);
        GameDataManager.TmpEnemyData = EnemyFactory.CreateEnemy(enemyId);
 
        //TODO: 設定玩家資料
       // GameDataManager.PlayerData = new PlayerData();
        
        // 使用帶延遲的場景載入
        SceneLoader.LoadSceneWithDelay("DiceGame", () => ChangeState(GameState.DiceGame));
    }
    void OnEnterAVG(object[] args)
    {
        string _chapter = (string)args[0];
        GameDataManager.TmpAvgChapter = _chapter;
        Debug.Log("StateManager: 進入 AVG 模式");
        
        // 使用帶延遲的場景載入
        SceneLoader.LoadSceneWithDelay("AVGScene", () => ChangeState(GameState.AVG));
    }
    void OnEnterMap(object[] args)
    {
        Debug.Log("StateManager: 進入地圖模式");
        
        // 使用帶延遲的場景載入
        SceneLoader.LoadSceneWithDelay("StageMap", () => ChangeState(GameState.Map));
    }
    void OnEnterPreparationRoom(object[] args)
    {
        Debug.Log("StateManager: 進入準備室模式");
        
        // 使用帶延遲的場景載入
        SceneLoader.LoadSceneWithDelay("PreparationRoom", () => ChangeState(GameState.PreparationRoom));
    }
    void OnEnterShop(object[] args)
    {
        Debug.Log("StateManager: 進入商店模式");
        
        // 使用帶延遲的場景載入
        SceneLoader.LoadSceneWithDelay("ShopScene", () => ChangeState(GameState.Shop));
    }
    void OnLoadingScreen(object[] args)
    {
        bool showLoading = (bool)args[0];
        if (showLoading)
        {
            //顯示黑幕
            fadeImage.gameObject.SetActive(true);
            fadeImage.CrossFadeAlpha(1f, 0.5f, false);
        }
        else
        {
            //隱藏黑幕
            fadeImage.CrossFadeAlpha(0f, 0.5f, false);
            Invoke("DisableFadeImage", 0.5f);
        }
    }
    void DisableFadeImage()
    {
        fadeImage.gameObject.SetActive(false);
    }
    
    // 靜態便利方法
    public static void ChangeToState(GameState newState)
    {
        Instance?.ChangeState(newState);
    }
    
    public static GameState CurrentState => Instance?.currentState ?? GameState.MainMenu;
}