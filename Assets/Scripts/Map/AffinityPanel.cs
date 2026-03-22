using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AffinityPanel : MonoBehaviour
{
    [SerializeField] Image img_loveValue;
    [SerializeField] private List<Sprite> roleSprites;
    [SerializeField] private Transform affinityEventParent;
    [SerializeField] private GameObject affinityEventPrefab;
    [SerializeField] private TextMeshProUGUI txt_chat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //test
        SetUp("JailerGirl");
    }

    public async void SetUp(string role)
    {
        roleSprites = await AddressableManager.LoadLabelAsync<Sprite>(role);
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
    }
    /// <summary>
    /// 逐字顯示範例文字
    /// </summary>
    private IEnumerator ShowChat(string chat, string face)
    {
        txt_chat.text = "";
        foreach (char c in chat)
        {
            txt_chat.text += c;
            yield return new WaitForSeconds(0.1f);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
