using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using ANT_Managed_Library;

using UnityEngine;
using System.IO;

namespace MedRoad.Ant
{
    public class AntStick
    {
        #region ANT Configuration

        private const uint RESPONSE_WAIT_TIME = 500;
        private const int CONNECT_TIMEOUT = 10000; // ms

        private const byte NETWORK_NUMBER = 0;
        private static byte[] NETWORK_KEY = { 0xB9, 0xA5, 0x21, 0xFB, 0xBD, 0x72, 0xC3, 0x45 };

        private const int CHANNEL = 0;
        private const ANT_ReferenceLibrary.ChannelType CHANNEL_TYPE =
            ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00;

        private const string ANT_DEVICE_ID_FILE_PATH = "C:\\Ant_DeviceID.txt";

        private const ushort DEVICE_ID = 0;           // Scan for devices.
        private const byte   DEVICE_TYPE = 11;        // ANT+ Bike Power Sensor.
        private const byte   TRANSMISSION_TYPE = 0;   // Scan for devices (165 for Wahoo only).
        private const bool   PAIRING_ENABLED = false;

        // This "should" be 8182 but I think the Kickr might actually be using 4091.
        private const ushort CHANNEL_PERIOD = 4091;   // 4091 / 32768 second period (~8.01 Hz).
        private const byte   CHANNEL_FREQUENCY = 57;  // 2457 MHz.

        #endregion

        #region EventHandlers

        /// <summary>
        /// Occurs when the AntStick state changes.
        /// </summary>
        public event EventHandler<StateChangeEventArgs> OnStateChange;

        public class StateChangeEventArgs : EventArgs
        {
            public AntState State { get; set; }
        }

        /// <summary>
        /// Fires the OnStateChange event.
        /// </summary>
        private void FireStateChange()
        {
            StateChangeEventArgs args = new StateChangeEventArgs();
            args.State = this.state;

            EventHandler<StateChangeEventArgs> temp = OnStateChange;
            if (temp != null)
                temp(this, args);
        }

        #endregion

        public enum AntState
        {
            NotStarted = 0,
            Starting = 1,
            Connected = 2,
            ReConnecting = 3,
            Finished = 4,
            StartFail = 5,
            ConnectFail = 6
        }

        public AntStats Stats { get; private set; }

        private ANT_Device device;
        private ANT_Channel channel;
        private AntState state = AntState.NotStarted;

        public AntStick()
        {
            antSticks.Add(this);
        }

        private void UpdateState(AntState state)
        {
            this.state = state;
            FireStateChange();
        }

