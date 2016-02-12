using UnityEngine;

namespace MedRoad.VRBike
{
    /// <summary>
    /// BikePhysics is a helper class for the BikeController class that handles processing and
    /// calculations related to the physical movement of the bike.
    /// </summary>
    public class BikePhysics
    {
        public const float AVERAGE_HUMAN_MASS = 80.7f; // kg
        public const float AVERAGE_BIKE_MASS = 9f; // kg

        public const float EARTH_MEAN_RADIUS = 6371000f; // m
        public const float EARTH_STANDARD_GRAVITY = 9.80665f; // m/s^2

        public const float EARTH_SEA_LEVEL_STANDARD_ATMO_PRESSUE = 101325f; // Pa
        public const float EARTH_SEA_LEVEL_STANDARD_TEMP = 15f; // Celcius

        protected const float IDEAL_GAS_CONSTANT = 8.31447f; // J/(molK)
        protected const float MOLAR_MASS_OF_DRY_AIR = 0.0289644f; // kg/mol

        public const float DEFAULT_FORCE_CUTOFF = 0.33f;

        /// <summary>
        /// When _fallVelocity is greater than this amount we consider the bike to
        /// be off the ground.
        /// </summary>
        public const float OFF_GROUND_FALL_VELOCITY = 1f;

        protected BikeController bikeController;

        public BikePhysics(BikeController bikeController)
        {
            this.bikeController = bikeController;

            this._gravity = CalculateGravity();
            this._airDensity = CalculateAirDensity();

            // Use the value of going vertically straight up against gravity as the maximum force.
            this._maxForceForResistance = this._gravity * this._mass;

            this.heading = bikeController.Heading;
            this.pitch = bikeController.Pitch;
        }

        #region PLANET

        private float _planetMeanRadius = EARTH_MEAN_RADIUS; // m
        private float _planetStandardGravity = EARTH_STANDARD_GRAVITY; // m/s^2
        private float _gravity; // m/s^2

        private float _planetSeaLevelStandardAtmoPressure = EARTH_SEA_LEVEL_STANDARD_ATMO_PRESSUE; // Pa
        private float _planetSeaLevelStandardTemp = EARTH_SEA_LEVEL_STANDARD_TEMP; // Celcius
        private float _airDensity; // kg/m^3

        /// <summary>
        /// Gets or sets the mean radius of the planet in meters. By default this will be the mean
        /// radius of Earth. Setting this value will cause gravity to be recalculated.
        /// </summary>
        /// <value>The planet mean radius.</value>
        public float PlanetMeanRadius
        {
            get { return this._planetMeanRadius; }
            set
            {
                this._planetMeanRadius = value;
                this._gravity = this.CalculateGravity();
            }
        }

        /// <summary>
        /// Gets or sets the standard gravity of the planet in m/s^2. This is the acceleration due
        /// to gravity (in a vacuum) at sea level of the planet. By default this will be the
        /// standard gravity of Earth. Setting this value will cause gravity and air density to be
        /// recalculated.
        /// </summary>
        /// <value>The planet standard gravity.</value>
        public float PlanetStandardGravity
        {
            get { return this._planetStandardGravity; }
            set
            {
                this._planetStandardGravity = value;
                this._gravity = this.CalculateGravity();
                this._airDensity = this.CalculateAirDensity();
            }
        }

        /// <summary>
        /// Gets gravity in m/s^2, which depends on the PlanetMeanRadius, PlanetStandardGravity,
        /// and Altitude.
        /// </summary>
        public float Gravity
        {
            get { return this._gravity; }
        }

        /// <summary>
        /// Gets or sets the standard atmospheric pressure at sea level of the planet in Pa. By
        /// default this will be the standard atmospheric pressure at sea level for Earth. Setting
        /// this value will cause air density to be recalculated.
        /// </summary>
        /// <value>The planet sea level standard atmo pressure.</value>
        public float PlanetSeaLevelStandardAtmoPressure
        {
            get { return this._planetSeaLevelStandardAtmoPressure; }
            set
            {
                this._planetSeaLevelStandardAtmoPressure = value;
                this._airDensity = this.CalculateAirDensity();
            }
        }

