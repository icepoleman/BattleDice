using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;
public class RoleView : MonoBehaviour
{
    Animator anim;
    RectTransform rectTrans;
    [SerializeField] Image roleImage;
    string portraitPos;
    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        rectTrans = GetComponent<RectTransform>();
        roleImage.DOFade(1, 0.5f);
    }

    public void ShowCharacter(Sprite _newPortrait, string _animationName, string _portraitPos)
    {
        if (_portraitPos != "")
            portraitPos = _portraitPos;

        // 停止之前的淡入淡出動畫
        roleImage.DOKill();

        //如果roleImage沒fadein 補fadein
        if (roleImage.color.a < 1)
        {
            roleImage.DOFade(1, 0.5f);
        }
        roleImage.sprite = _newPortrait;
        roleImage.SetNativeSize();

        if (_animationName == "hide")
        {
            //roleImage淡出
            roleImage.DOFade(0, 0.5f).OnComplete(() =>
            {
                Destroy(this.gameObject);
            });
            return;
        }
        if (_animationName != "")
            anim.Play(_animationName);
    }
    //動畫event呼叫
    public void HideCharacterFade()
    {
        //roleImage淡出
        roleImage.DOKill();
        roleImage.DOFade(0, 0.5f);
    }
    public void ShowCharacterFade()
    {
        //roleImage淡入
        roleImage.DOKill();
        roleImage.DOFade(1, 0.5f);
    }
    ////
    public void MovePosition(Vector2 _newPos)
    {
        // 確保 rectTrans 已初始化
        if (rectTrans == null)
            rectTrans = GetComponent<RectTransform>();

        // 停止之前的位置動畫
        rectTrans.DOKill();

        // 開始新的位移動畫
        rectTrans.DOAnchorPos(_newPos, 0.5f).SetEase(Ease.OutQuart);
    }

    // 立即移動到指定位置（無動畫）
    public void SetPosition(Vector2 _newPos)
    {
        // 確保 rectTrans 已初始化
        if (rectTrans == null)
            rectTrans = GetComponent<RectTransform>();

        // 停止之前的位置動畫
        rectTrans.DOKill();

        // 立即設置位置
        rectTrans.anchoredPosition = _newPos;

        Debug.Log($"角色 {gameObject.name} 立即設置位置為: {_newPos}");
    }
    public void SetSize(Vector2 _newSize)
    {
        transform.localScale = _newSize;
    }

    public string GetPortraitPos()
    {
        return portraitPos;
    }

    // 設置角色高亮狀態（講話時高亮，其他角色變暗）
    public void SetHighlight(bool isHighlighted)
    {
        roleImage.DOKill();
        Color targetColor = isHighlighted ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        roleImage.DOColor(targetColor, 0.3f);
    }
}
