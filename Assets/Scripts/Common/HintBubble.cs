using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class HintBubble : MonoBehaviour
{
    [SerializeField] TMP_Text text_hint;
    public void SetUp(string _hintText)
    {
        text_hint.text = _hintText;
        Destroy(gameObject, 3f);
    }
}
