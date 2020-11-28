using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector2 movement;
    private float moveSpeed = 5f;
    public Camera cam;
    Vector3 mousePos;
    public PlayerManager player;
    public Rigidbody2D rb;

    public GameObject projectilePrefab;
    public float shootingCooldown = 0.2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.up * 2, Color.red);
        Vector3 cursorInWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(cursorInWorldPos);

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKey(KeyCode.Mouse0) && shootingCooldown <= 0)
        {
            Vector3 newProjectilePos = transform.position + (mousePos - transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, newProjectilePos, transform.rotation);
            ClientSend.SpawnProjectile(projectile.GetComponent<Projectile>(), player.id);

            shootingCooldown = 0.2f;
        }

        shootingCooldown -= Time.deltaTime;
    }
    private void FixedUpdate()
    {
        SendPositionToServer();

        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");
        transform.position += (Vector3)movement.normalized * moveSpeed * Time.fixedDeltaTime;

        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f; //- or + 90?
        player.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void SendPositionToServer()
    {
        ClientSend.PlayerPosition(transform.position);
    }
}
