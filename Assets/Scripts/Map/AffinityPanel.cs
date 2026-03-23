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

    [SerializeField] private Button btn_back;
    [SerializeField] private Button btn_h_a;
    [SerializeField] private Button btn_h_b;
    [SerializeField] private GameObject[] obj_h_lock;

    string roleStr;
    int lastChatIndex = -1;

    List<PreparationRoomShortData> shortChatData = new List<PreparationRoomShortData>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameDataManager.DreamMode = true; // 開啟回看模式測試
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
        btn_role.image.color = new Color(1f, 1f, 1f, 0f);
        btn_role.image.DOFade(1f, 1f);
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

        btn_role.image.sprite = PortraitManager.Show(roleStr, face);

        txt_chat.text = "";
        foreach (char c in chat)
        {
            txt_chat.text += c;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
