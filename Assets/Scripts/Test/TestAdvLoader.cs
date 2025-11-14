using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestAdvLoader : MonoBehaviour
{
    [SerializeField]Button btn_loadAdv;
    [SerializeField]TMP_InputField inputField_advName;
    void Start()
    {
        btn_loadAdv.onClick.AddListener(()=>
        {
            string advName = inputField_advName.text;
            GameDataManager.TmpAvgChapter = advName;
            //切換scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("AvgScene");
        });
    }
}