        /// <summary>
        /// Gets or sets the standard temperature at sea level of the planet in degrees celcius.
        /// Setting this value will cause air density to be recalculated.
        /// </summary>
        /// <value>The planet sea level standard temp.</value>
        public float PlanetSeaLevelStandardTemp
        {
            get { return this._planetSeaLevelStandardTemp; }
            set
            {
                this._planetSeaLevelStandardTemp = value;
                this._airDensity = this.CalculateAirDensity();
            }
        }

        /// <summary>
        /// Gets the air density in kg/m^3. The air density is dependent on
        /// PlanetSeaLevelStandardTemp, PlanetSeaLevelStandardAtmoPressure, PlanetStandardGravity,
        /// Temperature, and Altitude.
        /// </summary>
        public float AirDensity
        {
            get { return this._airDensity; }
        }

        /// <summary>
        /// Calculates the gravity in m/s^2 based on planet mean radius, planet standard gravity,
        /// and altitude.
        /// </summary>
        /// <returns>The gravity.</returns>
        private float CalculateGravity()
        {
            // See https://en.wikipedia.org/wiki/Gravity_of_Earth#Altitude

            return this._planetStandardGravity *
                   Mathf.Pow(this._planetMeanRadius / (this._planetMeanRadius + this._altitude), 2.0f);
        }

        /// <summary>
        /// Calculates the air density in Pa/m^3 based on temperature, planet sea level standard
        /// temperature, altitude, planet standard gravity, and planet sea level standard
        /// atmospheric pressure.
        /// </summary>
        /// <returns>The air density.</returns>
        private float CalculateAirDensity()
        {
            // See https://en.wikipedia.org/wiki/Density_of_air#Altitude

            float tempRatio = (273.15f + this._temperature) / (273.15f + this._planetSeaLevelStandardTemp);

            float tempLapseRate = (this._planetSeaLevelStandardTemp - this._temperature) / this._altitude;

            float exponent = (this._planetStandardGravity * MOLAR_MASS_OF_DRY_AIR) / (IDEAL_GAS_CONSTANT * tempLapseRate);

            float pressure = this._planetSeaLevelStandardAtmoPressure * Mathf.Pow(tempRatio, exponent);

            return (pressure * MOLAR_MASS_OF_DRY_AIR) / (IDEAL_GAS_CONSTANT * (273.15f + this._temperature));
        }

        #endregion

        #region ENVIRONMENT

        private float _altitude = 0f; // m above sea level
        private float _temperature = 20f; // celcius
        private Vector3 _windVelocity = new Vector3(0, 0, 0); // magnitude is speed in m/s

        /// <summary>
        /// Gets or sets the altitude of the player in meters above sea level. Setting this value
        /// will cause gravity and air density to be recalculated. Changes in altitude during
        /// gameplay will have little effect so rather than updating this values often (if at all)
        /// it should be set for the average altitude of the map.
        /// </summary>
        /// <value>The altitude.</value>
        public float Altitude
        {
            get { return this._altitude; }
            set
            {
                this._altitude = value;
                this._gravity = this.CalculateGravity();
                this._airDensity = this.CalculateAirDensity();
            }
        }

        /// <summary>
        /// Gets or sets the temperature in degrees celcius. Setting this value will cause air
        /// density to be recalculated.
        /// </summary>
        /// <value>The temperature.</value>
        public float Temperature
        {
            get { return this._temperature; }
            set
            {
                this._temperature = value;
                this._airDensity = this.CalculateAirDensity();
            }
        }

        /// <summary>
        /// Gets or sets the wind velocity as a velocity vector in m/s. Each component of the
        /// vector represents component of the wind velocity along that axis in m/s (the total wind
        /// speed would be the magnitude of the vector).
        /// </summary>
        /// <value>The wind velocity.</value>
        public Vector3 WindVelocity
        {
            get { return this._windVelocity; }
            set { this._windVelocity = value; }
        }

