using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AffinityPanel : MonoBehaviour
{
    [SerializeField] Image img_loveValue;
    [SerializeField] private Transform affinityEventParent;
    [SerializeField] private GameObject affinityEventPrefab;
    [SerializeField] private TextMeshProUGUI txt_chat;
    [SerializeField] private Button btn_role;//點擊角色頭像顯示對話
    [SerializeField] private Image img_role;

    [SerializeField] private Button btn_back;
    [SerializeField] private Button btn_h_a;
    [SerializeField] private Button btn_h_b;
    [SerializeField] private GameObject[] obj_h_lock;
    [SerializeField] private Button btn_loveEvent;

    string roleStr;
    int lastChatIndex = -1;

    List<PreparationRoomShortData> shortChatData = new List<PreparationRoomShortData>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameDataManager.DreamMode = true; // 開啟回看模式測試
        btn_loveEvent.onClick.AddListener(() =>
        {
            if (GameDataManager.SocialPoint > 0)
            {
                RoleEventData eventDataList = AffinityEventConfig.GetRoleEventData(roleStr);
                string nextSpEvent = GetNextUnseenSpEvent(eventDataList);
                if (!string.IsNullOrEmpty(nextSpEvent))
                {
                    ShowSpAffinityEvent(nextSpEvent);
                }
                else
                {
                    UIManager.ShowHintBubble(LanguageManager.GetText("T_SpEvent_ReedALL"));
                }
            }
            else
            {
                UIManager.ShowHintBubble(LanguageManager.GetText("T_SocialPoint_notEnough"));
            }
        });
    }

    string GetNextUnseenSpEvent(RoleEventData eventDataList)
    {
        if (eventDataList == null || eventDataList.allSpEventNames == null)
        {
            return null;
        }

        foreach (string eventName in eventDataList.allSpEventNames)
        {
            if (!GameDataManager.unlockedAffinityStages.Contains(eventName))
            {
                return eventName;
            }
        }

        return null;
    }

    void ShowSpAffinityEvent(string _roleEventName)
    {
        GameDataManager.SocialPoint -= 1;
        GameDataManager.UnlockAffinityStage(_roleEventName);
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, _roleEventName);//解鎖好感度事件
        Destroy(gameObject);
    }
    void OnDestroy()
    {
        GameDataManager.DreamMode = false; // 關閉回看模式測試
    }

    public async void SetUp(string role)
    {
        roleStr = role;
        await PortraitManager.LoadRoleIfNeeded(role);
        img_loveValue.fillAmount = GameDataManager.GetRoleAffinity(role) * 0.01f;
        List<AffinityStoryData> storyDataList = CSVReader.Instance.LoadAffinityStoryCSV(role);
        for (int i = 0; i < storyDataList.Count; i++)
        {
            GameObject affinityObj = Instantiate(affinityEventPrefab, affinityEventParent);
            AffinityItem affinityItem = affinityObj.GetComponent<AffinityItem>();
            affinityItem.SetUp(storyDataList[i], i);
            affinityObj.SetActive(true);
            Debug.Log("故事名稱: " + storyDataList[i].storyName);
            Debug.Log("解鎖提示: " + storyDataList[i].unlockHint);
        }
        shortChatData = CSVReader.Instance.LoadPreparationRoomShortCSV(role);
        btn_role.onClick.AddListener(() =>
        {
            if (shortChatData.Count > 0)
            {
                StartCoroutine(ShowChat());
            }
        });

        // 初始淡入效果
        img_role.color = new Color(1f, 1f, 1f, 0f);
        img_role.DOFade(1f, 1f);
        StartCoroutine(ShowChat());

        //H固定
        btn_h_a.onClick.AddListener(() =>
        {
            // 進入H關卡的事件
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "H_" + roleStr + "_1");
        });
        btn_h_b.onClick.AddListener(() =>
        {
            // 進入H關卡的事件
            EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, "H_" + roleStr + "_2");
        });
        if (roleStr == "JailerGirl")
        {
            obj_h_lock[0].SetActive(!GameDataManager.unlocked_H_Stages.Contains("H_JailerGirl_1"));//其他人預設開啟 只對獄卒鎖住第一個H關卡
        }
        obj_h_lock[1].SetActive(GameDataManager.GetRoleAffinity(role) < 100);//第二個H關卡需要滿好感才開啟
        btn_back.onClick.AddListener(() => Destroy(gameObject));

        switch (role)
        {
            case "JailerGirl":
                img_role.rectTransform.localPosition = new Vector3(54, -300f, 0f);
                break;
            case "Witch":
                img_role.rectTransform.localPosition = new Vector3(54, -520f, 0f);
                break;
            case "Idol":
                img_role.rectTransform.localPosition = new Vector3(54, -390, 0f);
                break;
            case "WolfGirl":
                img_role.rectTransform.localPosition = new Vector3(54, -430, 0f);
                break;
            default:

                break;
        }
    }
    /// <summary>
    /// 逐字顯示範例文字
    /// </summary>
    private IEnumerator ShowChat()
    {
        int randomIndex;
        if (shortChatData.Count == 1)
        {
            randomIndex = 0;
        }
        else
        {
            do
            {
                randomIndex = Random.Range(0, shortChatData.Count);
            } while (randomIndex == lastChatIndex);
        }
        lastChatIndex = randomIndex;
        string chat = shortChatData[randomIndex].dialogue;
        string face = shortChatData[randomIndex].face;

        img_role.sprite = PortraitManager.Show(roleStr, face);

        txt_chat.text = "";
        foreach (char c in chat)
        {
            txt_chat.text += c;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
