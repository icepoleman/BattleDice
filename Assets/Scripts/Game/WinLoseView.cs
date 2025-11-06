using UnityEngine;
using UnityEngine.UI;

public class WinLoseView : MonoBehaviour
{
    [SerializeField] GameObject obj_win;
    [SerializeField] GameObject obj_lose;
    [SerializeField] Button btn_check;

    public void PlayWinAnimation(bool isWin , System.Action onComplete = null)
    {
        obj_win.SetActive(isWin);
        obj_lose.SetActive(!isWin);

        btn_check.onClick.AddListener(() =>
        {
            onComplete?.Invoke();
        });
    }
}
