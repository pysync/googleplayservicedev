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

public enum ControlState
{
	WaitingForFirstTouch,
	WaitingForSecondTouch,
	MovingCharacter,
	WaitingForMovement,
	ZoomingCamera,
	RotatingCamera,
	WaitingForNoFingers
}

public class TapController : SyncController {

	public GameObject cameraObject;
	public Transform cameraPivot;
	public GUITexture jumpButton;
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

	private ZoomCamera zoomCamera;
	private Camera cam ;
	private Transform thisTransform;
	private CharacterController character;
	private AnimationController animationController;
	private Vector3 targetLocation;
	private bool moving = false;
	private float rotationTarget;
	private float rotationVelocity;
	private Vector3 velocity;

	// State for tracking touches
	private ControlState state = ControlState.WaitingForFirstTouch;
	private int[] fingerDown = new int[ 2 ];
	private Vector2[] fingerDownPosition = new Vector2[ 2 ];
	private int[] fingerDownFrame = new int[ 2 ];
	private float firstTouchTime;


	// Use this for initialization
	void Start () {

		// Cache component lookups at startup instead of every frame
		thisTransform = transform;
		zoomCamera = cameraObject.GetComponent<ZoomCamera>();
		cam = cameraObject.camera;
		character = GetComponent<CharacterController>();
		animationController = GetComponent<AnimationController>();	
		
		// Set the maximum speed, so that the animation speed adjustment works	
		animationController.maxForwardSpeed = speed;
		
		// Initialize control state
		ResetControlState();
	}

	void OnEndGame()
	{
		// Don't allow any more control changes when the game ends	
		this.enabled = false;
	}


	void ResetControlState()
	{
		// Return to origin state and reset fingers that we are watching
		state = ControlState.WaitingForFirstTouch;
		fingerDown[ 0 ] = -1;
		fingerDown[ 1 ] = -1;
	}


	void FaceMovementDirection()
	{
		Vector3 horizontalVelocity = character.velocity;
		horizontalVelocity.y = 0; // Ignore vertical movement
		
		// If moving significantly in a new direction, point that character in that direction
		if( horizontalVelocity.magnitude > 0.1f )
			thisTransform.forward = horizontalVelocity.normalized;
	}

	void LateUpdate()
	{
		// Seek towards target rotation, smoothly
		Vector3 eulerAngles = cameraPivot.eulerAngles;
		eulerAngles.y = Mathf.SmoothDampAngle( cameraPivot.eulerAngles.y, rotationTarget, ref rotationVelocity, 0.3f);
		cameraPivot.eulerAngles = eulerAngles;
	}

	void CameraControl( Touch touch0  , Touch touch1 )
	{						
		if( rotateEnabled && state == ControlState.RotatingCamera )
		{			
			Vector2 currentVector = touch1.position - touch0.position;
			Vector2 currentDir = currentVector / currentVector.magnitude;
			Vector2 lastVector = ( touch1.position - touch1.deltaPosition ) - ( touch0.position - touch0.deltaPosition );
			Vector2 lastDir = lastVector / lastVector.magnitude;
			
			// Get the rotation amount between last frame and this frame
			float rotationCos = Vector2.Dot( currentDir, lastDir );
			
			if ( rotationCos < 1 ) // if it is 1, then we have no rotation
			{
				Vector3 currentVector3 = new Vector3( currentVector.x, currentVector.y, 0);
				Vector3 lastVector3 = new Vector3( lastVector.x, lastVector.y, 0);				
				float rotationDirection = Vector3.Cross( currentVector3, lastVector3 ).normalized.z;
				
				// Accumulate the rotation change with our target rotation
				float rotationRad = Mathf.Acos( rotationCos );
				rotationTarget += rotationRad * Mathf.Rad2Deg * rotationDirection;
				
				// Wrap rotations to keep them 0-360 degrees
				if ( rotationTarget < 0 )
					rotationTarget += 360;
				else if ( rotationTarget >= 360 )
					rotationTarget -= 360;
			}
		}
		else if( zoomEnabled && state == ControlState.ZoomingCamera )
		{
			float touchDistance = ( touch1.position - touch0.position ).magnitude;
			float lastTouchDistance = ( ( touch1.position - touch1.deltaPosition ) - ( touch0.position - touch0.deltaPosition ) ).magnitude;
			float deltaPinch = touchDistance - lastTouchDistance;	
			
			// Accumulate the pinch change with our target zoom
			zoomCamera.zoom += deltaPinch * zoomRate * Time.deltaTime;
		}
	}


