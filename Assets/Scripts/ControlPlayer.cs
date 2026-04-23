using UnityEngine;
using System.Collections.Generic; 

public class ControlPlayer : MonoBehaviour
{
    public enum WeaponType { None, Rifle, Pistol, Knife, Dynamite, Key_1, Key_2, Money, GoldenCoin }
    public WeaponType currentWeapon = WeaponType.None; 

    [Header("ช่องเก็บของในกระเป๋า (Inventory)")]
    public List<WeaponType> inventory = new List<WeaponType>();

    [Header("Weapon Unlocks (สถานะการครอบครองไอเทม)")]
    public bool hasRifle = false;  
    public bool hasPistol = false; 
    public bool hasKnife = false;  
    public bool hasDynamite = false; 
    public bool hasKey_1 = false;    
    public bool hasKey_2 = false;  
    public bool hasMoney = false;   
    public bool hasGoldenCoin = false; 

    [Header("Weapon Models (โมเดลอาวุธและไอเทม)")]
    public GameObject rifleModel;
    public GameObject pistolModel;
    public GameObject knifeModel;
    public GameObject dynamiteModel; 
    public GameObject key1Model;     
    public GameObject key2Model;  
    public GameObject moneyModel;   
    public GameObject goldenCoinModel;   

    [Header("Fire Points (จุดยิง/จุดปา/จุดฟัน)")]
    public Transform firePoint1_Rifle;
    public Transform firePoint2_Pistol;
    public Transform firePoint3_Knife;
    public Transform firePoint4_Dynamite; 

    [Header("อาวุธ: Rifle (กด 1)")]
    public GameObject rifleBulletPrefab;
    public float rifleCooldown = 0.2f;
    public AudioSource rifleAudioSource;

    [Header("อาวุธ: Pistol (กด 2)")]
    public GameObject pistolBulletPrefab;
    public float pistolCooldown = 0.5f;
    public AudioSource pistolShotSound;

    [Header("อาวุธ: Knife (กด 3)")]
    public float knifeRange = 1.5f;
    public float knifeCooldown = 0.8f;
    public float knifeDamage = 3f; 
    public GameObject knifeHitFXPrefab; 
    public Animator knifeAnimator;
    public AudioSource knifeStab;
    public AudioSource knifeSwing;

    [Header("อาวุธ: Dynamite (กด 4)")]
    public GameObject dynamitePrefab; 
    public float dynamiteCooldown = 1.0f;
    public AudioSource dynamiteThrowSound;

    [Header("การเคลื่อนที่")]
    public float speed = 3f;

    [Header("การหมุนตัว")]
    public float turnSpeed = 20f;

    [Header("ระบบโชว์ชื่ออาวุธ")]
    public TMPro.TextMeshProUGUI weaponNameText; 
    [Header("Audio Settings")]
    public AudioSource audioSource; // ลาก AudioSource ของตัวละครมาใส่
    public AudioClip switchSound;   // เสียงตอนเปลี่ยนอาวุธ (เช่น เสียงขึ้นลำปืน)

    [Header("ระบบ Sprint & Stamina")]
    public float sprintSpeed = 5f;      // ความเร็วตอนวิ่ง
    public float maxStamina = 100f;     // ค่า Stamina สูงสุด
    public float currentStamina;        // ค่า Stamina ปัจจุบัน
    public float staminaDrain = 20f;    // อัตราการลดของ Stamina ต่อวินาที
    public float staminaRegen = 10f;    // อัตราการฟื้นฟู Stamina ต่อวินาที
    public UnityEngine.UI.Image staminaBarFill; // ลาก UI Stamina (Image) มาใส่
    private bool isSprinting = false;

    public bool canMove = true;

    private Coroutine hideTextCoroutine;
    private Vector3 targetLookPoint;  

    private Rigidbody rb;
    private float nextFireTime = 0f;
    private Vector3 movementInput; 

