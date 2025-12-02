using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestAdvLoader : MonoBehaviour
{
    public static string testAdvName = "test666";
    [SerializeField]Button btn_loadAdv;
    [SerializeField]TMP_InputField inputField_advName;
    void Start()
    {
        inputField_advName.text = testAdvName;
        btn_loadAdv.onClick.AddListener(()=>
        {
            string advName = inputField_advName.text;
            testAdvName = advName;
            GameDataManager.TmpAvgChapter = advName;
            //切換scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("AvgScene");
        });
    }
}
