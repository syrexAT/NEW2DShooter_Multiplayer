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


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.up * 2, Color.red);
        Vector3 cursorInWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log(cursorInWorldPos);
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 newProjectilePos = transform.position + mousePos.normalized;
            GameObject projectile = Instantiate(projectilePrefab, newProjectilePos, player.transform.rotation);
        }

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
    }
    private void FixedUpdate()
    {
        SendPositionToServer();

        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");
        transform.position += (Vector3)movement * moveSpeed * Time.fixedDeltaTime;

        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f; //- or + 90?
        player.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void SendPositionToServer()
    {
        ClientSend.PlayerPosition(transform.position);
    }
}