        /// <summary>
        /// Sets the wind velocity with a speed and direction.
        /// </summary>
        /// <param name="windSpeed">The wind speed in m/s.</param>
        /// <param name="direction">The direction in the wind is blowing in the XZ-plane in degrees
        /// where 0 degrees is forward (along the blue arrow in Unity), 90 degrees is to the right
        /// (along the red arrow in Unity) 180 degrees is backward, and 270 degrees is to the left.
        /// </param>
        public void SetWindVelocity(float windSpeed, float direction)
        {
            Vector3 directionVector = Quaternion.AngleAxis(direction, Vector3.up) * Vector3.forward;
            this._windVelocity = directionVector * windSpeed;
        }

        #endregion

        #region BIKE

        private float _mass = AVERAGE_HUMAN_MASS + AVERAGE_BIKE_MASS; // kg

        private float heading = 0f;
        private float pitch = 0f;

        private float _velocity = 0f; // magnitude is speed in m/s
        private float _energy = 0f; // J
        private Vector3 _movementDirection;
        private Vector3 _fastMovementDirection;

        private float _maxForceForResistance;
        private float _kickrResistance = 0;

        /// <summary>
        /// Gets or sets the total mass of the bike and rider. This defaults to the average mass of
        /// a human added to the average mass of a bike.
        /// </summary>
        /// <value>The mass.</value>
        public float Mass
        {
            get { return this._mass; }
            set { this._mass = value; }
        }

        /// <summary>
        /// Gets the heading of the bike in degrees. This value will be lerped to smooth changes in
        /// heading, so note that it will lag behind the heading value of the bike controller.
        /// </summary>
        /// <value>The heading.</value>
        public float Heading
        {
            get { return this.heading; }
        }

        /// <summary>
        /// Gets the pitch of the bike in degrees above the horizontal axis. This value will be
        /// lerped to smooth changes in pitch, so note that it will lag behind the pitch value of
        /// the bike controller.
        /// </summary>
        /// <value>The pitch.</value>
        public float Pitch
        {
            get { return this.pitch; }
        }

        /// <summary>
        /// Gets the magnitiude of the velocity of the bike in m/s.
        /// </summary>
        /// <value>The velocity.</value>
        public float Velocity
        {
            get { return this._velocity; }
        }

        /// <summary>
        /// Gets the velocity as a velocity vector (that is, the x, y, and z components of the
        /// velocity of the bike) in m/s.
        /// </summary>
        /// <value>The velocity vector.</value>
        public Vector3 VelocityVector
        {
            get { return this._velocity * this._movementDirection; }
        }

        /// <summary>
        /// Gets the movement direction of the bike. This is a normalized vector that faces the
        /// direction of movement of the bike. Note that this is based on the interpolated values
        /// of heading and pitch, so it will lag behind the true heading and pitch depending on
        /// their lerp rates.
        /// </summary>
        /// <value>The movement direction.</value>
        public Vector3 MovementDirection
        {
            get { return this._movementDirection; }
        }

        /// <summary>
        /// Gets the fast movement direction of the bike. This is a normalized vector that faces
        /// the direction of movement of the bike. Unlike MovementDirection, FastMovementDirection
        /// uses the immediate values of heading and pitch without any interpolation.
        /// </summary>
        /// <value>The fast movement direction.</value>
        public Vector3 FastMovementDirection
        {
            get { return this._fastMovementDirection; }
        }

        /// <summary>
        /// Gets or sets the maximum force for resistance. This value is used to scale the forces
        /// applied to the bike to a value for the kickr resistance. If the forces applied to the
        /// bike are MaxForceForResistance, the value of the KickrResistance will be 100.
        /// </summary>
        /// <value>The max force to be used in calculating resistance for the Kickr.</value>
        public float MaxForceForResistance
        {
            get { return this._maxForceForResistance; }
            set { this._maxForceForResistance = value; }
        }

