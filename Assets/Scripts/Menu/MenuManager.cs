using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button btn_startGame;
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
        int mapID = GameDataManager.CurrentMap = 1;
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, mapID);
        Debug.Log("Start Game Clicked");
        // 在這裡添加開始遊戲的邏輯
    }
}
