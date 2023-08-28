using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;
    public Data data;
    public int countCoins;

    private void OnEnable()
    {
        PlayerData.Instance = this;

        data = new Data();
        countCoins = 0;
    }

    public string PlayerToString()
    {
        return JsonUtility.ToJson(PlayerData.Instance.data);
    }
}

[System.Serializable]
public class Data
{
    public int myNumberInRoom;
    public Color helmetColor;
}
