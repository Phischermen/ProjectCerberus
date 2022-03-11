using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : PuzzleEntity
{
    public class GateUndoData : UndoData
    {
        public Gate gate;
        public bool open;
        public bool wantsToClose;

        public GateUndoData(Gate gate, bool open, bool wantsToClose)
        {
            this.gate = gate;
            this.open = open;
            this.wantsToClose = wantsToClose;
        }

        public override void Load()
        {
            // Stop animation
            if (gate.animationRoutine != null)
            {
                gate.StopCoroutine(gate.animationRoutine);
                gate.animationIsRunning = false;
                gate.animationMustStop = false;
            }

            if (open)
            {
                gate.OpenGate(false);
            }
            else
            {
                gate.CloseGate(false);
            }
            // This has to be set last because OpenGate() & CloseGate() affect _wantsToClose.
            gate._wantsToClose = wantsToClose;
        }
    }

    private bool _wantsToClose;
    [ShowInTileInspector] public bool open;
    private bool _lastOpen;
    
    [Tooltip("First element is the closed sprite.")]
    [SerializeField] private Sprite[] doorSprites;
    private Sprite openSprite => doorSprites[doorSprites.Length - 1];
    private Sprite closeSprite => doorSprites[0];
    private static int turnAnimationWasPlayed;
    
    private SpriteRenderer _spriteRenderer;

    public AudioSource openAudioSource;
    public AudioSource closeAudioSource;

    public Gate()
    {
        entityRules = "Can be opened and closed via switches and levers. Jumpable when closed and landable when open.";
        isStatic = true;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new GateUndoData(this, open, _wantsToClose);
        return undoData;
    }

    protected override void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (open)
        {
            OpenGate(false);
        }
        else
        {
            CloseGate(false);
        }
    }

    private new void Update()
    {
        base.Update();
        if (_wantsToClose)
        {
            // Set collision parameters in preparation for collision check.
            stopsPlayer = true;
            stopsBlock = true;
            if (!CollidesWithAny(currentCell.puzzleEntities))
            {
                CloseGate(true);
            }
            else
            {
                // Reset collision parameters.
                stopsBlock = false;
                stopsPlayer = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (_lastOpen != open)
        {
            if (open)
            {
                PlaySfx(openAudioSource);
            }
            else
            {
                PlaySfx(closeAudioSource);
            }
        }

        _lastOpen = open;
    }

    public void OpenGate()
    {
        OpenGate(true);
    }

    public void OpenGate(bool withAnimation)
    {
        _wantsToClose = false;
        // Play an open animation.
        if (withAnimation)
        {
            turnAnimationWasPlayed = manager.move;
            PlayAnimation(OpenGateAnimation(AnimationUtility.gateOpenDuration));
        }
        else
        {
            _spriteRenderer.sprite = openSprite;
        }

        SetFieldsToOpenPreset();
    }

    public void RequestCloseGate()
    {
        _wantsToClose = true;
    }

    private void CloseGate(bool withAnimation)
    {
        _wantsToClose = false;
        if (withAnimation)
        {
            turnAnimationWasPlayed = manager.move;
            PlayAnimation(CloseGateAnimation(AnimationUtility.gateCloseDuration));
        }
        else
        {
            _spriteRenderer.sprite = closeSprite;
        }

        SetFieldsToClosedPreset();
    }

    protected virtual void SetFieldsToOpenPreset()
    {
        open = true;
        stopsBlock = false;
        stopsPlayer = false;
        landableScore = 0;
        jumpable = false;
    }

    protected virtual void SetFieldsToClosedPreset()
    {
        open = false;
        stopsBlock = true;
        stopsPlayer = true;
        landableScore = -1;
        jumpable = true;
    }

    // Animations
    public IEnumerator OpenGateAnimation(float animationDuration)
    {
        animationIsRunning = true;
        var timeEllapsed = 0f;
        // Cycle through frames. 
        while (timeEllapsed < animationDuration && animationMustStop == false)
        {
            timeEllapsed += Time.fixedDeltaTime * (turnAnimationWasPlayed == manager.move ? 1f : 2f);
            var spriteIdx = (int) (timeEllapsed * (doorSprites.Length - 1) / animationDuration);
            _spriteRenderer.sprite = doorSprites[spriteIdx];
            yield return new WaitForFixedUpdate();
        }

        // Goto final frame.
        _spriteRenderer.sprite = openSprite;
        animationIsRunning = false;
        animationMustStop = false;
    }

    public IEnumerator CloseGateAnimation(float animationDuration)
    {
        animationIsRunning = true;
        var timeEllapsed = 0f;
        // Cycle through frames.
        while (timeEllapsed < animationDuration && animationMustStop == false)
        {
            timeEllapsed += Time.fixedDeltaTime * (turnAnimationWasPlayed == manager.move ? 1f : 2f);
            var spriteIdx = (doorSprites.Length - 1) -
                            (int) (timeEllapsed * (doorSprites.Length - 1) / animationDuration);
            _spriteRenderer.sprite = doorSprites[spriteIdx];
            yield return new WaitForFixedUpdate();
        }

        // Goto final frame.
        _spriteRenderer.sprite = closeSprite;
        animationIsRunning = false;
        animationMustStop = false;
    }
}