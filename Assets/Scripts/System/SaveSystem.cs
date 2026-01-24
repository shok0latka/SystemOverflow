using UnityEngine;
using System.IO;

// Пригодится, но пока что юзлес


[System.Serializable]
public class SaveData
{
    public float posX;
    public float posY;
}

public class SaveSystem : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/save.json";
    }

    public void SavePlayer(GameObject player)
    {
        SaveData data = new SaveData
        {
            posX = player.transform.position.x,
            posY = player.transform.position.y
        };

        File.WriteAllText(savePath, JsonUtility.ToJson(data));
    }

    public void LoadPlayer(GameObject player)
    {
        if (!File.Exists(savePath)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(
            File.ReadAllText(savePath)
        );

        player.transform.position = new Vector2(data.posX, data.posY);
    }
}
