using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputAction movement;
    [SerializeField] private InputAction turning;
    [SerializeField] private InputAction jump;

    public GameObject world;

    public WorldScript worldScript;

    private GameObject Player;
    private CharacterController controller;

    private Camera cam;

    private float camRot = 0;

    public float speed = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 100;

    Vector3 velocity;
    public bool isGrounded;
    // Start is called before the first frame update
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;

        Player = this.gameObject;

        worldScript = world.GetComponent<WorldScript>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = Camera.main;
    }
    private void OnEnable()
    {
        movement.Enable();
        turning.Enable();
        jump.Enable();
    }
    private void OnDisable()
    {
        movement.Disable();
        turning.Disable();
        jump.Disable();
    }
    private void Update()
    {
        if (Player.transform.position.y - worldScript.currentChunk.GetHighestBlock(new Vector3(Mathf.FloorToInt(Player.transform.position.x) % 16, Player.transform.position.y, Mathf.FloorToInt(Player.transform.position.x) % 16)) < 2)
        {
            isGrounded = true;
        }
        else isGrounded = false;
        Move();
        Turn();
    }
    private void OnJump()
    {
        velocity.y += 10f;
    }
    private void Move()
    {
        Debug.Log(isGrounded);

        float x = movement.ReadValue<Vector2>().x;
        float z = movement.ReadValue<Vector2>().y;


        Vector3 direction = transform.right * x + transform.forward * z;
        controller.Move(direction * speed * Time.deltaTime);

        if(!isGrounded)velocity.y += gravity * Time.deltaTime;
        else
        {
            if(velocity.y<0)velocity.y = 0;
        }
        controller.Move(velocity * Time.deltaTime);


    }
    private void Turn()
    {
        float mouseX = turning.ReadValue<Vector2>().x * mouseSensitivity * Time.deltaTime;
        float mouseY = turning.ReadValue<Vector2>().y * mouseSensitivity * Time.deltaTime;

        camRot -= mouseY;
        camRot = Mathf.Clamp(camRot, -90, 90);

        cam.transform.localRotation = Quaternion.Euler(camRot, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}
