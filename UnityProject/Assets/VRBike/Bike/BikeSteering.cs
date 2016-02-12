using UnityEngine;
using System.Collections;

/// <summary>
/// BikeSteering is a helper class for the BikeController class that handles processing and
/// calculations to convert the raw angle of the handles into a rotation for the bike game object.
/// </summary>
public class BikeSteering {

    protected const float DEFAULT_DEAD_ZONE_SIZE = 2f;

    protected BikeController bikeController;

    private float _handlesHeadingCenter =  0f;
	private float _handlesHeadingLeft   = -90f;
    private float _handlesHeadingRight  =  90f;
 
    private float _relHandlesHeadingLeft;
    private float _relHandlesHeadingRight;

    private float _deadZoneSize  = DEFAULT_DEAD_ZONE_SIZE;
    private bool  _applySteering = true;
	
	private AnimationCurve _steeringCurve = null;
	
	private float _handlesAngle;
    private float _handlesAngleCentered;
    private float _handlesAngleScaled;
    private float _handlesAnglePostProcessed;

    private float _bikeHeadingChange;

    #region Properties

    /// <summary>
    /// Gets or sets the center point for the heading of the handles in degrees. This is
    /// the position of the handles that we take to be the center position (i.e., steering
    /// straight ahead).
    /// </summary>
    /// <value>The heading of the handles we consider the center position.</value>
    public float HandlesHeadingCenter
	{
		get { return this._handlesHeadingCenter; }
		set {
            this._handlesHeadingCenter = value;
            this.SetRelativeLeftAndRightPoints();
        }
	}
	
	/// <summary>
	/// Gets or sets the left-most point for the heading of the handles in degrees. This is
	/// the position of the handles that we take to be the farthest left position possible
	/// (i.e., steering 90 degrees to the left).
	/// </summary>
	/// <value>The heading of the handles we consider the left-most position.</value>
	public float HandlesHeadingLeft
	{
		get { return this._handlesHeadingLeft; }
		set {
            this._handlesHeadingLeft = value;
            this.SetRelativeLeftAndRightPoints();
        }
	}
	
	/// <summary>
	/// Gets or sets the right-most point for the heading of the handles in degrees. This is
	/// the position of the handles that we take to be the farthest right position possible
	/// (i.e., steering 90 degrees to the right).
	/// </summary>
	/// <value>The heading of the handles we consider the right-most position.</value>
	public float HandlesHeadingRight
	{
		get { return this._handlesHeadingRight; }
		set {
            this._handlesHeadingRight = value;
            this.SetRelativeLeftAndRightPoints();
        }
	}

    /// <summary>
    /// Gets the left-most point for the heading of the handles after it has been
    /// translated and clamped relative to HandlesHeadingCenter. This value will be
    /// in [-180, 0).
    /// </summary>
    /// <value>The heading of the handles we consider the left-most position
    /// relative to the center point.</value>
    public float RelativeHandlesHeadingLeft
    {
        get { return this._relHandlesHeadingLeft; }
    }

    /// <summary>
    /// Gets the right-most point for the heading of the handles after it has been
    /// translated and clamped relative to HandlesHeadingCenter. This value will be
    /// in (0, 180].
    /// </summary>
    /// <value>The heading of the handles we consider the right-most position
    /// relative to the center point.</value>
    public float RelativeHandlesHeadingRight
    {
        get { return this._relHandlesHeadingRight; }
    }

    /// <summary>
    /// Gets or sets the size of the dead zone in degrees. In the case that
    /// <c>ABS(handlesAngle - HandlesHeadingCenter) < DeadZoneSize</c> we treat it
    /// as if the handlesAngle is zero. 
    /// </summary>
    /// <value>The size of the dead zone.</value>
    public float DeadZoneSize
	{
		get { return this._deadZoneSize; }
		set { this._deadZoneSize = value; }
	}
	