    private KeyCode[] numberKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina;
        EquipWeapon(WeaponType.None); 
    }

    void Update()
    {
        HandleStamina();
        GetInput();
        HandleWeaponSwitch();
        HandleShooting();

        // 1. หาว่าขณะนี้กล้องตัวไหนกำลังทำงานอยู่ (ใช้ได้ทั้ง menuCamera และ playerCamera)
        Camera currentCam = Camera.main;

        // 🌟 ถ้า Camera.main เป็น null (เพราะไม่ได้ตั้ง Tag) ให้ลองหากล้องที่กำลังเปิดใช้งานอยู่แทน
        if (currentCam == null)
        {
            currentCam = Camera.current; 
        }

        // 2. ถ้ามีกล้องที่พร้อมทำงาน ให้คำนวณการหมุนตัว
        if (currentCam != null)
        {
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
            Ray ray = currentCam.ScreenPointToRay(Input.mousePosition); // ใช้ currentCam แทน Camera.main
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 target = ray.GetPoint(rayDistance);
                target.y = transform.position.y; 
                Vector3 direction = target - transform.position;

                if (direction.sqrMagnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed); 
                }
            }
        }
        if (currentWeapon == WeaponType.Rifle)
        {
            // ถ้ากดคลิกซ้ายค้าง
            if (Input.GetMouseButton(0))
            {
                if (!rifleAudioSource.isPlaying)
                {
                    rifleAudioSource.Play();
                }
            }
            // ถ้าปล่อยเมาส์ หรือ เปลี่ยนอาวุธกะทันหัน
            else
            {
                if (rifleAudioSource.isPlaying) rifleAudioSource.Stop();
            }
        }
        else
        {
            // ถ้าเปลี่ยนไปถือปืนอื่น ให้หยุดเสียง Rifle ทันที
            if (rifleAudioSource.isPlaying) rifleAudioSource.Stop();
        }
    }


    void FixedUpdate()
    {
        // 🌟 ให้ FixedUpdate ทำหน้าที่จัดการแค่เรื่อง "เดิน" อย่างเดียวพอครับ
        MovePlayer();
    }

    void GetInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        
        // เช็คว่ากดปุ่ม Shift และมีการขยับตัวอยู่หรือไม่
        bool isMoving = moveX != 0 || moveZ != 0;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving && currentStamina > 0;

        // ปรับความเร็วตามสถานะการวิ่ง
        float currentSpeed = isSprinting ? sprintSpeed : speed;
        movementInput = new Vector3(moveX, 0, moveZ).normalized * currentSpeed;
    }

    void MovePlayer()
    {
        if (!canMove) 
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        rb.linearVelocity = new Vector3(movementInput.x, rb.linearVelocity.y, movementInput.z);
    }

    // --- ระบบสลับอาวุธแบบจัดเรียงอัตโนมัติ ---
    // --- ระบบสลับอาวุธแบบจัดเรียงอัตโนมัติ ---
    void HandleWeaponSwitch()
    {
        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Input.GetKeyDown(numberKeys[i]))
            {
                if (i < inventory.Count)
                {
                    // เก็บข้อมูลมาก่อนว่าผู้เล่นกดเลือกอาวุธชิ้นไหน
                    WeaponType targetWeapon = inventory[i];

                    // 🌟 เช็คว่าอาวุธที่กดเลือก คืออันเดียวกับที่ถืออยู่ตอนนี้ไหม?
                    if (currentWeapon == targetWeapon)
                    {
                        // ถ้าถืออยู่แล้วกดซ้ำ = สั่งให้เก็บเป็นมือเปล่า
                        EquipWeapon(WeaponType.None);
                    }
                    else
                    {
                        // ถ้าเป็นอาวุธชิ้นอื่น = สั่งให้หยิบขึ้นมาตามปกติ
                        EquipWeapon(targetWeapon);
                    }
                }
            }
        }

        // กด 0 เพื่อเก็บของมือเปล่าเสมอ (อันนี้คงไว้เผื่อฉุกเฉินครับ)
        if (Input.GetKeyDown(KeyCode.Alpha0)) 
        {
            EquipWeapon(WeaponType.None); 
        }
    }

    void EquipWeapon(WeaponType newWeapon)
    {
        currentWeapon = newWeapon;

        if (rifleModel != null) rifleModel.SetActive(false);
        if (pistolModel != null) pistolModel.SetActive(false);
        if (knifeModel != null) knifeModel.SetActive(false);
        if (dynamiteModel != null) dynamiteModel.SetActive(false);
        if (key1Model != null) key1Model.SetActive(false);
        if (key2Model != null) key2Model.SetActive(false);
        if (moneyModel != null) moneyModel.SetActive(false); 
        if (goldenCoinModel != null) goldenCoinModel.SetActive(false);

        switch (currentWeapon)
        {
            case WeaponType.Rifle: if (rifleModel != null) rifleModel.SetActive(true); break;
            case WeaponType.Pistol: if (pistolModel != null) pistolModel.SetActive(true); break;
            case WeaponType.Knife: if (knifeModel != null) knifeModel.SetActive(true); break;
            case WeaponType.Dynamite: if (dynamiteModel != null) dynamiteModel.SetActive(true); break;
            case WeaponType.Key_1: if (key1Model != null) key1Model.SetActive(true); break;
            case WeaponType.Key_2: if (key2Model != null) key2Model.SetActive(true); break;
            case WeaponType.Money: if (moneyModel != null) moneyModel.SetActive(true); break;
            case WeaponType.GoldenCoin: if (goldenCoinModel != null) goldenCoinModel.SetActive(true); break;
            case WeaponType.None: break;
        }
        
        ShowWeaponName(newWeapon);
        if (audioSource != null && switchSound != null && currentWeapon != WeaponType.None)
        {
            audioSource.PlayOneShot(switchSound);
        }
    }

    public void UnlockWeapon(WeaponType weaponToUnlock)
    {
        if (weaponToUnlock != WeaponType.None && !inventory.Contains(weaponToUnlock))
        {
            inventory.Add(weaponToUnlock);
        }

        if (weaponToUnlock == WeaponType.Rifle) hasRifle = true;
        if (weaponToUnlock == WeaponType.Pistol) hasPistol = true;
        if (weaponToUnlock == WeaponType.Knife) hasKnife = true;
        if (weaponToUnlock == WeaponType.Dynamite) hasDynamite = true;
        if (weaponToUnlock == WeaponType.Key_1) hasKey_1 = true;
        if (weaponToUnlock == WeaponType.Key_2) hasKey_2 = true; 
        if (weaponToUnlock == WeaponType.Money) hasMoney = true; 
        if (weaponToUnlock == WeaponType.GoldenCoin) hasGoldenCoin = true;
        
        EquipWeapon(weaponToUnlock);
    }

    public void RemoveWeapon(WeaponType weaponToRemove)
    {
        if (inventory.Contains(weaponToRemove))
        {
            inventory.Remove(weaponToRemove);
        }

        if (weaponToRemove == WeaponType.Rifle) hasRifle = false;
        if (weaponToRemove == WeaponType.Pistol) hasPistol = false;
        if (weaponToRemove == WeaponType.Knife) hasKnife = false;
        if (weaponToRemove == WeaponType.Dynamite) hasDynamite = false;
        if (weaponToRemove == WeaponType.Key_1) hasKey_1 = false;
        if (weaponToRemove == WeaponType.Key_2) hasKey_2 = false;
        if (weaponToRemove == WeaponType.Money) hasMoney = false;
        if (weaponToRemove == WeaponType.GoldenCoin) hasGoldenCoin = false;

        if (currentWeapon == weaponToRemove)
        {
            EquipWeapon(WeaponType.None);
        }
    }

    // --- ระบบโจมตี ---
    void HandleShooting()
    {
        if (currentWeapon == WeaponType.None || currentWeapon == WeaponType.Key_1 || currentWeapon == WeaponType.Key_2 || currentWeapon == WeaponType.Money) return;

        bool isTryingToShoot = false;

        if (currentWeapon == WeaponType.Rifle)
        {
            isTryingToShoot = Input.GetMouseButton(0); 
        }
        else 
        {
            isTryingToShoot = Input.GetMouseButtonDown(0); 
        }

        if (isTryingToShoot && Time.time >= nextFireTime)
        {
            Attack();
        }
    }

    void Attack()
    {
        switch (currentWeapon)
        {
            case WeaponType.Rifle:
                Shoot(rifleBulletPrefab, firePoint1_Rifle);
                nextFireTime = Time.time + rifleCooldown;
                break;
            case WeaponType.Pistol:
                if (pistolShotSound != null)
                {
                    pistolShotSound.Play();
                }
                Shoot(pistolBulletPrefab, firePoint2_Pistol);
                nextFireTime = Time.time + pistolCooldown;
                break;
            case WeaponType.Knife:
                if (knifeAnimator != null) knifeAnimator.SetTrigger("Attack"); 
                nextFireTime = Time.time + knifeCooldown;
                break;
            case WeaponType.Dynamite:
                if (dynamiteThrowSound != null)
                {
                    dynamiteThrowSound.Play();
                }
                Shoot(dynamitePrefab, firePoint4_Dynamite);
                nextFireTime = Time.time + dynamiteCooldown;
                break;
        }
    }

    void Shoot(GameObject bulletPrefab, Transform firePoint)
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    public void ExecuteKnifeDamage()
    {
        if (currentWeapon == WeaponType.Knife)
        {
            MeleeAttack(firePoint3_Knife);
        }
    }

    void MeleeAttack(Transform attackPoint)
    {
        if (attackPoint == null) return;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, knifeRange);
        knifeSwing.Play();
        
        foreach (Collider hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy")) 
            {
                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(knifeDamage);
                knifeStab.Play();
                if (knifeHitFXPrefab != null)
                {
                    Instantiate(knifeHitFXPrefab, hit.transform.position, Quaternion.identity);
                }

            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint3_Knife != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint3_Knife.position, knifeRange);
        }
    }

    // 🌟 ระบบแปลง Enum เป็นข้อความสวยๆ และสั่งให้ UI ทำงาน
    void ShowWeaponName(WeaponType weapon)
    {
        if (weaponNameText == null) return; // ถ้าลืมลาก Text มาใส่ จะได้ไม่ Error

        // กำหนดชื่อเท่ๆ ให้แต่ละอาวุธ (เปลี่ยนคำในเครื่องหมายคำพูดได้ตามชอบเลยครับ)
        string nameToShow = "";
        switch (weapon)
        {
            case WeaponType.Rifle: nameToShow = "Assault Rifle"; break;
            case WeaponType.Pistol: nameToShow = "Handgun"; break;
            case WeaponType.Knife: nameToShow = "Tactical Knife"; break;
            case WeaponType.Dynamite: nameToShow = "Dynamite"; break;
            case WeaponType.Key_1: nameToShow = "Golden Key 1"; break;
            case WeaponType.Key_2: nameToShow = "Golden Key 2"; break;
            case WeaponType.Money: nameToShow = "Money"; break;
            case WeaponType.GoldenCoin: nameToShow = "Golden Coin"; break;
            case WeaponType.None: nameToShow = ""; break;
        }

        weaponNameText.text = nameToShow;

        weaponNameText.color = new Color(weaponNameText.color.r, weaponNameText.color.g, weaponNameText.color.b, 1f);
    }

    void HandleStamina()
    {
        if (isSprinting)
        {
            // ลด Stamina เมื่อวิ่ง
            currentStamina = Mathf.Max(currentStamina - staminaDrain * Time.deltaTime, 0f);
        }
        else
        {
            // ฟื้นฟู Stamina เมื่อไม่ได้วิ่ง
            currentStamina = Mathf.Min(currentStamina + staminaRegen * Time.deltaTime, maxStamina);
        }

        // อัปเดต Stamina Bar (ถ้ามีการลาก UI มาใส่)
        if (staminaBarFill != null)
        {
            staminaBarFill.fillAmount = currentStamina / maxStamina;
        }
    }
}