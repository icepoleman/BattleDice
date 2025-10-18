using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CharacterView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    GameObject diceBox = null;
    //todo
    //血量顯示相關
    //生成彈出文字
    Sprite[] diceSprites;

    public void Awake()
    {
        diceBox = gameObject.transform.Find("diceBox").gameObject;
        diceSprites = Resources.LoadAll<Sprite>("dice/dice_base");
        
        //if (animator == null)
          //  animator = GetComponent<Animator>();
    }
    

    // 只負責顯示骰子動畫，不處理數值邏輯
    public IEnumerator ShowRollAnimation(List<int> rollResults)
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
        EventCenter.Dispatch(GameEvent.EVENT_CHANGE_STATE, TurnState.playerTurn);
        ClearDiceBox();
    }
    
    // 公開方法供 DiceGame 直接呼叫
    public void PlayAnim(string animName)
    {
        animator.SetTrigger(animName);
        Debug.Log("播放" + animName + "動畫");
    }
    
    void ClearDiceBox()
    {
        foreach (Transform child in diceBox.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
