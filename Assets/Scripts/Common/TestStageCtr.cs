using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TestStageCtr : MonoBehaviour
{
    [SerializeField] StageNode stageNode;
    [SerializeField] TMP_InputField inputField_stageInfo;
    void Start()
    {
        inputField_stageInfo.onValueChanged.AddListener(OnStageInfoChanged);
    }
    void OnStageInfoChanged(string newInfo)
    {
        stageNode.SetStageInfo(newInfo);
    }
}
