using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CharacterView
{
    private Image img_character, img_characterEffect;
    private string enemyLabel = "";
    public override void Init()
    {
        img_character = gameObject.transform.Find("characterImage").GetComponent<Image>();
        img_characterEffect = gameObject.transform.Find("characterEffect").GetComponent<Image>();
        base.Init();
    }
    public void SetEnemyLabel(string label)
    {
        enemyLabel = label;
        PlayAnim("idle");
    }
    public override void PlayAnim(string animName)
    {
        Debug.Log($"EnemyView 播放動畫: {animName} for {enemyLabel}");
        if (enemyLabel != "")
        {
            if (animName == "idle")
                img_character.sprite = EnemyPortraitManager.GetEnemySprite(enemyLabel, "idle");
            else
                img_characterEffect.sprite = EnemyPortraitManager.GetEnemySprite(enemyLabel, animName);
        }
        base.PlayAnim(animName);
    }
}
