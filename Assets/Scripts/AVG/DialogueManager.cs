using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class DialogueManager : MonoBehaviour
{
    enum DialogueState
    {
        Story,
        SpineShow,//之後可能會換聊天框
    }
    DialogueState currentState = DialogueState.Story;
    [SerializeField] private string nowChapter = "";
    [SerializeField] private List<DialogueData> dialogueDatas = new List<DialogueData>();
    [Header("背景 CG")]
    [SerializeField] Image img_cg1;
    [SerializeField] Image img_cg2;
    private bool useCg1 = true; // 追蹤當前使用的是哪個 Image
    private string currentBgAddress = ""; // 追蹤當前背景地址（用於卸載）
    [SerializeField] float cgFadeDuration = 0.5f; // CG 淡入淡出時間

    // 快轉模式設定
    [Header("快轉設定")]
    [SerializeField] private float fastForwardInterval = 0.05f; // 快轉時每行間隔時間
    private bool isFastForwarding = false;
    private float fastForwardTimer = 0f;

    // 跳過確認彈窗
    private bool isSkipPanelOpen = false;

    private int pageIndex = 0;
    [SerializeField] private ChatWindow chatWindow;
    [Header("選項按鈕")]
    [SerializeField] Transform trans_chooseBoxParent;
    GameObject chooseBtnPrefab;

    private List<string> jumpTo = new List<string>();
    private string pendingJumpTag = null; // 等待跳轉的標籤（選項文字顯示後跳轉）

    public PortraitStageManager stageManager;

    // 文本替換的特殊暗號
    private readonly string PLAYER_NAME_TOKEN = "{PlayerName}";

    Animator animator;
    bool isOpen = false;

    SkeletonAnimation spineCharacter;
    async void Start()
    {
        if (isOpen) return;
        isOpen = true;
        chatWindow.SetDatalogueManager(this);
        chooseBtnPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.AVG_PREFABS + "btn_choose" + ".prefab");
        animator = GetComponent<Animator>();
        AddEvent();
        LoadDialogue(GameDataManager.TmpAvgChapter);//讀取劇情
    }
    void Update()
    {
        // 檢測 ESC 鍵跳過劇情
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isOver && !isSkipPanelOpen)
            {
                ShowSkipConfirmPanel();
            }
        }

        // 檢測 Backspace 鍵快速跳到下一個選項
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            if (!isOver && !isSkipPanelOpen && !onChoose)
            {
                JumpToNextChoose();
            }
        }

        // 檢測 CTRL 鍵或 Skip 按鈕快轉
        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl);
        bool shouldFastForward = (ctrlPressed || chatWindow.tog_skip.isOn) && !onChoose && !isOver;

        if (shouldFastForward)
        {
            if (!isFastForwarding)
            {
                isFastForwarding = true;
                chatWindow.SetFastForwardMode(true);
                Debug.Log("⏩ 開始快轉");
            }

            fastForwardTimer += Time.deltaTime;
            if (fastForwardTimer >= fastForwardInterval)
            {
                fastForwardTimer = 0f;
                // 模擬點擊下一步
                OnNextClick();
            }
        }
        else if (isFastForwarding)
        {
            isFastForwarding = false;
            chatWindow.SetFastForwardMode(false);
            Debug.Log("⏸️ 停止快轉");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void AddEvent()
    {
        EventCenter.AddListener(AdvEvent.EVENT_CLICK_CHOICE, OnClickChoice);
    }
    private void OnDestroy()
    {
        PortraitManager.UnloadAll();
        AddressableManager.ReleaseAll();
        EventCenter.RemoveListener(AdvEvent.EVENT_CLICK_CHOICE, OnClickChoice);
    }

    [Header("自動播放設定")]
    [SerializeField] private float autoPlayDelay = 1f; // 自動播放延遲時間

    public async void AutoNextCoroutine()
    {
        await Task.Delay((int)(autoPlayDelay * 1000));
        if (!onChoose && !isOver) // 再次檢查狀態
        {
            OnNextClick();
        }
    }
    void OnClickChoice(object[] args)
    {
        if (args.Length >= 2 && args[0] is string targetTag && args[1] is string choiceText)
        {
            // 先顯示選項文字
            chatWindow.ShowDialogue(GetPlayerName(), choiceText);
            // 記錄待跳轉標籤，等下一次點擊時跳轉
            pendingJumpTag = targetTag;
            jumpTo.Clear();
            onChoose = false;
        }
        else
        {
            Debug.LogWarning("❌ OnClickChoice: 無效的參數");
        }
    }
    public void LoadDialogue(string _chapter_csv)
    {
        dialogueDatas = CSVReader.Instance.LoadDialogueCSV(_chapter_csv);//依照章節讀取CSV
        pageIndex = 0;
        nowChapter = dialogueDatas[pageIndex].Chapter;
        if (nowChapter == "")
        {
            Debug.LogError("❌ Chapter 欄位不可為空，請檢查 CSV 檔案");
            return;
        }
        CheckDialogueCmd(pageIndex);
    }
    bool onChoose;
    bool isOver;
    public void OnNextClick()
    {
        if (isOver) return;

        // 處理選項文字顯示後的跳轉
        if (pendingJumpTag != null)
        {
            if (chatWindow.isTyping)
            {
                chatWindow.CompleteDialogue();
                return;
            }
            string tag = pendingJumpTag;
            pendingJumpTag = null;
            JumpToTag(tag);
            return;
        }

        switch (nowChapter)
        {
            case "END":
                Debug.Log("劇情結束");
                isOver = true;
                if (GameDataManager.TestMode)
                    EventCenter.Dispatch(StateEvent.EVENT_TEST_AVGMENU);
                else
                    EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
                return;
            case "BATTLE":
                // 在這些章節中不處理下一步
                isOver = true;
                chatWindow.HideWindow();
                Debug.Log("劇情結束 進入戰鬥" + int.Parse(dialogueDatas[pageIndex].Flag));
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, int.Parse(dialogueDatas[pageIndex].Flag));
                return;
            case "DIE":
                isOver = true;
                Debug.Log("角色死亡，跳回主畫面");
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_MENU);
                GameDataManager.CurrentStage = GameDataManager.LastCurrentStage;//回到上一個關卡
                SaveManager.AutoSave();//快速存檔
                //EventCenter.Dispatch(StateEvent.EVENT_ENTER_PREPARATION_ROOM);
                break;
        }

        //處理跳轉邏輯
        if (jumpTo.Count > 0)
        {
            if (jumpTo.Count == 1)
            {
                JumpToTag(jumpTo[0]);
                jumpTo.Clear();
                return;
            }
            else if (!onChoose)
            {
                //多選一跳轉
                Debug.Log("生成多選一按鈕");
                onChoose = true;
                CreateChooseBtns(dialogueDatas[pageIndex].Choices, dialogueDatas[pageIndex].JumpTo);
            }
        }
        if (onChoose) return;//如果在選擇狀態則不處理下一步

        if (pageIndex < dialogueDatas.Count - 1)
        {
            if (chatWindow.isTyping)
            {
                //如果正在打字則直接顯示完整文本
                chatWindow.CompleteDialogue();
                return;
            }
            pageIndex++;
            CheckDialogueCmd(pageIndex);
        }
        else
        {
            Debug.Log("沒文本了!");
            chatWindow.HideWindow();
        }
    }
    private async void CheckDialogueCmd(int _page)
    {
        if (dialogueDatas[_page].Chapter != "")
        {
            nowChapter = dialogueDatas[_page].Chapter;
            Debug.Log("切換章節:" + nowChapter);
        }
        //紀錄跳轉
        if (dialogueDatas[_page].JumpTo.Length > 0)
        {
            jumpTo = new List<string>(dialogueDatas[_page].JumpTo);
        }
        //更換背景
        if (dialogueDatas[_page].Background != "")
        {
            Debug.Log("更換背景:" + dialogueDatas[_page].Background);
            await CrossFadeBackground(dialogueDatas[_page].Background);
        }
        //更換立繪
        if (dialogueDatas[_page].Portrait != "" && dialogueDatas[_page].Character != "Hero" && dialogueDatas[_page].Character != "Choose")
        {
            // 先載入角色立繪（如果尚未載入）
            await PortraitManager.LoadRoleIfNeeded(dialogueDatas[_page].Character);

            Sprite _sprite = PortraitManager.Show(dialogueDatas[_page].Character, dialogueDatas[_page].Portrait);
            stageManager.SetCharacter(dialogueDatas[_page].Character, _sprite, dialogueDatas[_page].Anim, dialogueDatas[_page].Pos);
            Debug.Log("更換立繪:" + dialogueDatas[_page].Portrait);
        }
        //紀錄flag
        string flag = dialogueDatas[_page].Flag;
        if (!string.IsNullOrEmpty(flag))
        {
            Debug.Log("紀錄flag:" + flag);

            if (flag.StartsWith("Spine_"))
            {
                await HandleSpineModel(flag);
            }
            else if (flag.StartsWith("SpineAnim_") && spineCharacter != null)
            {
                string animName = flag.Substring(10); // "SpineAnim_".Length = 10
                spineCharacter.AnimationState.SetAnimation(0, animName, true);
                Debug.Log("Spine模型播放動畫:" + animName);
            }
            else if (flag.StartsWith("SpineSpeed_") && spineCharacter != null)
            {
                string speedStr = flag.Substring(11); // "SpineSpeed_".Length = 11
                if (float.TryParse(speedStr, out float speed))
                {
                    DOTween.To(() => spineCharacter.timeScale, x => spineCharacter.timeScale = x, speed, 0.5f);
                    Debug.Log("Spine模型設置速度:" + speed);
                }
                else
                {
                    Debug.LogError("❌ 無法解析Spine速度:" + speedStr);
                }
            }
        }
        if (dialogueDatas[_page].CameraAnim != "")
        {
            Debug.Log("相機flag:" + dialogueDatas[_page].CameraAnim);
            animator.Play(dialogueDatas[_page].CameraAnim);
        }
        if (!string.IsNullOrEmpty(dialogueDatas[_page].Sound))
        {
            string _sound = dialogueDatas[_page].Sound;
            if (_sound.StartsWith("Sound_"))
            {
                AudioManager.Instance.PlaySFX(_sound);
                Debug.Log("播放音效:" + _sound);
            }
            if (_sound.StartsWith("Bgm_"))
            {
                AudioManager.Instance.PlayBGM(_sound, true, 1.0f);
                Debug.Log("播放音樂:" + _sound);
            }
            Debug.Log("播放音效:" + dialogueDatas[_page].Sound);
        }
        //顯示對話
        if (dialogueDatas[pageIndex].Dialogue != "")
        {
            // 替換文本中的玩家名字
            string processedDialogue = ReplaceDialoguePlayerName(dialogueDatas[pageIndex].Dialogue);
            if (dialogueDatas[pageIndex].Character == "Hero")
                dialogueDatas[pageIndex].Character = GetPlayerName();
            else if (dialogueDatas[pageIndex].Character != "Choose" && dialogueDatas[pageIndex].Character != "")
                dialogueDatas[pageIndex].Character = LanguageManager.GetText("T_" + dialogueDatas[pageIndex].Character);

            chatWindow.ShowDialogue(dialogueDatas[pageIndex].Character, processedDialogue);
        }
        else
        {
            //跳過無對話的行
            OnNextClick();
        }
    }
    //    
    // 替換劇情玩家名字的方法
    private string ReplaceDialoguePlayerName(string originalText)
    {
        if (string.IsNullOrEmpty(originalText))
            return originalText;

        if (originalText.Contains(PLAYER_NAME_TOKEN))
        {
            string playerName = GetPlayerName();
            string processedText = originalText.Replace(PLAYER_NAME_TOKEN, playerName);
            Debug.Log($"文本替換: {PLAYER_NAME_TOKEN} → {playerName}");
            return processedText;
        }

        return originalText;
    }

    // 獲取玩家名字
    private string GetPlayerName()
    {
        // 從存檔獲取玩家名字
        if (GameDataManager.PlayerName != null)
        {
            return GameDataManager.PlayerName;
        }

        // 預設名字
        return "主角";
    }

    // 交叉淡入淡出換背景
    private async System.Threading.Tasks.Task CrossFadeBackground(string backgroundAddress)
    {
        // 記錄舊背景地址（用於卸載）
        string oldBgAddress = ABconfig.AVG_BG + currentBgAddress + ".png";
        currentBgAddress = ABconfig.AVG_BG + backgroundAddress + ".png";

        // 載入新背景圖片
        Sprite newSprite = await AddressableManager.LoadAssetAsync<Sprite>(currentBgAddress);
        if (newSprite == null)
        {
            Debug.LogError($"❌ 無法載入背景: {currentBgAddress}");
            return;
        }



        // 決定使用哪個 Image
        Image fadeInImage = useCg1 ? img_cg1 : img_cg2;
        Image fadeOutImage = useCg1 ? img_cg2 : img_cg1;

        // 設置新背景到要淡入的 Image
        fadeInImage.sprite = newSprite;
        fadeInImage.color = new Color(1, 1, 1, 0); // 初始透明
        fadeInImage.gameObject.SetActive(true);

        // 確保淡入的 Image 在上層
        fadeInImage.transform.SetAsLastSibling();

        // 同時執行淡入淡出動畫
        fadeInImage.DOKill();
        fadeOutImage.DOKill();

        fadeInImage.DOFade(1f, cgFadeDuration);
        fadeOutImage.DOFade(0f, cgFadeDuration).OnComplete(() =>
        {
            fadeOutImage.gameObject.SetActive(false);
            fadeOutImage.sprite = null;

            // 卸載舊背景
            if (!string.IsNullOrEmpty(oldBgAddress))
            {
                AddressableManager.ReleaseAsset(oldBgAddress);
                Debug.Log($"已卸載舊背景: {oldBgAddress}");
            }
        });

        // 切換標記
        useCg1 = !useCg1;

        Debug.Log($"背景切換完成: {currentBgAddress}");
    }

    void JumpToTag(string _tag)
    {
        for (int i = 0; i < dialogueDatas.Count; i++)
        {
            if (dialogueDatas[i].Tag == _tag)
            {
                Debug.Log("找到標籤:" + _tag);
                pageIndex = i;
                CheckDialogueCmd(pageIndex);
                return;
            }
        }
        Debug.Log("找不到標籤:" + _tag);
    }

    /// <summary>
    /// 處理 Spine 模型的生成與刪除（toggle 模式）
    /// </summary>
    private async System.Threading.Tasks.Task HandleSpineModel(string spineAddress)
    {
        // 已存在則淡出刪除（toggle off）
        if (spineCharacter != null)
        {
            var fadeOutTarget = spineCharacter;
            fadeOutTarget.AnimationState.Event -= OnSpineEvent; // 解除事件訂閱
            DOTween.To(() => fadeOutTarget.skeleton.A, x => fadeOutTarget.skeleton.A = x, 0f, 0.5f)
                .OnComplete(() =>
                {
                    Destroy(fadeOutTarget.gameObject);
                });
            spineCharacter = null;
            currentState = DialogueState.Story;
            Debug.Log("淡出刪除Spine模型");
            return;
        }

        // 不存在則生成並淡入（toggle on）
        currentState = DialogueState.SpineShow;
        GameObject spinePrefab = await AddressableManager.LoadAssetAsync<GameObject>(ABconfig.H_DATA_PREFABS + spineAddress);
        if (spinePrefab != null)
        {
            GameObject spineObj = Instantiate(spinePrefab);
            spineCharacter = spineObj.GetComponent<SkeletonAnimation>();
            spineCharacter.skeleton.A = 0f; // 初始透明
            spineCharacter.AnimationState.SetAnimation(0, "A", true);
            spineCharacter.AnimationState.Event += OnSpineEvent;

            // 淡入效果
            DOTween.To(() => spineCharacter.skeleton.A, x => spineCharacter.skeleton.A = x, 1f, 0.5f);
            Debug.Log("淡入生成Spine模型:" + spineAddress);
        }
        else
        {
            Debug.LogError("❌ 無法載入Spine模型:" + spineAddress);
            currentState = DialogueState.Story;
        }
    }
    //spine事件回調
    void OnSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        string eventName = e.Data.Name;
        Debug.Log("Spine事件:" + eventName);
        if (eventName.StartsWith("Sound_"))
        {
            AudioManager.Instance.PlaySFX(eventName);
            Debug.Log("Spine事件播放音效:" + eventName);
        }
        else if (eventName.StartsWith("SpineAnim_"))
        {
            string animName = eventName.Substring(10); // "SpineAnim_".Length = 10
            spineCharacter.AnimationState.SetAnimation(0, animName, true);
            Debug.Log("Spine模型播放動畫:" + animName);
        }
    }
    /// <summary>
    /// 顯示跳過劇情確認彈窗
    /// </summary>
    private async void ShowSkipConfirmPanel()
    {
        isSkipPanelOpen = true;
        await UIManager.ShowConfirmPanel(LanguageManager.GetText("T_Skip_Dialogue_Hint"),
                    () =>
                    {
                        // 確認跳過
                        Debug.Log("跳過劇情");
                        isSkipPanelOpen = false;
                        SkipDialogue();
                    },
                    () =>
                    {
                        // 取消
                        isSkipPanelOpen = false;
                    });
    }

    /// <summary>
    /// 跳過劇情
    /// </summary>
    private void SkipDialogue()
    {
        isOver = true;
        chatWindow.HideWindow();

        // 檢查是否有戰鬥章節，如果有則跳轉到戰鬥
        for (int i = pageIndex; i < dialogueDatas.Count; i++)
        {
            if (dialogueDatas[i].Chapter == "BATTLE")
            {
                Debug.Log("跳過劇情，進入戰鬥: " + int.Parse(dialogueDatas[i].Flag));
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, int.Parse(dialogueDatas[i].Flag));
                return;
            }
        }

        if (GameDataManager.TestMode)
        {
            EventCenter.Dispatch(StateEvent.EVENT_TEST_AVGMENU);
        }
        else
        {
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
        }
    }

    /// <summary>
    /// 跳到下一個選項（Choose），如果沒有就跳過劇情
    /// </summary>
    private void JumpToNextChoose()
    {
        // 從當前位置往後搜尋
        for (int i = pageIndex + 1; i < dialogueDatas.Count; i++)
        {
            if (dialogueDatas[i].Character == "Choose")
            {
                Debug.Log($"跳到選項: index {i}");
                pageIndex = i;
                CheckDialogueCmd(pageIndex);
                return;
            }
        }

        // 找不到選項，直接跳過劇情
        Debug.Log("沒有更多選項，跳過劇情");
        SkipDialogue();
    }

    public void AnimShootEnd()
    {
        //閃白光換模型
        spineCharacter.AnimationState.SetAnimation(0, "End", true);
    }

    #region 選項按鈕
    void CreateChooseBtns(string[] btnText, string[] targetTag)
    {
        if (btnText.Length != targetTag.Length)
        {
            Debug.LogError("❌ CreateChooseBtns: btnText 和 targetTag 長度不一致");
            return;
        }

        if (chooseBtnPrefab == null)
        {
            Debug.LogError("❌ chooseBtnPrefab 尚未設定");
            return;
        }

        for (int i = 0; i < btnText.Length; i++)
        {
            int index = i;
            string choiceText = btnText[i];
            GameObject btn = Instantiate(chooseBtnPrefab, trans_chooseBoxParent);
            btn.GetComponentInChildren<Text>().text = choiceText;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                EventCenter.Dispatch(AdvEvent.EVENT_CLICK_CHOICE, targetTag[index], choiceText);
                ClearChooseBtn();
            });
            btn.gameObject.SetActive(true);
        }
    }

    void ClearChooseBtn()
    {
        foreach (Transform child in trans_chooseBoxParent)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion
}