using UnityEngine;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }
    public static CharacterSaveData playerData;

    public enum GameState
    {
        MainMenu,
        AVG,
        DiceGame,
        Map,
        PreparationRoom,
        H_EVENT
    }

    [Header("當前遊戲狀態")]
    public GameState currentState = GameState.MainMenu;
    
    //todo 轉場載入畫面黑幕
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
      //  EventCenter.AddListener(GameEvent.EVENT_STAGE_COMPLETE, OnStageComplete);
       // EventCenter.AddListener(GameEvent.EVENT_PLAYER_DIED, OnPlayerDied);
     //   EventCenter.AddListener(GameEvent.EVENT_ENEMY_DIED, OnEnemyDied);
        // 添加其他事件監聽...
    }
    void RemoveEventListeners()
    {
        EventCenter.RemoveListener(StateEvent.EVENT_ENTER_DICEGAME, EnterDiceGame);
        EventCenter.RemoveListener(StateEvent.EVENT_ENTER_AVG, OnEnterAVG);
      //  EventCenter.RemoveListener(GameEvent.EVENT_STAGE_COMPLETE, OnStageComplete);
      //  EventCenter.RemoveListener(GameEvent.EVENT_PLAYER_DIED, OnPlayerDied);
      //  EventCenter.RemoveListener(GameEvent.EVENT_ENEMY_DIED, OnEnemyDied);
    }
    void EnterDiceGame(object[] args)
    {
        int enemyId = (int)args[0];
        LevelData.EnemyData = EnemyFactory.CreateEnemy(enemyId);
        LevelData.enemyConfig = EnemyFactory.GetEnemyConfig(enemyId);

        //TODO: 設定玩家資料
        LevelData.PlayerData = new PlayerData();
        //進入關卡場景 unity讀取scene
        SceneLoader.LoadScene("DiceGame");
        ChangeState(GameState.DiceGame);
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
    void OnStageComplete(object[] args)
    {
        Debug.Log("StateManager: 接收到關卡完成事件");
        ChangeState(GameState.Map);
    }
    
    void OnPlayerDied(object[] args)
    {
        Debug.Log("StateManager: 接收到玩家死亡事件");
        ChangeState(GameState.MainMenu);
    }
    
    void OnEnemyDied(object[] args)
    {
        Debug.Log("StateManager: 接收到敵人死亡事件");
        // 可能切換到戰利品界面或直接完成關卡
        // 這裡可以根據具體需求決定是否切換狀態
    }
    
    void OnEnterAVG(object[] args)
    {
        string _chapter = (string)args[0];
        AvgData.chapter = _chapter;
        SceneLoader.LoadScene("AVGScene");
        Debug.Log("StateManager: 進入 AVG 模式");
        ChangeState(GameState.AVG);
    }
    
    // 靜態便利方法
    public static void ChangeToState(GameState newState)
    {
        Instance?.ChangeState(newState);
    }
    
    public static GameState CurrentState => Instance?.currentState ?? GameState.MainMenu;
}