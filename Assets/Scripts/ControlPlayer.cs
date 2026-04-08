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

    [Header("อาวุธ: Pistol (กด 2)")]
    public GameObject pistolBulletPrefab;
    public float pistolCooldown = 0.5f;

    [Header("อาวุธ: Knife (กด 3)")]
    public float knifeRange = 1.5f;
    public float knifeCooldown = 0.8f;
    public float knifeDamage = 3f; 
    public GameObject knifeHitFXPrefab; 
    public Animator knifeAnimator;

    [Header("อาวุธ: Dynamite (กด 4)")]
    public GameObject dynamitePrefab; 
    public float dynamiteCooldown = 1.0f;

    [Header("การเคลื่อนที่")]
    public float speed = 4f;

    [Header("การหมุนตัว")]
    public float turnSpeed = 20f;

    [Header("ระบบโชว์ชื่ออาวุธ")]
    public TMPro.TextMeshProUGUI weaponNameText; 

    private Coroutine hideTextCoroutine;
    private Vector3 targetLookPoint;  

    private Rigidbody rb;
    private float nextFireTime = 0f;
    private Vector3 movementInput; 

    private KeyCode[] numberKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        EquipWeapon(WeaponType.None); 
    }

    void Update()
    {
        GetInput();
        HandleWeaponSwitch();
        HandleShooting();

        // 🌟 ย้ายระบบเล็งและการหมุนตัวมารวมกันใน Update ทั้งหมด เพื่อให้ลื่นไหลไปกับเมาส์
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 target = ray.GetPoint(rayDistance);
            target.y = transform.position.y; 
            Vector3 direction = target - transform.position;

            if (direction.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                // ใช้ transform.rotation แทน rb.MoveRotation เพราะเราติ๊ก Freeze Rotation ไปแล้ว
                // เปลี่ยน Time.fixedDeltaTime เป็น Time.deltaTime ด้วยครับ
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed); 
            }
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
        movementInput = new Vector3(moveX, 0, moveZ).normalized * speed;
    }

    void MovePlayer()
    {
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
        
        if (newWeapon != WeaponType.None)
        {
            ShowWeaponName(newWeapon);
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
                Shoot(pistolBulletPrefab, firePoint2_Pistol);
                nextFireTime = Time.time + pistolCooldown;
                break;
            case WeaponType.Knife:
                if (knifeAnimator != null) knifeAnimator.SetTrigger("Attack"); 
                nextFireTime = Time.time + knifeCooldown;
                break;
            case WeaponType.Dynamite:
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
        
        foreach (Collider hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy")) 
            {
                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(knifeDamage);

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
            case WeaponType.None: nameToShow = "Hands"; break;
        }

        weaponNameText.text = nameToShow;

        // ถ้ามี Coroutine เดิมที่กำลังเฟดอยู่ ให้หยุดก่อน แล้วเริ่มโชว์ใหม่ให้ชัด 100% ทันที
        if (hideTextCoroutine != null) StopCoroutine(hideTextCoroutine);
        hideTextCoroutine = StartCoroutine(FadeOutText());
    }

    // 🌟 ระบบตั้งเวลา: โชว์ค้างไว้ -> ค่อยๆ จางหาย
    System.Collections.IEnumerator FadeOutText()
    {
        // 1. ตั้งค่าให้สีตัวอักษรทึบแสง (Alpha = 1) ทันที
        weaponNameText.color = new Color(weaponNameText.color.r, weaponNameText.color.g, weaponNameText.color.b, 1f);
        
        // 2. ค้างโชว์ไว้ 1.5 วินาที
        yield return new WaitForSeconds(1.5f);
        
        // 3. ค่อยๆ เฟดจางหายไปภายใน 0.5 วินาที
        float fadeTime = 0.5f;
        float currentFade = 0f;
        while (currentFade < fadeTime)
        {
            currentFade += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentFade / fadeTime);
            weaponNameText.color = new Color(weaponNameText.color.r, weaponNameText.color.g, weaponNameText.color.b, alpha);
            yield return null; // รอเฟรมถัดไป
        }
    }
}