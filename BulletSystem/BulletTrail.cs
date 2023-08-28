using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;

public class BulletTrail : MonoBehaviourPunCallbacks//, IPunObservable
{
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public float progress;
    public AnimationCurve WidthCurve;
    //public TrailRenderer trail;

    public PhotonView photonViewBullet;

    [SerializeField]
    private float speed = 40f;

    [SerializeField]
    private float MAX_bulletDamage = 20f;
    [SerializeField]
    private float MIN_bulletDamage = 10f;

    private BulletPool bulletPool;
    //public bool isMoving;


    public Texture2D trailTexture;
    public LineRenderer lineRenderer;

    public float lifeTime = 2.5f;

    private void Start()
    {
        //trail.widthCurve = WidthCurve;
        //if (trail == null)
        //trail = GetComponentInChildren<TrailRenderer>();

        bulletPool = transform.parent.gameObject.GetComponent<BulletPool>();

        //isMoving = false;
        //startPosition = transform.position.ChangeAxis(Axis.Z, -1);

        lineRenderer.material.SetTexture("_MainTex", trailTexture);
        lineRenderer.material.SetFloat("_Speed", speed);
        lineRenderer.positionCount = 2;
        lineRenderer.widthCurve = WidthCurve;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, targetPosition);
    }

    private void Update()
    {
        //if (this.GetComponent<PhotonView>().IsMine)
        //{
        if (/*photonViewBullet != null && */photonViewBullet.IsMine/* && isMoving*/)
        {
            photonViewBullet.RPC("Coroutine_DestroyAfterLifeTime", RpcTarget.All);

            //photonViewBullet.RPC("synchronized_BulletMovement", RpcTarget.All, startPosition, targetPosition);
            progress += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

            
            photonViewBullet.RPC("synchronized_TrailMovement", RpcTarget.All, startPosition, transform.position);



            /*if (transform.position == targetPosition)
            {
                isMoving = false;
            }*/
        }
            
        //if(gameObject.activeInHierarchy)    
            //transform.Translate((targetPosition - transform.position).normalized * Time.deltaTime * speed);
        //}
       
        
        /*if (this.GetComponent<PhotonView>().IsMine)
        {
            if (transform.position == targetPosition)
            {
                this.GetComponent<PhotonView>().RPC("synchronized_DestroyObject", RpcTarget.AllBuffered);
            }
        }*/
    }

    [PunRPC]
    public void Coroutine_DestroyAfterLifeTime()
    {
        StartCoroutine(DestroyAfterLifeTime());
    }

    IEnumerator DestroyAfterLifeTime()
    {
        Vector3 tempPos = transform.position;

        yield return new WaitForSeconds(lifeTime);

        if (photonViewBullet.IsMine/* || PhotonNetwork.IsMasterClient*/)
        {
            // Если разница пройденного расстояния за время lifeTime больше, чем 0.01, то позиция изменилась
            if (Vector3.Distance(tempPos, transform.position) > 0.01f) {}
            // Если позиция не изменилась, значит, пуля зависла в воздухе и не двигается, поэтому убираем её
            else
            {
                bulletPool.photonViewPool.RPC("UseBullet", RpcTarget.All, photonViewBullet.ViewID);
            }
            
            //this.gameObject.SetActive(false);
        }
        /*else
        {
            this.gameObject.SetActive(false);
        }*/

        /*if (photonViewBullet.IsMine || PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }*/
    }

    [PunRPC]
    public void synchronized_SetTargetPosition(float x, float y, Vector3 startPosition, Quaternion rotation)
    {
        targetPosition = new Vector3(x, y, -1f);
        transform.rotation = rotation;
        //transform.position = startPosition;
        this.startPosition = startPosition.ChangeAxis(Axis.Z, -1);
        progress = 0f;
        //isMoving = true;
    }

    /*public void SetTargetPosition(float x, float y, Vector3 startPosition, Quaternion rotation)
    {
        targetPosition = new Vector3(x, y, -1f);
        transform.rotation = rotation;
        //transform.position = startPosition;
        this.startPosition = startPosition.ChangeAxis(Axis.Z, -1);
        progress = 0f;
        //isMoving = true;
    }*/

    [PunRPC]
    private void synchronized_TrailMovement(Vector3 startPos, Vector3 targetPos)
    {
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, targetPos);
    }

    /*[PunRPC]
    private void synchronized_BulletMovement(Vector3 startPos, Vector3 targetPos)
    {
        progress += (float) PhotonNetwork.Time * speed;
        transform.position = Vector3.Lerp(startPos, targetPos, progress);
    }*/

    /*[PunRPC]
    public void synchronized_SetTargetPosition(float x, float y, Vector3 startPosition)
    {
        targetPosition = new Vector3(x, y, -1f);
        this.startPosition = startPosition.ChangeAxis(Axis.Z, -1);
        progress = 0f;
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!gameObject.activeSelf || !photonViewBullet.IsMine)
            return;

        if(photonViewBullet.IsMine)
        {
            bulletPool.photonViewPool.RPC("UseBullet", RpcTarget.All, photonViewBullet.ViewID);


            PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();

            if (targetView != null && (!targetView.IsMine || targetView.IsRoomView))
            {
                if (collision.tag == "Player")
                {
                    // Логика нанесения урона
                    float damageAmount = Random.Range(MIN_bulletDamage, MAX_bulletDamage);

                    targetView.RPC("DealDamage", RpcTarget.AllBuffered, damageAmount);
                }
            }

            //this.gameObject.SetActive(false);
            //bulletPool.photonViewPool.RPC("UseBullet", RpcTarget.All, gameObject.name);
        }
        /*else
        {
            this.gameObject.SetActive(false);
        }*/

        /*if (photonViewBullet.IsMine || PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }*/

        /*if (!photonView.IsMine || !gameObject.activeSelf)
            return;

        
        PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
        
        //this.GetComponent<PhotonView>().RPC("synchronized_DestroyObject", RpcTarget.AllBuffered);
        if (targetView != null && (!targetView.IsMine || targetView.IsRoomView))
        {
            if(collision.tag == "Player")
            {
                // Логика нанесения урона
            }

            BulletPool.Instance.UseBullet(this);
            /////////BulletPool.Instance.photonViewPool.RPC("UseBullet", RpcTarget.AllBuffered, photonViewBullet.ViewID);
            //BulletPool.Instance.photonViewPool.RPC("ReturnBullet", RpcTarget.AllBuffered, gameObject.name);

            //this.GetComponent<PhotonView>().RPC("synchronized_DestroyObject", RpcTarget.AllBuffered);
        }
        else
        {
            Debug.Log("I'm must to inactive!");
            BulletPool.Instance.UseBullet(this);
            /////////BulletPool.Instance.photonViewPool.RPC("UseBullet", RpcTarget.AllBuffered, photonViewBullet.ViewID);

            //BulletPool.Instance.photonViewPool.RPC("ReturnBullet", RpcTarget.AllBuffered, gameObject.name);

            //this.GetComponent<PhotonView>().RPC("synchronized_DestroyObject", RpcTarget.AllBuffered);
        }*/


    }

    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Мы владелец
        if (stream.IsWriting)
        {
            //stream.SendNext(startPosition);
            //stream.SendNext(targetPosition);
            //stream.SendNext(progress);
            
            //stream.SendNext(isMoving);

            //if (isMoving)
            //{
                stream.SendNext(this.startPosition);
                stream.SendNext(this.targetPosition);
            //}

            stream.SendNext(this.gameObject.activeInHierarchy);
        } // Мы принимающая сторона
        else
        {
            //startPosition = (Vector3)stream.ReceiveNext();
            //targetPosition = (Vector3)stream.ReceiveNext();
            //progress = (double)stream.ReceiveNext();

            //isMoving = (bool) stream.ReceiveNext();

            //if (isMoving)
            //{
                startPosition = (Vector3) stream.ReceiveNext();
                targetPosition = (Vector3) stream.ReceiveNext();
            //}

            this.gameObject.SetActive((bool) stream.ReceiveNext());
        }
    }*/

    /*[PunRPC]
    public void synchronized_DestroyObject()
    {
        Destroy(this.gameObject);
    }*/
}

public static class VectorExtension
{
    public static Vector3 ChangeAxis(this Vector3 vector, Axis axis, float value)
    {
        return new Vector3(
            axis == Axis.X ? value : vector.x,
            axis == Axis.Y ? value : vector.y,
            axis == Axis.Z ? value : vector.z
            );
    }
}

public enum Axis 
{ 
    X, Y, Z
}