	void CharacterControl()
	{
		int count = Input.touchCount;	
		if( count == 1 && state == ControlState.MovingCharacter )
		{
			Touch touch = Input.GetTouch(0);
			
			// Check for jump
			if ( character.isGrounded && jumpButton.HitTest( touch.position ) )
			{
				// Apply the current movement to launch velocity
				velocity = character.velocity;
				velocity.y = jumpSpeed;

				// Notify to sync input controller
				// 1. Move State: M = moving, J = jumping, R = reset
				// 2. Move data: x - z - y = 0
				SendInput('J');
			}
			else if ( !jumpButton.HitTest( touch.position ) && touch.phase != TouchPhase.Began )
			{
				// If we aren't jumping, then let's move to where the touch was placed
				var ray = cam.ScreenPointToRay( new Vector3( touch.position.x, touch.position.y) );
				
				RaycastHit hit;
				if( Physics.Raycast(ray, out hit) )
				{
					float touchDist  = (transform.position - hit.point).magnitude;
					if( touchDist > minimumDistanceToMove )
					{
						targetLocation = hit.point;
					}
					moving = true;
				}
			}
		}
		
		Vector3 movement = Vector3.zero;
		
		if( moving )
		{
			// Notify to sync input controller
			// 1. Move State: M = moving, J = jumping, R = reset
			// 2. Move data: x - z - y = 0
			SendInput('M', targetLocation.x, targetLocation.z);

			// Move towards the target location
			movement = targetLocation - thisTransform.position;
			movement.y=0;
			float dist = movement.magnitude;



			if( dist < 0.5f )
			{
				moving = false;
				// Notify to sync input controller
				// 1. Move State: M = moving, J = jumping, R = reset, P = position 
				// 2. Move data: x - z - y = 0
				SendInput('P', thisTransform.position.x, thisTransform.position.z);
			}
			else
			{
				movement = movement.normalized * speed;
			}
		}
		
		if ( !character.isGrounded )
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

	
	// Update is called once per frame
	void FixedUpdate () {
		// UnityRemote inherently introduces latency into the touch input received
		// because the data is being passed back over WiFi. Sometimes you will get 
		// an TouchPhase.Moved event before you have even seen an 
		// TouchPhase.Began. The following state machine takes this into
		// account to improve the feedback loop when using UnityRemote.


		int touchCount = Input.touchCount;
		if ( touchCount == 0 )
		{
			ResetControlState();
		}
		else
		{
			int i;
			Touch touch ;
			Touch[] touches = Input.touches;
			
			Touch touch0 = new Touch();
			Touch touch1 = new Touch();
			bool gotTouch0 = false;
			bool gotTouch1 = false;		
			
			// Check if we got the first finger down
			if ( state == ControlState.WaitingForFirstTouch )
			{
				for ( i = 0; i < touchCount; i++ )
				{
					touch = touches[ i ];
					
					if ( touch.phase != TouchPhase.Ended
					    && touch.phase != TouchPhase.Canceled )
					{
						state = ControlState.WaitingForSecondTouch;
						firstTouchTime = Time.time;
						fingerDown[ 0 ] = touch.fingerId;
						fingerDownPosition[ 0 ] = touch.position;
						fingerDownFrame[ 0 ] = Time.frameCount;
						break;
					}
				}
			}
			
			// Wait to see if a second finger touches down. Otherwise, we will
			// register this as a character move					
			if ( state == ControlState.WaitingForSecondTouch )
			{
				for ( i = 0; i < touchCount; i++ )
				{
					touch = touches[ i ];
					
					if ( touch.phase != TouchPhase.Canceled )
					{
						if ( touchCount >= 2 && touch.fingerId != fingerDown[ 0 ] )
						{
							// If we got a second finger, then let's see what kind of 
							// movement occurs
							state = ControlState.WaitingForMovement;
							fingerDown[ 1 ] = touch.fingerId;
							fingerDownPosition[ 1 ] = touch.position;
							fingerDownFrame[ 1 ] = Time.frameCount;						
							break;
						}
						else if ( touchCount == 1 )
						{
							//var deltaSinceDown = touch.position - fingerDownPosition[ 0 ];
							
							// Either the finger is held down long enough to count
							// as a move or it is lifted, which is also a move. 
							if ( touch.fingerId == fingerDown[ 0 ] &&
							    ( Time.time > firstTouchTime + minimumTimeUntilMove
							 || touch.phase == TouchPhase.Ended ) )
							{
								state = ControlState.MovingCharacter;
								break;
							}							
						}
					}
				}
			}
			
			// Now that we have two fingers down, let's see what kind of gesture is made			
			if ( state == ControlState.WaitingForMovement )
			{	
				// See if we still have both fingers	
				for ( i = 0; i < touchCount; i++ )
				{
					touch = touches[ i ];
					
					if ( touch.phase == TouchPhase.Began )
					{
						if ( touch.fingerId == fingerDown[ 0 ]
						    && fingerDownFrame[ 0 ] == Time.frameCount )
						{
							// We need to grab the first touch if this
							// is all in the same frame, so the control 
							// state doesn't reset.
							touch0 = touch;
							gotTouch0 = true;
						}
						else if ( touch.fingerId != fingerDown[ 0 ]
						         && touch.fingerId != fingerDown[ 1 ] )
						{
							// We still have two fingers, but the second
							// finger was lifted and touched down again
							fingerDown[ 1 ] = touch.fingerId;
							touch1 = touch;
							gotTouch1 = true;
						}
					}
					
					if ( touch.phase == TouchPhase.Moved
					    || touch.phase == TouchPhase.Stationary
					    || touch.phase == TouchPhase.Ended )
					{
						if ( touch.fingerId == fingerDown[ 0 ] )
						{
							touch0 = touch;
							gotTouch0 = true;
						}
						else if ( touch.fingerId == fingerDown[ 1 ] )
						{
							touch1 = touch;
							gotTouch1 = true;
						}
					}
				}
				
				if ( gotTouch0 )
				{
					if ( gotTouch1 )
					{
						Vector2 originalVector = fingerDownPosition[ 1 ] - fingerDownPosition[ 0 ];
						Vector2 currentVector = touch1.position - touch0.position;
						Vector2 originalDir = originalVector / originalVector.magnitude;
						Vector2 currentDir = currentVector / currentVector.magnitude;
						float rotationCos = Vector2.Dot( originalDir, currentDir );
						
						if ( rotationCos < 1 ) // if it is 1, then we have no rotation
						{
							var rotationRad = Mathf.Acos( rotationCos );
							if ( rotationRad > rotateEpsilon * Mathf.Deg2Rad )
							{
								// Enough rotation was applied with the two-finger movement,
								// so let's switch to rotate the camera
								state = ControlState.RotatingCamera;
							}
						}
						
						// If we aren't rotating the camera, then let's check for a zoom
						if ( state == ControlState.WaitingForMovement )
						{
							float deltaDistance = originalVector.magnitude - currentVector.magnitude;
							if ( Mathf.Abs( deltaDistance ) > zoomEpsilon )
							{
								// The distance between fingers has changed enough
								// to count this as a pinch
								state = ControlState.ZoomingCamera;
							}
						}		
					}
				}
				else
				{
					// A finger was lifted, so let's just wait until we have no fingers
					// before we reset to the origin state
					state = ControlState.WaitingForNoFingers;
				}
			}	
			
			// Now that we are either rotating or zooming the camera, let's keep
			// feeding those changes until we no longer have two fingers
			if ( state == ControlState.RotatingCamera
			    || state == ControlState.ZoomingCamera )
			{
				for ( i = 0; i < touchCount; i++ )
				{
					touch = touches[ i ];
					
					if ( touch.phase == TouchPhase.Moved
					    || touch.phase == TouchPhase.Stationary
					    || touch.phase == TouchPhase.Ended )
					{
						if ( touch.fingerId == fingerDown[ 0 ] )
						{
							touch0 = touch;
							gotTouch0 = true;
						}
						else if ( touch.fingerId == fingerDown[ 1 ] )
						{
							touch1 = touch;
							gotTouch1 = true;
						}
					}
				}
				
				if ( gotTouch0 )
				{
					if ( gotTouch1 )
					{
						CameraControl( touch0, touch1 );
					}
				}
				else
				{
					// A finger was lifted, so let's just wait until we have no fingers
					// before we reset to the origin state
					state = ControlState.WaitingForNoFingers;
				}
				
			}		
		}
		
		// Apply character movement if we have any		
		CharacterControl();
	}
}
