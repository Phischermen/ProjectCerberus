using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : BasicBlock
{
    public class WoodBlockUndoData : BasicBlockUndoData
    {
        public WoodBlock wood;
        public bool burned;

        public WoodBlockUndoData(WoodBlock wood, Vector2Int position, bool burned, bool inHole) : base(wood, position, inHole)
        {
            this.wood = wood;
            this.burned = burned;
        }

        public override void Load()
        {
            base.Load();
            wood.SetFieldsToShotPreset(burned);
            wood._spriteRenderer.sprite = burned ? wood.destroyedSprite : wood.wholeSprite;
        }
    }

    [SerializeField] private Sprite wholeSprite;
    [SerializeField] private Sprite destroyedSprite;
    private SpriteRenderer _spriteRenderer;
    public AudioSource woodHitSfx;

    protected WoodBlock()
    {
        entityRules = "Wood blocks burn up when they are shot by Kahuna. The ashes make a nice cushion for Cerberus.";
        pushableByFireball = false;
        interactsWithFireball = true;
    }

    private new void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnShotByKahuna()
    {
        SetFieldsToShotPreset(true);
    }

    public override void OnShotByKahunaVisually()
    {
        spriteRenderer.sprite = destroyedSprite;
        woodHitSfx.Play();
    }

    public virtual void SetFieldsToShotPreset(bool shot)
    {
        if (shot)
        {
            stopsPlayer = false;
            stopsBlock = false;
            isBlock = false;
            landableScore = 1;
            jumpable = false;
            interactsWithFireball = false;
            pushableByStandardMove = false;
            pushableByJacksMultiPush = false;
            pushableByJacksSuperPush = false;
        }
        else
        {
            stopsPlayer = true;
            stopsBlock = true;
            isBlock = true;
            landableScore = -1;
            jumpable = true;
            interactsWithFireball = true;
            pushableByStandardMove = true;
            pushableByJacksMultiPush = true;
            pushableByJacksSuperPush = true;
        }
    }

    public override UndoData GetUndoData()
    {
        var undoData = new WoodBlockUndoData(this, position, burned: !stopsPlayer, inHole);
        return undoData;
    }
}