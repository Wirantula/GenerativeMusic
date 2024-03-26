using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float acceleration = 1.5f; // How quickly the player reaches max speed.
	public float maxSpeed = 5.0f; // The top speed the player can move at.
	public float mouseSensitivity = 100.0f;
	private Rigidbody rb;
	private Camera playerCamera;
	private float xRotation = 0f;
	private Vector3 currentVelocity;
	private Vector3 desiredVelocity;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		playerCamera = Camera.main;

		// Lock cursor to the center of the screen and hide it.
		Cursor.lockState = CursorLockMode.Locked;
	}

	void FixedUpdate()
	{
		// Handle mouse look.
		float mouseX = Input.GetAxis( "Mouse X" ) * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis( "Mouse Y" ) * mouseSensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp( xRotation, -90f, 90f );

		playerCamera.transform.localRotation = Quaternion.Euler( xRotation, 0f, 0f );
		transform.Rotate( Vector3.up * mouseX );

		// Calculate desired velocity based on input.
		float x = Input.GetAxis( "Horizontal" );
		float z = Input.GetAxis( "Vertical" );

		Vector3 moveDirection = transform.right * x + transform.forward * z;
		desiredVelocity = moveDirection.normalized * maxSpeed;

		// Apply acceleration and deceleration to the movement.
		currentVelocity = Vector3.MoveTowards( currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime );
		rb.velocity = new Vector3( currentVelocity.x, rb.velocity.y, currentVelocity.z );
	}
}