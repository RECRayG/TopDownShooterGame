using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateColors : MonoBehaviourPunCallbacks/*, IPunObservable*/
{
    public static GenerateColors Instance;

    public PhotonView viewColors;

    //private static List<Color> generatedColors = new List<Color>();
    private float colorDistanceThreshold = 0.2f; // Порог расстояния между цветами
    public ColorList colorList;
    public Color[] colL;
    public bool[] boolL;

    public Color currentColor;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (PhotonNetwork.IsMasterClient)
        {
            colorList = new ColorList();
            colorList.colors = new List<Color>();
            colorList.isUsedList = new List<bool>();

            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                Color newColor = GenerateDistinctNonRepeatingColor();
                colorList.colors.Add(newColor);
                colorList.isUsedList.Add(false);
                Debug.Log("Сгенерированный цвет " + i + ": " + newColor);
            }

            boolL = new bool[colorList.isUsedList.Count];
            boolL = colorList.isUsedList.ToArray();

            colL = new Color[colorList.colors.Count];
            colL = colorList.colors.ToArray();

            //string colorsString = JsonUtility.ToJson(colorList);
            //viewColors.RPC("synchronized_Colors", RpcTarget.AllBuffered, colorsString);
        }

    }

    [PunRPC]
    public void synchronized_Colors(string colorsString)
    {
        if (colorsString != null && !colorsString.Equals(""))
        {
            //Debug.Log(colorsString);
            colorList = JsonUtility.FromJson<ColorList>(colorsString);

            boolL = new bool[colorList.isUsedList.Count];
            boolL = colorList.isUsedList.ToArray();

            colL = new Color[colorList.colors.Count];
            colL = colorList.colors.ToArray();
        }
    }

    /*[PunRPC]
    public void GetRandomColor(string colorsString, int photonViewId)
    {
        if (colorsString != null && !colorsString.Equals(""))
        {
            Debug.Log(colorsString);
            colorList = JsonUtility.FromJson<ColorList>(colorsString);

            for (int i = 0; i < colorList.colors.Count; i++)
            {
                if (!colorList.isUsedList[i])
                {
                    colorList.isUsedList[i] = true;

                    boolL = new bool[colorList.isUsedList.Count];
                    boolL = colorList.isUsedList.ToArray();

                    colL = new Color[colorList.colors.Count];
                    colL = colorList.colors.ToArray();

                    PhotonNetwork.GetPhotonView(photonViewId).RPC("SetHelmetColor", RpcTarget.AllBuffered, colorList.colors[i].r, colorList.colors[i].g, colorList.colors[i].b, colorList.colors[i].a);
                    //viewColors.RPC("synchronized_Colors", RpcTarget.AllBuffered, colorsString);
                    return;
                }
            }
        }

        // Если каким-то образом всё занято, но появился ещё игрок, то вернуть белый цвет по-умолчанию
        //Color.white;
        PhotonNetwork.GetPhotonView(photonViewId).RPC("SetHelmetColor", RpcTarget.AllBuffered, 1f, 1f, 1f, 1f);
    }*/

    public Color GetRandomColor()
    {
        if (colorList != null)
        {
            for (int i = 0; i < colorList.colors.Count; i++)
            {
                if (!colorList.isUsedList[i])
                {
                    colorList.isUsedList[i] = true;
                    string colorsString = JsonUtility.ToJson(colorList);
                    viewColors.RPC("synchronized_Colors", RpcTarget.AllBuffered, colorsString);
                    currentColor = colorList.colors[i];
                    return colorList.colors[i];
                }
            }
        }

        // Если каким-то образом всё занято, но появился ещё игрок, то вернуть белый цвет по-умолчанию
        return Color.white;
    }

    public void RemoveLastUsedColor()
    {
        if (colorList != null)
        {
            for (int i = colorList.colors.Count - 1; i >= 0; i--)
            {
                if (colorList.isUsedList[i] && colorList.colors[i].Equals(currentColor))
                {
                    colorList.isUsedList[i] = false;
                    string colorsString = JsonUtility.ToJson(colorList);
                    viewColors.RPC("synchronized_Colors", RpcTarget.AllBuffered, colorsString);
                    return;
                }
            }
        }
    }

    /*public GenerateDistinctNonRepeatingColors(int count)
    {
        int numberOfColors = count; // Количество требуемых цветов

        for (int i = 0; i < numberOfColors; i++)
        {
            Color newColor = GenerateDistinctNonRepeatingColor();
            colorList.colors.Add(newColor);
            Debug.Log("Сгенерированный цвет " + i + ": " + newColor);
        }
    }*/

    private Color GenerateDistinctNonRepeatingColor()
    {
        Color newColor;
        bool colorIsDistinct = false;

        do
        {
            newColor = GenerateRandomColor();
            colorIsDistinct = IsColorDistinct(newColor);
        }
        while (!colorIsDistinct);

        return newColor;
    }

    private Color GenerateRandomColor()
    {
        float hue = Random.Range(0f, 1f); // Случайное значение оттенка (0-1)
        float saturation = Random.Range(0.4f, 1f); // Случайное значение насыщенности (0.4-1)
        float value = Random.Range(0.7f, 1f); // Случайное значение яркости (0.7-1)

        return Color.HSVToRGB(hue, saturation, value);
    }

    private bool IsColorDistinct(Color newColor)
    {
        foreach (Color existingColor in colorList.colors)
        {
            if (ColorDistance(newColor, existingColor) < colorDistanceThreshold)
            {
                return false;
            }
        }
        return true;
    }

    private float ColorDistance(Color color1, Color color2)
    {
        // Вычисление евклидового расстояния между цветами в пространстве RGB
        return Mathf.Sqrt(Mathf.Pow(color1.r - color2.r, 2) + Mathf.Pow(color1.g - color2.g, 2) + Mathf.Pow(color1.b - color2.b, 2));
    }

    // МОЖНО УБРАТЬ, НО ПРОВЕРИТЬ ПОТОМ
    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Мы владелец
        if (stream.IsWriting)
        {
            stream.SendNext(JsonUtility.ToJson(colorList));
        } // Мы принимающая сторона
        else
        {
            colorList = JsonUtility.FromJson<ColorList>((string) stream.ReceiveNext());

            boolL = new bool[colorList.isUsedList.Count];
            boolL = colorList.isUsedList.ToArray();

            colL = new Color[colorList.colors.Count];
            colL = colorList.colors.ToArray();
        }
    }*/
}

public class ColorList
{
    public List<Color> colors;
    public List<bool> isUsedList;
}