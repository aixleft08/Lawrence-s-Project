using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
	[Header("PLEASE SET PLAYER TAG")]
	[Space(20)]
	public float walkSpeed = 5;
	public float runSpeed = 10;
	public KeyCode runKey = KeyCode.LeftShift;

	[Header("PLEASE SET GROUND TAGS")]
	[Space(20)]
	public float jumpStrength = 5;
	public float gravity = -20;
	public KeyCode jumpKey = KeyCode.Space;

	[Space(20)]
	public float camSensitivity = 1;
	public float camSmoothing = 2;

	[Space(20)]
	public bool allowMove = true;
	public bool allowJump = true;
	public bool allowLook = true;

	CharacterController cc;
	float speed;
	bool isGrounded = true;

	Transform charCamera;
	Vector2 currentMouseLook;
	Vector2 appliedMouseDelta;
	Vector3 moveDirection = Vector3.zero;
	float yVel;


	void Start()
	{
		cc = GetComponent<CharacterController>();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		charCamera = Camera.main.transform;

	}
	void Update()
	{
		if(allowJump)
			Jump();
		if(allowLook)
			Look();
		if(allowMove)
			Move();
	}

	void Move()
	{
		speed = Input.GetKey(runKey) ? runSpeed : walkSpeed;

		Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = speed * Input.GetAxis("Vertical");
        float curSpeedY = speed * Input.GetAxis("Horizontal");
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
		moveDirection.y = yVel;

        // Move the controller
        cc.Move(moveDirection * Time.deltaTime);
	}

	void Jump()
	{	
		isGrounded = cc.isGrounded;

		if(yVel > gravity + 1)
			yVel += gravity * Time.deltaTime;
		else
			yVel = gravity;
			
		if (Input.GetKeyDown(jumpKey) && isGrounded)
		{
           yVel = Mathf.Sqrt(jumpStrength * -2f * (gravity ));
		}
	}

	void Look()
	{
		Vector2 smoothMouseDelta =  Vector2.Scale(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")), Vector2.one * camSensitivity * camSmoothing);
		appliedMouseDelta = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1 / camSmoothing);
		currentMouseLook += appliedMouseDelta;
		currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -90, 90);

		charCamera.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
		transform.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
	}
	
}
