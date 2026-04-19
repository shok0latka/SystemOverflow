using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;
    public PlayerSaveData player = new();
    public List<EnemyRuntimeSaveData> enemies = new();
}
