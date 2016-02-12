using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class defines references to the elements in the Standard UI so that scripts can
/// reference them as needed.
/// </summary>
public class StandardUI : MonoBehaviour {

    [SerializeField]
    private Text hrText;

    [SerializeField]
    private Text avgHrText;

    [SerializeField]
    private Text oxText;

    [SerializeField]
    private Text pwrText;

    [SerializeField]
    private Text avgPwrText;

    [SerializeField]
    private Text timeText;

    [SerializeField]
    private Text velText;

    [SerializeField]
    private Text avgVelText;

    [SerializeField]
    private Text distText;

    [SerializeField]
    private Text goalDistText;

    [SerializeField]
    private Image uphillIcon;

    [SerializeField]
    private Image downhillIcon;

    [SerializeField]
    private Text slopeGradientText;

    [SerializeField]
    private Text encouragementText;

    [SerializeField]
    private Text debugHeadingArrow;

    [SerializeField]
    private Text debugHeadingStraightArrow;

    [SerializeField]
    private Text debugHeadingText;

    [SerializeField]
    private Text debugSpeedText;

    [SerializeField]
    private Text debugNetworkingPhysicsText;

    [SerializeField]
    private Text debugFPSText;


    public Text HRText
    {
        get { return this.hrText; }
    }

    public Text AvgHRText
    {
        get { return this.avgHrText; }
    }

    public Text OXText
    {
        get { return this.oxText; }
    }

    public Text PWRText
    {
        get { return this.pwrText; }
    }

    public Text AvgPWRText
    {
        get { return this.avgPwrText; }
    }

    public Text TimeText
    {
        get { return this.timeText; }
    }

    public Text VelText
    {
        get { return this.velText; }
    }

    public Text AvgVelText
    {
        get { return this.avgVelText; }
    }

    public Text DistText
    {
        get { return this.distText; }
    }

    public Text GoalDistText
    {
        get { return this.goalDistText; }
    }

    public Image UphillIcon
    {
        get { return this.uphillIcon; }
    }

    public Image DownhillIcon
    {
        get { return this.downhillIcon; }
    }

    public Text SlopeGradientText
    {
        get { return this.slopeGradientText; }
    }

    public Text EncouragementText
    {
        get { return this.encouragementText; }
    }

    public Text DebugHeadingArrow
    {
        get { return this.debugHeadingArrow; }
    }

    public Text DebugHeadingStraightArrow
    {
        get { return this.debugHeadingStraightArrow; }
    }

    public Text DebugHeadingText
    {
        get { return this.debugHeadingText; }
    }

    public Text DebugSpeedText
    {
        get { return this.debugSpeedText; }
    }

    public Text DebugNetworkingPhysicsText
    {
        get { return this.debugNetworkingPhysicsText; }
    }

    public Text DebugFPSText
    {
        get { return this.debugFPSText; }
    }

}
