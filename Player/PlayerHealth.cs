using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable
{
    public float health;

    public PhotonView photonViewHealth;

    public Camera playerCamera;

    [SerializeField]
    private float maxHealth = 100;
    [SerializeField]
    private float maxHealthToFill = 0.25f;

    public Image fillHealthImage;
    public Text percentHealthText;

    [SerializeField]
    private float lerpSpeedMultiplyer = 3f;
    private float lerpSpeed;

    //private Transform playerTransform;

    private void Awake()
    {
        if (photonViewHealth.IsMine)
        {
            health = maxHealth;
        }
    }

    private void Update()
    {
        if (photonViewHealth.IsMine)
        {
            percentHealthText.text = health.ToString();

            

            lerpSpeed = lerpSpeedMultiplyer * Time.deltaTime;

            HealthBarFiller(health);
            ColorChanger(health);
        }
        else
        {
            // ≈сли текст здоровь€ не €вл€етс€ собственностью текущего игрока, то
            // перевернЄм в сторону текущего игрока текст дл€ удобочитаемости
            //percentHealthText.rectTransform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z);

            // ѕоворачивайте индикатор в сторону текущей камеры
            /*Vector3 toCamera = playerCamera.transform.position - transform.position;
            float angle = Mathf.Atan2(toCamera.y, toCamera.x) * Mathf.Rad2Deg;
            percentHealthText.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));*/
        }
    }

    [PunRPC]
    public void DealDamage(float amountDamage)
    {
        Damage(amountDamage);
    }

    private void Damage(float amountDamage)
    {
        if (photonViewHealth.IsMine)
        {
            if (health > 0)
            {
                // ¬ычтем потер€нное здоровье из текущего значени€
                health -= amountDamage;
            }
        }
    }

    private void HealthBarFiller(float amountHealth)
    {
        fillHealthImage.fillAmount = Mathf.Lerp(fillHealthImage.fillAmount, (amountHealth / maxHealth) * maxHealthToFill, lerpSpeed);
    }

    private void ColorChanger(float amountHealth)
    {
        Color healthColor = Color.Lerp(Color.red, Color.green, amountHealth / maxHealth);
        fillHealthImage.color = healthColor;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // “екущий игрок
        if (stream.IsWriting)
        {
            stream.SendNext(health);
        } // ƒругие игроки
        else
        {
            float amountHealthAnotherPlayer = (float)stream.ReceiveNext();

            ColorChanger(amountHealthAnotherPlayer);
            HealthBarFiller(amountHealthAnotherPlayer);
        }
    }
}
