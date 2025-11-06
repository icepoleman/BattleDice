using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] Button btn_back;
    void Start()
    {
        btn_back.onClick.AddListener(OnBackButtonClick);
    }
    void OnBackButtonClick()
    {
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
