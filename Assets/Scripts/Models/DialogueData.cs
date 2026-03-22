/// <summary>
/// 對話資料 - 從 CSV 載入
/// </summary>
[System.Serializable]
public class DialogueData
{
    public string Chapter;
    public string Character;
    public string Dialogue;     // 對話內容
    public string Portrait;     // 表情立繪
    public string Pos;
    public string[] Choices;
    public string[] JumpTo;
    public string Tag;
    public string Anim;
    public string Flag;
    public string Background;
    public string CameraAnim;
    public string Sound;
}
