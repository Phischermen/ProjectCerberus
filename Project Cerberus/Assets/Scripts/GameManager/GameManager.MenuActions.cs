using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager
{
    // Replay Level/Proceed Game
        public void UndoLastMove()
        {
            _puzzleContainer.UndoLastMove();
            gameplayEnabled = true;
            // Note: Timer is reset via GameManagerStateData.Load()
        }
    
        public void ReplayLevel()
        {
            _puzzleContainer.UndoToFirstMove();
            // NOTE: Only master client can send the following command.
            SendRPCReplayLevel();
            gameplayEnabled = true;
            // Repopulate availableCerberus
            RepopulateAvailableCerberus();
            // Note: Timer is reset via GameManagerStateData.Load()
        }
    
        public void ProceedToNextLevel()
        {
            var nextScene = levelSequence.GetSceneBuildIndexForLevel(currentLevel + 1, andPlayMusic: true);
            if (nextScene == -1)
            {
                Debug.Log($"Could not find next level {currentLevel + 1}");
            }
            else
            {
                currentLevel += 1;
                SceneManager.LoadScene(nextScene);
            }
        }
}