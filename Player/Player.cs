using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.InteropServices.WindowsRuntime;

public class Player : MonoBehaviour// Photon.MonoBehaviour
{
    public PhotonView photonView;

    public BulletPool bulletPool;

    public Rigidbody2D playerRigidbody;
    public Animator playerAnimator;
    private const string animationRunName = "isRuning";
    public GameObject playerCameraObject;
    private Camera playerCamera;

    public Joystick playerMovementJoystick;
    public Joystick playerAimJoystick;
    [SerializeField]
    private int playerSpeed;
    [SerializeField]
    private int rotationSpeed;
    private Vector2 moveInput;
    private Vector2 moveVelocity;

    [SerializeField]
    private float rotationOffset = -90f;
    private Vector3 difference;
    private float rotationZ = 0f;

    [SerializeField]
    public SpriteRenderer helmetSprite;
    [SerializeField]
    public SpriteRenderer helmetStrokeSprite;

    private GameManager gameManager;

    public LayerMask targetLayer;
    public Transform shootPoint;
    public float raycastDistance = 10f;
    public float raycastWidth = 1f;
    public float shootInterval = 3f;
    private float lastShotTime;
    //public float weaponRange = 10f;

    public float x_min = 0f;
    public float x_max = 1f;
    public float y_min = 0f;
    public float y_max = 1.16f;

    public GameObject bulletPrefab;
    public GameObject bullet;

    public PlayerData data;
    public GenerateColors generateColors;

    private void Awake()
    {
        if(!photonView.IsMine)
        {
            Destroy(playerCameraObject);
            Destroy(playerRigidbody);
            //Destroy(playerAimJoystick.transform.parent);
        }

        /*if (PhotonNetwork.IsMasterClient)
        {
            Color color = GenerateColors.Instance.GetRandomColor();
            photonView.RPC("SetHelmetColor", RpcTarget.AllBuffered, color.r, color.g, color.b, color.a);
        }*/

        if (photonView.IsMine)
        {
            generateColors = GetComponent<GenerateColors>();
            playerCamera = playerCameraObject.GetComponent<Camera>();

            /*playerMovementJoystick = GameObject.FindGameObjectWithTag("JoystickMove").GetComponent<Joystick>();
            playerAimJoystick = GameObject.FindGameObjectWithTag("JoystickAim").GetComponent<Joystick>();*/

            //SetRandomHelmetColor();
            //gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            //GameManager.Instance.photonViewManage.RPC("GetRandomColor", RpcTarget.AllBuffered);
            //SetHelmetColor(gameManager.colorBuffered);
            //gameManager.AssignColor(photonView.Owner.ActorNumber, helmetSprite.color);

            //Debug.Log(JsonUtility.ToJson(GenerateColors.Instance));

            //generateColors.GenerateColorsMaster();
            
            Color color = GenerateColors.Instance.GetRandomColor();
            //GenerateColors.Instance.viewColors.RPC("synchronized_Colors", RpcTarget.AllBuffered, JsonUtility.ToJson(GenerateColors.Instance.colorList));
            photonView.RPC("SetHelmetColor", RpcTarget.AllBuffered, color.r, color.g, color.b, color.a);

            //SetHelmetColor(GenerateColors.Instance.GetRandomColor());

            //GenerateColors.Instance.viewColors.RPC("GetRandomColor", RpcTarget.AllBuffered, JsonUtility.ToJson(GenerateColors.Instance.colorList), photonView.ViewID);

            playerMovementJoystick = GameManager.Instance.playerMovementJoystick;
            playerAimJoystick = GameManager.Instance.playerAimJoystick;

            playerCameraObject.SetActive(true);
        }
    }

    [PunRPC]
    public void SetHelmetColor(float r, float g, float b, float a)
    {
        helmetSprite.color = new Color(r, g, b, a);

        if (r <= 0.2f && g <= 0.2f && b <= 0.2f)
            helmetStrokeSprite.color = new Color(1f, 1f, 1f, 1f);
        else
            helmetStrokeSprite.color = new Color(0f, 0f, 0f, 1f);
    }

