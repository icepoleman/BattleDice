using UnityEngine;
using UnityEngine.UI;

public class EditPanel : MonoBehaviour
{
    [SerializeField] Button btn_saveGame;
    [SerializeField] Button btn_set;
    [SerializeField] Button btn_backToMenu;
    [SerializeField] Button btn_close;
    void Start()
    {
        btn_backToMenu.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
        });
        btn_saveGame.onClick.AddListener(() =>
        {
            AddressableManager.LoadAndInstantiateAsync("SavePanel", transform);
        });
        btn_set.onClick.AddListener(() =>
        {
            AddressableManager.LoadAndInstantiateAsync("SetPanel", transform);
        });
        btn_close.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }
}