        public void Start()
        {
            if (state != AntState.NotStarted)
            {
                Debug.LogWarningFormat("[AntStick] Start called a second time (ignored).");
                return;
            }

            UpdateState(AntState.Starting);

            ushort deviceId = DEVICE_ID;

            try
            {
                string line = null;
                StreamReader reader = new StreamReader(ANT_DEVICE_ID_FILE_PATH, Encoding.Default);
                using (reader)
                {
                    line = reader.ReadLine();
                    reader.Close();
                }

                if (line == null)
                {
                    Debug.LogWarningFormat("[AntStick] Could not get Ant Device ID from {0}. File exists but is empty.", ANT_DEVICE_ID_FILE_PATH);
                }
                else
                {
                    deviceId = UInt16.Parse(line);
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.LogWarningFormat("[AntStick] Could not get Ant Device ID from {0}. File not found. {1}", ANT_DEVICE_ID_FILE_PATH, ex.Message);
            }
            catch(FormatException ex)
            {
                Debug.LogWarningFormat("[AntStick] Could not get Ant Device ID from {0}. Could not parse first line as ushort. {1}", ANT_DEVICE_ID_FILE_PATH, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("[AntStick] Could not get Ant Device ID from {0}. Exception occurred. {1}", ANT_DEVICE_ID_FILE_PATH, ex.Message);
            }

            Debug.LogFormat("[AntStick] Using Device ID {0}.", deviceId);

            Stats = new AntStats();

            try
            {
                device = new ANT_Device();
            }
            catch (ANT_Exception ex)
            {
                Debug.LogWarningFormat("[AntStick] Could not open device (perhaps something else is using it?).\n{0}", ex.Message);
                UpdateState(AntState.StartFail);
                Stop();
                return;
            }

            try
            {
                channel = device.getChannel(CHANNEL);
            }
            catch (ANT_Exception ex)
            {
                Debug.LogWarningFormat("[AntStick] Could not get channel {0}.\n{1}", CHANNEL, ex.Message);
                UpdateState(AntState.StartFail);
                Stop();
                return;
            }

            device.deviceResponse += new ANT_Device.dDeviceResponseHandler(DeviceResponse);
            channel.channelResponse += new dChannelResponseHandler(ChannelResponse);

            try
            {
                if (!device.setNetworkKey(NETWORK_NUMBER, NETWORK_KEY, RESPONSE_WAIT_TIME))
                {
                    Debug.LogWarning("[AntStick] Failed to set network key.");
                    UpdateState(AntState.StartFail);
                    Stop();
                    return;
                }

                if (!channel.assignChannel(CHANNEL_TYPE, NETWORK_NUMBER, RESPONSE_WAIT_TIME))
                {
                    Debug.LogWarning("[AntStick] Failed to assign channel.");
                    UpdateState(AntState.StartFail);
                    Stop();
                    return;
                }

                if (!channel.setChannelID(deviceId, PAIRING_ENABLED, DEVICE_TYPE, TRANSMISSION_TYPE, RESPONSE_WAIT_TIME))
                {
                    Debug.LogWarning("[AntStick] Failed to set channel Id.");
                    UpdateState(AntState.StartFail);
                    Stop();
                    return;
                }

                if (!channel.setChannelPeriod(CHANNEL_PERIOD, RESPONSE_WAIT_TIME))
                {
                    Debug.LogWarning("[AntStick] Failed to set channel period.");
                    UpdateState(AntState.StartFail);
                    Stop();
                    return;
                }

                if (!channel.setChannelFreq(CHANNEL_FREQUENCY, RESPONSE_WAIT_TIME))
                {
                    Debug.LogWarning("[AntStick] Failed to set channel frequency.");
                    UpdateState(AntState.StartFail);
                    Stop();
                    return;
                }

                if (!channel.openChannel(RESPONSE_WAIT_TIME))
                {
                    Debug.LogWarning("[AntStick] Failed to open the channel.");
                    UpdateState(AntState.StartFail);
                    Stop();
                    return;
                }

            }
            catch (ANT_Exception ex)
            {
                Debug.LogWarningFormat("[AntStick] Could not configure channel.\n{0}", ex.Message);
                UpdateState(AntState.StartFail);
                Stop();
                return;
            }

            StartConnectTimeout();
        }

        private void StartConnectTimeout()
        {
            (new Thread(new ThreadStart(ConnectTimeout))).Start();
        }

        private void ConnectTimeout()
        {
            Thread.Sleep(CONNECT_TIMEOUT);

            if (state == AntState.Starting)
            {
                Debug.LogWarning("[AntStick] Timed out while connecting to device (no device?).");
                UpdateState(AntState.ConnectFail);
                Stop();
            }

            if (state == AntState.ReConnecting)
            {
                Debug.LogWarning("[AntStick] Timed out while reconnecting (device lost?).");
                UpdateState(AntState.ConnectFail);
                Stop();
            }
        }


        private static List<AntStick> antSticks = new List<AntStick>();

        public static void StopAll()
        {
            foreach (AntStick antStick in antSticks)
                antStick.Stop();
        }

        public void Stop()
        {
            // Nothing to stop if not started or already finished.
            if (state == AntState.NotStarted || state == AntState.Finished)
                return;

            //if (state == AntState.StartFail || state == AntState.ConnectFail)
            //    return;

            if (this.channel != null)
                try
                {
                    ANT_Channel tempChannel = this.channel;
                    this.channel = null;
                    tempChannel.closeChannel(RESPONSE_WAIT_TIME);
                    tempChannel.Dispose();
                }
                catch { }

            if (this.device != null)
                try
                {
                    ANT_Device tempDevice = this.device;
                    this.device = null;

                    // We use a temp var here because this Dispose method is a little strange...
                    tempDevice.Dispose();
                }
                catch { }

                UpdateState(AntState.Finished);
        }

        //public void RequestDataPage(byte pageNumber)
        //{
        //    if (state != AntState.Connected)
        //        return;

        //    byte[] message = new byte[8];
        //    message[0] = 0x46;                           // Data Page Request
        //    message[1] = 0xFF;                           // Reserved
        //    message[2] = 0xFF;                           // Reserved
        //    message[3] = 0xFF;                           // Descriptor Byte 1 (0xFF = None)
        //    message[4] = 0xFF;                           // Descriptor Byte 2 (0xFF = None)
        //    message[5] = 0x04;                           // Ask to transmit 4 times.
        //    message[6] = pageNumber;                     // Requested Page Number
        //    message[7] = 0x01;                           // 0x01 = Data Page, 0x02 = ANT-FS

        //    // Debug.LogFormat("[AntStick] Sending Broadcast {0}", BitConverter.ToString(message));
        //
        //    if (channel != null)
        //        channel.sendBroadcastData(message);
        //}

        public void SendResistance(float resistance)
        {
            if (state != AntState.Connected)
                return;

            // The control point parameter for resistance mode is a 14-bit value transmitted in a
            // 16-bit unsigned. This value is the PWM duty cycle, 0 is full off, and 0x3FFF is full
            // on.

            // Clamp the resistance between 0 and 1.
            if (resistance < 0f) resistance = 0f;
            if (resistance > 1f) resistance = 1f;

            // Invert the scale to account for active low.
            resistance = 1f - resistance;

            // Convert the scale to 14-bit integer.
            ushort resistance16bit = (ushort)(resistance * 0x3FFF);

            // Build the message. The Kickr doesn't actually seem to care if the device Id is set
            // correctly so don't even bother setting it.
            byte[] message = new byte[8];
            message[0] = 0xF1;                           // Wahoo Kickr Command Page
            message[1] = 0x40;                           // Set Resistance Mode
            message[2] = 0xFF;                           // Device Id Low Byte
            message[3] = 0xFF;                           // Device Id High Byte
            message[4] = (byte)  resistance16bit;        // Resistance Low Byte
            message[5] = (byte) (resistance16bit >> 8);  // Resistance High Byte
            message[6] = 0xFF;                           // Reserved
            message[7] = 0xFF;                           // Reserved

            // Debug.LogFormat("[AntStick] Sending Broadcast {0}", BitConverter.ToString(message));

            if (channel != null)
                channel.sendBroadcastData(message);
        }

        private void DeviceResponse(ANT_Response response)
        {
            // Debug.Log("[AntStick] " + decodeDeviceFeedback(response));
        }


        private void ChannelResponse(ANT_Response response)
        {
            // Debug.Log("[AntStick] " + decodeChannelFeedback(response));

            if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40)
            {
                if (response.messageContents[2] == (byte)ANT_ReferenceLibrary.ANTEventID.EVENT_RX_SEARCH_TIMEOUT_0x01)
                {
                    Debug.LogWarning("[AntStick] Failed to connect to any device.");
                    Debug.LogWarning("[AntStick] <b>WARNING --- UNITY MAY CRASH!</b> Try lowering AntStick.CONNECT_TIMEOUT value.");
                    UpdateState(AntState.ConnectFail);
                    Stop();
                }

                if (response.messageContents[2] == (byte)ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_0x02)
                {
                    Stats.IncrementRxFailCount();
                }

                if (response.messageContents[2] == (byte)ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_GO_TO_SEARCH_0x08)
                {
                    UpdateState(AntState.ReConnecting);
                    StartConnectTimeout();
                }
            }
            else
            {
                if (state == AntState.Starting || state == AntState.ReConnecting)
                {
                    Debug.Log("[AntStick] Successfully began receiving data.");
                    UpdateState(AntState.Connected);
                }
                
                DataPage dataPage = DataPage.BuildDataPageFromReceivedData(response.messageContents);
                Stats.IncrementPageCount(dataPage.DataPageNumber);
                dataPage.FireReceived();
            }
        }

        /// <summary>
        /// This function decodes the message code into human readable form and shows the error
        /// value on failures for device response events.
        /// </summary>
        /// <param name="response">The ANT Response received from the device.</param>
        /// <returns>A nicely formatted string</returns>
        private string decodeDeviceFeedback(ANT_Response response)
        {
            string toDisplay = "Device: ";

            // The ANTReferenceLibrary contains all the message and event codes in user-friendly
            // enums. This allows for more readable code and easy conversion to human readable
            // strings for display.

            // So, for the device response we first check if it is an event, then we check if it
            // failed, and display the failure if that is the case. "Events" use message code 0x40.
            if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40)
            {
                // We cast the byte to its messageID string and add the channel number byte.
                // associated with the message
                toDisplay += (ANT_ReferenceLibrary.ANTMessageID)response.messageContents[1] + ", Ch:" + response.messageContents[0];
                // Check if the eventID shows an error, if it does, show the error message.
                if ((ANT_ReferenceLibrary.ANTEventID)response.messageContents[2] != ANT_ReferenceLibrary.ANTEventID.RESPONSE_NO_ERROR_0x00)
                    toDisplay += Environment.NewLine + ((ANT_ReferenceLibrary.ANTEventID)response.messageContents[2]).ToString();
            }
            else   // If the message is not an event, we just show the messageID.
                toDisplay += ((ANT_ReferenceLibrary.ANTMessageID)response.responseID).ToString();

            // Finally we display the raw byte contents of the response, converting it to hex.
            toDisplay += Environment.NewLine + "::" + Convert.ToString(response.responseID, 16)
                      + ", " + BitConverter.ToString(response.messageContents) + Environment.NewLine;
            return toDisplay;
        }


