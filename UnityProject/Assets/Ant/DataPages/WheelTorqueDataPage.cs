using System;
using System.Text;

namespace MedRoad.Ant
{
    public class WheelTorqueDataPage : DataPage
    {
        private const byte DATA_PAGE_NUMBER = 0x11;

        #region EventHandlers

        /// <summary>
        /// Occurs when a new data page of this type is received.
        /// </summary>
        public static event EventHandler<EventArgs> OnReceived;

        /// <summary>
        /// Fires the OnReceived event.
        /// </summary>
        internal override void FireReceived()
        {
            EventHandler<EventArgs> temp = OnReceived;
            if (temp != null)
                temp(this, new EventArgs());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data page number for this class.
        /// </summary>
        public override byte DataPageNumber
        {
            get { return DATA_PAGE_NUMBER; }
        }

        /// <summary>
        /// The update event count field is incremented each time the information in the message is
        /// updated. Rolls over at 255.
        /// </summary>
        public byte UpdateEventCount { get; private set; }

        /// <summary>
        /// Increments with each wheel revolution and is used to calculate linear distance
        /// traveled. Rolls over every 256 wheel revolutions, which is approximately 550 meters
        /// assuming a 2 m wheel circumference.There are no invalid values for this field.
        /// </summary>
        public byte WheelTicks { get; private set; }

        /// <summary>
        /// Indicates if <see cref="InstantaneousCadence"/> is available from this sensor. If this
        /// is <c>false</c>, this value will not have valid data.
        /// </summary>
        public bool InstantaneousCadenceAvailable { get; private set; }

        /// <summary>
        /// The instantaneous pedaling cadence recorded by the power sensor, from 0 RPM to 254 RPM.
        /// 
        /// This property is only valid if <see cref="InstantaneousCadenceAvailable"/> is
        /// <c>true</c>.
        /// </summary>
        public byte InstantaneousCadence { get; private set; }

        /// <summary>
        /// The accumulated wheel period. It is used to indicate the average rotation period of the
        /// wheel during the last update interval, in increments of 1/2048 s. Each Wheel Period
        /// tick represents a 488-microsecond interval. In event-synchronous systems, the
        /// accumulated wheel period time stamp field rolls over in 32 seconds. In fixed time
        /// interval update systems, the time to rollover depends on wheel speed but is greater
        /// than 32 seconds.
        /// </summary>
        public ushort WheelPeriod { get; private set; }

        /// <summary>
        /// The accumulated torque is the cumulative sum of the average torque measured every
        /// update event count. The unit is 1/32 Nm and it will rollover at 2048 Nm.
        /// </summary>
        public ushort AccumulatedTorque { get; private set; }

        #endregion

        /// <summary>
        /// Instantiates a new instance of this data page.
        /// </summary>
        protected internal WheelTorqueDataPage() { }

        /// <summary>
        /// Populates the fields of this data page by parsing the given byte data received from the
        /// channel listener.
        /// </summary>
        /// <param name="receivedData">The raw array of byte data received from the channel.
        /// </param>
        /// <param name="skipCheck">If <c>true</c>, skips calling
        /// <see cref="DataPage.CheckRecievedData(byte[], bool, byte)"/> to verify that the length
        /// and page number are correct. This should be set to true if the check has been
        /// performed already.</param>
        protected internal override void ParseReceivedData(byte[] receivedData, bool skipCheck)
        {
            // See ANT+ Device Profile - Bicycle Power, page 34 (ver. 4.2)
            // https://www.thisisant.com/resources/bicycle-power/

            if (!skipCheck)
                DataPage.CheckRecievedData(receivedData, true, DATA_PAGE_NUMBER);

            UpdateEventCount = receivedData[2];

            WheelTicks = receivedData[3];

            byte instCadence = receivedData[4];
            if (instCadence == 0xFF)
            {
                InstantaneousCadenceAvailable = false;
            }
            else
            {
                InstantaneousCadenceAvailable = true;
                InstantaneousCadence = instCadence;
            }

            WheelPeriod = (ushort)(((uint)receivedData[5]) + (((uint)receivedData[6]) << 8));

            AccumulatedTorque = (ushort)(((uint)receivedData[7]) + (((uint)receivedData[8]) << 8));
        }

        /// <summary>
        /// Builds a string representation of this data page.
        /// </summary>
        /// <returns>A string representation of this data page.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("Update Event Count:       {0}\n", UpdateEventCount);
            b.AppendFormat("Wheel Ticks:              {0} revs\n", WheelTicks);
            b.AppendFormat("Inst. Cadence Available:  {0}\n", InstantaneousCadenceAvailable ? "YES" : "NO");
            if (InstantaneousCadenceAvailable)
            {
                b.AppendFormat("Inst. Cadence:            {0} RPM\n", InstantaneousCadence);
            }
            b.AppendFormat("Accumulated Wheel Period: {0:F3} s\n", WheelPeriod / 2048f);
            b.AppendFormat("Accumulated Torque:       {0:F3} Nm\n", AccumulatedTorque / 32f);
            return b.ToString();
        }

