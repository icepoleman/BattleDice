using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CharacterView
{
    private Image img_character;
    public override void Init()
    {
        img_character = gameObject.transform.Find("characterImage").GetComponent<Image>();
        base.Init();
    }
    public void SetEnemySprite(Sprite enemySprite)
    {
        img_character.sprite = enemySprite;
        img_character.SetNativeSize();
        PlayAnim("idle");
    }
}