        /// <summary>
        /// Gets the Kickr resistance.
        /// </summary>
        /// <value>The Kickr resistance value.</value>
        public float KickrResistance
        {
            get { return this._kickrResistance; }
        }

        /// <summary>
        /// Calculates the Kickr resistance as a combination of the gravity force, tire drag force,
        /// collision force, and wind force.
        /// </summary>
        /// <returns>The Kickr resistance value.</returns>
        private float CalculateResistance()
        {
            if (this._velocity < 0 || this.OffGround)
                return 0f;

            float forces = -this._gravityForce - this._tireDragForce - this._collisionForce - this._windForce;

            // if (this._windForce < 0f)
            //	forces += this._windForce;

            return (forces / this._maxForceForResistance) * 100f;
        }

        #endregion

        #region FORCES

        // The Crr of a name-brand racing tire will range from 0.004 to 0.007. I assume this is on
        // pavement. From what I can tell gravel will be about 2.5 times this value, and grass will
        // be 6 times the value.
        private float _coeffRollingResistance = 0.08f;

        // The CdA of the combined bike and rider in a triathlon can fall into the range of about
        // 0.25 to 0.33. I will note that very few people get as low as 0.25; the fat part of the
        // bell curve seems to be in the 0.28-0.31 range and plenty of folks are over 0.33.
        private float _coeffOfDragTimesFrontalArea = 3.99f; // m^2

        private float _kickrVelocity = 0f;

        private float _forceCutoff = DEFAULT_FORCE_CUTOFF;

        private float _tireDragForce = 0f;
        private float _windForce = 0f;
        private float _gravityForce = 0f;
        private float _collisionForce = 0f;
        private float _totalForce = 0f;

        /// <summary>
        /// Gets or sets the coefficient of rolling resistance (unitless), which depends both on
        /// the type of tire and the surface material of the track (pavement, gravel, grass, etc.). 
        /// </summary>
        /// <value>The coefficient of rolling resistance.</value>
        public float CoefficientOfRollingResistance
        {
            get { return this._coeffRollingResistance; }
            set { this._coeffRollingResistance = value; }
        }

        /// <summary>
        /// Gets or sets the coefficient of drag times the frontal area in square meters. This
        /// value is used for calculating wind resistance. 
        /// </summary>
        /// <value>The coefficient of drag times frontal area.</value>
        public float CoefficientOfDragTimesFrontalArea
        {
            get { return this._coeffOfDragTimesFrontalArea; }
            set { this._coeffOfDragTimesFrontalArea = value; }
        }

