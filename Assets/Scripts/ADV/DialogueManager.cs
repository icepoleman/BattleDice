using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private string nowChapter = "";
    [SerializeField]
    private List<DialogueData> lines = new List<DialogueData>();
    private int pageIndex = 0;
    private ChatWindow chatWindow;
    private PlayerInputActions inputActions;

    private List<string> jumpTo = new List<string>();

    [SerializeField]
    private GameObject chooseBtnPrefab;
    [SerializeField]
    private GameObject chooseBtnParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        chatWindow = gameObject.GetComponentInChildren<ChatWindow>();
        AddClickEvent();

        ShowDialogue("test3.csv");//讀取劇情
    }
    public void ShowDialogue(string _chapter_csv)
    {
        lines = CSVReader.Instance.LoadCSV(_chapter_csv);//依照章節讀取CSV
        pageIndex = 0;
        nowChapter = lines[pageIndex].Chapter;
        if(nowChapter=="")
        {
            Debug.LogError("❌ Chapter 欄位不可為空，請檢查 CSV 檔案");
            return;
        }
    }
    bool onChoose;
    private void OnNextClick(InputAction.CallbackContext context)
    {
        if (nowChapter == "END")
        {
            Debug.Log("劇情結束");
            return;//劇情結束不處理
        }

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
                for (int i = 0; i < jumpTo.Count; i++)
                {
                    int index = i;
                    GameObject btn = Instantiate(chooseBtnPrefab, chooseBtnParent.transform);
                    btn.GetComponentInChildren<TMPro.TMP_Text>().text = lines[pageIndex].Choices[i];
                    //設定按鈕點擊事件
                    btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                    {
                        JumpToTag(jumpTo[index]);
                        jumpTo.Clear();
                        onChoose = false;
                        ClearChooseBtn();
                    });
                }
            }
            return;
        }
        if (onChoose) return;//如果在選擇狀態則不處理下一步

        if (pageIndex < lines.Count - 1)
        {
            pageIndex++;
            CheckDialogueCmd(pageIndex);
        }
        else
        {
            Debug.Log("沒文本了!");
            nowChapter = "END";
            chatWindow.HideWindow();
        }

    }
    private void CheckDialogueCmd(int _page)
    {
        if (nowChapter == "END")
        {
            chatWindow.HideWindow();
            Debug.Log("劇情結束");
        }
        //紀錄跳轉
        if (lines[_page].JumpTo.Length > 0)
        {
            jumpTo = new List<string>(lines[_page].JumpTo);
        }
        //播放動畫
        if (lines[_page].Anim != "")
        {
            Debug.Log("播放動畫:" + lines[_page].Anim);
        }
        //更換背景
        if (lines[_page].Background != "")
        {
            Debug.Log("更換背景:" + lines[_page].Background);
        }
        //更換立繪
        if (lines[_page].Portrait != "")
        {
            Debug.Log("更換立繪:" + lines[_page].Portrait);
        }
        //顯示對話
        if (lines[pageIndex].Dialogue != "")
        {
            chatWindow.ShowDialogue(lines[pageIndex].Character, lines[pageIndex].Dialogue);
        }
        else
        {
            //跳過無對話的行
            OnNextClick(new InputAction.CallbackContext());
        }
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
    void ClearChooseBtn()
    {
        foreach (Transform child in chooseBtnParent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    void AddClickEvent()
    {
        inputActions.Player.next.performed += OnNextClick;
    }
    private void OnDisable()
    {
        // 解除綁定，避免記憶體洩漏
        inputActions.Player.next.performed -= OnNextClick;
        inputActions.Player.Disable();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
