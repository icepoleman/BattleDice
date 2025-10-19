using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CharacterView : MonoBehaviour
{
    private Animator animator;
    private GameObject diceBox = null;
    //todo
    private TextMeshProUGUI txt_blood = null;
    private Slider slider_blood = null;
    //生成彈出文字
    private Sprite[] diceSprites;

    public void Init()
    {
        diceBox = gameObject.transform.Find("diceBox").gameObject;
        diceSprites = Resources.LoadAll<Sprite>("dice/dice_base");
        txt_blood = gameObject.transform.Find("txt_blood").GetComponent<TextMeshProUGUI>();
        slider_blood = gameObject.transform.Find("slider_blood").GetComponent<Slider>();
        animator = GetComponent<Animator>(); 
    }
    

    // 只負責顯示骰子動畫，不處理數值邏輯
    public IEnumerator ShowRollAnimation(List<int> rollResults, System.Action onComplete = null)
    {
        Debug.Log("顯示擲骰子動畫");
        ClearDiceBox();
        
        yield return new WaitForSeconds(0.5f);
        
        //生成骰子物件在diceBox下
        for (int i = 0; i < rollResults.Count; i++)
        {
            GameObject dice = new GameObject("dice");
            dice.transform.SetParent(diceBox.transform);
            RectTransform rt = dice.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(25, 25);
            rt.localScale = new Vector3(1, 1, 1);
            Image sr = dice.AddComponent<Image>();
            sr.sprite = diceSprites[rollResults[i] - 1];
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        ClearDiceBox();
        
        // 執行回調，讓調用者決定後續行為
        onComplete?.Invoke();
    }
    public void UpdateBlood(float currentBlood, float maxBlood)
    {
        txt_blood.text = $"{currentBlood}/{maxBlood}";
        slider_blood.value = currentBlood / maxBlood;
    }
    // 公開方法供 DiceGame 直接呼叫
    public void PlayAnim(string animName)
    {
        animator.SetTrigger(animName);
        Debug.Log("播放" + animName + "動畫");
    }
    //生成飛行文字
    public void CreateFlyText(float damageAmount)
    {
        GameObject damageText = new GameObject("DamageText");
        damageText.transform.SetParent(transform);
        damageText.transform.position = transform.position;
        damageText.transform.localScale = new Vector3(1, 1, 1);

        TextMeshProUGUI textMesh = damageText.AddComponent<TextMeshProUGUI>();
        textMesh.text = $"-{damageAmount}";
        textMesh.fontSize = 24;
        textMesh.color = Color.red;
        textMesh.alignment = TextAlignmentOptions.Center;

        // 添加飛行動畫
        //dottween動畫
        damageText.GetComponent<RectTransform>().DOMoveY(damageText.transform.position.y + 1, 1).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            Destroy(damageText);
        });
    }

    void ClearDiceBox()
    {
        foreach (Transform child in diceBox.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
