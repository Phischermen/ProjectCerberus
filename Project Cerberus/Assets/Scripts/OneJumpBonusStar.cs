using UnityEngine;


public class OneJumpBonusStar : BonusStar
{
    public class OneJumpBonusStarUndoData : BonusStarUndoData
    {
        private OneJumpBonusStar _oneJumpBonusStar;
        private bool _oneJumpChanceAboutToBeMissed;

        public OneJumpBonusStarUndoData(OneJumpBonusStar bonusStar, bool collected, bool unavailable,
            bool oneJumpChanceAboutToBeMissed) : base(bonusStar, collected, unavailable)
        {
            _oneJumpBonusStar = bonusStar;
            _oneJumpChanceAboutToBeMissed = oneJumpChanceAboutToBeMissed;
        }

        public override void Load()
        {
            base.Load();
            _oneJumpBonusStar._oneJumpChanceAboutToBeMissed = _oneJumpChanceAboutToBeMissed;
        }
    }

    private bool _oneJumpChanceAboutToBeMissed;

    public OneJumpBonusStar()
    {
        entityRules = "A bonus bonusStar. Disappears after CerberusMajor moves.";
    }

    public override UndoData GetUndoData()
    {
        return new OneJumpBonusStarUndoData(this, collected, unavailable, _oneJumpChanceAboutToBeMissed);
    }

    public override string GetStatusMessageForUI()
    {
        if (collected)
        {
            return "Bonus star collected!\n";
        }

        if (unavailable)
        {
            return "Bonus star can no longer be obtained.\n";
        }

        if (_oneJumpChanceAboutToBeMissed)
        {
            return "Bonus star must be collected on this turn!\n";
        }

        return "Bonus star disappears after you make a move with Cerberus.\n";
    }

    public override void OnPlayerMadeMove()
    {
        if (collected)
        {
            SetFieldsToCollectedPreset();
        }
        else if (unavailable || manager.currentCerberus.isCerberusMajor)
        {
            if (_oneJumpChanceAboutToBeMissed)
            {
                SetFieldsToUnavailablePreset();
            }
            else
            {
                // If player does not collect star on their next move, they'll lose it.
                _oneJumpChanceAboutToBeMissed = true;
            }
        }
        else
        {
            _oneJumpChanceAboutToBeMissed = false;
            SetFieldsToUncollectedPreset();
        }
    }
}