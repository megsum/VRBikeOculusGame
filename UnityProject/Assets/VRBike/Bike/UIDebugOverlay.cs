using System;

using UnityEngine;
using UnityEngine.UI;

using MedRoad.Utils;

/// <summary>
/// This object populates and controls the debugging overlay.
/// </summary>
public class UIDebugOverlay : MonoBehaviour {

    private static BikeController bikeController;
	
	public static void SetBikeController(BikeController bikeController)
	{
		UIDebugOverlay.bikeController = bikeController;
	}

	private static KeyCode DebugKeySteering = KeyCode.Keypad1;
    private static KeyCode DebugKeySteeringZeroGyro = KeyCode.Z;
    private static KeyCode DebugKeySteeringOnOff = KeyCode.UpArrow;
    private static KeyCode DebugKeySteeringSetCenter = KeyCode.DownArrow;
    private static KeyCode DebugKeySteeringSetLeft = KeyCode.LeftArrow;
    private static KeyCode DebugKeySteeringSetRight = KeyCode.RightArrow;
    private static KeyCode DebugKeySteeringArrowModePrev = KeyCode.LeftBracket;
    private static KeyCode DebugKeySteeringArrowModeNext = KeyCode.RightBracket;

    private static KeyCode DebugKeyCruiseControl = KeyCode.Keypad2;
	private static KeyCode DebugKeyCruiseControlUp = KeyCode.PageUp;
	private static KeyCode DebugKeyCruiseControlDown = KeyCode.PageDown;
	private static KeyCode DebugKeyCruiseControlModifier = KeyCode.RightShift;

	private static KeyCode DebugKeyNetworking = KeyCode.Keypad3;

	private static KeyCode DebugKeyPhysics = KeyCode.Keypad4;

	private static KeyCode DebugKeyFPS = KeyCode.Keypad5;

    private static KeyCode DebugKeyCameraPositionNext = KeyCode.Period;
    private static KeyCode DebugKeyCameraPositionPrev = KeyCode.Comma;

	private static KeyCode DebugKeyBackToSpawn = KeyCode.Keypad9;

    private StandardUI stdUI;

    void Start () {
        // Get the Standard UI
        this.stdUI = GameObject.FindObjectOfType<StandardUI>();
        if (this.stdUI == null)
        {
            Debug.LogWarning("[UIDebugOverlay] StandardUI not found! UIDebugOverlay is DISABLED.");
            return;
        }

        // Get the current position of the bike camera
        this.bikeCameraTransform = bikeController.transform.Find("BikeCamera");
        this.camPositions[0] = new PositionRotation(this.bikeCameraTransform);
	}

	void Update () {
        if (this.stdUI == null)
            return;

        this.Steering();
		this.CruiseControl();
		this.NetworkingAndPhysics();
		this.FPS();
        this.CameraPosition();
		this.BackToSpawn();
	}

	#region STEERING

	private bool? debugSteering = null;
	private HeadingArrowMode headingArrowMode = HeadingArrowMode.Scaled;

	private enum HeadingArrowMode
	{
		Min = 0,
		Scaled = 1,
		Centered = 2,
		Raw = 3,
		Max = 4,
	}

	private void Steering()
	{
        // XSens Gyro can be reset without displaying debugging arrow.
        if (Input.GetKeyDown(DebugKeySteeringZeroGyro) && bikeController.xsensGyro != null)
            bikeController.xsensGyroBridge.Zero();

        if (Input.GetKeyDown(DebugKeySteering)) {
			ToggleDebug(ref this.debugSteering, this.stdUI.DebugHeadingText);
			this.stdUI.DebugHeadingArrow.gameObject.SetActive(this.debugSteering.Value);
			this.stdUI.DebugHeadingStraightArrow.gameObject.SetActive(this.debugSteering.Value);
		}
		
		if (!this.debugSteering.HasValue || !this.debugSteering.Value)
			return;

		if (Input.GetKeyDown(DebugKeySteeringOnOff))
			bikeController.bikeSteering.ApplySteering = !bikeController.bikeSteering.ApplySteering;
		
		if (Input.GetKeyDown(DebugKeySteeringSetCenter))
			bikeController.bikeSteering.HandlesHeadingCenter = bikeController.bikeData.BikeGYR[0];
		
		if (Input.GetKeyDown(DebugKeySteeringSetLeft))
			bikeController.bikeSteering.HandlesHeadingLeft = bikeController.bikeData.BikeGYR[0];
		
		if (Input.GetKeyDown(DebugKeySteeringSetRight))
			bikeController.bikeSteering.HandlesHeadingRight = bikeController.bikeData.BikeGYR[0];

		if (Input.GetKeyDown(DebugKeySteeringArrowModePrev))
			this.ChangeHeadingArrowMode(false);
			
		if (Input.GetKeyDown(DebugKeySteeringArrowModeNext))
			this.ChangeHeadingArrowMode(true);

        // \u00A0 is a non-breaking space - Unity will trim whitespace
        // on the ends of a line EVEN IN THE MIDDLE OF A STRING.
        this.stdUI.DebugHeadingText.text = String.Format(
            "\u00A0Steering: {6}  Arrow Mode: {7,-13}\u00A0\n" +
		    "LEFT:   {0: 000;-000}  CENTER: {1: 000;-000}  RIGHT:  {2: 000;-000}\n" +
		    "RAW:    {3: 000;-000}  CENTERD:{4: 000;-000}  SCALED: {5: 000;-000}",
		    bikeController.bikeSteering.HandlesHeadingLeft,
		    bikeController.bikeSteering.HandlesHeadingCenter,
		    bikeController.bikeSteering.HandlesHeadingRight,
		    bikeController.bikeSteering.HandlesAngle,
		    bikeController.bikeSteering.HandlesAngleCentered,
		    bikeController.bikeSteering.HandlesAngleScaled,
		    bikeController.bikeSteering.ApplySteering ? "ON " : "OFF",
		    this.headingArrowMode.ToString());

        float headingArrowAngle =
            bikeController.bikeData.BikeGYR[0] - bikeController.bikeSteering.HandlesHeadingCenter;

        switch (this.headingArrowMode)
		{
		case HeadingArrowMode.Scaled:
			headingArrowAngle = bikeController.bikeSteering.HandlesAngleScaled;
			break;
		case HeadingArrowMode.Centered:
			headingArrowAngle = bikeController.bikeSteering.HandlesAngleCentered;
			break;
		case HeadingArrowMode.Raw:
			headingArrowAngle = bikeController.bikeSteering.HandlesAngle;
			break;
		}

        this.stdUI.DebugHeadingArrow.rectTransform.rotation =
            Quaternion.AngleAxis(headingArrowAngle, Vector3.back);
	}