        /// <summary>
        /// This function decodes the message code into human readable form and shows the error
        /// value on failures for channel response events.
        /// </summary>
        /// <param name="response">The ANT Response received from the channel.</param>
        /// <returns>A nicely formatted string.</returns>
        String decodeChannelFeedback(ANT_Response response)
        {
            // We use a stringbuilder for speed and better memory usage.
            StringBuilder stringToPrint;    
            stringToPrint = new StringBuilder("Channel: ", 100);

            // In the channel feedback we will get either RESPONSE_EVENTs or receive events. If it
            // is a response event we display what the event was and the error code if it failed.
            // Mostly, these response_events will all be broadcast events from a Master channel.     
            if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40)
                stringToPrint.AppendLine(((ANT_ReferenceLibrary.ANTEventID)response.messageContents[2]).ToString());
            else   // This is a receive event, so display the ID
                stringToPrint.AppendLine("Received " + ((ANT_ReferenceLibrary.ANTMessageID)response.responseID).ToString());

            // Always print the raw contents in hex, with leading '::' for easy visibility/parsing.
            // If this is a receive event it will contain the payload of the message.
            stringToPrint.Append("  :: ");
            stringToPrint.Append(Convert.ToString(response.responseID, 16));
            stringToPrint.Append(", ");
            stringToPrint.Append(BitConverter.ToString(response.messageContents) + Environment.NewLine);

            return stringToPrint.ToString();
        }

    }
}
