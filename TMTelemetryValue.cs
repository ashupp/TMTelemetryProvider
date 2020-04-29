namespace SimFeedback.telemetry
{
    public class TMTelemetryValue : AbstractTelemetryValue
    {
        public TMTelemetryValue(string name, object value) : base()
        {
            Name = name;
            Value = value;
        }

        public override object Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.Value, this.Unit);
        }
    }
}