using System;
using System.Text;

namespace MedRoad.Ant
{
    class PowerOnlyDataPage : DataPage
    {
        private const byte DATA_PAGE_NUMBER = 0x10;

        /// <summary>
        /// If Pedal Power is available from this sensor, this value indicates if the sensor is
        /// capable of distinguishing between left and right pedal power contributions.
        /// </summary>
        public enum PedalDifferentiationValue : byte
        {
            /// <summary>
            /// The pedal power sensor is unable to differentiate between the left and right
            /// pedals.
            /// </summary>
            UnknownPedalPowerContribution = 0,

            /// <summary>
            /// The value stored in <see cref="PedalPowerPercent"/> represents the percent power
            /// contribution applied to the right pedal, and the remaining percent (i.e.,
            /// 100% - value) is the percent power contribution applied to the left pedal.
            /// </summary>
            RightPedalPowerContribution = 1,
        }

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
        /// Indicates if <see cref="PedalDifferentiation"/> and <see cref="PedalPowerPercent"/> are
        /// used by this sensor. If this is <c>false</c>, these values will not have valid data.
        /// </summary>
        public bool PedalPowerUsed { get; private set; }

        /// <summary>
        /// Indicates if the pedal power sensor is capable of differentiating between the left and
        /// right pedlas. Some sensors may or may not know which pedal has the greatest power
        /// contribution.
        /// 
        /// This property is only valid if <see cref="PedalPowerUsed"/> is <c>true</c>.
        /// </summary>
        public PedalDifferentiationValue PedalDifferentiation { get; private set; }

        /// <summary>
        /// The pedal power data field provides the user’s power contribution (as a percentage)
        /// between the left and right pedals, as measured by a pedal power sensor. For example, if
        /// the user’s power were evenly distributed between the left and right pedals, this value
        /// would read 50. This value is a percentage between 0 and 100.
        /// 
        /// This property is only valid if <see cref="PedalPowerUsed"/> is <c>true</c>.
        /// </summary>
        public byte PedalPowerPercent { get; private set; }

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
        /// Accumulated power is the running sum of the instantaneous power data and is incremented
        /// at each update of the update event count. The accumulated power field rolls over at
        /// 65.535kW. The unit is Watts and the resolution is 1 Watt.
        /// </summary>
        public ushort AccumulatedPower { get; private set; }

        /// <summary>
        /// The instantaneous power reading. The unit is Watts with a maximum value of 65.535kW and
        /// the resolution is 1 Watt.
        /// </summary>
        public ushort InstantaneousPower { get; private set; }

        #endregion

        /// <summary>
        /// Instantiates a new instance of this data page.
        /// </summary>
        protected internal PowerOnlyDataPage() { }

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
            // See ANT+ Device Profile - Bicycle Power, page 31 (ver. 4.2)
            // https://www.thisisant.com/resources/bicycle-power/

            if (!skipCheck)
                DataPage.CheckRecievedData(receivedData, true, DATA_PAGE_NUMBER);

            UpdateEventCount = receivedData[2];

            byte pedalPower = receivedData[3];
            if (pedalPower == 0xFF)
            {
                PedalPowerUsed = false;
            }
            else
            {
                PedalPowerUsed = true;
                PedalDifferentiation = (PedalDifferentiationValue)((byte)(pedalPower >> 7));
                PedalPowerPercent = (byte)(pedalPower & 0x7F);
            }

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

            AccumulatedPower = (ushort)(((uint)receivedData[5]) + (((uint)receivedData[6]) << 8));

            InstantaneousPower = (ushort)(((uint)receivedData[7]) + (((uint)receivedData[8]) << 8));
        }

        /// <summary>
        /// Builds a string representation of this data page.
        /// </summary>
        /// <returns>A string representation of this data page.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("Update Event Count:       {0}\n", UpdateEventCount);
            b.AppendFormat("Pedal Power Used:         {0}\n", PedalPowerUsed ? "YES" : "NO");
            if (PedalPowerUsed)
            {
                b.AppendFormat("Pedal Differentiation:    {0}\n", PedalDifferentiation.ToString());
                b.AppendFormat("Pedal Power Percent:      {0}%\n", PedalPowerPercent);
            }
            b.AppendFormat("Inst. Cadence Available:  {0}\n", InstantaneousCadenceAvailable ? "YES" : "NO");
            if (InstantaneousCadenceAvailable)
            {
                b.AppendFormat("Inst. Cadence:            {0} RPM\n", InstantaneousCadence);
            }
            b.AppendFormat("Accumulated Power:        {0} W\n", AccumulatedPower);
            b.AppendFormat("Inst. Power:              {0} W\n", InstantaneousPower);
            return b.ToString();
        }

        /// <summary>
        /// Calculates the average power in W.
        /// 
        /// Under normal conditions with complete RF reception, average power equals instantaneous
        /// power. In conditions where packets are lost, average power accurately calculates power
        /// over the interval between the received messages.
        /// </summary>
        /// <param name="message1">The most recent WheelTorqueDataPage message recieved.</param>
        /// <param name="message2">The second most recent WheelTorqueDataPage message recieved.
        /// </param>
        /// <returns>The average power in W.</returns>
        public static float CalculateAvgPower(PowerOnlyDataPage message1, PowerOnlyDataPage message2)
        {
            // See ANT+ Device Profile - Bicycle Power, page 33 (ver. 4.2)
            // https://www.thisisant.com/resources/bicycle-power/

            return AntUtilFunctions.rolloverDiff(message1.AccumulatedPower, message2.AccumulatedPower) /
                   ((float) AntUtilFunctions.rolloverDiff(message1.UpdateEventCount, message2.UpdateEventCount));
        }

    }
}
