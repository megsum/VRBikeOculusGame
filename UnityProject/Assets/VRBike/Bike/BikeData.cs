using System;

using UnityEngine;

namespace MedRoad.VRBike
{
    /// <summary>
    /// This class provides an interface between the BikeController and other classes to exchange
    /// biodata and data about the bike (e.g., speed, power, etc.).
    /// </summary>
    public class BikeData
    {

        #region BioData

        // This section contains the fields, properties, and methods to deal with the
        // bio data we receive from the phone.

        [FlagsAttribute]
        public enum BioDevice : int
        {
            None = 0,
            HeartRate = 1,
            BloodOxygen = 2,
            BloodPressure = 4,
            ECG = 8
        }

        private BioDevice _bioDevices = BioDevice.None;

        private int _bioHR; // BPM
        private int _bioOX; // SpO2
        private int[] _bioBP = new int[2]; // mmHg
        private int[] _bioECG = new int[25];

        private bool _isSnap; // True = Kickr Snap else False

        /// <summary>
        /// When a bioDevice has its value set for the first time we set the flag for
        /// that device here. This allows us to tell when we receive the first reading
        /// for each biodevice and reset the StatsManager appropriately.
        /// </summary>
        private BioDevice _bioDeviceValueSet = BioDevice.None;


        /// <summary>
        /// Gets or sets BioDevice flags indicating which biodevices are connected.
        /// </summary>
        /// <value>BioDevice flags.</value>
        public BioDevice BioDevices
        {
            get { return this._bioDevices; }
            set { this._bioDevices = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether a heartrate monitor is connected.
        /// </summary>
        /// <value><c>true</c> if we have a heartrate monitor; otherwise, <c>false</c>.</value>
        public bool BioHasHR
        {
            get { return (this._bioDevices & BioDevice.HeartRate) != 0; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether an oximeter is connected.
        /// </summary>
        /// <value><c>true</c> if we have an oximeter; otherwise, <c>false</c>.</value>
        public bool BioHasOX
        {
            get { return (this._bioDevices & BioDevice.BloodOxygen) != 0; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether a blood pressure monitor is connected.
        /// </summary>
        /// <value><c>true</c> if we have a blood pressure monitor; otherwise, <c>false</c>.</value>
        public bool BioHasBP
        {
            get { return (this._bioDevices & BioDevice.BloodPressure) != 0; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether an ECG is connected.
        /// </summary>
        /// <value><c>true</c> if we have an ECG; otherwise, <c>false</c>.</value>
        public bool BioHasECG
        {
            get { return (this._bioDevices & BioDevice.ECG) != 0; }
        }

        /// <summary>
        /// Gets or sets the value last sent by the heartrate monitor in BPM.
        /// </summary>
        /// <value>The heartrate value in BPM.</value>
        public int BioHR
        {
            get { return this._bioHR; }
            set
            {
                // Sometimes the device sends 0, ignore it.
                if (value == 0)
                    return;

                if ((this._bioDeviceValueSet & BioDevice.HeartRate) == 0)
                    this._bioDeviceValueSet = this._bioDeviceValueSet | BioDevice.HeartRate;

                this._bioHR = value;
            }
        }

        /// <summary>
        /// Gets or sets the value last sent by the oximeter (%SpO2).
        /// </summary>
        /// <value>The oxygen saturation value (%SpO2).</value>
        public int BioOX
        {
            get { return this._bioOX; }
            set
            {
                if ((this._bioDeviceValueSet & BioDevice.BloodOxygen) == 0)
                    this._bioDeviceValueSet = this._bioDeviceValueSet | BioDevice.BloodOxygen;

                this._bioOX = value;
            }
        }

        /// <summary>
        /// Gets or sets a 2-item array containing the values last sent by the blood pressure
        /// monitor (mmHg) where the value at index 0 is the systolic reading and the value at
        /// index 1 is the diastolic reading.
        /// </summary>
        /// <value>An array of blood pressure values (mmHg).</value>
        public int[] BioBP
        {
            get { return this._bioBP; }
            set { this._bioBP = value; }
        }

        /// <summary>
        /// This method should be called whenever the BioBP array is updated with new values.
        /// </summary>
        public void BioBPUpdated()
        {
            // Since the elements of the array can be updated individually, we can't rely on
            // the property to tell when new blood pressure values have been received.

            if ((this._bioDeviceValueSet & BioDevice.BloodPressure) == 0)
                this._bioDeviceValueSet = this._bioDeviceValueSet | BioDevice.BloodPressure;
        }

        /// <summary>
        /// Gets or sets a 25-item array containing the values last sent by the ECG.
        /// </summary>
        /// <value>An array of ECG values.</value>
        public int[] BioECG
        {
            get { return this._bioECG; }
            set { this._bioECG = value; }
        }

        /// <summary>
        /// This method should be called whenever the BioECG array is updated with new values.
        /// </summary>
        public void BioECGUpdated()
        {
            // Since the elements of the array can be updated individually, we can't rely on
            // the property to tell when new blood pressure values have been received.

            if ((this._bioDeviceValueSet & BioDevice.ECG) == 0)
                this._bioDeviceValueSet = this._bioDeviceValueSet | BioDevice.ECG;
        }

        /// <summary>
        /// True if the user is using Kickr Snap else False.
        /// </summary>
        public bool IsSnap
        {
            get { return this._isSnap; }
            set { this._isSnap = value; }
        }

        #endregion

        #region BikeData

        // This section contains the fields, properties, and methods to deal with the
        // bike data we receive from the phone.

        private float _bikeWheelDiameter = 0.7366f; // m (29 in)
        private float _bikeSpeedSensitivity = 0; // float to set the field of view of camera
        private float _bikeCR; // RPM

        private bool _useAntKickr = false;
        private float _phoneWR;  // RPM
        private float _phonePWR; // Watts
        private float _antSpeed; // m/s
        private float _antPWR;   // Watts

        private bool _useXsensGyro = false;
        private float[] _phoneGYR = new float[3]; // Degrees
        private float[] _xsensGYR = new float[3]; // Degrees
        

        // Set by default to 29 in so the bike always animates.
        private float _bikeRPMToLinearVelocityFactor = 0.03856828581f;
        private bool _ignoreKickrVelocity = false;

        /// <summary>
        /// Gets or sets the wheel diameter of the bike in meters. Setting the value of the
        /// wheel diameter will also internally update the conversion factor used to calculate
        /// the linear velocity from the wheel revs.
        /// </summary>
        /// <value>The bike wheel diameter in meters.</value>
        public float BikeWheelDiameter
        {
            get { return this._bikeWheelDiameter; }
            set
            {
                this._bikeWheelDiameter = value;
                this._bikeRPMToLinearVelocityFactor = value * Mathf.PI / 60.0f;
            }
        }

        /// <summary>
        /// Gets a factor based on the <see cref="BikeWheelDiameter"/> that can be used to convert
        /// between wheel RPMs and linear velocity.
        /// </summary>
        public float BikeRPMToLinearVelocityFactor
        {
            get { return this._bikeRPMToLinearVelocityFactor; }
        }

        /// <summary>
        /// Gets or sets the speed sensitivity of the bike.  This float value is used to
        /// set the field of view of the camera to make the user feel like they are
        /// going faster(higher float number) or slower (lower float number).  The
        /// sensitivity value is sent from the phone (processed in UDPSocketServer and 
        /// set in ConnectWithPhone classes) with the range of 1-10 as float.
        /// </summary>
        /// <value> Speed sensivity observed by the user. </value>
        public float BikeSpeedSensitivity
        {
            get { return this._bikeSpeedSensitivity; }
            set
            {
                this._bikeSpeedSensitivity = value;
            }
        }

        // TODO Implement some way to tell if we're receiving crank revs or not?
        // (Does the phone send us this data?)

        /// <summary>
        /// Gets or sets the latest number of crank revs (RPM) sent by the phone. This value is not
        /// always available.
        /// </summary>
        /// <value>The latest number of crank revs in RPM.</value>
        public float BikeCR
        {
            get { return this._bikeCR; }
            set
            {
                this._bikeCR = value;
            }
        }

        /// <summary>
        /// Gets or sets whether an Ant USB Stick is being used to connect directly to the Kickr.
        /// If <c>false</c>, the phone-relayed wheel revs and power readings will be used.
        /// </summary>
        public bool UsingAntKickr
        {
            get { return this._useAntKickr; }
            set { this._useAntKickr = value; }
        }

        /// <summary>
        /// Gets the latest number of wheel revs (RPM) sent by either the phone or, if
        /// <see cref="_useAntKickr"/> is <c>true</c>, Ant.
        /// </summary>
        /// <value>The latest number of wheel revs in RPM.</value>
        public float BikeWR
        {
            get { return (this._useAntKickr) ? this._antSpeed / this._bikeRPMToLinearVelocityFactor : this._phoneWR; }
        }

        /// <summary>
        /// Gets the latest linear speed of the bike sent by either the phone or, if
        /// <see cref="_useAntKickr"/> is <c>true</c>, Ant.
        /// </summary>
        /// <value>The latest bike speed in m/s.</value>
        public float BikeSpeed
        {
            get { return (this._useAntKickr) ? this._antSpeed : this._phoneWR * this._bikeRPMToLinearVelocityFactor; }
        }

        /// <summary>
        /// Gets or sets the latest number of wheel revs (RPM) sent by the phone. Setting this
        /// value will also update the KickrVelocity property in BikePhysics with the calculated
        /// linear velocity based on the wheel revs and wheel diameter.
        /// </summary>
        /// <value>The latest number of wheel revs in RPM.</value>
        public float PhoneWR
        {
            get { return this._phoneWR; }
            set
            {
                this._phoneWR = value;
            }
        }

        /// <summary>
        /// Gets or sets the latest number of wheel revs (RPM) sent by Ant. Setting this
        /// value will also update the KickrVelocity property in BikePhysics with the calculated
        /// linear velocity based on the wheel revs and wheel diameter.
        /// </summary>
        /// <value>The latest number of wheel revs in RPM.</value>
        public float AntSpeed
        {
            get { return this._antSpeed; }
            set
            {
                this._antSpeed = value;
            }
        }

        /// <summary>
        /// Gets the latest power reading in Watts sent by the phone or Ant.
        /// </summary>
        /// <value>The latest power reading in Watts.</value>
        public float BikePWR
        {
            get { return (this._useAntKickr) ? this._antPWR : this._phonePWR; }
        }

        /// <summary>
        /// Gets or sets the latest power reading in Watts sent by the phone.
        /// </summary>
        /// <value>The latest power reading in Watts.</value>
        public float PhonePWR
        {
            get { return this._phonePWR; }
            set
            {
                this._phonePWR = value;
            }
        }

        /// <summary>
        /// Gets or sets the latest power reading in Watts sent by Ant.
        /// </summary>
        /// <value>The latest power reading in Watts.</value>
        public float AntPWR
        {
            get { return this._antPWR; }
            set
            {
                this._antPWR = value;
            }
        }

        /// <summary>
        /// Gets or sets whether an XSens gyroscope is being used. If <c>false</c>,
        /// the phone gyroscope will be used.
        /// </summary>
        public bool UsingXSensGyro
        {
            get { return this._useXsensGyro; }
            set { this._useXsensGyro = value; }
        }

        /// <summary>
        /// Gets the latest gyrscope reading either from the phone or xsens gyroscope
        /// depending on the value of <see cref="UsingXSensGyro"/>. This is an array
        /// consisting of the x, y, and z euler-angles of the phone or gyroscope.
        /// </summary>
        /// <value>The latest gyrscope readings in degrees.</value>
        public float[] BikeGYR
        {
            get { return (this._useXsensGyro) ? this._xsensGYR : this._phoneGYR; }
        }

        /// <summary>
        /// Gets the latest gyrscope reading (degrees) sent by the phone. This is an array
        /// consisting of the x, y, and z euler-angles of the phone.
        /// </summary>
        /// <value>The latest gyrscope readings in degrees.</value>
        public float[] PhoneGYR
        {
            get { return this._phoneGYR; }
        }

        /// <summary>
        /// Gets the latest gyrscope reading (degrees) sent by an xsens gyroscope. This is
        /// an array consisting of the x, y, and z euler-angles of the gyroscope.
        /// </summary>
        /// <value>The latest gyrscope readings in degrees.</value>
        public float[] XsensGyro
        {
            get { return this._xsensGYR; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether changes in wheel revs should be used
        /// to update the KickrVelocity in BikePhysics. If set to <c>true</c> changes in the
        /// Kickr wheel revs will effectively be ignored. Primarily this is used to apply a
        /// constant velocity for debugging purposes.
        /// </summary>
        /// <value><c>false</c> if KickrVelocity in BikePhysics should be updated according to
        /// changes in wheel revs; otherwise, <c>true</c>.</value>
        public bool IgnoreKickrVelocity
        {
            get { return this._ignoreKickrVelocity; }
            internal set { this._ignoreKickrVelocity = value; }
        }

        #endregion

    }
}
