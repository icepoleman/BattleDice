using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ChatWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueText; // 對話框文字
    [SerializeField] private TMP_Text nameText; // 角色名稱文字
    [SerializeField] private GameObject doneImg; // 對話結束圖示

    private Animator animator;
    [SerializeField] private float typingSpeed = 0.01f; // 逐字顯示速度

    private string str_dialogue = "你好，這是一個簡易的文字冒險範例。";

    private bool isTyping = false;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        ShowDialogue("主角", str_dialogue);
    }
    private void OnEnable()
    {
        // 啟用 Action Map
        inputActions.Player.Enable();

        // 綁定事件 callback
      // inputActions.Player.next.performed += HideWindow;
    }

    private void OnDisable()
    {
        // 解除綁定，避免記憶體洩漏
       // inputActions.Player.next.performed -= OnJump;
     //   inputActions.Player.Disable();
    }

    private void ShowDialogue(string speaker, string text)
    {
        nameText.text = speaker;
        if (isTyping)
        {
            // 直接顯示完整文字
            StopAllCoroutines();
            dialogueText.text = str_dialogue;
            doneImg.SetActive(true);
            isTyping = false;
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
        doneImg.SetActive(false);
        dialogueText.text = "";
        foreach (char c in str_dialogue.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        doneImg.SetActive(true);
        isTyping = false;
    }
}
