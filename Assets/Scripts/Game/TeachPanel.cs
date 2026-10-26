using UnityEngine;
using UnityEngine.UI;

public class TeachPanel : MonoBehaviour
{
    [SerializeField] Button btn_close;
    [SerializeField] Button btn_block;
    void Start()
    {
        btn_close.onClick.AddListener(ClosePanel);
        btn_block.onClick.AddListener(ClosePanel);
    }

    private void ClosePanel()
    {
        Destroy(gameObject);
    }
}
