using System;

using MedRoad.Ant;
using MedRoad.Utils;

namespace MedRoad.VRBike
{
    public class AntStickBridge
    {

        private static AntStick lastAntStick = null;

        public static void SendResistance(float resistance)
        {
            if (lastAntStick != null)
                lastAntStick.SendResistance(resistance / 100f);
        }

        private AntStick antStick;
        private BikeData bikeData;

        public AntStickBridge(AntStick antStick, BikeData bikeData)
        {
            lastAntStick = antStick;

            this.antStick = antStick;
            this.bikeData = bikeData;

            if (!ThreadHelper.SingletonCheck("AntStickBridge"))
                return;

            antStick.OnStateChange += delegate (object sender, AntStick.StateChangeEventArgs e)
            {
                ThreadHelper.Singleton.PerformActionOnMainThread(() => AntStickStateChange(sender, e));
            };

            PowerOnlyDataPage.OnReceived += delegate (object sender, EventArgs e)
            {
                ThreadHelper.Singleton.PerformActionOnMainThread(() => PowerOnlyReceived(sender, e));
            };

            WheelTorqueDataPage.OnReceived += delegate (object sender, EventArgs e)
            {
                ThreadHelper.Singleton.PerformActionOnMainThread(() => WheelTorqueReceived(sender, e));
            };
        }

        private void AntStickStateChange(object sender, AntStick.StateChangeEventArgs e)
        {
            if (e.State == AntStick.AntState.Connected)
            {
                bikeData.UsingAntKickr = true;
            }
        }

        #region Power

        private PowerOnlyDataPage lastPowerOnly;

        void PowerOnlyReceived(object sender, EventArgs e)
        {
            PowerOnlyDataPage powerOnly = (PowerOnlyDataPage)sender;

            if (lastPowerOnly != null)
            {
                float power = PowerOnlyDataPage.CalculateAvgPower(powerOnly, lastPowerOnly);
                if (!float.IsNaN(power))
                    bikeData.AntPWR = power;
            }

            lastPowerOnly = powerOnly;
        }

        #endregion

        #region WheelTorque

        private WheelTorqueDataPage lastWheelTorque;

        private const int ZERO_SPEED_COUNT_MIN_THRESHOLD = 8;
        private const int ZERO_SPEED_COUNT_MAX_THRESHOLD = 16;
        private const int ZERO_SPEED_COUNT_IDLE_THRESHOLD = 24;
        private const int ZERO_SPEED_COUNT_THRESHOLD_OFFSET = 4;
        private int zeroSpeedCountThreshold = ZERO_SPEED_COUNT_MIN_THRESHOLD;

        private ZeroSpeedCountBuffer zeroSpeedCounts = new ZeroSpeedCountBuffer(5);

        private int zeroSpeedCount = 0;

        void WheelTorqueReceived(object sender, EventArgs e)
        {
            WheelTorqueDataPage wheelTorque = (WheelTorqueDataPage)sender;

            if (lastWheelTorque != null)
            {
                float speed = WheelTorqueDataPage.CalculateAvgSpeed(wheelTorque, lastWheelTorque, bikeData.BikeWheelDiameter * (float) Math.PI);

                if (WheelTorqueDataPage.IsZeroVelocityEventSynchronous(wheelTorque, lastWheelTorque))
                {
                    if (++zeroSpeedCount > zeroSpeedCountThreshold)
                    {
                        if (zeroSpeedCount == zeroSpeedCountThreshold + 1)
                            //Debug.LogFormat("Zero speed detected (zeroSpeedCountThreshold was {0})",
                            //    zeroSpeedCountThreshold);

                            speed = 0f;
                    }
                }
                else
                {
                    if (zeroSpeedCountThreshold > ZERO_SPEED_COUNT_IDLE_THRESHOLD)
                    {
                        zeroSpeedCounts.Reset();
                        zeroSpeedCounts.Add(ZERO_SPEED_COUNT_MIN_THRESHOLD);
                    }
                    else
                    {
                        zeroSpeedCounts.Add(AntUtilFunctions.IntegerClamp(
                            ZERO_SPEED_COUNT_THRESHOLD_OFFSET + zeroSpeedCount,
                            ZERO_SPEED_COUNT_MIN_THRESHOLD,
                            ZERO_SPEED_COUNT_MAX_THRESHOLD));
                    }
                    zeroSpeedCountThreshold = zeroSpeedCounts.Average();

                    //Debug.LogFormat("zeroSpeedCount reset at {0} (zeroSpeedCountThreshold updated to {1})",
                    //    zeroSpeedCount, zeroSpeedCountThreshold);

                    zeroSpeedCount = 0;
                }

                if (!float.IsNaN(speed))
                    bikeData.AntSpeed = speed;
            }

            lastWheelTorque = wheelTorque;
        }

        #endregion

    }
}
