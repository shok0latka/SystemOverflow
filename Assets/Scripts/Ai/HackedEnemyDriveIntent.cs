using UnityEngine;

[System.Serializable]
public struct HackedEnemyDriveIntent
{
    public float MoveRight;
    public float MoveForward;
    public float Turn;

    public HackedEnemyDriveIntent(float moveRight, float moveForward, float turn)
    {
        MoveRight = Mathf.Clamp(moveRight, -1f, 1f);
        MoveForward = Mathf.Clamp(moveForward, -1f, 1f);
        Turn = Mathf.Clamp(turn, -1f, 1f);
    }

    public static HackedEnemyDriveIntent Clamp(HackedEnemyDriveIntent intent)
    {
        intent.MoveRight = Mathf.Clamp(intent.MoveRight, -1f, 1f);
        intent.MoveForward = Mathf.Clamp(intent.MoveForward, -1f, 1f);
        intent.Turn = Mathf.Clamp(intent.Turn, -1f, 1f);
        return intent;
    }
}
