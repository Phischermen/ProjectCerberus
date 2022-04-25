using UnityEngine;


public class OneJumpBonusStar : BonusStar
{
    public class OneJumpBonusStarStateData : BonusStarStateData
    {
        private OneJumpBonusStar _oneJumpBonusStar;
        private bool _oneJumpChanceAboutToBeMissed => booleans[0];

        public OneJumpBonusStarStateData(OneJumpBonusStar bonusStar, bool collected, bool unavailable,
            bool oneJumpChanceAboutToBeMissed) : base(bonusStar, collected, unavailable)
        {
            _oneJumpBonusStar = bonusStar;
            booleans[0] = oneJumpChanceAboutToBeMissed;
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

    public override StateData GetUndoData()
    {
        return new OneJumpBonusStarStateData(this, collected, unavailable, _oneJumpChanceAboutToBeMissed);
    }

    public override string GetStatusMessageForUI()
    {
        if (collected)
        {
            return "Bonus star collected!";
        }

        if (unavailable)
        {
            return "Bonus star can no longer be obtained.";
        }

        if (_oneJumpChanceAboutToBeMissed)
        {
            return "Bonus star must be collected in one jump!";
        }

        return "Bonus star disappears after you jump with Cerberus.";
    }

    public override void OnPlayerMadeMove()
    {
        if (collected)
        {
            SetFieldsToCollectedPreset();
        }
        else if (unavailable || manager.currentCerberus.isCerberusMajor && manager.currentCerberus.hasPerformedSpecial)
        {
            SetFieldsToUnavailablePreset();
        }
        else
        {
            _oneJumpChanceAboutToBeMissed = false;
            SetFieldsToUncollectedPreset();
        }
    }
}