	/// <summary>
	/// Turns steering on or off. If <c>false</c>, CalculateScaledHandlesAngle will always
	/// return 0 (as if the player is steering straight ahead).
	/// </summary>
	/// <value><c>true</c> if steering is on; otherwise, <c>false</c>.</value>
	public bool ApplySteering
	{
		get { return this._applySteering; }
		set { this._applySteering = value; }
	}
	
	/// <summary>
	/// Gets or sets an animation curve that we will use as a steering curve, mapping the
	/// calculated angle to a new angle defined by the steering curve. The x-axis represents
	/// a handles angle between 0.0 = 0 deg and 1.0 = 90 deg and the y-axis represents the
	/// new value to map a given handles angle to (again, with 0.0 = 0 deg and 1.0 = 90 deg).
	/// By default the steering curve used is the linear line y = x which has no effect. 
	/// </summary>
	/// <value>The steering curve.</value>
	public AnimationCurve SteeringCurve
	{	
		get { return this._steeringCurve; }
		set { this._steeringCurve = value; }
	}

    /// <summary>
    /// Gets the raw angle for the heading of the handles in degrees.
    /// </summary>
    public float HandlesAngle
    {
        get { return this._handlesAngle; }
    }

    /// <summary>
    /// Gets the centered angle for the heading of the handles in degrees.
    /// This is the value of HandlesAngle after it has been translated relative to
    /// HandlesHeadingCenter. This value should be 0 when the handle bars are straight.
    /// </summary>
    public float HandlesAngleCentered
    {
        get { return this._handlesAngleCentered; }
    }

    /// <summary>
    /// Gets the scaled angle for the heading of the handles in degrees.
    /// This is the value of HandlesAngleCentered after it has been scaled between
    /// HandlesHeadingLeft and HandlesHeadingRight.
    /// </summary>
    public float HandlesAngleScaled
    {
        get { return this._handlesAngleScaled; }
    }

    /// <summary>
    /// Gets the post-processed angle for the heading of the handles in degrees.
    /// This is the value of HandlesAngleScaled after any additional post-processing has
    /// been applied. For instance, if a custom steering curve function has been set it
    /// will be applied here.
    /// </summary>
    public float HandlesAnglePostProcessed
    {
        get { return this._handlesAnglePostProcessed; }
    }

    /// <summary>
    /// Gets the change in bike heading that should be applied as calculated based on
    /// the current angle of the handles.
    /// </summary>
    public float BikeHeadingChange
    {
        get { return this._bikeHeadingChange; }
    }

    #endregion

    /// <summary>
    /// Instantiate a new BikeSteering class that is connected to the given BikeController.
    /// </summary>
    /// <param name="bikeController">The BikeController this BikeSteering instance is for.</param>
    public BikeSteering(BikeController bikeController) : this(bikeController, null) { }

    /// <summary>
    /// Instantiate a new BikeSteering class that is connected to the given BikeController.
    /// </summary>
    /// <param name="bikeController">The BikeController this BikeSteering instance is for.</param>
    /// <param name="bikeSteeringCurve">Gets or sets an animation curve that we will use as a
    /// steering curve, mapping the calculated angle to a new angle defined by the steering curve.
    /// The x-axis represents a handles angle between 0.0 = 0 deg and 1.0 = 90 deg and the y-axis
    /// represents the new value to map a given handles angle to (again, with 0.0 = 0 deg and
    /// 1.0 = 90 deg). By default the steering curve used is the linear line y = x which has no
    /// effect. </param>
    public BikeSteering(BikeController bikeController, AnimationCurve bikeSteeringCurve)
    {
        this.bikeController = bikeController;
        this.SetRelativeLeftAndRightPoints();
        this._steeringCurve = bikeSteeringCurve;
    }

