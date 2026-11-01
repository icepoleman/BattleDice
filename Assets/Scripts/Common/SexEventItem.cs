using UnityEngine;
using UnityEngine.UI;
public class SexEventItem : MonoBehaviour
{
    [SerializeField] private GameObject obj_lock;
    [SerializeField] private TMPro.TMP_Text txt_unLockHint;
    [SerializeField] private Button btn_event;
    [SerializeField] private Image img_hcgPic;
    [SerializeField] BaseMouseEffect mouseEffect;
    private bool isLock = false;

    void Start()
    {
        btn_event.onClick.AddListener(() =>
        {
            OpenEvent();
        });
    }
    string avgID;
    public void SetUp(bool isLock, RoleType roleType, int unLockIndex)
    {
        mouseEffect.enabled = !isLock;
        this.isLock = isLock;
        obj_lock.SetActive(isLock);
        img_hcgPic.gameObject.SetActive(!isLock);
        img_hcgPic.sprite = isLock ? null : Resources.Load<Sprite>("H_EventLittlePic/" + "HCG_Pic_" + roleType.ToString() + "_" + unLockIndex);
        txt_unLockHint.gameObject.SetActive(isLock);
        txt_unLockHint.text = unLockIndex == 2 ? LanguageManager.GetText("T_UnLockMax_H_Hint") : LanguageManager.GetText("T_UnLockOther_H_Hint");
        avgID = "H_" + roleType.ToString() + "_" + unLockIndex;
    }
    void OpenEvent()
    {
        if (isLock)
        {
            return;
        }
        EventCenter.Dispatch(StateEvent.EVENT_ENTER_AVG, avgID);
    }
}
