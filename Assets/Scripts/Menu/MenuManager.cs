using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    //測試用UI元件
    [SerializeField] Button btn_startGame;
    [SerializeField] TMP_InputField testMap;
    [SerializeField] TMP_InputField testStage;
    [SerializeField] TMP_InputField blood;
    [SerializeField] TMP_InputField freeze;
    [SerializeField] TMP_InputField maxDiceCount;
    [SerializeField] TMP_InputField burndiceCount;
    [SerializeField] TMP_InputField rollDiceCount;
    void Start()
    {
        btn_startGame.onClick.AddListener(OnStartGameClicked);
        //TEST
        BuffDatabase.LoadFromCSV();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnStartGameClicked()
    {
        //Todo 加入載入存檔
        GameDataManager.CurrentMap = int.Parse(testMap.text);
        GameDataManager.CurrentStage = testStage.text;
        GameDataManager.PlayerData.currentBlood = int.Parse(blood.text);
        GameDataManager.PlayerData.maxBlood = int.Parse(blood.text);
        GameDataManager.PlayerData.keepDiceCount = int.Parse(freeze.text);
        GameDataManager.PlayerData.manaRollerMaxDiceCount = int.Parse(maxDiceCount.text);
        GameDataManager.PlayerData.diceCount = int.Parse(burndiceCount.text);
        GameDataManager.PlayerData.maxRollCount = int.Parse(rollDiceCount.text);
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
        Debug.Log("Start Game Clicked");
        // 在這裡添加開始遊戲的邏輯
    }
}
