using XDA;
using Xsens;

using MedRoad.XSensGyroscope;

namespace MedRoad.VRBike
{
    public class XSensGyroBridge
    {
        private XSensGyro xsensGyro;
        private BikeController bikeController;

        private ConnectedMtData _connectedData = new ConnectedMtData();
        private bool performZeroOnNextDataUpdate = false;
        private double zeroX;
        private double zeroY;
        private double zeroZ;

        public XSensGyroBridge(XSensGyro xsensGyro, BikeController bikeController)
        {
            this.xsensGyro = xsensGyro;
            this.bikeController = bikeController;

            xsensGyro.OnStateChange += XSensGyroStateChange;
            xsensGyro.OnDataAvailable += XSensGyroDataAvailable;
        }

        private void XSensGyroStateChange(object sender, XSensGyro.StateChangeEventArgs e)
        {
            if (e.State == XSensGyro.XSensState.Started)
                bikeController.bikeData.UsingXSensGyro = true;
        }

        private void XSensGyroDataAvailable(object sender, DataAvailableArgs e)
        {
            // Get Euler angles.
            XsEuler oriEuler = e.Packet.orientationEuler();
            _connectedData._orientation = oriEuler;

            if (_connectedData._orientation != null)
            {
                if (this.performZeroOnNextDataUpdate)
                {
                    this.zeroX = _connectedData._orientation.x();
                    this.zeroY = _connectedData._orientation.y();
                    this.zeroZ = _connectedData._orientation.z();
                    this.performZeroOnNextDataUpdate = false;
                }

                bikeController.bikeData.XsensGyro[0] = (float)-(_connectedData._orientation.z() - this.zeroZ);
                bikeController.bikeData.XsensGyro[1] = (float)(_connectedData._orientation.y() - this.zeroY);
                bikeController.bikeData.XsensGyro[2] = (float)(_connectedData._orientation.x() - this.zeroX);

                //Debug.LogFormat("{0,-5:f2}, {1,-5:f2}, {2,-5:f2} [°]\n",
                //                       _connectedData._orientation.x(),
                //                       _connectedData._orientation.y(),
                //                       _connectedData._orientation.z());
            }
        }

        public void Zero()
        {
            this.performZeroOnNextDataUpdate = true;
        }
    }
}
