using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUiView : MonoBehaviour
{
    [SerializeField] Slider slider_playerBlood;
    [SerializeField] Text text_playerBlood;
    [SerializeField] Slider slider_enemyBlood;
    [SerializeField] Text text_enemyBlood;
    [SerializeField] Transform trans_playerBuffParent;
    [SerializeField] Transform trans_enemyBuffParent;
    [SerializeField] Text text_playerDiceCount;
    [SerializeField] Text text_enemyDiceCount;
    public void UpdateDiceCount(int playerDiceCount, int enemyDiceCount)
    {
        text_playerDiceCount.text = playerDiceCount.ToString();
        text_enemyDiceCount.text = enemyDiceCount.ToString();
    }
    public void UpdateBlood(bool isPlayer, float currentBlood, float maxBlood)
    {
        if (isPlayer)
        {
            slider_playerBlood.value = currentBlood / maxBlood;
            text_playerBlood.text = $"{currentBlood}/{maxBlood}";
        }
        else
        {
            slider_enemyBlood.value = currentBlood / maxBlood;
            text_enemyBlood.text = $"{currentBlood}/{maxBlood}";
        }
    }
    public async void UpdateBuffs(bool isPlayer, IBuffData[] buffs)
    {
        Transform buffParent = isPlayer ? trans_playerBuffParent : trans_enemyBuffParent;
        ClearBuffs(buffParent);
        if (buffs == null)
        {
            return;
        }
        foreach (var buff in buffs)
        {
            GameObject buffIcon = Instantiate(AddressableManager.GetLoadedAsset<GameObject>("buffCard"));
            buffIcon.transform.SetParent(buffParent);
            buffIcon.transform.localScale = Vector3.one;
            buffIcon.transform.localPosition = Vector3.zero;
            buffIcon.GetComponent<BuffCard>().SetBuffInfo(buff);
        }
    }
    void ClearBuffs(Transform buffParent)
    {
        foreach (Transform child in buffParent)
        {
            Destroy(child.gameObject);
        }
    }
}
