using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class BuffDetailHint : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt_buffEffect;
    [SerializeField] Image img_buffIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetData(int buffID)
    {
        BuffConfigData buffConfigData = BuffDatabase.GetBuffConfig(buffID);
        txt_buffEffect.text = buffConfigData.describe;
        img_buffIcon.sprite = AtlasLoader.Instance.GetBuffSprite(buffID.ToString());
    }
}
