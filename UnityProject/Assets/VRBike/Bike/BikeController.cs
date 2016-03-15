using System;
using System.Collections;

using UnityEngine;

using MedRoad.VRBike;
using MedRoad.Utils;
using MedRoad.XSensGyroscope;
using MedRoad.Ant;

public class BikeController : MonoBehaviour {

    internal UIOverlay uiOverlay;
    internal UIDebugOverlay uiDebugOverlay;
    internal BikePhysics bikePhysics;
    internal BikeSteering bikeSteering;
    internal BikeData bikeData;

    internal XSensGyro xsensGyro;
    internal XSensGyroBridge xsensGyroBridge;

    internal AntStick antStick;
    internal AntStickBridge antStickBridge;

    #region Properties

    /// <summary>
    /// The character controller attached to this game object.
    /// </summary>
    private CharacterController characterController;

    /// <summary>
    /// The time the game is started at, which allows us to calculate the duration of the game.
    /// </summary>
    private DateTime _refTime;
    
    /// <summary>
    /// The time the game is started at, which allows us to calculate the duration of the game.
    /// </summary>
    public DateTime ReferenceTime
	{
		get { return this._refTime; }
	}

    /// <summary>
    /// Whether or not the timer has started for the game yet.
    /// </summary>
    private bool _timerStarted = false;

    /// <summary>
    /// Whether or not the timer has started for the game yet.
    /// </summary>
    public bool TimerStarted
	{
		get { return this._timerStarted; }
	}

    /// <summary>
    /// The altitude of the bike at the start of the game session. All altitudes recorded are
    /// relative to this altitude.
    /// </summary>
    private float _startingAltitude = float.NaN; // in m

    /// <summary>
    /// The total distance the bike has travelled since the start of the game session.
    /// </summary>
    private float _distanceTravelled = 0f; // in m

    /// <summary>
    /// Gets the total distance the bike has travelled since the start of the game session.
    /// </summary>
    public float DistanceTravelled
    {
        get { return this._distanceTravelled; }
    }

    /// <summary>
    /// The intial position of the bike at the start of the game.
    /// </summary>
    private Vector3 initialPosition = Vector3.zero;

    /// <summary>
    /// The initial rotation of the bike at the start of the game.
    /// </summary>
    private Quaternion initialRotation = Quaternion.Euler(0f, 0f, 0f);
    
    [SerializeField]
	private BikeAnimator bikeAnimator = null;

	[SerializeField]
	private LayerMask groundLayer = 0;
	public LayerMask GroundLayer
	{
		get { return groundLayer; }
		set { groundLayer = value; Update(); }
	}
	
	[SerializeField]
	[Range(0.0f, 2.5f)]
	private float groundHeight = 1.5f;
	public float GroundHeight
	{
		get { return groundHeight; }
		set { groundHeight = value; Update(); }
	}
	
	[SerializeField]
	public float currentSlope = 0f;
	
	[SerializeField]
	public float currentAltitude = 0f;
	
	[SerializeField]
	[Range(0.0f, 1.0f)]
	public float HeadingLerpAmount = 0.8f;
	
	[SerializeField]
	[Range(0.0f, 1.0f)]
	public float PitchLerpAmount = 0.03f;
	
	[SerializeField]
	[Range(-180.0f, 180.0f)]
	private float heading = 0f;
	public float Heading
	{
		get { return heading; }
		set
		{
			// clamp the angles
			while (value > 180.0f) value -= 360.0f;
			while (value < -180.0f) value += 360.0f;
			heading = value;
		}
	}

	[SerializeField]
	[Range(-180.0f, 180.0f)]
	private float pitch = 0f;
	public float Pitch
	{
		get { return pitch; }
		set
		{
			// clamp the angles
			while (value > 180.0f) value -= 360.0f;
			while (value < -180.0f) value += 360.0f;
			pitch = value;
		}
	}

    [SerializeField]
    AnimationCurve steeringCurve;

    #endregion

