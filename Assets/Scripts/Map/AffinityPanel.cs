using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class AffinityPanel : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] CanvasGroup canvasGroup;
    [Header("好感度")]
    [SerializeField] TextMeshProUGUI txt_loveValue;
    [SerializeField] Image img_loveFill;
    [SerializeField] TextMeshProUGUI txt_loveLevel;
    [SerializeField] TextMeshProUGUI txt_supPowerUp;
    [Header("其他")]
    [SerializeField] TextMeshProUGUI txt_roleName;
    /* [SerializeField] private Transform affinityEventParent;
     [SerializeField] private GameObject affinityEventPrefab;*/
    [SerializeField] private TextMeshProUGUI txt_chat;
    [SerializeField] private Button btn_role;//點擊角色頭像顯示對話
    [SerializeField] private Image img_role;

    [SerializeField] private Button btn_back;
    [SerializeField] private Button btn_loveEvent;

    [Header("buff提示")]
    [SerializeField] GameObject obj_buffTip;
    [SerializeField] TextMeshProUGUI text_buffTip;

    [Header("H回想")]
    [SerializeField] private GameObject hEventRoot;
    [SerializeField] private GameObject sexEventItemPrefab;


    string roleStr;
    int lastChatIndex = -1;

    List<PreparationRoomShortData> shortChatData = new List<PreparationRoomShortData>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);

        GameDataManager.DreamMode = true; // 開啟回看模式測試
        btn_loveEvent.onClick.AddListener(() =>
        {
            OpenSpEvent();
        });
    }

    void OpenSpEvent()
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
                EventCenter.Dispatch(StateEvent.EVENT_SHOW_HINT_BUBBLE, LanguageManager.GetText("T_SpEvent_ReedALL"));
            }
        }
        else
        {
            EventCenter.Dispatch(StateEvent.EVENT_SHOW_HINT_BUBBLE, LanguageManager.GetText("T_SocialPoint_notEnough"));
        }
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
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, _roleEventName);
        Destroy(gameObject);
    }
    void OnDestroy()
    {
        GameDataManager.DreamMode = false; // 關閉回看模式測試
    }

    public async void SetUp(string role)
    {
        roleStr = role;
        btn_back.onClick.AddListener(() =>
        {
            EventCenter.Dispatch(PreparationRoomEvent.OPEN_FIRE);
            Destroy(gameObject);
        });
        await PortraitManager.LoadRoleIfNeeded(role);
        UpdateLoveUI(role);
        GenerateSexEventItems(role);
        /*List<AffinityStoryData> storyDataList = CSVReader.Instance.LoadAffinityStoryCSV(role);
        for (int i = 0; i < storyDataList.Count; i++)
        {
            GameObject affinityObj = Instantiate(affinityEventPrefab, affinityEventParent);
            AffinityItem affinityItem = affinityObj.GetComponent<AffinityItem>();
            affinityItem.SetUp(storyDataList[i], i);
            affinityObj.SetActive(true);
            Debug.Log("故事名稱: " + storyDataList[i].storyName);
            Debug.Log("解鎖提示: " + storyDataList[i].unlockHint);
        }*/
        shortChatData = CSVReader.Instance.LoadPreparationRoomShortCSV(role);
        btn_role.onClick.AddListener(() =>
        {
            if (shortChatData.Count > 0)
            {
                StartCoroutine(ShowChat());
            }
        });
        StartCoroutine(ShowChat());

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

    private void GenerateSexEventItems(string role)
    {
        if (hEventRoot == null || sexEventItemPrefab == null ||
            !GameDataManager.H_Stages_Affinity.TryGetValue(role, out string[] stageIds) ||
            !System.Enum.TryParse(role, out RoleType roleType))
        {
            return;
        }

        foreach (string stageId in stageIds)
        {
            int separatorIndex = stageId.LastIndexOf('_');
            if (separatorIndex < 0 || !int.TryParse(stageId.Substring(separatorIndex + 1), out int unlockIndex))
            {
                continue;
            }

            GameObject itemObject = Instantiate(sexEventItemPrefab, hEventRoot.transform);
            SexEventItem item = itemObject.GetComponent<SexEventItem>();
            if (item == null)
            {
                Destroy(itemObject);
                continue;
            }

            bool isLocked = !GameDataManager.unlocked_H_Stages.Contains(stageId);
            item.SetUp(isLocked, roleType, unlockIndex);
            itemObject.SetActive(true);
        }
    }

    void UpdateLoveUI(string role)
    {
        int roleAffinity = GameDataManager.GetRoleAffinity(role);
        int affinityLevel = GameDataManager.GetRoleAffinityLevel(role);
        bool isMaxAffinityLevel = affinityLevel >= 5;
        int affinityProgress = roleAffinity % 100;
        float targetFill = isMaxAffinityLevel ? 1f : Mathf.Clamp01(affinityProgress / 100f);
        txt_loveValue.text = isMaxAffinityLevel ? string.Empty : affinityProgress + "/100";
        img_loveFill.fillAmount = 0f;
        img_loveFill.DOFillAmount(targetFill, 0.6f).SetEase(Ease.OutCubic);
        txt_roleName.text = LanguageManager.GetText("T_" + role);
        txt_loveLevel.text = affinityLevel >= 5 ? "Lv.Max" : "Lv." + affinityLevel;
        txt_supPowerUp.text = LanguageManager.GetText("T_SupPowerUp_" + role + "_" + affinityLevel);
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

    #region BUFF說明的Tooltip
    private int currentLinkIndex = -1;
    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            txt_supPowerUp,
            eventData.position,
            eventData.enterEventCamera
        );

        if (linkIndex != -1)
        {
            if (linkIndex != currentLinkIndex)
            {
                currentLinkIndex = linkIndex;
                TMP_LinkInfo linkInfo = txt_supPowerUp.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();

                // 取得 link 在文字中的位置（使用第一個字符的位置）
                int firstCharIndex = linkInfo.linkTextfirstCharacterIndex;
                TMP_CharacterInfo charInfo = txt_supPowerUp.textInfo.characterInfo[firstCharIndex];
                Vector3 charWorldPos = txt_supPowerUp.transform.TransformPoint(charInfo.topLeft);

                obj_buffTip.SetActive(true);
                BuffConfigData buffData = BuffDatabase.GetBuffConfig(int.Parse(linkId));
                text_buffTip.text = buffData.describe;
            }
        }
        else
        {
            if (currentLinkIndex != -1)
            {
                currentLinkIndex = -1;
                obj_buffTip.SetActive(false);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        currentLinkIndex = -1;
        obj_buffTip.SetActive(false);
    }
    #endregion
}
