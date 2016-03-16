using System;
using System.Text;

namespace MedRoad.Ant
{
    public class BatteryStatusDataPage : DataPage
    {
        private const byte DATA_PAGE_NUMBER = 0x52;

        /// <summary>
        /// A series of values that define the status of the battery.
        /// </summary>
        public enum BatteryStatusValue : byte
        {
            Reserved_0 = 0,
            New = 1,
            Good = 2,
            Ok = 3,
            Low = 4,
            Critical = 5,
            Reserved_6 = 6,
            Invalid = 7
        };

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
        /// Indicates if <see cref="BatteryIdentifier"/> and <see cref="NumberOfBatteries"/>
        /// information is available.
        /// </summary>
        public bool BatteryIdentifierUsed { get; private set; }

        /// <summary>
        /// Identifies the battery in the system to which this message pertains.
        /// 
        /// This property is only valid if <see cref="BatteryIdentifierUsed"/> is <c>true</c>.
        /// </summary>
        public byte BatteryIdentifier { get; private set; }

        /// <summary>
        /// The total number of batteries in the system needed to report battery status.
        /// </summary>
        public byte NumberOfBatteries { get; private set; }

        /// <summary>
        /// The resolution of <see cref=CumulativeOperatingTime"/>. This will either be <c>2</c>
        /// seconds or <c>16</c> seconds.
        /// </summary>
        public byte OperatingTimeResolution { get; private set; }

        /// <summary>
        /// The cumulative operating time of the device (since the insertion of a new battery) in
        /// seconds.
        /// </summary>
        public uint CumulativeOperatingTime { get; private set; }

        /// <summary>
        /// Indicates if <see cref="Voltage"/> is available from this sensor. This value will be
        /// <c>false</c> if the device is unable to measure and transmit the battery voltage.
        /// </summary>
        public bool VoltageIsValid { get; private set; }

        /// <summary>
        /// The voltage of the battery. This value varies between 0 and 14.996 volts.
        /// 
        /// This property is only valid if <see cref="VoltageIsValid"/> is <c>true</c>.
        /// </summary>
        public float Voltage { get; private set; }

        /// <summary>
        /// The status of the battery.
        /// </summary>
        public BatteryStatusValue BatteryStatus { get; private set; }

        #endregion

        /// <summary>
        /// Instantiates a new instance of this data page.
        /// </summary>
        protected internal BatteryStatusDataPage() { }

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
            // See ANT+ Common Pages, page 24 (ver. 2.4)
            // https://www.thisisant.com/resources/common-data-pages/

            if (!skipCheck)
                DataPage.CheckRecievedData(receivedData, true, DATA_PAGE_NUMBER);

            byte batteryIdentifier = receivedData[3];
            if (batteryIdentifier == 0xFF)
            {
                BatteryIdentifierUsed = false;
            }
            else
            {
                BatteryIdentifierUsed = true;
                NumberOfBatteries = (byte)(receivedData[3] & 0xF);
                BatteryIdentifier = (byte)(receivedData[3] >> 4);
            }

            OperatingTimeResolution = (byte)(((receivedData[8] >> 7) == 1) ? 2 : 16);
            uint operatingTimeTicks = (uint)receivedData[4] + (((uint)receivedData[5]) << 8) + (((uint)receivedData[6]) << 16);
            CumulativeOperatingTime = operatingTimeTicks * OperatingTimeResolution;

            byte fractionalBatteryVoltage = receivedData[7];
            byte coarseBatteryVoltage = (byte)(receivedData[8] & 0xF);
            if (coarseBatteryVoltage == 0xF)
            {
                VoltageIsValid = false;
            }
            else
            {
                VoltageIsValid = true;
                Voltage = coarseBatteryVoltage + (fractionalBatteryVoltage / 256f);
            }

            BatteryStatus = (BatteryStatusValue)((byte)((receivedData[8] & 0x70) >> 4));
        }

        /// <summary>
        /// Builds a string representation of this data page.
        /// </summary>
        /// <returns>A string representation of this data page.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("Battery Id. Info Used:    {0}\n", BatteryIdentifierUsed ? "YES" : "NO");
            if (BatteryIdentifierUsed)
            {
                b.AppendFormat("Battery Identifier:       {0}\n", BatteryIdentifier);
                b.AppendFormat("Number of Batteries:      {0}\n", NumberOfBatteries);
            }
            b.AppendFormat("Battery Voltage Valid:    {0}\n", VoltageIsValid ? "YES" : "NO");
            b.AppendFormat("Battery Voltage:          {0:f3} V\n", Voltage);
            b.AppendFormat("Battery Status:           {0}\n", BatteryStatus);
            b.AppendFormat("Time Resolution:          {0} seconds\n", OperatingTimeResolution);
            TimeSpan operatingTime = new TimeSpan(0, 0, (int)CumulativeOperatingTime);
            b.AppendFormat("Operating Time:           {0:c}\n", operatingTime);
            return b.ToString();
        }

    }
}