    /// <summary>
    /// This is a standard Unity method that resets components in the inspector.
    /// </summary>
    void Reset()
    {
        this.steeringCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    /// <summary>
    /// The standard Unity Start method.
    /// </summary>
	void Start () {
        // Set the initial position and rotation; set the character controller.
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        this.characterController = this.GetComponent<CharacterController>();

        // Reset position (altitude) and heading.
        this.ResetPositionAndHeadingToTransform();

        // Get a new BikeData object.
        bikeData = new BikeData();

        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        // Try to start Ant.
        try
        {
            this.antStick = new AntStick();
            this.antStickBridge = new AntStickBridge(antStick, bikeData);
            this.antStick.Start();
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("[BikeController] Exception while loading Ant.\n{0}", ex.Message);
        }

        // Try to start the XSens Gyro.
        try
        {
            this.xsensGyro = new XSensGyro();
            this.xsensGyroBridge = new XSensGyroBridge(xsensGyro, this);
            this.xsensGyro.Start();
            // We have to wait after starting the gyro to zero it.
            StartCoroutine(WaitThenZeroXSensGyro());
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("[BikeController] Exception while loading XSens Gyro. The DLL is probably missing.\n{0}", ex.Message);
        }

        #else

        Debug.LogWarning("[BikeController] ANT and XSens are not available on non-Windows platforms.");

        #endif

        // Initialize and attach all the supplemental components needed by Bike Controller.

        UIOverlay.SetBikeController(this);
        this.uiOverlay = gameObject.AddComponent<UIOverlay>();

        UIDebugOverlay.SetBikeController(this);
        this.uiDebugOverlay = gameObject.AddComponent<UIDebugOverlay>();

        this.bikePhysics = new BikePhysics(this);
		this.bikeSteering = new BikeSteering(this, this.steeringCurve);

        // Disable collisions between player character and the terrain.
        CharacterController characterController = this.GetComponent<CharacterController>();
        TerrainCollider terrainCollider = CollisionDisabler.GetTerrainColliderForActiveTerrain();
        (new CollisionDisabler(characterController, terrainCollider)).Start();

        
    }

    /// <summary>
    /// When starting the gyro we want to zero but we have to wait a second or so first.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitThenZeroXSensGyro()
    {
        yield return new WaitForSeconds(1.5f);
        Debug.Log("[BikeController] Initial XSens Gyro zeroing.");
        this.xsensGyroBridge.Zero();
    }

    /// <summary>
    /// Resets the CurrentAltitude property according to the current y position. Resets the Heading
    /// property according to the rotation along the y-axis of the bike's Transform. After manually
    /// setting the position or rotation of the bike this method needs to be called or the
    /// transform will be reset according to the last CurrentAltitude and Heading values.
    /// </summary>
    public void ResetPositionAndHeadingToTransform()
	{
        this.currentAltitude = this.transform.position.y - this.groundHeight;
        this.Heading = this.transform.rotation.eulerAngles.y;
	}

    /// <summary>
    /// The is a standard Unity method to do some cleanup when the game ends.
    /// </summary>
    void OnApplicationQuit()
    {
        // Stop any XSensGyro instances.
        XSensGyro.StopAll();

        // Stop any AntStick instances.
        AntStick.StopAll();
    }

    void FixedUpdate()
	{
        // Steering Update
        this.bikeSteering.DoOnFixedUpdate();
        this.Heading += this.bikeSteering.BikeHeadingChange;
        this.bikeAnimator.handlesAngle = this.bikeSteering.HandlesAnglePostProcessed;

        // Physics Update
        if (!this.bikeData.IgnoreKickrVelocity)
            this.bikePhysics.KickrVelocity = this.bikeData.BikeSpeed;
        this.bikePhysics.DoOnFixedUpdate();

        if (this.bikeData.UsingAntKickr)
            AntStickBridge.SendResistance(this.bikePhysics.KickrResistance);
        transform.position = this.bikePhysics.FallPosition;
        transform.rotation = Quaternion.LookRotation(this.bikePhysics.MovementDirection);
		
        // Perform Movement
		Vector3 desiredMove = this.bikePhysics.VelocityVector * Time.deltaTime;
		CollisionFlags flags = characterController.collisionFlags;
		if (flags == CollisionFlags.Sides)
		{   // COLLISION
			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, characterController.radius, Vector3.forward, out hitInfo, characterController.height/2f); 
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized * bikePhysics.Velocity * Time.deltaTime;
			
			float moveVelocity = desiredMove.magnitude;
			float acceleration = (bikePhysics.Velocity - moveVelocity) / Time.deltaTime;
			bikePhysics.ApplyCollisionForce(acceleration);

			this._distanceTravelled += (moveVelocity * Time.deltaTime);
		}
        else
        {   // NO COLLISION
			this._distanceTravelled += (bikePhysics.Velocity * Time.deltaTime);

			if (bikePhysics.Velocity >= 0f && bikePhysics.Velocity <= 200f) {
				this.bikeAnimator.wheelRPM = bikePhysics.Velocity / this.bikeData.BikeRPMToLinearVelocityFactor;
				this.bikeAnimator.pedalRPM = this.bikeAnimator.wheelRPM / 2f;
			}

		}
		characterController.Move(desiredMove);
	}

