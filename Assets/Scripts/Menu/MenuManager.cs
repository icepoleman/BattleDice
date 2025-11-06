using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button btn_startGame;
    [SerializeField] TMP_InputField testMap;
    [SerializeField] TMP_InputField testStage;
    void Start()
    {
        btn_startGame.onClick.AddListener(OnStartGameClicked);
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
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP);
        Debug.Log("Start Game Clicked");
        // 在這裡添加開始遊戲的邏輯
    }
}
