using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CharacterView : MonoBehaviour
{
    private Animator anim;
    private GameObject diceBox = null;
    //todo
    private TextMeshProUGUI txt_blood = null;
    private Slider slider_blood = null;
    private Text text_diceCount;
    private Slider slider_cd = null;
    public virtual void Init()
    {
        diceBox = transform.Find("diceBox").gameObject;
        txt_blood = transform.Find("txt_blood").GetComponent<TextMeshProUGUI>();
        slider_blood = transform.Find("slider_blood").GetComponent<Slider>();
        text_diceCount = transform.Find("img_dice/txt_diceCount").GetComponent<Text>();
        anim = transform.GetComponent<Animator>();
        slider_cd = transform.Find("slider_cd").GetComponent<Slider>();
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
            BurnDice(rollResults[i]);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        ClearDiceBox();

        // 執行回調，讓調用者決定後續行為
        onComplete?.Invoke();
    }
    public void ClearDiceBox()
    {
        foreach (Transform child in diceBox.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void BurnDice(int sideNum)
    {
        GameObject dice = new GameObject("dice");
        dice.transform.SetParent(diceBox.transform);
        RectTransform rt = dice.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(25, 25);
        rt.localScale = new Vector3(1, 1, 1);
        Image sr = dice.AddComponent<Image>();
        sr.sprite = ResourcesLoader.GetDiceSprite(sideNum);
    }
    float blood = 0;
    public void UpdateBlood(float currentBlood, float maxBlood)
    {
        if (currentBlood < blood)
        {
            //受傷動畫
            anim.Play("hurt");
            CreateFlyText("-" + (blood - currentBlood).ToString(), Color.red, 1f, Ease.InOutBack);
        }//回血動畫
        else if (currentBlood > blood)
        {
            CreateFlyText("+" + (currentBlood - blood).ToString(), Color.green, 1f, Ease.InOutBack);
        }
        blood = currentBlood;
        txt_blood.text = $"{currentBlood}/{maxBlood}";
        slider_blood.value = currentBlood / maxBlood;
    }
    public void UpdateCD(float currentCD)
    {
        slider_cd.value = currentCD;
    }
    // 公開方法供 DiceGame 直接呼叫
    public virtual void PlayAnim(string animName)
    {
        anim.Play(animName);
        Debug.Log("播放" + animName + "動畫");
    }
    public void UpdateDiceCount(int count)
    {
        text_diceCount.text = count.ToString();
    }
    //生成飛行文字
    public void CreateFlyText(string _flyTxt, Color32 _color, float _time = 1, Ease _ease = Ease.Linear)
    {
        GameObject damageText = Instantiate(Resources.Load<GameObject>("UI/flyText"));
        damageText.transform.SetParent(transform);
        damageText.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        damageText.transform.localScale = new Vector3(1, 1, 1);

        Text textMesh = damageText.GetComponent<Text>();
        textMesh.text = _flyTxt;
        textMesh.fontSize = 28;
        textMesh.color = _color;
        // 添加飛行動畫
        //dottween動畫
        damageText.GetComponent<RectTransform>().DOMoveY(damageText.transform.position.y + 0.5F, _time).SetEase(_ease).OnComplete(() =>
        {
            Destroy(damageText);
        });
    }
}
