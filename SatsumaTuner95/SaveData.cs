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
        public bool CustomWheelsOffsetEnabled;
        public float FrontWheelsOffsetX;
        public float RearWheelsOffsetX;
        public System.Collections.Generic.List<float> GearRatios = new System.Collections.Generic.List<float>();
    }
}
