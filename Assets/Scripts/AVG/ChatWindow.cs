using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChatWindow : MonoBehaviour
{
    DialogueManager dialogueManager;
    private List<ChatLog> showedDialogues = new List<ChatLog>();//已顯示過的對話（用於記錄日誌）
    [SerializeField] private Text dialogueText; // 對話框文字
    [SerializeField] private Text nameText; // 角色名稱文字
    [SerializeField] private GameObject img_done; // 對話結束圖示

    [Header("控制按鈕")]
    [SerializeField] private Toggle tog_auto;
    public Toggle tog_skip;
    [SerializeField] private Toggle tog_hide;
    [SerializeField] private Button btn_log;

    [SerializeField] private Button btn_next;
    [SerializeField] private Button btn_set;

    [SerializeField] private Animator animator;
    private float typingSpeed = 0.05f; // 逐字顯示速度
    [SerializeField] private float fastTypingSpeed = 0.005f; // 快轉時的打字速度
    private bool isFastForwardMode = false;

    private const string TYPING_SPEED_KEY = "TypingSpeed";
    private const float DEFAULT_TYPING_SPEED = 0.05f;
    private ContentSizeFitter fitter;
    private Tween autoRotateTween;

    private void Start()
    {
        fitter = dialogueText.GetComponent<ContentSizeFitter>();
        typingSpeed = TypingSpeed; // 從 PlayerPrefs 讀取速度
        tog_hide.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                HideWindow();
            }
            else
            {
                ShowWindow();
            }
        });
        btn_next.onClick.AddListener(() =>
        {
            dialogueManager.OnNextClick();
            tog_skip.isOn = false;// 點擊下一句時取消跳過模式
        });
        btn_set.onClick.AddListener(async () => { await UIManager.ShowCommonPanel("SetPanel"); });
        btn_log.onClick.AddListener(async () =>
        {
            tog_auto.isOn = false;// 打開日誌時取消自動模式
            GameObject logPanelprefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.AVG_PREFABS + "logPanel" + ".prefab");
            GameObject logPanelObj = Instantiate(logPanelprefab, transform);
            logPanelObj.GetComponent<LogView>().SetData(showedDialogues);
        });
        EventCenter.AddListener(StateEvent.EVENT_SETTING_CHANGED, OnSettingChanged);
        tog_auto.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                CompleteDialogue();
                tog_auto.image.color = new Color(1f, 1f, 1f, 0f);
                // 開始順時針旋轉
                autoRotateTween?.Kill();
                autoRotateTween = tog_auto.image.rectTransform
                    .DORotate(new Vector3(0, 0, -360f), 5f, RotateMode.FastBeyond360)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(Ease.Linear);
            }
            else
            {
                tog_auto.image.color = new Color(1f, 1f, 1f, 1f); // 關閉時全亮
                // 停止旋轉並回到 0
                autoRotateTween?.Kill();
                tog_auto.image.rectTransform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            }
        });
        tog_skip.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                tog_skip.image.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                tog_skip.image.color = new Color(1f, 1f, 1f, 1f); // 關閉時全亮
            }
        });

        // img_done 上下飄動動畫
        RectTransform doneRect = img_done.GetComponent<RectTransform>();
        float originalY = doneRect.anchoredPosition.y;
        doneRect.DOAnchorPosY(originalY + 5f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
    void OnDestroy()
    {
        EventCenter.RemoveListener(StateEvent.EVENT_SETTING_CHANGED, OnSettingChanged);
    }
    private void OnSettingChanged(object[] args)
    {
        typingSpeed = TypingSpeed; // 更新打字速度
    }
    /// <summary>
    /// 取得/設定文字顯示速度（使用 PlayerPrefs 存取）
    /// </summary>
    public static float TypingSpeed
    {
        get => PlayerPrefs.GetFloat(TYPING_SPEED_KEY, DEFAULT_TYPING_SPEED);
        set
        {
            PlayerPrefs.SetFloat(TYPING_SPEED_KEY, value);
            PlayerPrefs.Save();
        }
    }

    private string str_dialogue = "你好，這是一個簡易的文字冒險範例。";

    public bool isTyping = false;

    public void SetDatalogueManager(DialogueManager manager)
    {
        dialogueManager = manager;
    }

    public void CompleteDialogue()
    {
        // 直接顯示完整文字
        StopAllCoroutines();
        dialogueText.text = str_dialogue;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        if (!isFastForwardMode)
            img_done.SetActive(true);
        isTyping = false;
        if (tog_auto.isOn)
            dialogueManager.AutoNextCoroutine();
    }
    public void ShowDialogue(string speaker, string text)
    {
        // 記錄已顯示的對話（用於日誌）
        showedDialogues.Add(new ChatLog { name = speaker, dialogue = text });
        nameText.text = speaker;
        str_dialogue = text;
        if (isTyping)
        {
            CompleteDialogue();
        }
        else
        {
            StartCoroutine(TypeLine());
        }
    }

    public void ShowWindow()
    {
        animator.Play("show");
    }
    public void HideWindow()
    {
        animator.Play("hide");
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        img_done.SetActive(false);

        // 暫時顯示完整文字以獲取寬度
        dialogueText.text = str_dialogue;
        Canvas.ForceUpdateCanvases();
        float targetWidth = dialogueText.rectTransform.rect.width;

        // 固定寬度，禁用 horizontal fit
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        dialogueText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

        // 清空並開始逐字顯示
        dialogueText.text = "";
        foreach (char c in str_dialogue.ToCharArray())
        {
            dialogueText.text += c;
            float currentSpeed = isFastForwardMode ? fastTypingSpeed : typingSpeed;
            yield return new WaitForSeconds(currentSpeed);
        }

        // 恢復 Content Size Fitter
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        yield return new WaitForSeconds(0.1f);
        img_done.SetActive(true);
        isTyping = false;
        if (tog_auto.isOn)
            dialogueManager.AutoNextCoroutine();
    }

    // 設定快轉模式
    public void SetFastForwardMode(bool fastForward)
    {
        isFastForwardMode = fastForward;
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            tog_hide.isOn = false;
        }
    }
}
public class ChatLog
{
    public string name;
    public string dialogue;
}