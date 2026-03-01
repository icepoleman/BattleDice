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
            Destroy(gameObject);
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
        });
        btn_saveGame.onClick.AddListener(async () =>
        {
            await UIManager.ShowCommonPanel("SavePanel");
            Destroy(gameObject);
        });
        btn_set.onClick.AddListener(async () =>
        {
            await UIManager.ShowCommonPanel("SetPanel");
            Destroy(gameObject);
        });
        btn_close.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }
}
