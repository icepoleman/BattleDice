using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TreasureBoxGame : MonoBehaviour
{
    [SerializeField] private Image[] img_diceList;
    [SerializeField] private Sprite[] spr_diceList;
    [SerializeField] private Image img_box;
    [SerializeField] private Button btn_up;
    [SerializeField] private Button btn_down;
    [SerializeField] private Button btn_close;
    [SerializeField] private TextMeshProUGUI txt_lastPoint;
    [SerializeField] private TextMeshProUGUI txt_lockCount;
    [SerializeField] private Color selectedBtnColor = new Color(0.3f, 0.55f, 1f, 1f);
    [SerializeField] private Color normalBtnColor = Color.white;
    [SerializeField] private float rollAnimationDuration = 1f;

    private int lastPoint = 6;
    private bool isRolling = false;
    private int lockCount = 3;

    bool isDone;

    void Start()
    {
        btn_close.onClick.AddListener(() => Destroy(gameObject));
        btn_up.onClick.AddListener(() => RollAndJudge(true));
        btn_down.onClick.AddListener(() => RollAndJudge(false));
        RefreshLastPointText();
        ResetButtonSelection();
    }

    void OnDestroy()
    {
        btn_up.onClick.RemoveAllListeners();
        btn_down.onClick.RemoveAllListeners();

        if (img_diceList != null)
        {
            for (int i = 0; i < img_diceList.Length; i++)
            {
                if (img_diceList[i] != null)
                {
                    img_diceList[i].DOKill();
                }
            }
        }
    }

    private async void RollAndJudge(bool chooseUp)
    {
        if (isRolling || img_diceList == null || img_diceList.Length == 0 || isDone)
        {
            return;
        }

        isRolling = true;
        btn_up.interactable = false;
        btn_down.interactable = false;
        SetButtonSelection(chooseUp);

        int currentPoint = 0;
        int rollCount = img_diceList.Length;
        List<Task> rollTasks = new List<Task>(rollCount);

        for (int i = 0; i < rollCount; i++)
        {
            int side = Random.Range(1, 7);
            currentPoint += side;
            rollTasks.Add(AnimateDiceRoll(img_diceList[i], side, rollAnimationDuration));
        }

        await Task.WhenAll(rollTasks);

        bool isWin = chooseUp ? currentPoint > lastPoint : currentPoint <= lastPoint;
        if (isWin)
        {
            lastPoint = currentPoint;
            OnWin();
            RefreshLastPointText();
        }
        else
        {
            OnLose();
        }

        btn_up.interactable = true;
        btn_down.interactable = true;
        isRolling = false;
    }

    private Task AnimateDiceRoll(Image diceImage, int targetSide, float totalDuration)
    {
        if (diceImage == null)
        {
            return Task.CompletedTask;
        }

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        int switchCount = 15;
        int currentSwitch = 0;
        int lastRandomSide = -1;

        diceImage.DOKill();

        DOVirtual.Float(0f, 1f, totalDuration, (progress) =>
        {
            int targetSwitch = Mathf.FloorToInt(progress * switchCount);
            if (targetSwitch > currentSwitch && targetSwitch < switchCount)
            {
                currentSwitch = targetSwitch;

                int randomSide;
                do
                {
                    randomSide = Random.Range(1, 7);
                }
                while (randomSide == lastRandomSide && spr_diceList != null && spr_diceList.Length > 1);

                lastRandomSide = randomSide;
                diceImage.sprite = GetDiceSprite(randomSide);
            }
        })
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            diceImage.sprite = GetDiceSprite(targetSide);
            tcs.TrySetResult(true);
        })
        .OnKill(() =>
        {
            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    private Sprite GetDiceSprite(int side)
    {
        if (spr_diceList == null || spr_diceList.Length == 0)
        {
            return null;
        }

        int index = Mathf.Clamp(side - 1, 0, spr_diceList.Length - 1);
        return spr_diceList[index];
    }

    private void SetButtonSelection(bool chooseUp)
    {
        SetButtonColor(btn_up, chooseUp ? selectedBtnColor : normalBtnColor);
        SetButtonColor(btn_down, chooseUp ? normalBtnColor : selectedBtnColor);
    }

    private void ResetButtonSelection()
    {
        SetButtonColor(btn_up, normalBtnColor);
        SetButtonColor(btn_down, normalBtnColor);
    }

    private void SetButtonColor(Button button, Color color)
    {
        Image buttonImage = button.GetComponent<Image>();
        buttonImage.color = color;
    }

    private void RefreshLastPointText()
    {
        txt_lastPoint.text = lastPoint.ToString();
        txt_lockCount.text = lockCount.ToString();
    }

    private void OnWin()
    {
        ShakeBox();

        lockCount--;
        if (lockCount <= 0)
        {
            UIManager.ShowHintBubble("獲得xx獎勵");
            Debug.LogError("玩家獲勝！");
            isDone = true;
        }
    }

    private void ShakeBox()
    {
        if (img_box == null) return;
        img_box.rectTransform.DOKill();
        img_box.rectTransform.localRotation = Quaternion.identity;
        Sequence seq = DOTween.Sequence();
        seq.Append(img_box.rectTransform.DORotate(new Vector3(0f, 0f, 15f), 0.07f).SetEase(Ease.OutQuad));
        seq.Append(img_box.rectTransform.DORotate(new Vector3(0f, 0f, -15f), 0.1f).SetEase(Ease.InOutQuad));
        seq.Append(img_box.rectTransform.DORotate(new Vector3(0f, 0f, 10f), 0.08f).SetEase(Ease.InOutQuad));
        seq.Append(img_box.rectTransform.DORotate(new Vector3(0f, 0f, -10f), 0.08f).SetEase(Ease.InOutQuad));
        seq.Append(img_box.rectTransform.DORotate(new Vector3(0f, 0f, 5f), 0.07f).SetEase(Ease.InOutQuad));
        seq.Append(img_box.rectTransform.DORotate(Vector3.zero, 0.07f).SetEase(Ease.OutQuad));
    }

    private async void OnLose()
    {
        UIManager.ShowHintBubble(LanguageManager.GetText("T_UnLock_Fail"));
        isDone = true;
        await Task.Delay(1000);
        Destroy(gameObject);
    }
}
