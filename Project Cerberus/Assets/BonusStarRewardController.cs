using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BonusStarRewardController : MonoBehaviour
{
    int _starsEarned = 0;
    int _starsAvailable = 0;
    private bool _starsAreDisplaying;
    public AudioClip rewardMusic;
    private Gate _gate;
    private BonusStar _star;

    private void Start()
    {
        _star = FindObjectOfType<BonusStar>();
        _gate = FindObjectOfType<Gate>();
        var levelSequence = MainMenuController.chosenLevelSequence;
        foreach (var world in levelSequence.worlds)
        {
            foreach (var level in world.levels)
            {
                // Count how many stars have been earned.
                // Do not include the scene that takes players to the main menu.
                if (level.idxForInstancing == (int) Scenum.Scene.MainMenu) continue;
                // Set fields for LevelChoice.


                if (level.isGameplay)
                {
                    // Increment current level index.
                    var settings = PlayerPrefs.GetString(levelSequence.name + level.idxForInstancing,
                        TrophyData.initialTrophyCode);
                    _starsAvailable += 1;
                    if (settings[2] == TrophyData.goldCode)
                    {
                        _starsEarned += 1;
                    }
                }
            }
        }
    }

    public void DisplayStars()
    {
        if (!_starsAreDisplaying && !_gate.open)
        {
            StartCoroutine(DisplayStarsRoutine());
        }
    }

    IEnumerator DisplayStarsRoutine()
    {
        _starsAreDisplaying = true;
        var earned = Mathf.Min(_starsAvailable, _starsEarned + (_star.collected ? 1 : 0));
        for (int i = 0; i < earned; i++)
        {
            var popup = TextPopup.Create("<sprite index=28>", Color.yellow, true);

            popup.transform.position = _gate.transform.position;
            popup.PlayRiseAndFadeAnimation();
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
        var popup1 = TextPopup.Create($"{earned}/{_starsAvailable}", Color.yellow);
        // Adjust sizing to allow message to be seen.
        var textMeshPro = popup1.GetComponent<TextMeshPro>();
        textMeshPro.enableAutoSizing = false;
        textMeshPro.enableWordWrapping = false;
        popup1.transform.position = _gate.transform.position;
        popup1.PlayRiseAndFadeAnimation();

        if (_starsAvailable == (earned) && _star.collected)
        {
            _gate.OpenGate();
            DiskJockey.PlayTrack(null);
        }

        _starsAreDisplaying = false;
    }

    public void PlayMusic()
    {
        DiskJockey.PlayTrack(rewardMusic);
        PlayerPrefs.SetInt("UnlockedAltSkin", 1);
        PlayerPrefs.Save();
    }
}