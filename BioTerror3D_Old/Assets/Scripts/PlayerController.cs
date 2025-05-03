using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityModifier = 1f;
    [SerializeField] private bool isOnGround = true;

    private Animator playerAnimator;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject playerBulletPrefab; 
    [SerializeField] private GameObject skill1Prefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float skill1Speed = 5f;
    [SerializeField] private int bulletCount = 6;
    [SerializeField] private  float bulletCooldown = 2f;
    [SerializeField] private float skillCooldown = 5f;
    [SerializeField] private float shootingRange = 10f;

    void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        if (playerAnimator == null)
        {
            Debug.LogError("Animator missing on player!");
            return;
        }
    }

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        if (playerRb == null)
        {
            Debug.LogError("Rigidbody missing on player!");
            return;
        }

        Physics.gravity = new Vector3(0, -9.81f * gravityModifier, 0);
        StartCoroutine(ShootBulletsRoutine());
        StartCoroutine(ShootSkill1Routine());
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            Jump();
        }
    }

    void MovePlayer()
{
    float moveX = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
    float moveZ = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

    bool isSprinting = Input.GetKey(KeyCode.LeftShift);
    float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

    Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

    // Cập nhật animation
    bool isMoving = moveDirection.magnitude > 0;
    playerAnimator.SetBool("IsRunning", isMoving);
    playerAnimator.SetBool("IsIdle", !isMoving);

    // Di chuyển nhân vật
    Vector3 newPosition = playerRb.position + moveDirection * currentSpeed * Time.fixedDeltaTime;
    playerRb.MovePosition(newPosition);
}


    void Jump()
    {
        if (playerRb == null) return;

        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, jumpForce, playerRb.linearVelocity.z);
        isOnGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }


    IEnumerator ShootBulletsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(bulletCooldown);
            ShootPlayerBullets();
        }
    }

    IEnumerator ShootSkill1Routine()
    {
        while (true)
        {
            yield return new WaitForSeconds(skillCooldown);
            ShootSkill1();
        }
    }

    void ShootPlayerBullets()
    {
        if (playerBulletPrefab == null)
        {
            Debug.LogError("Player Bullet Prefab is missing!");
            return;
        }

        Transform targetEnemy = FindClosestEnemy();
        Vector3 shootDirection;

        if (targetEnemy != null)
        {
            shootDirection = (targetEnemy.position - transform.position).normalized;
        }
        else
        {
            shootDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }

        // Xoay nhân vật theo hướng bắn
        if (shootDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(shootDirection);
            transform.rotation = targetRotation;
        }

        float angleStep = 15f;
        float startAngle = -((bulletCount - 1) / 2f) * angleStep;

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 bulletDir = rotation * shootDirection;

            GameObject bullet = Instantiate(playerBulletPrefab, transform.position + bulletDir * 1.5f, Quaternion.LookRotation(bulletDir));
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bulletDir * bulletSpeed;
            }
        }
    }

    void ShootSkill1()
    {
        if (skill1Prefab == null)
        {
            Debug.LogError("Skill1 Bullet Prefab is missing!");
            return;
        }

        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float bulletDirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float bulletDirZ = Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 bulletDirection = new Vector3(bulletDirX, 0, bulletDirZ).normalized;
            Vector3 spawnPosition = transform.position + bulletDirection * 1.5f;

            GameObject bullet = Instantiate(skill1Prefab, spawnPosition, Quaternion.LookRotation(bulletDirection));
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bulletDirection * skill1Speed;
            }

            angle += angleStep;
        }
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;
        float minDistance = shootingRange;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }
}
