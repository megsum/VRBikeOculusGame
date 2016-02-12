using System;
using System.Collections.Generic;
using System.Reflection;

namespace MedRoad.Ant
{
    public abstract class DataPage
    {
        /// <summary>
        /// The expected length of the byte array returned by Ant.
        /// </summary>
        protected const int EXPECTED_BYTE_ARRAY_LENGTH = 9;

        /// <summary>
        /// The expected position of the data page number in the byte array returned by Ant.
        /// </summary>
        protected const int EXPECTED_DATA_PAGE_NUM_POS = 1;

        /// <summary>
        /// Checks the data page number in the given byte array and creates a new instance of the
        /// appropriate <see cref="DataPage"/> subclass (if none with a mataching data page number
        /// can be found, it will be a <see cref="GenericDataPage"/>). The byte data is then
        /// parsed by the data page to set its properties, and the resulting data page is returned.
        /// </summary>
        /// <param name="receivedData">The received byte array.</param>
        /// <returns>A new DataPage subclass representing the given data.</returns>
        public static DataPage BuildDataPageFromReceivedData(byte[] receivedData)
        {
            CheckRecievedData(receivedData, false, 0);

            byte dataPageNumber = receivedData[EXPECTED_DATA_PAGE_NUM_POS];
            DataPage dataPage = GetNewDataPageInstance(dataPageNumber);

            dataPage.ParseReceivedData(receivedData, true);

            return dataPage;
        }

        /// <summary>
        /// Returns a new instance of the appropriate <see cref="DataPage"/> subclass for the
        /// given data page number. If no matching subclass can be found, a
        /// <see cref="GenericDataPage"/> will be returned.
        /// </summary>
        /// <param name="dataPageNumber">The data page number.</param>
        /// <returns>A new instance of a DataPage subclass matching the given data page number,
        /// or a GenericDataPage if not match can be found.</returns>
        internal static DataPage GetNewDataPageInstance(byte dataPageNumber)
        {
            Type dataPageType;
            if (!dataPageDictionary.TryGetValue(dataPageNumber, out dataPageType))
                return new GenericDataPage();

            return GetNewDataPageInstanceFromType(dataPageType);
        }

        /// <summary>
        /// Returns a new instance of the given DataPage type. Will throw an exception if the
        /// given class is not a <see cref="DataPage"/> subclass.
        /// </summary>
        /// <param name="T">The DataPage type to instantiate.</param>
        /// <returns>A new instance of the given DataPage type.</returns>
        internal static DataPage GetNewDataPageInstanceFromType(Type T)
        {
            if (!T.IsClass || T.IsAbstract || !T.IsSubclassOf(typeof(DataPage)))
                throw new Exception();

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            return (DataPage)Activator.CreateInstance(T, flags, null, new object[] { }, null);
        }

        /// <summary>
        /// A dictionary mapping data page numbers to their respective data page types.
        /// </summary>
        internal static Dictionary<byte, Type> dataPageDictionary = BuildDataPageDictionary();

        /// <summary>
        /// Builds the <see cref="dataPageDictionary"/>, mapping from data page numbers to their
        /// respective data page types.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<byte, Type> BuildDataPageDictionary()
        {
            Dictionary<byte, Type> dataPageDictionary = new Dictionary<byte, Type>();
            foreach (Type dataPageType in AntUtilFunctions.GetEnumerableOfType<DataPage>())
                if (dataPageType != typeof(GenericDataPage))
                    dataPageDictionary.Add(GetNewDataPageInstanceFromType(dataPageType).DataPageNumber, dataPageType);

            return dataPageDictionary;
        }

        #region Methods for Subclasses

        /// <summary>
        /// Fires the static OnReceived event for this <see cref="DataPage"/> subclass.
        /// </summary>
        internal abstract void FireReceived();

        /// <summary>
        /// Gets the data page number for this class.
        /// </summary>
        public abstract byte DataPageNumber { get; }

        /// <summary>
        /// Populates the fields of this data page by parsing the given byte data received from the
        /// channel listener.
        /// </summary>
        /// <param name="receivedData">The raw array of byte data received from the channel.
        /// </param>
        public void FillFromReceivedData(byte[] receivedData)
        {
            ParseReceivedData(receivedData, false);
        }

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
        internal protected abstract void ParseReceivedData(byte[] receivedData, bool skipCheck);

        /// <summary>
        /// Checks the length of the given byte array and, if checkDataPageNumber is <c>true</c>,
        /// that the data page number in the byte array matches the given data page number. Throws
        /// exceptions if either of these checks fails.
        /// </summary>
        /// <param name="receivedData">The byte array to check. It's length will be compared
        /// against the constant <see cref="EXPECTED_BYTE_ARRAY_LENGTH"/>.</param>
        /// <param name="checkDataPageNumber">Whether or not to check the given data page number.
        /// </param>
        /// <param name="dataPageNumber">If checkDataPageNumber is <c>true</c>, this value will be
        /// compared against the byte in position <see cref="EXPECTED_DATA_PAGE_NUM_POS"/> in the
        /// given byte array.</param>
        protected static void CheckRecievedData(byte[] receivedData, bool checkDataPageNumber, byte dataPageNumber)
        {
            if (receivedData.Length != EXPECTED_BYTE_ARRAY_LENGTH)
                throw new Exception(String.Format(
                    "Received data has wrong number of bytes (has {0}, expecting {1}).",
                    receivedData.Length, EXPECTED_BYTE_ARRAY_LENGTH));

            if (checkDataPageNumber && (receivedData[EXPECTED_DATA_PAGE_NUM_POS] != dataPageNumber))
                throw new Exception(String.Format(
                    "Received data has wrong data page number (has 0x{0:X}, expecting 0x{1:X}).",
                    receivedData[1], dataPageNumber));
        }

        #endregion

    }
}