        /// <summary>
        /// Gets or sets the linear velocity of the Kickr in m/s.
        /// </summary>
        /// <value>The kickr velocity.</value>
        public float KickrVelocity
        {
            get { return this._kickrVelocity; }
            set { this._kickrVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the force cutoff velocity. When the velocity of the bike is below this
        /// value gravity and wind will only act to oppose the movement of the bike (i.e., the bike
        /// won't be pulled down a hill by gravity, or a tailwind won't push the bike forward).
        /// </summary>
        /// <value>The force cutoff velocity.</value>
        public float ForceCutoffVelocity
        {
            get { return this._forceCutoff; }
            set { this._forceCutoff = value; }
        }

        /// <summary>
        /// Gets the force being applied along the movement direction due to gravity.
        /// </summary>
        /// <value>The gravity force.</value>
        public float GravityForce
        {
            get { return this._gravityForce; }
        }

        /// <summary>
        /// Gets the force being applied along the movement direction due to tire drag.
        /// </summary>
        /// <value>The tire drag force.</value>
        public float TireDragForce
        {
            get { return this._tireDragForce; }
        }

        /// <summary>
        /// Gets the force being applied along the movement direction due to wind.
        /// </summary>
        /// <value>The wind resistance force.</value>
        public float WindForce
        {
            get { return this._windForce; }
        }

        /// <summary>
        /// Gets the force being applied to the bike due to a collision.
        /// </summary>
        /// <value>The collision force.</value>
        public float CollisionForce
        {
            get { return this._collisionForce; }
        }

        /// <summary>
        /// Gets the sum of all forces being applied to the bike along the direction of movement.
        /// </summary>
        public float TotalForce
        {
            get { return this._totalForce; }
        }

        /// <summary>
        /// Calculates the force applied along the direction of travel generated by gravity. This
        /// value is dependent on Mass, Gravity, and the FastMovementDirection.
        /// </summary>
        /// <returns>The gravity force.</returns>
        protected void CalculateGravityForce()
        {
            Vector3 gravityForce = Vector3.down * this._mass * this._gravity;
            // Get the component of gravityForce along the vector opposing the direction of travel.
            this._gravityForce = Vector3.Dot(gravityForce, this._fastMovementDirection);
        }

        /// <summary>
        /// Calculates the force applied along the direction of travel generated by the rolling
        /// friction of the tires. The tire drag force will always oppose the movment of the bike,
        /// so it's sign will always be opposite to the sign of the velocity. This value is
        /// dependent on Mass, Gravity, GravityForce, Velocity, and the
        /// CoefficientOfRollingResistance.
        /// </summary>
        /// <returns>The tire drag force.</returns>
        protected void CalculateTireDragForce()
        {
            if (this.OffGround)
            {
                this._tireDragForce = 0f;
                return;
            }

            float gravDownForce = this._mass * this._gravity;
            // Get the component of gravity perpendicular to the slope (i.e., the normal force).
            float gravityNormal = Mathf.Sqrt((gravDownForce * gravDownForce) - (this._gravityForce * this._gravityForce));
            float tireDrag = this._coeffRollingResistance * gravityNormal;
            this._tireDragForce = (this._velocity >= 0) ? -tireDrag : tireDrag;
        }

        /// <summary>
        /// Calculates the force applied along the direction of travel generated by wind. This
        /// value is dependent on Velocity, FastMovementDirection, WindVelocity, AirDensity, and
        /// the CoefficientOfDragTimesFrontalArea.
        /// </summary>
        /// <returns>The wind resistance force.</returns>
        protected void CalculateWindForce()
        {
            Vector3 relativeWindVelocity = (this._velocity * this._fastMovementDirection) - this._windVelocity;
            // Get the component of relativeWindVelocity along the vector opposing the direction of travel.
            float windVelocity = Vector3.Dot(relativeWindVelocity, this._fastMovementDirection);

            float windResistance = 0.5f * this._airDensity * this._coeffOfDragTimesFrontalArea * windVelocity * windVelocity;

            this._windForce = (windVelocity >= 0) ? -windResistance : windResistance;
        }

        /// <summary>
        /// Calculates the sum of all forces being applied to the bike.
        /// </summary>
        protected void CalculateTotalForce()
        {
            this.CalculateGravityForce();
            this.CalculateTireDragForce();
            this.CalculateWindForce();

            // float forces = this._kickrForce + this._tireDragForce + this._collisionForce;
            float forces = this._tireDragForce + this._collisionForce;

            // Reset collision force once applied.
            this._collisionForce = 0;

            // If velocity is below the force cutoff velocity, only apply gravity and wind forces
            // if they are opposing the direction of travel.
            if (this._velocity < this._forceCutoff)
            {
                if (Sign(this._velocity) != Sign(this._gravityForce))
                    forces += this._gravityForce;

                if (Sign(this._velocity) != Sign(this._windForce))
                    forces += this._windForce;

            }
            else
            {
                forces += this._gravityForce + this._windForce;
            }

            this._totalForce = forces;
        }

        /// <summary>
        /// Applies the given acceleration as a collision force in the next update.
        /// </summary>
        /// <param name="acceleration">The acceleration (deacceleration) that occurred due to the
        /// collision.</param>
        public void ApplyCollisionForce(float acceleration)
        {
            float collision = this._mass * acceleration;
            this._collisionForce += (this._velocity >= 0) ? -collision : collision;
        }

        #endregion

        #region FALLING

        private float _fallVelocity = 0f;

        private Vector3 _fallPosition;

        /// <summary>
        /// Gets the velocity currently being applied to the bike in the downward direction due to
        /// gravity moving the bike down. If the bike is on the ground (not falling), this value
        /// should be 0 (it may fluctutate slightly as slope changes).
        /// </summary>
        /// <value>The velocity being applied to make the bike fall.</value>
        public float FallVelocity
        {
            get { return this._fallVelocity; }
        }

        /// <summary>
        /// Gets the position of the bike shifted in the y-axis to account for the FallVelocity.
        /// </summary>
        public Vector3 FallPosition
        {
            get { return this._fallPosition; }
        }

        /// <summary>
        /// Gets a value indicating whether the bike is off the ground or not.
        /// </summary>
        /// <value><c>true</c> if the bike is off the ground; otherwise, <c>false</c> if it is on
        /// the ground.</value>
        public bool OffGround
        {
            get { return this._fallVelocity > OFF_GROUND_FALL_VELOCITY; }
        }

        /// <summary>
        /// Calculates the vertical position of the bike given its current position and its target
        /// vertical position (the vertical position of the bike if it were sitting on the ground).
        /// </summary>
        /// <returns>The new calculated height of the bike.</returns>
        /// <param name="currentHeight">The current height of the bike.</param>
        /// <param name="targetHeight">The target (on ground) height of the bike.</param>
        public float CalculateHeight(float currentHeight, float targetHeight)
        {
            // If we're higher than we should be, calculate and apply gravity.
            if (currentHeight > targetHeight)
            {
                this._fallVelocity += this._gravity * Time.deltaTime;
                currentHeight -= this._fallVelocity * Time.deltaTime;
            }

            // If we're lower than we should be, set our height to targetHeight and
            // reset our velocity due to gravity.
            if (currentHeight < targetHeight)
            {
                this._fallVelocity = 0f;
                currentHeight = targetHeight;
            }

            return currentHeight;
        }

        #endregion

        public void DoOnFixedUpdate()
        {
            // Adjust the pitch lerp rate... for sudden large changes in pitch we want to lerp more
            // quickly.
            float pitchLerpAmount = bikeController.PitchLerpAmount;
            if (Mathf.Abs(bikeController.Pitch - this.pitch) > 5f)
                pitchLerpAmount *= Mathf.Abs(bikeController.Pitch - this.pitch) / 5f * 1.5f;

            // Update heading and pitch.
            this.heading = Mathf.LerpAngle(this.heading, bikeController.Heading, bikeController.HeadingLerpAmount);
            this.pitch = Mathf.LerpAngle(this.pitch, bikeController.Pitch, pitchLerpAmount);

            // Update movement direction vector.
            this._movementDirection = Quaternion.Euler(-this.pitch, this.heading, 0f) * Vector3.forward;
            this._fastMovementDirection = Quaternion.Euler(-bikeController.Pitch, bikeController.Heading, 0f) * Vector3.forward;

            // Update all forces.
            this.CalculateTotalForce();

            // Use either the bike velocity, or the actual velocity of the kickr, whichever is
            // higher.
            float maxVelocity = Mathf.Max(this._velocity, this._kickrVelocity);
            this._velocity = maxVelocity + this._totalForce * Time.deltaTime / this._mass;

            // Don't allow the bike to move backwards.
            if (this._velocity < 0f)
                this._velocity = 0f;

            // Update resistance.
            this._kickrResistance = CalculateResistance();

            // Update the vertical position based on the fall velocity and ground height.
            float targetHeight = this.bikeController.currentAltitude + this.bikeController.GroundHeight;
            this._fallPosition = this.bikeController.transform.position;
            this._fallPosition.y = this.CalculateHeight(this._fallPosition.y, targetHeight);
        }

        /// <summary>
        /// Returns true if the number is non-negative, false otherwise.
        /// </summary>
        /// <param name="num">The number to test.</param>
        private static bool Sign(float num)
        {
            return (num >= 0);
        }

    }
}
