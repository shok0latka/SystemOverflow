using System;

[Serializable]
public class EnemyRuntimeSaveData
{
    public string saveId;
    public float posX;
    public float posY;
    public string state;
    public int patrolIndex;
    public float suspicion;
    public float timeSinceSeen;
    public float attackTimer;
    public float searchTimer;
    public float hackedTimer;
    public float hackDuration;
    public float lastKnownX;
    public float lastKnownY;
}