	void Update ()
    {	
		// Start the timer.
		if (!this._timerStarted && this.bikePhysics.Velocity > 0)
		{
			this._refTime = DateTime.Now;
			this._timerStarted = true;
		}
		// stop the timer if reachs trigger
		if (transform.position.z > -35)
		{
			this._timerStarted = false;
		}
        // default value of the speed sensitivity is 0
        if(bikeData.BikeSpeedSensitivity > 0)
        {
            // 60 is the mininum field of view and it increments 0.5 for 
            // every 0.1 value (e.g. 1.1 sent by phone = 60.5 field of view)
            Camera.main.fieldOfView = 60.0f + (bikeData.BikeSpeedSensitivity - 1.0f) * 5.0f;
        }

        #region HOVER_ABOVE_GROUND
		// make sure we maintain a position above the ground
		RaycastHit hit;
		Vector3 hitPoint = transform.position;
		Vector3 hitNormal = Vector3.up;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, 1 << groundLayer) ||
            Physics.Raycast(transform.position, Vector3.up,   out hit, Mathf.Infinity, 1 << groundLayer))
		{
			if (float.IsNaN(this._startingAltitude))
				this._startingAltitude = hit.point.y;
		
			this.currentAltitude = hit.point.y;
			
			// transform.position = new Vector3(transform.position.x, hit.point.y + groundHeight, transform.position.z);
			hitNormal = hit.normal;
			Debug.DrawLine(transform.position + (Vector3.up * 100), transform.position - (Vector3.up * 1000), Color.green);
		}
		else
			Debug.DrawLine(transform.position + (Vector3.up * 100), transform.position - (Vector3.up * 1000), Color.red);
        #endregion
		
        #region CALCULATE_SLOPE
		// ASSUME hit.normal.y is ALWAYS non-zero (i.e., the surface isn't perfectly vertical)
		// otherwise, destroy the universe with a /0
		float y = (-hitNormal.x * transform.forward.x - hitNormal.z * transform.forward.z) / hitNormal.y;
		Vector3 slopeDirection = new Vector3(transform.forward.x, y, transform.forward.z).normalized;
		
		// draw the slope direction so we know things are being proper
		Debug.DrawLine(hitPoint, hitPoint + slopeDirection, Color.cyan);
		
		// the slope is the angluar difference between the slope direction and up
		
		// downhill
		currentSlope = 90 - Mathf.Acos(Vector3.Dot(slopeDirection, Vector3.up)) * Mathf.Rad2Deg;
		
		//rot.x = -currentSlope;
        
		
        #endregion
		
		this.pitch = currentSlope;
	}

    /// <summary>
    /// When triggered, resets the player's position and rotation to their values at the start
    /// of the game.
    /// </summary>
    public void MovePlayerToInitialSpawn()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        ResetPositionAndHeadingToTransform();
    }

}
