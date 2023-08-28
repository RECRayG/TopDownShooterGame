using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;

public class BulletPool : MonoBehaviourPunCallbacks//, IPunObservable//, IOnEventCallback
{
    //public static BulletPool Instance;

    public PhotonView photonViewPool;

    public GameObject bulletPrefab;
    public int initialPoolSize = 30;

    public List<BulletTrail> bulletPool = new List<BulletTrail>();
    public List<PoolData> bulletPoolData = new List<PoolData>();
    public BulletTrail[] temp;
    public Transform[] temp2;

    private void Start()
    {
        /*if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;*/




        //if (PhotonNetwork.IsMasterClient)
        //{
        //CreateBullets();
        //}
    }

    /*private void Update()
    {
        if (bulletPool != null && bulletPoolData != null &&
            bulletPool.Count == bulletPoolData.Count &&
            bulletPool.Count > 0 && bulletPoolData.Count > 0)
        {
            SyncPoolData();
        }
    }*/

    /*private void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient &&
            PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            // Разослать событие передачи данных о состоянии пула
            string[] serializedBulletPool = bulletPoolData.Select(data => JsonUtility.ToJson(data)).ToArray();

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(3, serializedBulletPool, options, sendOptions);

            SyncPoolData();
        }
    }*/

    /*public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 3:
                string[] deserializedBulletPool = (string[]) photonEvent.CustomData;

                List<PoolData> poolDataAnotherClient = deserializedBulletPool
                                                        .ToList()
                                                        .Select(dataString => JsonUtility.FromJson<PoolData>(dataString))
                                                        .ToList();

                bulletPoolData = poolDataAnotherClient;

                SyncPoolData();

                break;
        }
    }*/

    /*private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);   
    }*/

    public void SyncPoolData()
    {
        if (bulletPool.Count != bulletPoolData.Count)
            return;

        for (int i = 0; i < bulletPoolData.Count; i++)
        {
            //bulletPool[i].startPosition = bulletPoolData[i].startPosition;
            //bulletPool[i].progress = bulletPoolData[i].progress;
            bulletPool[i].gameObject.SetActive(bulletPoolData[i].state);
            //bulletPool[i].SetTargetPosition(bulletPoolData[i].targetPosition.x, bulletPoolData[i].targetPosition.y, bulletPoolData[i].startPosition);
            
            //bulletPool[i].targetPosition = bulletPoolData[i].targetPosition;
        }
    }

    public void CreateBullets()
    {
        photonViewPool.ObservedComponents = new List<Component>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateBullet(i);
        }
    }

    private BulletTrail CreateBullet(int index)
    {
        BulletTrail bullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity).GetComponent<BulletTrail>();

        bullet.transform.SetParent(this.transform, false);
        bullet.gameObject.name = bulletPrefab.name + "_" + index;
        bullet.gameObject.SetActive(false);
        bulletPool.Add(bullet);

        

        return bullet;
    }

    [PunRPC]
    public void GetBullet(Vector3 startPosition, Quaternion rotation, float x, float y)
    {
        //if (!PhotonNetwork.IsMasterClient) return;

        BulletTrail bullet = GetInactiveBullet();

        if (bullet != null)
        {
            bullet.transform.position = startPosition;
            bullet.transform.rotation = rotation;
            bullet.gameObject.SetActive(true);

            //bullet.SetTargetPosition(x, y, startPosition, rotation);

            bullet.photonViewBullet.RPC("synchronized_SetTargetPosition", RpcTarget.All, x, y, startPosition, rotation);

            //int bulletViewID = bullet.GetPhotonView().ViewID;
            //photonView.RPC("UseBullet", info.Sender, bulletViewID);
        }
    }


    /*[PunRPC]
    public void GetBullet(Vector3 startPosition, Quaternion rotation, float x, float y, PhotonMessageInfo info)
    {
        //if (!PhotonNetwork.IsMasterClient) return;

        GameObject bullet = GetInactiveBullet();
        if (bullet != null)
        {
            bullet.transform.position = startPosition;
            bullet.transform.rotation = rotation;
            bullet.SetActive(true);

            bullet.GetComponent<PhotonView>().RPC("synchronized_SetTargetPosition", RpcTarget.AllBuffered, x, y, startPosition);

            //int bulletViewID = bullet.GetPhotonView().ViewID;
            //photonView.RPC("UseBullet", info.Sender, bulletViewID);
        }
    }*/

    private BulletTrail GetInactiveBullet()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].gameObject.activeInHierarchy)
            {
                /*Dictionary<int, BulletTrail> result = new Dictionary<int, BulletTrail>();
                result.Add(i, bulletPool[i]);
                return result;*/

                //PoolData temp = bulletPoolData.Find(data => data.index == i);
                return bulletPool[i];
            }
        }

        /*foreach (var bullet in bulletPool)
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                return bullet;
            }
        }*/
        return CreateBullet(bulletPool.Count + 1);
    }

    /*public void UseBullet(BulletTrail bulletTrail)
    {
        bulletPool.Find(b => b.gameObject.name.Equals(bulletTrail.gameObject.name)).gameObject.SetActive(false);

        //bulletTrail.gameObject.SetActive(false);

        *//*int index = bulletPool.IndexOf(bulletTrail);
        PoolData temp = bulletPoolData[index];
        temp.state = false;
        bulletPoolData[index] = temp;

        int index = bulletPool.IndexOf(bulletTrail);
        if (bulletIsActive.ContainsKey(index))
        {
            bulletIsActive[index] = false;
        }*//*

        //bulletTrail.gameObject.SetActive(false);
    }*/

    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Мы владелец
        if (stream.IsWriting)
        {
            string[] serializedBulletPool = bulletPoolData.Select(data => JsonUtility.ToJson(data)).ToArray();

            stream.SendNext(serializedBulletPool);
        } // Мы принимающая сторона
        else
        {
            string[] deserializedBulletPool = (string[]) stream.ReceiveNext();

            List<PoolData> poolDataAnotherClient = deserializedBulletPool
                                                    .ToList()
                                                    .Select(dataString => JsonUtility.FromJson<PoolData>(dataString))
                                                    .ToList();

            bulletPoolData = poolDataAnotherClient;
        }
    }*/

    [PunRPC]
    public void UseBullet(int bulletViewID)
    {
        //bulletPool.Find(b => b.gameObject.name.Equals(bulletName)).gameObject.SetActive(false);

        GameObject bullet = PhotonView.Find(bulletViewID).gameObject;
        bullet.SetActive(false);
    }

}

[Serializable]
public class PoolData
{
    public int index;
    public bool state;
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public double progress;
}