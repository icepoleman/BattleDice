using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

public class AtlasLoader
{
    private static AtlasLoader instance;

    public static AtlasLoader Instance => instance ??= new AtlasLoader();

    private SpriteAtlas atlas_skill;
    private SpriteAtlas atlas_buff;
    private SpriteAtlas atlas_Dice;
    private bool isInitialized;

    private AtlasLoader() { }

    public void Init()
    {
        if (isInitialized)
        {
            return;
        }

        atlas_skill = Resources.Load<SpriteAtlas>("Atlas/Skills");
        atlas_buff = Resources.Load<SpriteAtlas>("Atlas/Buffs");
        atlas_Dice = Resources.Load<SpriteAtlas>("Atlas/Dice");
        isInitialized = true;
    }

    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            Init();
        }
    }

    public Sprite GetSkillSprite(string spriteName)
    {
        EnsureInitialized();
        return atlas_skill != null ? atlas_skill.GetSprite(spriteName) : null;
    }

    public Sprite GetBuffSprite(string spriteName)
    {
        string buffspName = $"buff_{spriteName}";
        EnsureInitialized();
        return atlas_buff != null ? atlas_buff.GetSprite(buffspName) : null;
    }

    public Sprite GetDiceSprite(int diceNum)
    {
        string spriteName = $"dice_{diceNum}";
        EnsureInitialized();
        return atlas_Dice != null ? atlas_Dice.GetSprite(spriteName) : null;
    }

    public List<Sprite> GetAllDiceSprites()
    {
        EnsureInitialized();
        List<Sprite> diceSprites = new List<Sprite>();
        for (int i = 0; i <= 6; i++)
        {
            diceSprites.Add(atlas_Dice.GetSprite($"dice_{i}"));
        }
        return diceSprites;
    }

}
