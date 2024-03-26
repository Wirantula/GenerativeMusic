using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementTracker : MonoBehaviour
{

	public Vector3 LastPosition { get => lastPosition; private set => lastPosition = value; }
	public float Speed { get => speed; private set => speed = value; }
	public bool IsMovingForward { get => isMovingForward; private set => isMovingForward = value; }
	public Vector3 CurrentPosition => transform.position;


	[SerializeField]
	private float speed;
	[SerializeField]
	private bool isMovingForward;
	[SerializeField]
	private Vector3 lastPosition;

	private void FixedUpdate()
	{
		// Calculate speed
		speed = ( CurrentPosition - lastPosition ).magnitude / Time.deltaTime;

		// Determine if moving forward - this is a simplified example
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			isMovingForward = true;
		}
		else
		{
			isMovingForward = false;
		}
		//isMovingForward = Vector3.Dot( transform.forward, ( CurrentPosition - lastPosition ).normalized ) > 0;

		// Update last position for the next frame's calculation
		lastPosition = CurrentPosition;
	}
}
