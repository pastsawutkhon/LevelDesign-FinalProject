using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    public enum WeaponType { None, Rifle, Pistol, Knife, Dynamite, Key_1, Key_2 }
    public WeaponType currentWeapon = WeaponType.None; 

    [Header("Weapon Unlocks (สถานะการครอบครองไอเทม)")]
    public bool hasRifle = false;  
    public bool hasPistol = false; 
    public bool hasKnife = false;  
    public bool hasDynamite = false; 
    public bool hasKey_1 = false;    
    public bool hasKey_2 = false;    

    [Header("Weapon Models (ใส่โมเดลที่อยู่ใต้ Player)")]
    public GameObject rifleModel;
    public GameObject pistolModel;
    public GameObject knifeModel;
    public GameObject dynamiteModel; 
    public GameObject key1Model;     
    public GameObject key2Model;     

    [Header("Fire Points (จุดยิง/จุดปา)")]
    public Transform firePoint1_Rifle;
    public Transform firePoint2_Pistol;
    public Transform firePoint3_Knife;
    public Transform firePoint4_Dynamite; 

    [Header("Rifle (ปืนไรเฟิล - กด 1)")]
    public GameObject rifleBulletPrefab;
    public float rifleCooldown = 0.2f;

    [Header("Pistol (ปืนพก - กด 2)")]
    public GameObject pistolBulletPrefab;
    public float pistolCooldown = 0.5f;

    [Header("Knife (มีด - กด 3)")]
    public float knifeRange = 1.5f;
    public float knifeCooldown = 0.8f;
    public float knifeDamage = 3f; // ปรับดาเมจของมีดได้ใน Inspector
    public GameObject knifeHitFXPrefab; // ลากเอฟเฟกต์ตอนฟันโดนมาใส่ (เช่น รอยฟัน, เลือด)
    public Animator knifeAnimator;

    [Header("Dynamite (ระเบิด - กด 4)")]
    public GameObject dynamitePrefab; 
    public float dynamiteCooldown = 1.0f;

    [Header("General Settings")]
    public float speed = 5f;

    private Rigidbody rb;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        EquipWeapon(WeaponType.None); 
    }

    void Update()
    {
        Move();
        RotateToMouse();
        HandleWeaponSwitch();
        HandleShooting();
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * speed;
        rb.linearVelocity = movement;
    }

    void RotateToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 target = hit.point;
            target.y = transform.position.y;
            Vector3 direction = target - transform.position;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation;
            }
        }
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasRifle) EquipWeapon(WeaponType.Rifle);
        if (Input.GetKeyDown(KeyCode.Alpha2) && hasPistol) EquipWeapon(WeaponType.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha3) && hasKnife) EquipWeapon(WeaponType.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && hasDynamite) EquipWeapon(WeaponType.Dynamite);
        if (Input.GetKeyDown(KeyCode.Alpha5) && hasKey_1) EquipWeapon(WeaponType.Key_1);
        if (Input.GetKeyDown(KeyCode.Alpha6) && hasKey_2) EquipWeapon(WeaponType.Key_2); 
        if (Input.GetKeyDown(KeyCode.Alpha0)) EquipWeapon(WeaponType.None); 
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

        switch (currentWeapon)
        {
            case WeaponType.Rifle: if (rifleModel != null) rifleModel.SetActive(true); break;
            case WeaponType.Pistol: if (pistolModel != null) pistolModel.SetActive(true); break;
            case WeaponType.Knife: if (knifeModel != null) knifeModel.SetActive(true); break;
            case WeaponType.Dynamite: if (dynamiteModel != null) dynamiteModel.SetActive(true); break;
            case WeaponType.Key_1: if (key1Model != null) key1Model.SetActive(true); break;
            case WeaponType.Key_2: if (key2Model != null) key2Model.SetActive(true); break;
            case WeaponType.None: break;
        }
    }

    public void UnlockWeapon(WeaponType weaponToUnlock)
    {
        if (weaponToUnlock == WeaponType.Rifle) hasRifle = true;
        if (weaponToUnlock == WeaponType.Pistol) hasPistol = true;
        if (weaponToUnlock == WeaponType.Knife) hasKnife = true;
        if (weaponToUnlock == WeaponType.Dynamite) hasDynamite = true;
        if (weaponToUnlock == WeaponType.Key_1) hasKey_1 = true;
        if (weaponToUnlock == WeaponType.Key_2) hasKey_2 = true; 
        
        EquipWeapon(weaponToUnlock);
    }

    void HandleShooting()
    {
        if (currentWeapon == WeaponType.None || currentWeapon == WeaponType.Key_1 || currentWeapon == WeaponType.Key_2) return;

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
        
        foreach (Collider enemyCollider in hitEnemies)
        {
            if (enemyCollider.gameObject.CompareTag("Enemy")) 
            {
                // 1. ทำดาเมจ
                Enemy enemyScript = enemyCollider.gameObject.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(knifeDamage);
                }

                // 2. เล่นเอฟเฟกต์ฟันโดน
                if (knifeHitFXPrefab != null)
                {
                    // ให้ FX เกิดสูงขึ้นมาจากพื้นนิดนึง (จะได้อยู่กลางตัวศัตรู)
                    Vector3 fxPosition = enemyCollider.transform.position + new Vector3(0, 1f, 0);
                    Instantiate(knifeHitFXPrefab, fxPosition, Quaternion.identity);
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
}