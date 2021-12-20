namespace SatsumaTuner95
{
    [System.Serializable]
    public class SaveData
    {
        public float WheelPosLong;
        public float WheelPosRally;
        public float WheelPosStock;
        public float PowerMultiplierOverride;
        public bool PowerMultiplierOverrideEnabled;
        public bool Automatic = false;
        public bool AutoReverse = false;
        public Drivetrain.Transmissions Transmission = Drivetrain.Transmissions.FWD;
        public bool TCS;
        public float TcsAllowedSlip;
        public float TcsMinVelocity;
        public bool ABS;
        public float AbsAllowedSlip;
        public float AbsMinVelocity;
        public bool ESP;
        public float EspStrength;
        public float EspMinVelocity;

        // cambers
        public float FrontCamber;
        public float RearCamber;

        // wheel offset x
        public bool CustomWheelsOffsetEnabled;
        public float FrontWheelsOffsetX;
        public float RearWheelsOffsetX;

        // suspension travel
        public float TravelLong;
        public float TravelRally;
        public float TravelStock;

        // suspension rates
        public float RallyFrontRate;
        public float StockFrontRate;
        public float LongRearRate;
        public float RallyRearRate;
        public float StockRearRate;

        // suspension bump and rebound
        public float RallyFrontLBump;
        public float RallyFrontLRebound;
        public float RallyFrontRBump;
        public float RallyFrontRRebound;
        public float RallyRearLBump;
        public float RallyRearLRebound;
        public float RallyRearRBump;
        public float RallyRearRRebound;

        public float StockFrontBump;
        public float StockFrontRebound;
        public float StockRearBump;
        public float StockRearRebound;

        // Center of Gravity
        public float CoG;

        // Gear ratios
        public System.Collections.Generic.List<float> GearRatios = new System.Collections.Generic.List<float>();
    }
}
