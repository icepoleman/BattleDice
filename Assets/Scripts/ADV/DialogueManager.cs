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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        lines = CSVReader.Instance.LoadCSV("test2.csv");//依照章節讀取CSV
        chatWindow = gameObject.GetComponentInChildren<ChatWindow>();
        chatWindow.ShowDialogue(lines[pageIndex].Character, lines[pageIndex].Dialogue);
        AddClickEvent();
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
            else
            {
                //多選一跳轉
                Debug.Log("生成多選一按鈕");
            }
            return;
        }

        if (pageIndex < lines.Count - 1)
        {
            pageIndex++;
            CheckDialogueCmd(pageIndex);
        }
        else
        {
            Debug.Log("沒文本了!");
        }
    }
    private void CheckDialogueCmd(int _page)
    {
        //紀錄章節
        if (lines[_page].Chapter != "")
        {
            nowChapter = lines[_page].Chapter;
            Debug.Log("目前章節:" + nowChapter);
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
                pageIndex = i;
                CheckDialogueCmd(pageIndex);
                return;
            }
        }
        Debug.Log("找不到標籤:" + _tag);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
