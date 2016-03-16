using System;
using System.Text;

namespace MedRoad.Ant
{
    public class GenericDataPage : DataPage
    {
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

        private byte _dataPageNumber = 0xFF;

        /// <summary>
        /// Gets the data page number for this instance. Once populated with data this should be
        /// the actual page number of the received data. Initially it is set to <c>0xFF</c>, but
        /// note that this could conflict with a manufacturer-defined custom data page.
        /// </summary>
        public override byte DataPageNumber
        {
            get { return _dataPageNumber; }
        }

        private byte[] _data;

        /// <summary>
        /// Gets the raw byte array for this data page.
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
        }

        #endregion

        /// <summary>
        /// Instantiates a new instance of this data page.
        /// </summary>
        protected internal GenericDataPage() { }

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
            if (!skipCheck)
                DataPage.CheckRecievedData(receivedData, false, 0);

            _data = receivedData;
            _dataPageNumber = receivedData[DataPage.EXPECTED_DATA_PAGE_NUM_POS];
        }

        /// <summary>
        /// Builds a string representation of this data page.
        /// </summary>
        /// <returns>A string representation of this data page.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("Data Page Number:         0x{0:X} ({0:d})\n", _dataPageNumber);
            b.AppendFormat("Byte Data:                {0}\n", BitConverter.ToString(_data));
            return b.ToString();
        }

    }
}
