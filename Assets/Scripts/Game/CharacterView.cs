using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
public class CharacterView : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] protected GameObject diceBox = null;

    public virtual void Init()
    {
        diceBox = transform.Find("diceBox").gameObject;
        anim = transform.GetComponent<Animator>();
    }

    // 只負責顯示骰子動畫，不處理數值邏輯
    public async Task ShowRollAnimation(List<int> rollResults)
    {
        if(rollResults == null || rollResults.Count == 0)
        {
            Debug.LogWarning("骰子結果為空，無法顯示動畫");
            return;
        }
        Debug.Log("顯示擲骰子動畫");
        ClearDiceBox();

        await Task.Delay(500);// 等待0.5秒後開始顯示動畫

        //生成骰子物件在diceBox下
        for (int i = 0; i < rollResults.Count; i++)
        {
            BurnDice(rollResults[i]);
            await Task.Delay(100);
        }
        await Task.Delay(500);
    }
    public void ClearDiceBox()
    {
        foreach (Transform child in diceBox.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public virtual void BurnDice(int sideNum)
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
    public async Task UpdateBlood(float currentBlood, float maxBlood)
    {
        if (currentBlood < blood)
        {
            //受傷動畫
            anim.Play("hurt");
            await CreateFlyText("-" + (blood - currentBlood).ToString(), Color.red, 1f, Ease.InOutBack);
        }//回血動畫
        else if (currentBlood > blood)
        {
            await CreateFlyText("+" + (currentBlood - blood).ToString(), Color.green, 1f, Ease.InOutBack);
        }
        blood = currentBlood;
    }
    // 公開方法供 DiceGame 直接呼叫
    public virtual void PlayAnim(string animName)
    {
        anim.Play(animName);
        Debug.Log("播放" + animName + "動畫");
    }
    public void SetAnimBool(string paramName, bool value)
    {
        anim.SetBool(paramName, value);
    }
    //生成飛行文字
    public async Task CreateFlyText(string _flyTxt, Color32 _color, float _time = 1, Ease _ease = Ease.Linear)
    {
        GameObject damageText = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.GAME_PREFABS + "flyText.prefab");
        GameObject _instFlyText = Instantiate(damageText);
        _instFlyText.transform.SetParent(transform);
        _instFlyText.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        _instFlyText.transform.localScale = new Vector3(1, 1, 1);

        Text textMesh = _instFlyText.GetComponent<Text>();
        textMesh.text = _flyTxt;
        textMesh.fontSize = 28;
        textMesh.color = _color;
        // 添加飛行動畫
        //dottween動畫
        _instFlyText.GetComponent<RectTransform>().DOMoveY(_instFlyText.transform.position.y + 0.5F, _time).SetEase(_ease).OnComplete(() =>
        {
            Destroy(_instFlyText);
        });
    }
}
