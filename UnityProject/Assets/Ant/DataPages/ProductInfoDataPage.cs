using System;
using System.Text;

namespace MedRoad.Ant
{
    public class ProductInfoDataPage : DataPage
    {
        private const byte DATA_PAGE_NUMBER = 0x51;

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
        /// A manufacturer set software revision number.
        /// </summary>
        public uint SoftwareRevision { get; private set; }

        /// <summary>
        /// Indicates if the device has a serial number. If <c>true</c>, <see cref="SerialNumber"/>
        /// will the contain the lowest 32 bits of the device serial number.
        /// </summary>
        public bool HasSerialNumber { get; private set; }

        /// <summary>
        /// The lowest 32 bits of the device serial number.
        /// 
        /// This property is only valid if <see cref="HasSerialNumber"/> is <c>true</c>.
        /// </summary>
        public uint SerialNumber { get; private set; }

        #endregion

        /// <summary>
        /// Instantiates a new instance of this data page.
        /// </summary>
        protected internal ProductInfoDataPage() { }

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
            // See ANT+ Common Pages, page 23 (ver. 2.4)
            // https://www.thisisant.com/resources/common-data-pages/

            if (!skipCheck)
                DataPage.CheckRecievedData(receivedData, true, DATA_PAGE_NUMBER);

            byte supplementalSWRev = receivedData[3];
            byte mainSWRev = receivedData[4];
            if (supplementalSWRev == 0xFF)
                SoftwareRevision = mainSWRev;
            else
                SoftwareRevision = UInt32.Parse(String.Format("{0:d}{1:d}", mainSWRev, supplementalSWRev));

            uint serial =   ((uint)receivedData[5])
                         + (((uint)receivedData[6]) << 8)
                         + (((uint)receivedData[7]) << 16)
                         + (((uint)receivedData[8]) << 24);

            if (serial == 0xFFFFFFFF)
            {
                HasSerialNumber = false;
            }
            else
            {
                HasSerialNumber = true;
                SerialNumber = serial;
            }
        }

        /// <summary>
        /// Builds a string representation of this data page.
        /// </summary>
        /// <returns>A string representation of this data page.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("Software Revision:        {0}\n", SoftwareRevision);
            b.AppendFormat("Has Serial Number:        {0}\n", HasSerialNumber ? "YES" : "NO");
            if (HasSerialNumber)
                b.AppendFormat("Serial Number:            {0:d} (0x{0:X})\n", SerialNumber);
            return b.ToString();
        }

    }
}
