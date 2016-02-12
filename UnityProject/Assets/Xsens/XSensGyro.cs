using System;
using System.Collections.Generic;

using XDA;
using Xsens;

using UnityEngine;

namespace MedRoad.XSensGyroscope
{
    public class XSensGyro
    {
        private Xda _xda;
        private XsDevice _measuringDevice = null;
        private MyMtCallback _myMtCallback = null;

        private XSensState state = XSensState.NotStarted;

        #region EventHandlers

        /// <summary>
        /// Occurs when new data is available from the XSens device.
        /// </summary>
        public event EventHandler<DataAvailableArgs> OnDataAvailable;

        /// <summary>
        /// Fires the OnDataAvailable event.
        /// </summary>
        private void FireDataAvailable(DataAvailableArgs dataAvailableArgs)
        {
            EventHandler<DataAvailableArgs> temp = OnDataAvailable;
            if (temp != null)
                temp(this, dataAvailableArgs);
        }

        /// <summary>
        /// Occurs when the XSensGyro state changes.
        /// </summary>
        public event EventHandler<StateChangeEventArgs> OnStateChange;

        public class StateChangeEventArgs : EventArgs
        {
            public XSensState State { get; set; }
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


        #region States

        public enum XSensState
        {
            NotStarted = 0,
            Starting = 1,
            Started = 2,
            Failed = 3,
        }

        private void UpdateState(XSensState state)
        {
            this.state = state;
            FireStateChange();
        }

        #endregion

        public XSensGyro()
        {
            _xda = new Xda();
        }

        public void Start()
        {
            if (state != XSensState.NotStarted)
            {
                Debug.Log("[XSensGyroscope] Cannot call Start twice! Ignoring...\n");
                return;
            }

            this.UpdateState(XSensState.Starting);

            _xda.scanPorts();
            Debug.LogFormat("[XSensGyroscope] Found {0} device(s)\n", _xda._DetectedDevices.Count);
            if (_xda._DetectedDevices.Count > 0)
            {
                XsPortInfo portInfo = _xda._DetectedDevices[0];
                if (portInfo.deviceId().isMtMk4())
                {
                    _xda.openPort(portInfo);
                    MasterInfo ai = new MasterInfo(portInfo.deviceId());
                    ai.ComPort = portInfo.portName();
                    ai.BaudRate = portInfo.baudrate();

                    _measuringDevice = _xda.getDevice(ai.DeviceId);
                    ai.ProductCode = new XsString(_measuringDevice.productCode());

                    // Print information about detected MTi / MTx / MTmk4 device
                    Debug.LogFormat("[XSensGyroscope] Found a device with id: {0} @ port: {1}, baudrate: {2}\n",
                        _measuringDevice.deviceId().toXsString().toString(), ai.ComPort.toString(), ai.BaudRate);

                    // Create and attach callback handler to device
                    _myMtCallback = new MyMtCallback();
                    _measuringDevice.addCallbackHandler(_myMtCallback);

                    ConnectedMtData mtwData = new ConnectedMtData();

                    // connect signals
                    _myMtCallback.DataAvailable += new EventHandler<DataAvailableArgs>(DataAvailable);

                    // Put the device in configuration mode
                    Debug.Log("[XSensGyroscope] Putting device into configuration mode...\n");
                    if (!_measuringDevice.gotoConfig()) // Put the device into configuration mode before configuring the device
                    {
                        Debug.Log("[XSensGyroscope] Could not put device into configuration mode. Aborting.");
                        this.UpdateState(XSensState.Failed);
                        return;
                    }

                    // Configure the device. Note the differences between MTix and MTmk4
                    Debug.Log("[XSensGyroscope] Configuring the device...\n");
                    if (_measuringDevice.deviceId().isMt9c())
                    {
                        XsOutputMode outputMode = XsOutputMode.XOM_Orientation; // output orientation data
                        XsOutputSettings outputSettings = XsOutputSettings.XOS_OrientationMode_Quaternion; // output orientation data as quaternion
                        XsDeviceMode deviceMode = new XsDeviceMode(100); // make a device mode with update rate: 100 Hz
                        deviceMode.setModeFlag(outputMode);
                        deviceMode.setSettingsFlag(outputSettings);

                        // set the device configuration
                        if (!_measuringDevice.setDeviceMode(deviceMode))
                        {
                            Debug.Log("[XSensGyroscope] Could not configure MTix device. Aborting.");
                            this.UpdateState(XSensState.Failed);
                            return;
                        }
                    }
                    else if (_measuringDevice.deviceId().isMtMk4())
                    {
                        XsOutputConfiguration quat = new XsOutputConfiguration(XsDataIdentifier.XDI_Quaternion, 0);
                        XsOutputConfigurationArray configArray = new XsOutputConfigurationArray();
                        configArray.push_back(quat);
                        if (!_measuringDevice.setOutputConfiguration(configArray))
                        {
                            Debug.Log("[XSensGyroscope] Could not configure MTmk4 device. Aborting.");
                            this.UpdateState(XSensState.Failed);
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log("[XSensGyroscope] Unknown device while configuring. Aborting.");
                        this.UpdateState(XSensState.Failed);
                        return;
                    }

                    // Put the device in measurement mode
                    Debug.Log("[XSensGyroscope] Putting device into measurement mode...\n");
                    if (!_measuringDevice.gotoMeasurement())
                    {
                        Debug.Log("[XSensGyroscope] Could not put device into measurement mode. Aborting.");
                        this.UpdateState(XSensState.Failed);
                        return;
                    }

                    this.UpdateState(XSensState.Started);
                }
            }
        }

        private void DataAvailable(object sender, DataAvailableArgs e)
        {
            this.FireDataAvailable(e);
        }

        public void Stop()
        {
            if (_measuringDevice != null)
                _measuringDevice.clearCallbackHandlers();
            if (_myMtCallback != null)
                _myMtCallback.Dispose();

            _xda.Dispose();
            _xda = null;
        }

        private static List<XSensGyro> xsensGyros = new List<XSensGyro>();

        public static void StopAll()
        {
            foreach (XSensGyro xsensGyro in xsensGyros)
                xsensGyro.Stop();
        }

    }
}
