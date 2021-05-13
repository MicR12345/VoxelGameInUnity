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

    public Transform highlightBlock;
    private float checkIncrement = 0.1f;
    private float reach = 5;

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
    private void placeCursorBlocks()
    {

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.transform.position + (cam.transform.forward * step);

            if (worldScript.CheckForVoxel(pos))
            {

                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x)+0.5f, Mathf.FloorToInt(pos.y) + 0.5f, Mathf.FloorToInt(pos.z) + 0.5f);

                highlightBlock.gameObject.SetActive(true);
                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        highlightBlock.gameObject.SetActive(false);

    }
    private void Update()
    {
        placeCursorBlocks();
        if (Player.transform.position.y - worldScript.GetHighestBlock(Player.transform.position) <= 2f)
        {
            isGrounded = true;
        }
        else isGrounded = false;
        Move();
        Turn();
    }
    private void OnJump()
    {
        if(isGrounded)velocity.y += 10f;
    }
    private void Move()
    {
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
