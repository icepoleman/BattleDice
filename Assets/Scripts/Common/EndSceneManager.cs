using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndSceneManager : MonoBehaviour
{
    [SerializeField] GameObject flyingTextPrefab; // 飛出文字的預製件
    [SerializeField] Button btn_returnMainMenu; // 返回主選單按鈕
    
    [Header("生成設定")]
    [SerializeField] float spawnIntervalMin = 0.5f;  // 最小生成間隔
    [SerializeField] float spawnIntervalMax = 2f;    // 最大生成間隔
    [SerializeField] float spawnYMin = -1000f;        // Y軸最小值
    [SerializeField] float spawnYMax = -80f;         // Y軸最大值
    [SerializeField] float spawnX = -1000f;          // 生成X位置（左側）
    
    [Header("飛行設定")]
    [SerializeField] float speedMin = 100f;          // 最小速度
    [SerializeField] float speedMax = 300f;          // 最大速度
    [SerializeField] float scaleMin = 0.5f;          // 最小縮放
    [SerializeField] float scaleMax = 1.5f;          // 最大縮放
    [SerializeField] float lifetime = 10f;           // 存活時間
    
    string[] endMessages = new string[]
    {
        // 繁體中文
        "感謝遊玩",
        "期待下次見~",
        "你真棒",
        "希望你喜歡這款遊戲",
        "恭喜!!",
        "再見啦~",
        "辛苦了！",
        "下次再來玩喔",
        
        // 英文
        "Thank you for playing!",
        "See you next time~",
        "You're awesome!",
        "Hope you enjoyed it!",
        "Congratulations!!",
        "GG!",
        "Well played!",
        "Take care~",
        
        // 日文
        "プレイありがとう！",
        "また会おうね～",
        "すごい！",
        "お疲れ様でした！",
        "おめでとう!!",
        "またね～",
        "楽しんでくれたかな？",
        
        // 簡體中文
        "感谢游玩",
        "期待下次见~",
        "你真棒",
        "希望你喜欢这款游戏",
        "恭喜!!",
        "下次再来玩哦",
        "辛苦了！",
        
        // 韓文
        "플레이해 주셔서 감사합니다!",
        "다음에 또 만나요~",
        "대단해요!",
        "즐거우셨나요?",
        "축하합니다!!",
        "안녕~",
        "수고하셨습니다!",
        
        // 德文
        "Danke fürs Spielen!",
        "Bis zum nächsten Mal~",
        "Du bist toll!",
        "Hoffentlich hat es dir gefallen!",
        "Herzlichen Glückwunsch!!",
        "Tschüss~",
        "Gut gemacht!",
    };
    
    private float nextSpawnTime;
    
    void Start()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
        AudioManager.Instance.PlayBGM("Bgm_End");
        btn_returnMainMenu.onClick.AddListener(() =>
        {
            // 返回主選單
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
        });
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnFlyingText();
            nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }
    
    void SpawnFlyingText()
    {
        if (flyingTextPrefab == null) return;
        
        GameObject obj = Instantiate(flyingTextPrefab, transform);
        RectTransform rect = obj.GetComponent<RectTransform>();
        
        // 設定位置
        float y = Random.Range(spawnYMin, spawnYMax);
        rect.anchoredPosition = new Vector2(spawnX, y);
        
        // 設定大小
        float scale = Random.Range(scaleMin, scaleMax);
        rect.localScale = Vector3.one * scale;
        
        // 設定文字
        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = endMessages[Random.Range(0, endMessages.Length)];
        }
        
        // 添加飛行組件
        FlyingText flying = obj.GetComponent<FlyingText>();
        if (flying == null)
        {
            flying = obj.AddComponent<FlyingText>();
        }
        flying.Setup(Random.Range(speedMin, speedMax), lifetime);
    }
}
