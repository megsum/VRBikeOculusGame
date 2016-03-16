using System;
using System.Collections.Generic;
using System.Text;

namespace MedRoad.Ant
{
    public class ManufacturerInfoDataPage : DataPage
    {
        private const byte DATA_PAGE_NUMBER = 0x50;

        /// <summary>
        /// A dictionary mapping manufacturer Ids to manufacturer names.
        /// </summary>
        private static Dictionary<ushort, string> manufacturerNames =
            new Dictionary<ushort, string>
            {
                {1, "garmin"},
                {2, "garmin_fr405_antfs"},
                {3, "zephyr"},
                {4, "dayton"},
                {5, "idt"},
                {6, "srm"},
                {7, "quarq"},
                {8, "ibike"},
                {9, "saris"},
                {10, "spark_hk"},
                {11, "tanita"},
                {12, "echowell"},
                {13, "dynastream_oem"},
                {14, "nautilus"},
                {15, "dynastream"},
                {16, "timex"},
                {17, "metrigear"},
                {18, "xelic"},
                {19, "beurer"},
                {20, "cardiosport"},
                {21, "a_and_d"},
                {22, "hmm"},
                {23, "suunto"},
                {24, "thita_elektronik"},
                {25, "gpulse"},
                {26, "clean_mobile"},
                {27, "pedal_brain"},
                {28, "peaksware"},
                {29, "saxonar"},
                {30, "lemond_fitness"},
                {31, "dexcom"},
                {32, "wahoo_fitness"},
                {33, "octane_fitness"},
                {34, "archinoetics"},
                {35, "the_hurt_box"},
                {36, "citizen_systems"},
                {37, "magellan"},
                {38, "osynce"},
                {39, "holux"},
                {40, "concept2"},
                {42, "one_giant_leap"},
                {43, "ace_sensor"},
                {44, "brim_brothers"},
                {45, "xplova"},
                {46, "perception_digital"},
                {47, "bf1systems"},
                {48, "pioneer"},
                {49, "spantec"},
                {50, "metalogics"},
                {51, "4iiiis"},
                {52, "seiko_epson"},
                {53, "seiko_epson_oem"},
                {54, "ifor_powell"},
                {55, "maxwell_guider"},
                {56, "star_trac"},
                {57, "breakaway"},
                {58, "alatech_technology_ltd"},
                {59, "mio_technology_europe"},
                {60, "rotor"},
                {61, "geonaute"},
                {62, "id_bike"},
                {63, "specialized"},
                {64, "wtek"},
                {65, "physical_enterprises"},
                {66, "north_pole_engineering"},
                {67, "bkool"},
                {68, "cateye"},
                {69, "stages_cycling"},
                {70, "sigmasport"},
                {71, "tomtom"},
                {72, "peripedal"},
                {73, "wattbike"},
                {76, "moxy"},
                {77, "ciclosport"},
                {78, "powerbahn"},
                {79, "acorn_projects_aps"},
                {80, "lifebeam"},
                {81, "bontrager"},
                {82, "wellgo"},
                {83, "scosche"},
                {84, "magura"},
                {85, "woodway"},
                {86, "elite"},
                {87, "nielsen_kellerman"},
                {88, "dk_city"},
                {89, "tacx"},
                {90, "direction_technology"},
                {91, "magtonic"},
                {92, "1partcarbon"},
                {93, "inside_ride_technologies"},
                {94, "sound_of_motion"},
                {95, "stryd"},
                {96, "Indoorcycling Group (icg)"},
                {97, "mi_pulse"},
                {98, "bsx_athletics"},
                {99, "look"},
                {100, "campagnolo_srl"},
                {101, "body_bike_smart"},
                {102, "praxisworks"},
                {103, "Limits Technology Ltd."},
                {255, "development"},
                {257, "healthandlife"},
                {258, "lezyne"},
                {259, "scribe_labs"},
                {260, "zwift"},
                {261, "watteam"},
                {262, "recon"},
                {263, "favero_electronics"},
                {264, "dynovelo"},
                {265, "strava"},
                {266, "Amer Sports (precor)"},
                {267, "bryton"},
                {268, "sram"},
                {269, "MiTAC Global Corporation - Mio Technology (navman)"},
                {5759, "actigraphcorp"}
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
        /// A dictionary mapping manufacturer IDs to their names, according to the ANT alliance.
        /// </summary>
        public static Dictionary<ushort, string> ManufacturerNames
        {
            get { return manufacturerNames; }
        }

        /// <summary>
        /// A manufacturer set harware revision number.
        /// </summary>
        public byte HardwareRevision { get; private set; }

        /// <summary>
        /// A manufacturer ID number controlled by the ANT alliance.
        /// </summary>
        public ushort ManufacturerId { get; private set; }

        /// <summary>
        /// A manufacturer set model number.
        /// </summary>
        public ushort ModelNumber { get; private set; }

        #endregion

        /// <summary>
        /// Instantiates a new instance of this data page.
        /// </summary>
        protected internal ManufacturerInfoDataPage() { }

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
            // See ANT+ Common Pages, page 22 (ver. 2.4)
            // https://www.thisisant.com/resources/common-data-pages/

            if (!skipCheck)
                DataPage.CheckRecievedData(receivedData, true, DATA_PAGE_NUMBER);

            HardwareRevision = receivedData[4];

            ManufacturerId = (ushort)(((uint)receivedData[5]) + (((uint)receivedData[6]) << 8));

            ModelNumber = (ushort)(((uint)receivedData[7]) + (((uint)receivedData[8]) << 8));
        }

        /// <summary>
        /// Builds a string representation of this data page.
        /// </summary>
        /// <returns>A string representation of this data page.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("Manufacturer ID:          {0}\n", ManufacturerId);
            string manufacturerName = "";
            if (manufacturerNames.TryGetValue(ManufacturerId, out manufacturerName))
                b.AppendFormat("Manufacturer Name:        {0}\n", manufacturerName);

            b.AppendFormat("Harware Revision:         {0}\n", HardwareRevision);
            b.AppendFormat("Model Number:             {0}\n", ModelNumber);
            return b.ToString();
        }

    }
}
