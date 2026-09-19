using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestAdvLoader : MonoBehaviour
{
    public static string testAdvName = "test666";
    [SerializeField] Button btn_loadAdv;
    [SerializeField] TMP_InputField inputField_advName;
    [SerializeField] TMP_Dropdown dropdown_advList;

    private void Start()
    {
        GameDataManager.TestAVGMode = true;
        RefreshAdvList();

        dropdown_advList.onValueChanged.AddListener(index =>
        {
            if (dropdown_advList.options.Count == 0)
                return;

            string selectedAdvName = dropdown_advList.options[index].text;
            inputField_advName.text = selectedAdvName;
        });

        btn_loadAdv.onClick.AddListener(() =>
        {
            string advName = dropdown_advList.options.Count > 0
                ? dropdown_advList.options[dropdown_advList.value].text
                : inputField_advName.text;

            if (string.IsNullOrWhiteSpace(advName))
            {
                advName = inputField_advName.text;
            }

            testAdvName = advName;
            GameDataManager.TmpAvgChapter = advName;
            inputField_advName.text = advName;
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, advName);
        });

        SceneLoader.HideLoadingScreen();
    }

    private void RefreshAdvList()
    {
        List<string> advNames = CSVReader.Instance.GetAllDialogueCSVNames();
        dropdown_advList.ClearOptions();

        if (advNames == null || advNames.Count == 0)
        {
            inputField_advName.text = "";
            return;
        }

        dropdown_advList.AddOptions(advNames.ConvertAll(name => new TMP_Dropdown.OptionData(name)));
        dropdown_advList.value = 0;
        dropdown_advList.RefreshShownValue();
        inputField_advName.text = advNames[0];
    }

    bool isFullScreen = false;
    public void Use2kResolution()
    {
        isFullScreen = !isFullScreen;
        if (isFullScreen)
            Screen.SetResolution(2560, 1440, FullScreenMode.FullScreenWindow);
        else
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
    }
}