        /// <summary>
        /// Calculates the average speed in m/s.
        /// </summary>
        /// <param name="message1">The most recent WheelTorqueDataPage message recieved.</param>
        /// <param name="message2">The second most recent WheelTorqueDataPage message recieved.
        /// </param>
        /// <param name="circumference">The circumference of the bicycle wheel, in m.</param>
        /// <returns>The average linear speed of the bike in m/s.</returns>
        public static float CalculateAvgSpeed(WheelTorqueDataPage message1, WheelTorqueDataPage message2, float circumference)
        {
            // See ANT+ Device Profile - Bicycle Power, page 36 (ver. 4.2)
            // https://www.thisisant.com/resources/bicycle-power/

            return circumference * AntUtilFunctions.rolloverDiff(message1.UpdateEventCount, message2.UpdateEventCount) /
                   (AntUtilFunctions.rolloverDiff(message1.WheelPeriod, message2.WheelPeriod) / 2048f);
        }

        /// <summary>
        /// When updates are time synchronous, interpret the two given messages to determine if the
        /// wheel is not rotating.
        /// 
        /// I would think that you would probably want to count how many times this happens in a 
        /// row and then once it passes some threshold interpert that as zero speed (?).
        /// </summary>
        /// <param name="message1">The most recent WheelTorqueDataPage message recieved.</param>
        /// <param name="message2">The second most recent WheelTorqueDataPage message recieved.
        /// </param>
        /// <returns><c>True</c> if the speed appears to be zero.</returns>
        public static bool IsZeroVelocityTimeSynchronous(WheelTorqueDataPage message1, WheelTorqueDataPage message2)
        {
            // See ANT+ Device Profile - Bicycle Power, page 36 (ver. 4.2)
            // https://www.thisisant.com/resources/bicycle-power/

            return (message1.WheelTicks == message2.WheelTicks);
        }

        /// <summary>
        /// When updates are event synchronous, interpret the two given messages to determine if
        /// the wheel is not rotating.
        /// 
        /// I would think that you would probably want to count how many times this happens in a 
        /// row and then once it passes some threshold interpert that as zero speed (?).
        /// </summary>
        /// <param name="message1">The most recent WheelTorqueDataPage message recieved.</param>
        /// <param name="message2">The second most recent WheelTorqueDataPage message recieved.
        /// </param>
        /// <returns><c>True</c> if the speed appears to be zero.</returns>
        public static bool IsZeroVelocityEventSynchronous(WheelTorqueDataPage message1, WheelTorqueDataPage message2)
        {
            // See ANT+ Device Profile - Bicycle Power, page 36 (ver. 4.2)
            // https://www.thisisant.com/resources/bicycle-power/

            return (message1.UpdateEventCount == message2.UpdateEventCount) &&
                   (message1.WheelPeriod == message2.WheelPeriod) &&
                   (message1.WheelTicks == message2.WheelTicks);
        }

        /// <summary>
        /// Calculates distance travelled in m.
        /// </summary>
        /// <param name="message1">The most recent WheelTorqueDataPage message recieved.</param>
        /// <param name="message2">The second most recent WheelTorqueDataPage message recieved.
        /// </param>
        /// <param name="circumference">The circumference of the bicycle wheel, in m.</param>
        /// <returns>The distance travelled in m.</returns>
        public static float CalculateDistance(WheelTorqueDataPage message1, WheelTorqueDataPage message2, float circumference)
        {
            // See ANT+ Device Profile - Bicycle Power, page 37 (ver. 4.2)
            // https://www.thisisant.com/resources/bicycle-power/

            return circumference * AntUtilFunctions.rolloverDiff(message1.WheelTicks, message2.WheelTicks);
        }

    }
}
