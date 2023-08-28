using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using ExitGames.Client.Photon;
using System.Globalization;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static GameManager Instance;



    public GameObject playerPrefab;
    public GameObject whaitingPanel;
    public GameObject sceneCamera;

    public GameObject joystickPrefab;
    public Canvas joystickPrefabParent;
    public Joystick playerMovementJoystick;
    public Joystick playerAimJoystick;

    public Text pingText;
    public string loadSceneNameAfterLeftGame = "Loading";

    private const string PLAYER_CONNECTED = "Игрок №{0:D1} подключился к комнате";
    private const string PLAYER_DISCONNECTED = "Игрок №{0:D1} покинул комнату";

    public GameObject playerFeed;
    public GameObject greedFeed;

    //private Dictionary<Color, bool> helmetColors;
    private Dictionary<int, Color> helmetColors;

    [SerializeField]
    public PhotonView photonViewManage;

    //private GenerateDistinctNonRepeatingColors availableColors;

    private Player myPlayer;
    //public Color colorBuffered;

    public GameObject bulletTrailPrefab; // Префаб BulletTrail.

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        /*if(photonViewManage.IsMine)
            photonViewManage.RPC("synchronized_GenerateColors", RpcTarget.AllBuffered);*/

        /*generatedColors.GetColors().ForEach(color => {
            helmetColors.Add(color, false);
        });*/

        /*if (photonViewManage.IsMine)
        {
            whaitingPanel = GameObject.FindGameObjectWithTag("WhaitPanel");
            joystickPrefabParent = whaitingPanel.transform.parent.gameObject.GetComponent<Canvas>();
            pingText = GameObject.FindGameObjectWithTag("PingText").GetComponent<Text>();
        }*/

        GameObject joystick = Instantiate(joystickPrefab, Vector3.zero, Quaternion.identity);
        joystick.transform.SetParent(joystickPrefabParent.transform, false);
        playerMovementJoystick = joystick.transform.Find("Floating Joystick Move").GetComponent<Joystick>();
        playerAimJoystick = joystick.transform.Find("Floating Joystick Rotation").GetComponent<Joystick>();


        if (photonViewManage.IsMine)
        {
            whaitingPanel.SetActive(true);
            playerMovementJoystick.gameObject.SetActive(false);
            playerAimJoystick.gameObject.SetActive(false);


        }

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnPlayer();
        }
    }

    private void Update()
    {
        if (!photonViewManage.IsMine)
            return;

        pingText.text = "Пинг: " + PhotonNetwork.GetPing();

        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            whaitingPanel.SetActive(false);
            playerMovementJoystick.gameObject.SetActive(true);
            playerAimJoystick.gameObject.SetActive(true);
        }
        else
        {
            whaitingPanel.SetActive(true);
            playerMovementJoystick.gameObject.SetActive(false);
            playerAimJoystick.gameObject.SetActive(false);
        }
    }

    public Player SpawnPlayer()
    {
        float randomValue = Random.Range(-1f, 1f);
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector2(this.transform.position.x * randomValue, this.transform.position.y * randomValue), Quaternion.identity, 0);
        myPlayer = player.GetComponent<Player>();
        sceneCamera.SetActive(true);

        return myPlayer;
    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Destroy(myPlayer.photonView);
        PhotonNetwork.LoadLevel(loadSceneNameAfterLeftGame);
    }

    public void SendSyncColors(Photon.Realtime.Player player)
    {
        RaiseEventOptions options = new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(1, JsonUtility.ToJson(GenerateColors.Instance.colorList), options, sendOptions);
    }

    public void SendSyncColorsLeave(Photon.Realtime.Player player)
    {
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(2, JsonUtility.ToJson(GenerateColors.Instance.colorList), options, sendOptions);
    }

    /*public void SendSyncBulletPool(Photon.Realtime.Player player)
    {
        RaiseEventOptions options = new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        string[] serializedBulletPool = BulletPool.Instance.bulletPoolData.Select(data => JsonUtility.ToJson(data)).ToArray();

        PhotonNetwork.RaiseEvent(2, serializedBulletPool, options, sendOptions);
    }*/

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 1:
                string colorsString = (string) photonEvent.CustomData;
                GenerateColors.Instance.colorList = new ColorList();
                GenerateColors.Instance.colorList = JsonUtility.FromJson<ColorList>(colorsString);
                SpawnPlayer();
                break;
            /*case 2:
                string[] deserializedBulletPool = (string[]) photonEvent.CustomData;

                List<PoolData> poolDataAnotherClient = deserializedBulletPool
                                                        .ToList()
                                                        .Select(dataString => JsonUtility.FromJson<PoolData>(dataString))
                                                        .ToList();

                BulletPool.Instance.bulletPoolData = new List<PoolData>();
                BulletPool.Instance.bulletPoolData = poolDataAnotherClient;

                BulletPool.Instance.bulletPool = new List<BulletTrail>(BulletPool.Instance.initialPoolSize);

                //BulletPool.Instance.SyncPoolData();

               *//* foreach(BulletTrail bulletTrail in BulletPool.Instance.bulletPool)
                {
                    PhotonView.Find(bulletTrail.photonViewBullet.ViewID).gameObject.transform.SetParent(BulletPool.Instance.gameObject.transform);
                }*//*

                break;*/
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player player)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //SendSyncBulletPool(player);
            SendSyncColors(player);
        }

        GameObject tempObj = Instantiate(playerFeed, new Vector2(0,0), Quaternion.identity);
        tempObj.transform.SetParent(greedFeed.transform, false);
        Text tempText = tempObj.GetComponent<Text>();
        tempText.text = string.Format(PLAYER_CONNECTED, PhotonNetwork.CurrentRoom.PlayerCount);
        tempText.color = Color.white;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player player)
    {
        if (photonViewManage.IsMine)
        {
            GenerateColors.Instance.RemoveLastUsedColor();

            //SendSyncColorsLeave(player);
        }

        GameObject tempObj = Instantiate(playerFeed, new Vector2(0, 0), Quaternion.identity);
        tempObj.transform.SetParent(greedFeed.transform, false);
        Text tempText = tempObj.GetComponent<Text>();
        tempText.text = string.Format(PLAYER_DISCONNECTED, PhotonNetwork.CurrentRoom.PlayerCount + 1);
        tempText.color = Color.red;
    }

    
}