    /// <summary>
    /// This method is called internally whenever HandlesHeadingCenter, HandlesHeadingLeft, or
    /// HandlesHeadingRight is set to recalculate the relative left and right points.
    /// </summary>
    private void SetRelativeLeftAndRightPoints()
    {
        this._relHandlesHeadingLeft  = TareAngle(this._handlesHeadingLeft,  this._handlesHeadingCenter);
        this._relHandlesHeadingRight = TareAngle(this._handlesHeadingRight, this._handlesHeadingCenter);

        if (this._relHandlesHeadingLeft >= 0)
        {
            this._relHandlesHeadingLeft = -180f;
            Debug.LogWarningFormat("BAD STEERING CALIBRATION: Left calibration point ({0}) is more than 180 deg left of center point ({1}).",
                                   this._handlesHeadingLeft, this._handlesHeadingCenter);
        }

        if (this._relHandlesHeadingRight <= 0)
        {
            this._relHandlesHeadingRight = 180f;
            Debug.LogWarningFormat("BAD STEERING CALIBRATION: Right calibration point ({0}) is more than 180 deg right of center point ({1}).",
                                   this._handlesHeadingRight, this._handlesHeadingCenter);
        }
    }

    /// <summary>
    /// Tares the angle. Subtracts the given centerPoint from the given angle (so that centerPoint
    /// becomes the new 0 deg angle). The resulting angle is then normalized between [-180, 180)
    /// degrees.
    /// </summary>
    /// <returns>The tared and normalized angle.</returns>
    /// <param name="angle">The angle in degrees to tare.</param>
    /// <param name="centerPoint">An angle in degrees to take as the zero point.</param>
    public static float TareAngle(float angle, float centerPoint)
	{
		float relativeAngle = angle - centerPoint;
		
		while (relativeAngle > 180.0f)   relativeAngle -= 360.0f;
		while (relativeAngle <= -180.0f) relativeAngle += 360.0f;
		return relativeAngle;
	}

    #region Handles Angle Processing

    /// <summary>
    /// Given an angle in degrees, return an angle relative to HandlesHeadingCenter (i.e., return
    /// an angle between -180 and 180 where angle = HandlesHeadingCenter would be the zero point).
    /// Furthermore, angles within the dead zone will be treated as zero and angles outside of the
    /// dead zone will be shifted towards the center point.
    /// </summary>
    /// <param name="angle">Any angle in degrees (usually the raw angle of the handles).</param>
    /// <returns>An angle in [-180, 180) relative to HandlesHeadingCenter.</returns>
    public virtual float CenterHandlesAngle(float angle)
    {
        angle = TareAngle(angle, this._handlesHeadingCenter);

        if (Mathf.Abs(angle) < this._deadZoneSize) return 0f;
        angle += (angle < 0) ? this._deadZoneSize : -this._deadZoneSize;

        return angle;
    }

    /// <summary>
    /// Scales the given heading according to the values HandlesHeadingLeft, and
    /// HandlesHeadingRight. Values past the left and right points are clamped to +/- 90 deg and
    /// values inbetween are scaled accordingly.
    /// </summary>
    /// <param name="angle">An angle that has been centered on HandlesHeadingCenter.</param>
    /// <returns>The scaled heading in degrees in [-90, 90].</returns>
    public virtual float ScaleHandlesAngle(float angle)
	{	
		if (angle < 0) {
			if (angle < this._relHandlesHeadingLeft)
                angle = -90f;
            else
                angle = (angle / this._relHandlesHeadingLeft) * -90f;
			
		} else if (angle > 0) {
			if (angle > this._relHandlesHeadingRight)
                angle = 90f;
			else
                angle = (angle / this._relHandlesHeadingRight) * 90f;
		}

        return angle;
    }

    /// <summary>
    /// Perform post-processing on the given angle.
    /// </summary>
    /// <param name="angle">An angle that has been centered and scaled.</param>
    /// <returns>The post-processed angle.</returns>
    public virtual float PostProcessHandlesAngle(float angle)
    {
        // Apply the steering curve function, if one exists.
        if (this._steeringCurve != null)
            if (angle >= 0)
                angle =  90f * this._steeringCurve.Evaluate(angle /  90f);
            else
                angle = -90f * this._steeringCurve.Evaluate(angle / -90f);

        // After testing I found this unnecessary.
        // Round heading into buckets of 5 degrees.
        // angle = 5f * Mathf.Round(angle / 5f);

        return angle;
    }

	#endregion
	
	#region Steer Angle to Turn Radius
	
