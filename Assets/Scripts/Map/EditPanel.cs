using UnityEngine;
using UnityEngine.UI;

public class EditPanel : MonoBehaviour
{
    [SerializeField] Button btn_saveGame;
    [SerializeField] Button btn_loadGame;
    [SerializeField] Button btn_set;
    [SerializeField] Button btn_backToMenu;
    [SerializeField] Button btn_close;
    [SerializeField] Button btn_black_close;
    void Start()
    {

        btn_saveGame.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LanguageManager.GetText("T_Setting_Save");
        btn_loadGame.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LanguageManager.GetText("T_Setting_Load");
        btn_set.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LanguageManager.GetText("T_Setting_Set");
        btn_backToMenu.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LanguageManager.GetText("T_Setting_backMenu");

        btn_backToMenu.onClick.AddListener(() =>
        {
            Destroy(gameObject);
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
        });
        btn_saveGame.onClick.AddListener(async () =>
        {
            GameObject savePanel = await UIManager.ShowCommonPanel("SaveLoadPanel");
            savePanel.GetComponent<SaveLoadPanel>().SetUp(SaveLoadPanel.PanelType.Save);
            Destroy(gameObject);
        });
        btn_loadGame.onClick.AddListener(async () =>
        {
            GameObject loadPanel = await UIManager.ShowCommonPanel("SaveLoadPanel");
            loadPanel.GetComponent<SaveLoadPanel>().SetUp(SaveLoadPanel.PanelType.Load);
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
        btn_black_close.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }
}
