using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controlls the player, moving, rotating and making him able to shoot
/// </summary>
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

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        //Shooting with left mouse button, instantiating a projectile and calling the SpawnProjectile function to send it to the server
        if (Input.GetKey(KeyCode.Mouse0) && shootingCooldown <= 0)
        {
            Vector3 newProjectilePos = transform.position + (mousePos - transform.position).normalized * 0.7f;
            GameObject projectile = Instantiate(projectilePrefab, newProjectilePos, transform.rotation);
            ClientSend.SpawnProjectile(projectile.GetComponent<Projectile>(), player.id);

            shootingCooldown = 0.2f;
        }

        shootingCooldown -= Time.deltaTime;

        SendPositionToServer();
    }
    private void FixedUpdate()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");
        //transform.position += (Vector3)movement.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
        //transform.Translate(movement * moveSpeed * Time.fixedDeltaTime);

        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f; //- or + 90?
        player.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void SendPositionToServer()
    {
        ClientSend.PlayerPosition(transform.position);
    }
}
