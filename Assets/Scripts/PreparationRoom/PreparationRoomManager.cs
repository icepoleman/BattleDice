using UnityEngine;
using UnityEngine.UI;

public class PreparationRoomManager : MonoBehaviour
{    
    [SerializeField] Button btn_back;
    void Start()
    {
        btn_back.onClick.AddListener(OnBackButtonClick);
    }
    void OnBackButtonClick()
    {
        GameDataManager.PlayerData.currentBlood = GameDataManager.PlayerData.maxBlood;
        SaveManager.AutoSave();
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
