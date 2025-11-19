using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private string nowChapter = "";
    [SerializeField]
    private List<DialogueData> lines = new List<DialogueData>();
    private int pageIndex = 0;
    private ChatWindow chatWindow;
    private ChooseBox chooseBox;
    private PlayerInputActions inputActions;

    private List<string> jumpTo = new List<string>();

    public PortraitStageManager stageManager;

    // 文本替換的特殊暗號
    private readonly string PLAYER_NAME_TOKEN = "{PlayerName}";

    bool isOpen = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isOpen) return;
        isOpen = true;
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        chatWindow = gameObject.GetComponentInChildren<ChatWindow>();
        chooseBox = gameObject.GetComponentInChildren<ChooseBox>();
        AddEvent();
        // ShowDialogue("Prologue1_1");//讀取劇情
        ShowDialogue(GameDataManager.TmpAvgChapter);//讀取劇情
    }
    void AddEvent()
    {
        inputActions.Player.next.performed += OnNextClick;
        EventCenter.AddListener(AdvEvent.EVENT_CLICK_CHOICE, OnClickChoice);
    }
    private void OnDestroy()
    {
        // 解除綁定，避免記憶體洩漏
        inputActions.Player.next.performed -= OnNextClick;
        inputActions.Player.Disable();
        PortraitManager.UnloadAll();
        EventCenter.RemoveListener(AdvEvent.EVENT_CLICK_CHOICE, OnClickChoice);
    }
    void OnClickChoice(object[] args)
    {
        if (args.Length > 0 && args[0] is string targetTag)
        {
            JumpToTag(targetTag);
            jumpTo.Clear();
            onChoose = false;
        }
        else
        {
            Debug.LogWarning("❌ OnClickChoice: 無效的參數");
        }
    }
    public void ShowDialogue(string _chapter_csv)
    {
        lines = CSVReader.Instance.LoadDialogueCSV(_chapter_csv);//依照章節讀取CSV
        pageIndex = 0;
        nowChapter = lines[pageIndex].Chapter;
        if (nowChapter == "")
        {
            Debug.LogError("❌ Chapter 欄位不可為空，請檢查 CSV 檔案");
            return;
        }
        CheckDialogueCmd(pageIndex);
    }
    bool onChoose;
    private void OnNextClick(InputAction.CallbackContext context)
    {
        //處理跳轉邏輯
        if (jumpTo.Count > 0)
        {
            if (jumpTo.Count == 1)
            {
                JumpToTag(jumpTo[0]);
                jumpTo.Clear();
            }
            else if (!onChoose)
            {
                //多選一跳轉
                Debug.Log("生成多選一按鈕");
                onChoose = true;
                chooseBox.CreateChooseBtns(lines[pageIndex].Choices, lines[pageIndex].JumpTo);
            }
            return;
        }
        if (onChoose) return;//如果在選擇狀態則不處理下一步

        if (pageIndex < lines.Count - 1)
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

        if (nowChapter == "END")
        {
            Debug.Log("劇情結束");
            if (GameDataManager.TestMode)
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestAdvMenu");
            else
                EventCenter.Dispatch(StateEvent.EVENT_ENTER_MAP, GameDataManager.CurrentMap);
            return;//劇情結束不處理
        }
        if (nowChapter == "Battle")
        {
            chatWindow.HideWindow();
            Debug.Log("劇情結束 進入戰鬥"+int.Parse(lines[pageIndex].Flag));
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_DICEGAME, int.Parse(lines[pageIndex].Flag));
        }
    }
    private async void CheckDialogueCmd(int _page)
    {
        if (lines[_page].Chapter != "")
        {
            nowChapter = lines[_page].Chapter;
            Debug.Log("切換章節:" + nowChapter);
        }
        //紀錄跳轉
        if (lines[_page].JumpTo.Length > 0)
        {
            jumpTo = new List<string>(lines[_page].JumpTo);
        }
        //更換背景
        if (lines[_page].Background != "")
        {
            Debug.Log("更換背景:" + lines[_page].Background);
        }
        //更換立繪
        if (lines[_page].Portrait != "" && lines[_page].Character != "Hero" && lines[_page].Character != "Camera")
        {
            // 先載入角色立繪（如果尚未載入）
            await PortraitManager.LoadRoleIfNeeded(lines[_page].Character);

            Sprite _sprite = PortraitManager.Show(lines[_page].Character, lines[_page].Portrait);
            stageManager.SetCharacter(lines[_page].Character, _sprite, lines[_page].Anim, lines[_page].Pos);
            Debug.Log("更換立繪:" + lines[_page].Portrait);
        }
        //紀錄flag
        if (lines[_page].Flag != "")
        {
            Debug.Log("紀錄flag:" + lines[_page].Flag);
        }
        if (lines[_page].Character == "Camera" && lines[_page].Anim == "shake")
        {
            EventCenter.Dispatch(AdvEvent.EVENT_SHAKE_CAMERA);
        }
        //顯示對話
        if (lines[pageIndex].Dialogue != "")
        {
            // 替換文本中的玩家名字
            string processedDialogue = ReplacePlayerName(lines[pageIndex].Dialogue);
            string processedCharacter = ReplacePlayerName(lines[pageIndex].Character);

            chatWindow.ShowDialogue(processedCharacter, processedDialogue);
        }
        else
        {
            //跳過無對話的行
            OnNextClick(new InputAction.CallbackContext());
        }
    }

    // 替換玩家名字的方法
    private string ReplacePlayerName(string originalText)
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

    void JumpToTag(string _tag)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Tag == _tag)
            {
                Debug.Log("找到標籤:" + _tag);
                pageIndex = i;
                CheckDialogueCmd(pageIndex);
                return;
            }
        }
        Debug.Log("找不到標籤:" + _tag);
    }
}