    public void SetRandomHelmetColor()
    {
        helmetSprite.color = new Color(GetRandomNumber(0, 1f), GetRandomNumber(0, 1f), GetRandomNumber(0, 1f), 1f);

        if (helmetSprite.color.r == 0f && helmetSprite.color.g == 0f && helmetSprite.color.b == 0f)
            helmetStrokeSprite.color = new Color(1f, 1f, 1f, 1f);
        else
            helmetStrokeSprite.color = new Color(0f, 0f, 0f, 1f);
    }

    private float GetRandomNumber(float min, float max)
    {
        return Random.value > 0.5f ? 1f : 0f;

        //return new System.Random().Next() * (max - min) + min;
    }

    private void Start()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 10;
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (photonView.IsMine)
        {
            CheckInput();

            Shoot();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(shootPoint.position, shootPoint.position + transform.up * raycastDistance);
    }

    private bool IsObjectVisibleByCamera(GameObject target)
    {
        Vector3 targetPosition = target.transform.position;
        Vector3 targetViewportPosition = playerCamera.WorldToViewportPoint(targetPosition);

        return targetViewportPosition.x >= x_min && targetViewportPosition.x <= x_max &&
               targetViewportPosition.y >= y_min && targetViewportPosition.y <= y_max;
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            //photonView.RPC("synchronizedMovement", RpcTarget.AllBuffered);
            moveInput = new Vector2(playerMovementJoystick.Horizontal, playerMovementJoystick.Vertical);
            //moveVelocity = moveInput.normalized * playerSpeed;

            // –ассчитываем синус и косинус угла поворота персонажа
            float angleRad = transform.rotation.z * Mathf.Deg2Rad;
            float cosAngle = Mathf.Cos(angleRad);
            float sinAngle = Mathf.Sin(angleRad);

            // ¬ычисл€ем компоненты движени€ вдоль осей X и Y, учитыва€ угол поворота
            float moveX = cosAngle * playerMovementJoystick.Horizontal - sinAngle * playerMovementJoystick.Vertical;
            float moveY = sinAngle * playerMovementJoystick.Horizontal + cosAngle * playerMovementJoystick.Vertical;

            // —оздаем вектор движени€
            Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;

            // ƒвигаем персонажа
            transform.Translate(moveDirection * playerSpeed * Time.fixedDeltaTime);

            //photonView.RPC("synchronizedAnimator_Run", RpcTarget.AllBuffered);
            if (moveInput.x == 0)
                playerAnimator.SetBool(animationRunName, false);
            else
                playerAnimator.SetBool(animationRunName, true);
        }
    }

    private void CheckInput()
    {
        //photonView.RPC("synchronizedRotation", RpcTarget.AllBuffered);
        Vector3 inputDirection = new Vector3(playerAimJoystick.Horizontal, playerAimJoystick.Vertical, 0);

        if (inputDirection != Vector3.zero && Mathf.Abs(playerAimJoystick.Vertical) > 0.3f || Mathf.Abs(playerAimJoystick.Horizontal) > 0.3f)
        {
            float rotationAmount = -playerAimJoystick.Horizontal * rotationSpeed * Time.deltaTime; // ќтрицательное направление дл€ правильной ориентации
            transform.Rotate(0, 0, rotationAmount);
        }

        //photonView.RPC("synchronizedAnimator_Run", RpcTarget.AllBuffered);
        if (moveInput.x == 0)
            playerAnimator.SetBool(animationRunName, false);
        else
            playerAnimator.SetBool(animationRunName, true);
    }

    private void Shoot()
    {
        if (Time.time - lastShotTime >= shootInterval)
        {
            Vector2 raycastDirection = transform.up;
            RaycastHit2D hit = Physics2D.Raycast(shootPoint.position, raycastDirection, raycastDistance, targetLayer);

            // ѕроверить, если другой игрок находитс€ в области камеры и текущий игрок смотрит на него
            if (hit.collider != null && IsObjectVisibleByCamera(hit.collider.gameObject))
            {
                lastShotTime = Time.time;
                playerAnimator.SetTrigger("isShooting");

                //var bullet = PhotonNetwork.Instantiate(bulletPrefab.name, shootPoint.position, Quaternion.Euler(0, 0, transform.eulerAngles.z));

                //BulletPool.Instance.photonViewPool.RPC("GetBullet", RpcTarget.AllBuffered, photonView.ViewID);
                //////////photonView.RPC("RequestBullet", RpcTarget.AllBuffered, shootPoint.position, Quaternion.Euler(0, 0, transform.eulerAngles.z), hit.point.x, hit.point.y);
                //bulletPool.GetBullet(shootPoint.position, Quaternion.Euler(0, 0, transform.eulerAngles.z), hit.point.x, hit.point.y);
                //photonView.RPC("RequestBullet", RpcTarget.All, shootPoint.position, Quaternion.Euler(0, 0, transform.eulerAngles.z), hit.point.x, hit.point.y);
                bulletPool.photonViewPool.RPC("GetBullet", RpcTarget.All, shootPoint.position, Quaternion.Euler(0, 0, transform.eulerAngles.z), hit.point.x, hit.point.y);

                //bullet = PhotonNetwork.Instantiate(bulletPrefab.name, shootPoint.position, Quaternion.Euler(0, 0, transform.eulerAngles.z));
                //bullet.GetComponent<BulletTrail>().SetTargetPosition(hit.point.x, hit.point.y, shootPoint.position, Quaternion.Euler(0, 0, transform.eulerAngles.z));

                // «десь синхронизируютс€ данные между пулом и конкретным игроком
                //bullet.transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z);


                //bullet.GetComponent<PhotonView>().RPC("synchronized_SetTargetPosition", RpcTarget.AllBuffered, hit.point.x, hit.point.y);
            }
            /*else
            {
                var endPosition = shootPoint.position + transform.up * weaponRange;
                bullet.GetComponent<PhotonView>().RPC("synchronized_SetTargetPosition", RpcTarget.AllBuffered, endPosition);
            }*/
        }
    }

    /*[PunRPC]
    private void RequestBullet(Vector3 startPosition, Quaternion rotation, float x, float y, PhotonMessageInfo info)
    {
        if (bulletPool != null)
        {
            bulletPool.GetBullet(startPosition, rotation, x, y*//*, info*//*);
        }
    }*/

    /*[PunRPC]
    public void synchronized_CurrentBullet(string bulletName)
    {
        bullet = BulletPool.Instance.GetBulletByName(bulletName);
    }*/

    /*[PunRPC]
    private void synchronizedRotation()
    {
        *//*if (Mathf.Abs(playerAimJoystick.Vertical) > 0.3f || Mathf.Abs(playerAimJoystick.Horizontal) > 0.3f)
            rotationZ = Mathf.Atan2(playerAimJoystick.Vertical, playerAimJoystick.Horizontal) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ + rotationOffset);*//*

        Vector3 inputDirection = new Vector3(playerAimJoystick.Horizontal, playerAimJoystick.Vertical, 0);

        if (inputDirection != Vector3.zero && Mathf.Abs(playerAimJoystick.Vertical) > 0.3f || Mathf.Abs(playerAimJoystick.Horizontal) > 0.3f)
        {
            float rotationAmount = -playerAimJoystick.Horizontal * rotationSpeed * Time.deltaTime; // ќтрицательное направление дл€ правильной ориентации
            transform.Rotate(0, 0, rotationAmount);
        }
    }

    [PunRPC]
    private void synchronizedAnimator_Run()
    {
        if (moveInput.x == 0)
            playerAnimator.SetBool(animationRunName, false);
        else
            playerAnimator.SetBool(animationRunName, true);
    }

    [PunRPC]
    private void synchronizedMovement()
    {
        moveInput = new Vector2(playerMovementJoystick.Horizontal, playerMovementJoystick.Vertical);
        //moveVelocity = moveInput.normalized * playerSpeed;

        // –ассчитываем синус и косинус угла поворота персонажа
        float angleRad = transform.rotation.z * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angleRad);
        float sinAngle = Mathf.Sin(angleRad);

        // ¬ычисл€ем компоненты движени€ вдоль осей X и Y, учитыва€ угол поворота
        float moveX = cosAngle * playerMovementJoystick.Horizontal - sinAngle * playerMovementJoystick.Vertical;
        float moveY = sinAngle * playerMovementJoystick.Horizontal + cosAngle * playerMovementJoystick.Vertical;

        // —оздаем вектор движени€
        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;

        // ƒвигаем персонажа
        transform.Translate(moveDirection * playerSpeed * Time.fixedDeltaTime);

        //playerRigidbody.MovePosition(moveDirection * playerSpeed * Time.fixedDeltaTime);
    }*/
}