	private void ChangeHeadingArrowMode(bool increment)
	{
		int newMode = (int) this.headingArrowMode;
		if (increment) {
			newMode++;
			if (newMode >= (int) HeadingArrowMode.Max)
				newMode = (int) HeadingArrowMode.Min+1;
			
		} else {
			newMode--;
			if (newMode <= (int) HeadingArrowMode.Min)
				newMode = (int) HeadingArrowMode.Max-1;
		}
		
		this.headingArrowMode = (HeadingArrowMode) newMode;
	}

	#endregion

	#region CRUISE CONTROL

	private const float DEBUG_SPEED_INCREMENT = 0.25f; // in m/s
	private const float DEBUG_SPEED_MODIFIER_FACTOR = 5f;

	private bool? debugSpeed = null;
	
    private void CruiseControl()
	{
		if (Input.GetKeyDown (DebugKeyCruiseControl)) {
			ToggleDebug (ref this.debugSpeed, this.stdUI.DebugSpeedText);
			bikeController.bikeData.IgnoreKickrVelocity = (this.debugSpeed.HasValue) ? this.debugSpeed.Value : false;
		}

		if (!this.debugSpeed.HasValue || !this.debugSpeed.Value)
			return;

		float modifier = (Input.GetKey(DebugKeyCruiseControlModifier)) ? DEBUG_SPEED_MODIFIER_FACTOR : 1f;
		
		if (Input.GetKeyDown(DebugKeyCruiseControlUp)) {
			bikeController.bikePhysics.KickrVelocity += modifier * DEBUG_SPEED_INCREMENT;
		}
		if (Input.GetKeyDown(DebugKeyCruiseControlDown)) {
			bikeController.bikePhysics.KickrVelocity -= modifier * DEBUG_SPEED_INCREMENT;
			if (bikeController.bikePhysics.KickrVelocity < 0f)
				bikeController.bikePhysics.KickrVelocity = 0f;
		}
	}

	#endregion

	#region NETWORKING/PHYSICS

	private bool? debugNetworking = null;
	private bool? debugPhysics = null;

	private void NetworkingAndPhysics()
	{
		if (Input.GetKeyDown(DebugKeyNetworking)) {
			ToggleDebug(ref this.debugNetworking, this.stdUI.DebugNetworkingPhysicsText);

			if (this.debugNetworking.Value && (!this.debugPhysics.HasValue || this.debugPhysics.Value))
				this.debugPhysics = false;
		}

		if (Input.GetKeyDown(DebugKeyPhysics)) {
			ToggleDebug(ref this.debugPhysics, this.stdUI.DebugNetworkingPhysicsText);

			if (this.debugPhysics.Value && (!this.debugNetworking.HasValue || this.debugNetworking.Value)) {
				this.debugNetworking = false;
			}
		}
		
		if (!this.debugPhysics.HasValue || !this.debugPhysics.Value)
			return;

        stdUI.DebugNetworkingPhysicsText.text = String.Format(
			"Heading: {0:F1}    Pitch: {1:F1}\n\n" +

            "FORCES (along fwd. mov.)\n" +
			"Gravity:   {2:F5} N\n" +
			"Tire Drag: {3:F5} N\n" +
			"Wind:      {4:F5} N\n" +
			"Collision: {5:F5} N\n\n" +

			"Fall Vel.:  {6:F3} m/s  {7}\n" +
			"Kickr Vel.: {9:F3} m/s\n" +
			"Final Vel.: {10:F3} m/s\n\n" +

			"Max Kickr Rest. At: {11:F1} N\n" +
			"Kickr Resistance:   {8:F1}",
			bikeController.bikePhysics.Heading,
			bikeController.bikePhysics.Pitch,
			bikeController.bikePhysics.GravityForce,
			bikeController.bikePhysics.TireDragForce,
			bikeController.bikePhysics.WindForce,
			bikeController.bikePhysics.CollisionForce,
			bikeController.bikePhysics.FallVelocity,
			(bikeController.bikePhysics.OffGround) ? "FALLING" : "",
			bikeController.bikePhysics.KickrResistance,
			bikeController.bikePhysics.KickrVelocity,
			bikeController.bikePhysics.Velocity,
			bikeController.bikePhysics.MaxForceForResistance);
	}

