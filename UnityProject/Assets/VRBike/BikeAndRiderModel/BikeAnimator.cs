using UnityEngine;

public class BikeAnimator : MonoBehaviour {

    [Header("Bike Parameters")]
    [SerializeField]
    public float wheelRPM = 60f;

    [SerializeField]
    public float pedalRPM = 45f;

    [SerializeField]
    [Range(-90f, 90f)]
    public float handlesAngle = 0f;

    [SerializeField]
    public bool ApplyHandlesAngleCurve = true;

    [SerializeField]
    public AnimationCurve handlesAngleCurve = null;

    [Header("Animation Lerp Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    public float wheelRPMLerpRate = 0.1f;

    [SerializeField]
    [Range(0f, 1f)]
    public float pedalRPMLerpRate = 0.1f;

    [SerializeField]
    [Range(0f, 1f)]
    public float handlesAngleLerpRate = 0.1f;

    [Header("Animation Clips")]
    [SerializeField]
    string steeringLayerName = "Steering";

	[SerializeField]
    string pedalRevsParameterName = "Pedal_Revs_Per_4Seconds";

	[SerializeField]
    string wheelRevsParameterName = "Wheel_Revs_Per_Second";

	[SerializeField]
    string leftTurnClipName = "Turn_Left";

	[SerializeField]
    string rightTurnClipName = "Turn_Right";

	private Animator animator;

    private float lerpedWheelRPM;
    private float lerpedPedalRPM;
    private float lerpedHandlesAngle;

	private int pedalRevsId;
	private int wheelRevsId;
	private int steerLayerIndex;
	private int leftTurnClipHash;
	private int rightTurnClipHash;


    void Reset()
    {
        this.handlesAngleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    void Start()
	{
		this.animator = GetComponent<Animator>();

		this.pedalRevsId = Animator.StringToHash(this.pedalRevsParameterName);
		this.wheelRevsId = Animator.StringToHash(this.wheelRevsParameterName);
		this.steerLayerIndex = this.animator.GetLayerIndex(this.steeringLayerName);
		this.leftTurnClipHash = Animator.StringToHash(this.steeringLayerName + "." + this.leftTurnClipName);
		this.rightTurnClipHash = Animator.StringToHash(this.steeringLayerName + "." + this.rightTurnClipName);
	}

	void Update()
	{
        this.lerpedWheelRPM = Mathf.Lerp(this.lerpedWheelRPM, this.wheelRPM, this.wheelRPMLerpRate);
        this.lerpedPedalRPM = Mathf.Lerp(this.lerpedPedalRPM, this.pedalRPM, this.pedalRPMLerpRate);
        this.lerpedHandlesAngle = Mathf.Lerp(this.lerpedHandlesAngle, this.handlesAngle, this.handlesAngleLerpRate);

        // The animation has one pedal rotation per FOUR seconds (i.e., 1/4 rotation per second).
        this.animator.SetFloat(this.pedalRevsId, this.lerpedPedalRPM / 15f);

        // The animation has one wheel rotation per second.
        this.animator.SetFloat(this.wheelRevsId, this.lerpedWheelRPM / 60f);

        
        float adjustedHandlesAngle = Mathf.Abs(this.lerpedHandlesAngle / 90f);
        if (this.ApplyHandlesAngleCurve) {
            adjustedHandlesAngle = this.handlesAngleCurve.Evaluate(adjustedHandlesAngle);
        }

        int turnClip = (this.lerpedHandlesAngle >= 0) ? this.rightTurnClipHash : this.leftTurnClipHash;
        this.animator.Play(turnClip, this.steerLayerIndex, adjustedHandlesAngle);
	}
}