	/// <summary>
	/// Convert the given angle of the handles to a turn radius. The function used here is based
	/// on Andrew Dressel's measurements of steer angle and lean angle vs. turn radius. We assume
	/// the lean angle is 0-5 degrees.
	/// 
	/// https://pantherfile.uwm.edu/adressel/www/
	/// https://pantherfile.uwm.edu/adressel/www/index_files/Bike_lean_and_steer_angles_vs_turn_radius.png
	/// </summary>
	/// <returns>The turn radius in meters of the bike for the given angle of the handles.
    /// </returns>
	/// <param name="handlesAngle">The angle of the handlebars in degrees (0 is straight ahead,
	/// positive is to the right, and negative is to the left).</param>
	public static float SteerAngleToTurnRadius(float handlesAngle)
	{
		float functionConstant = (handlesAngle > 0) ? 80.51486491f : -80.51486491f;
		return functionConstant * Mathf.Pow(Mathf.Abs(handlesAngle), -0.9735792741f);
	}
	
	#endregion

	/// <summary>
	/// A method for calculating heading change based on Andrew Dressel's measurements of steer
    /// angle and lean angle vs. turn radius.
	/// </summary>
	/// <returns>The number of degrees to rotate the heading of the bike by.</returns>
	/// <param name="handlesAngle">The angle of the handles in degrees.</param>
	/// <param name="bikeVelocity">The velocity of the bike in m/s.</param>
	/// <param name="deltaTime">The time in seconds since the last heading change.</param>
	public virtual float GetHeadingChange(float handlesAngleProcessed, float bikeVelocity)
	{
        if (bikeVelocity == 0f)
            return 0f;

        float distanceMoved = bikeVelocity * Time.deltaTime;
		
		// We know the distance moved and the turn radius, taking the arccos of
		// that gets us the change in heading the bike has experienced.
		float headingChange = 90f - Mathf.Rad2Deg * Mathf.Acos((distanceMoved/2f) / SteerAngleToTurnRadius(handlesAngleProcessed));

        // The above should theoretically be distanceMoved/2f but we want to decrease the
        // sensitivity just a bit.

		return headingChange;
	}

	/// <summary>
	/// A naive method for calculating heading change that simply scales the scaled handles angle
    /// by the deltaTime.
	/// </summary>
	/// <returns>The number of degrees to rotate the heading of the bike by.</returns>
	/// <param name="handlesAngle">The angle of the handles in degrees.</param>
	/// <param name="bikeVelocity">The velocity of the bike in m/s.</param>
	/// <param name="deltaTime">The time in seconds since the last heading change.</param>
	public virtual float GetSimpleHeadingChange(float handlesAngleProcessed, float bikeVelocity)
	{
        if (bikeVelocity == 0f)
            return 0f;

        return handlesAngleProcessed * Time.deltaTime;
	}

    /// <summary>
    /// This method is called by BikeController on every fixed update.
    /// </summary>
    public virtual void DoOnFixedUpdate()
    {
        if (!this._applySteering) {
            this._handlesAngle = 0f;
            this._handlesAngleCentered = 0f;
            this._handlesAngleScaled = 0f;
            this._handlesAnglePostProcessed = 0f;
            this._bikeHeadingChange = 0f;
            return;
        }

        this._handlesAngle = this.bikeController.bikeData.BikeGYR[0];
        this._handlesAngleCentered = this.CenterHandlesAngle(this._handlesAngle);
        this._handlesAngleScaled = this.ScaleHandlesAngle(this._handlesAngleCentered);
        this._handlesAnglePostProcessed = this.PostProcessHandlesAngle(this._handlesAngleScaled);

        // Use Dressel's measurements to calculate heading change.
        this._bikeHeadingChange = this.GetHeadingChange(
            this._handlesAnglePostProcessed, this.bikeController.bikePhysics.Velocity);

        // Naive method for calculating heading change.
        // this._bikeHeadingChange = this.GetSimpleHeadingChange(
        // this._handlesAnglePostProcessed, this.bikeController.bikePhysics.Velocity);
    }

}
