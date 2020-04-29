using System;
using System.Reflection;

namespace SimFeedback.telemetry
{
    public class TMTelemetryInfo : EventArgs, TelemetryInfo
    {
        private TMData _telemetryData;

        public TMTelemetryInfo(TMData telemetryData, TMData lastTelemetryData)
        {
            _telemetryData = telemetryData;
        }

        public TelemetryValue TelemetryValueByName(string name)
        {
            TMTelemetryValue tv;
            switch (name)
            {
                default:
                    object data;
                    Type eleDataType = typeof(TMData);
                    PropertyInfo propertyInfo;
                    FieldInfo fieldInfo = eleDataType.GetField(name);
                    if (fieldInfo != null)
                    {
                        data = fieldInfo.GetValue(_telemetryData);
                    }
                    else if ((propertyInfo = eleDataType.GetProperty(name)) != null)
                    {
                        data = propertyInfo.GetValue(_telemetryData, null);
                    }
                    else
                    {
                        throw new UnknownTelemetryValueException(name);
                    }
                    tv = new TMTelemetryValue(name, data);
                    object value = tv.Value;
                    if (value == null)
                    {
                        throw new UnknownTelemetryValueException(name);
                    }

                    break;
            }

            return tv;
        }
    }
}