using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AutoLocalization : MonoBehaviour
{
    private TextMeshProUGUI tmpText;
    [SerializeField] string localizationKey;
    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = LanguageManager.GetText(localizationKey);
        }
    }
}