	#endregion

	#region FPS

	private const float DEBUG_FPS_UPDATE_TIME = 1f; // in seconds
	
	private bool? debugFPS = null;
	private float debugFPSTimeSum;
	private int   debugFPSTimeSamples;

	private void FPS()
	{
		if (Input.GetKeyDown(DebugKeyFPS)) {
			ToggleDebug(ref this.debugFPS, this.stdUI.DebugFPSText);
		}

		if (!this.debugFPS.HasValue || !this.debugFPS.Value)
			return;

		this.debugFPSTimeSum += Time.deltaTime;
		this.debugFPSTimeSamples++;

		if (this.debugFPSTimeSum >= DEBUG_FPS_UPDATE_TIME) {
			this.stdUI.DebugFPSText.text = String.Format(
                "FPS: {0:F1}", this.debugFPSTimeSamples / this.debugFPSTimeSum);

			this.debugFPSTimeSum = 0f;
			this.debugFPSTimeSamples = 0;
		}
	}

    #endregion

    #region CAMERA POSITION

    private struct PositionRotation
    {
        public Vector3 position;
        public Quaternion rotation;
        public PositionRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
        public PositionRotation(Transform transform)
        {
            this.position = transform.localPosition;
            this.rotation = transform.localRotation;
        }
        public void ApplyToTransform(Transform transform)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
        }
    }

    private static PositionRotation CamBehind  = new PositionRotation(new Vector3( 0f, 1.5f, -3f), Quaternion.Euler(15f,   0f, 0f));
    private static PositionRotation CamRight   = new PositionRotation(new Vector3( 4f,   0f,  1f), Quaternion.Euler( 0f, 270f, 0f));
    private static PositionRotation CamFront   = new PositionRotation(new Vector3( 0f,   0f,  4f), Quaternion.Euler(15f, 180f, 0f));
    private static PositionRotation CamLeft    = new PositionRotation(new Vector3(-4f,   0f,  1f), Quaternion.Euler( 0f,  90f, 0f));
    private static PositionRotation CamTopDown = new PositionRotation(new Vector3( 0f,   3f,  1f), Quaternion.Euler(90f,   0f, 0f));

    private Transform bikeCameraTransform;
    private PositionRotation[] camPositions = { new PositionRotation(), CamBehind, CamRight, CamFront, CamLeft, CamTopDown };
    private int camPositionIndex = 0;

    private void CameraPosition()
    {
        bool changeCamera = false;

        if (Input.GetKeyDown(DebugKeyCameraPositionPrev))
        {
            this.camPositionIndex--;
            if (this.camPositionIndex < 0) this.camPositionIndex = this.camPositions.Length - 1;
            changeCamera = true;
        }

        if (Input.GetKeyDown(DebugKeyCameraPositionNext))
        {
            this.camPositionIndex++;
            if (this.camPositionIndex >= this.camPositions.Length) this.camPositionIndex = 0;
            changeCamera = true;
        }

        if (changeCamera)
            this.camPositions[this.camPositionIndex].ApplyToTransform(bikeCameraTransform);
    }

    #endregion

    #region BACK TO SPAWN

    private void BackToSpawn()
	{
		if (Input.GetKeyDown(DebugKeyBackToSpawn))
            ScreenFader.PerformScreenFadeInOut(2f, bikeController.MovePlayerToInitialSpawn);
	}

    #endregion

    /// <summary>
    /// If the given property is null, finds the DrawTextOnGUI on the given propertyText and
    /// resets it. The property will then be set to <c>true</c> and the propertyText will be
    /// made active.
    /// 
    /// Otherwise, the value of property is toggled and propertyText is set to be active or
    /// inactive based on the new value of the property.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="propertyText"></param>
    private static void ToggleDebug(ref bool? property, Text propertyText)
	{
		if (!property.HasValue) {
			DrawTextOnGUI drawTextOnGUIComponent = propertyText.gameObject.GetComponent<DrawTextOnGUI>();
			if (drawTextOnGUIComponent != null)
				drawTextOnGUIComponent.ResetTargetText();
		}
		
		property = (property.HasValue) ? !property.Value : true;
		if (propertyText != null)
			propertyText.gameObject.SetActive(property.Value);
	}

}
