using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControllerTG : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float speed = 10f;

    [Header("Gun")]
    [SerializeField]
    private GameObject gun;
    [SerializeField]
    private GameObject muzzle;
    [SerializeField] 
    private Camera mainCamera;
    [SerializeField]
    private float recoilForce = 10f;
    [SerializeField]
    private GameObject bullet;

    [Header("Resources")]
    [SerializeField]
    private GameObject energyBar;
    [SerializeField]
    private float baseRechargeTime  = 0.8f;
    [SerializeField]
    private GameObject chargeBar;
    [SerializeField]
    private float maxChargeBonusMultiplier = 0.5f; //extra recoil multiplier
    [SerializeField]
    private float maxChargeTime = 0.5f;
    [SerializeField]
    private float chargePenaltyMultiplier = 1f;
    [SerializeField]
    private float chargeBarVisibleTimeThreshold = 0.1f;

    private Vector2 moveDirection;
    private Vector2 lookDirection;
    private bool isFacingRight = true;
    private bool canFire = true;
    private float angle;
    private float currentEnergy;
    private float rechargeTime;
    private float currentChargeTime = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer playerSprite;
    private SpriteRenderer gunSprite;  
    private Slider energySlider;
    private Slider chargeSlider;
    private PlayerInput playerInput;
    private InputAction fireAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSprite = GetComponent<SpriteRenderer>();
        gunSprite = gun.GetComponentInChildren<SpriteRenderer>();
        
        energySlider = energyBar.GetComponent<Slider>();
        energySlider.maxValue = rechargeTime;
        currentEnergy = rechargeTime;
        energyBar.SetActive(false);

        chargeSlider = chargeBar.GetComponent<Slider>();
        chargeSlider.maxValue = maxChargeTime;
        chargeBar.SetActive(false);

        playerInput = GetComponent<PlayerInput>();
        fireAction = playerInput.actions["Fire"];
        fireAction.canceled += FireCheck;

        GameManager.Instance.pause.AddListener(CancelFire);

        rechargeTime = baseRechargeTime;
    }

    private void OnDestroy()
    {
        fireAction.canceled -= FireCheck;
    }

    private void FireCheck(InputAction.CallbackContext obj)
    {
        if (canFire == true && GameManager.Instance.IsPlaying)
           Fire();
    }

    private void CancelFire()
    {
        currentChargeTime= 0f;
        chargeBar.SetActive(false);
    }

    void Update()
    {
        if (canFire == false)
        {
            if (currentEnergy < rechargeTime)
            {
                currentEnergy += Time.deltaTime;
                energySlider.value = currentEnergy;
            }
            else
            {
                energyBar.gameObject.SetActive(false);
                canFire = true;
            }
                
        }       

        if (fireAction.IsPressed() && canFire == true)
        {
            if (GameManager.Instance.IsPlaying && currentChargeTime >= chargeBarVisibleTimeThreshold)
                chargeBar.gameObject.SetActive(true);

            if (currentChargeTime < maxChargeTime)
            {
                currentChargeTime += Time.deltaTime;
                chargeSlider.value = currentChargeTime;
            }
        }
    }

    private void FixedUpdate()
    {      
        rb.AddForce(moveDirection * speed, ForceMode2D.Impulse);

        Vector2 playerPos = transform.position;

        lookDirection = getMouseWorldPos() - playerPos;

        if (checkFace() != isFacingRight)
            Flip();

        angle = Vector2.SignedAngle(Vector2.right, lookDirection);
        gun.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private bool checkFace() 
    {
        return lookDirection.x > 0;
    }

    public Vector2 getMouseWorldPos()
    { 
        return mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()); 
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        playerSprite.flipX = !playerSprite.flipX;
        gunSprite.flipY = !gunSprite.flipY;
    }

    void OnMove(InputValue inputValue)
    {
        moveDirection = inputValue.Get<Vector2>();      
    }

    void OnPause()
    {
        GameManager.Instance.TogglePause();
    }

    void Fire()
    {
        float recoil = recoilForce + recoilForce * maxChargeBonusMultiplier * currentChargeTime / maxChargeTime;
        rb.AddForce(-lookDirection.normalized * recoil, ForceMode2D.Impulse);
        //Debug.Log($"Shot with a force of {recoilForce + recoilForce * maxChargeBonusMultiplier * currentChargeTime / maxChargeTime}");
        GameObject curBullet = Instantiate(bullet, muzzle.transform.position, muzzle.transform.rotation);
        curBullet.GetComponent<BulletScript>().Launch(lookDirection);

        energyBar.SetActive(true);
        currentEnergy = 0;
        rechargeTime = baseRechargeTime + currentChargeTime * chargePenaltyMultiplier;
        energySlider.maxValue = rechargeTime;
        chargeBar.SetActive(false);
        currentChargeTime = 0;
        canFire = false;
    }
}
