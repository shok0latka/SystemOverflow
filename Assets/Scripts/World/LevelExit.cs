using UnityEngine;

// Сразу написал класс для финальной цели, пусть будет.

public class LevelExit : Interactable
{
    public override void Interact()
    {
        GameStateManager.Instance.WinLevel();
    }
}
