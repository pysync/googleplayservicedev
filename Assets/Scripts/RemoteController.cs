//////////////////////////////////////////////////////////////
// TapControl.js
// Penelope  Tutorial
//
// TapControl handles the control scheme in which Penelope is
// driven by a single finger. When the player touches the screen,
// Penelope will move toward the finger. The player can also
// use two fingers to do pinching and twisting gestures to do
// camera zooming and rotation. 
//////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class RemoteController : SyncController {


	public float speed;
	public float jumpSpeed;
	public float inAirMultiplier  = 0.25f;
	public float minimumDistanceToMove = 1.0f;
	public float minimumTimeUntilMove = 0.25f;
	public bool zoomEnabled;
	public float zoomEpsilon;
	public float zoomRate;
	public bool rotateEnabled;
	public float rotateEpsilon = 1f; // in degrees

	private Transform thisTransform;
	private CharacterController character;
	private AnimationController animationController;
	private Vector3 targetLocation;
	private float rotationVelocity;
	private Vector3 velocity;


	// Use this for initialization
	void  Start () {

		// Cache component lookups at startup instead of every frame
		thisTransform = transform;
		character = GetComponent<CharacterController>();
		animationController = GetComponent<AnimationController>();	
		
		// Set the maximum speed, so that the animation speed adjustment works	
		animationController.maxForwardSpeed = speed;
		
		// Initialize control state
		synJumping = synMoving = false;
		synReset = false;
		syncPosition = Vector3.zero;
		synTargetLocation = Vector3.zero;
	}

	void OnEndGame()
	{
		// Don't allow any more control changes when the game ends	
		this.enabled = false;
	}


	void FaceMovementDirection()
	{
		Vector3 horizontalVelocity = character.velocity;
		horizontalVelocity.y = 0; // Ignore vertical movement
		
		// If moving significantly in a new direction, point that character in that direction
		if( horizontalVelocity.magnitude > 0.1f )
			thisTransform.forward = horizontalVelocity.normalized;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		// Check for correct position
		if (!synMoving && synReset){
			synReset = false;
			synMoving = true;
			synTargetLocation = syncPosition;
		}

		// Check for jump
		if (character.isGrounded && synJumping){
			// Apply the current movement to launch velocity
			velocity = character.velocity;
			velocity.y = jumpSpeed;
			synJumping = false;
		}
		
		Vector3 movement = Vector3.zero;
		if (synMoving) {
			
			targetLocation = synTargetLocation;
			
			// Move towards the target location
			movement = targetLocation - thisTransform.position;
			movement.y = 0;
			float dist = movement.magnitude;
			
			if( dist < 0.5f )
			{
				synMoving = false;
			}
			else
			{
				movement = movement.normalized * speed;
			}
		}

		
		if (!character.isGrounded )
		{			
			// Apply gravity to our velocity to diminish it over time
			velocity.y += Physics.gravity.y * Time.deltaTime;
			
			// Adjust additional movement while in-air
			movement.x *= inAirMultiplier;
			movement.z *= inAirMultiplier;
		}
		
		movement += velocity;		
		movement += Physics.gravity;
		movement *= Time.deltaTime;
		
		// Actually move the character
		character.Move( movement );
		
		if ( character.isGrounded )
			// Remove any persistent velocity after landing
			velocity = Vector3.zero;
		
		// Face the character to match with where she is moving	
		FaceMovementDirection();
	}
}
