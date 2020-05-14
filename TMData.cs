using System;
using System.Runtime.InteropServices;
using Math = System.Math;

namespace SimFeedback.telemetry
{
    // See https://wiki.xaseco.org/wiki/Telemetry_interface

    public struct Vec3
    {
        public float x;
        public float y;
        public float z;
    }

    public struct Quat
    {
        public float w;
        public float x;
        public float y;
        public float z;
    }

    public struct SHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] Magic;     //  "ManiaPlanet_Telemetry"
        public uint Version;
        public uint Size;   // == sizeof(STelemetry)
    }

    public enum EGameState
    {
        EState_Starting,
        EState_Menus,
        EState_Running,
        EState_Paused,
    }

    public enum ERaceState
    {
        ERaceState_BeforeState,
        ERaceState_Running,
        ERaceState_Finished,
    }

    public struct SGameState
    {
        public EGameState State;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] GameplayVariant;  // player model 'StadiumCar', 'CanyonCar', ....
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] MapId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] MapName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] __future__;
    }

    public struct SRaceState
    {
        public ERaceState State;
        public uint Time;
        public uint NbRespawns;
        public uint NbCheckpoints;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 125)]
        public uint[] CheckpointTimes;
        public uint NbCheckpointsPerLap;    // new since Maniaplanet update 2019-10-10; not supported by Trackmania Turbo.
        public uint NbLaps; // new since Maniaplanet update 2019-10-10; not supported by Trackmania Turbo.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public char[] __future__;
    }
    public struct SObjectState
    {
        public uint Timestamp;
        public uint DiscontinuityCount; // the number changes everytime the object is moved not continuously (== teleported).
        public Quat Rotation;
        public Vec3 Translation;    // +x is "left", +y is "up", +z is "front"
        public Vec3 Velocity;   // (world velocity)
        public uint LatestStableGroundContactTime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] __future__;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SVehicleState
    {
        public uint Timestamp;
        public float InputSteer;
        public float InputGasPedal;
        public bool InputIsBraking;
        public bool InputIsHorn;
        public float EngineRpm; // 1500 -> 10000
        public int EngineCurGear;
        public float EngineTurboRatio;  // 1 turbo starting/full .... 0 -> finished
        public bool EngineFreeWheeling;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public bool[] WheelsIsGroundContact;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public bool[] WheelsIsSliping;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] WheelsDamperLen;
        public float WheelsDamperRangeMin;
        public float WheelsDamperRangeMax;
        public float RumbleIntensity;
        public uint SpeedMeter; // unsigned km/h
        public bool IsInWater;
        public bool IsSparkling;
        public bool IsLightTrails;
        public bool IsLightsOn;
        public bool IsFlying;   // long time since touching ground.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] __future__;
    }

    public struct SDeviceState
    {
        public Vec3 Euler;  // yaw, pitch, roll  (order: pitch, roll, yaw)
        public float CenteredYaw;   // yaw accumulated + recentered to apply onto the device
        public float CenteredAltitude;  // Altitude accumulated + recentered
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] __future__;
    }

    public struct TMData
    {
        public SHeader Header;
        public uint UpdateNumber;
        public SGameState Game;
        public SRaceState Race;
        public SObjectState Object;
        public SVehicleState Vehicle;
        public SDeviceState Device;


        public double Heave => Device.CenteredAltitude;

        public static double ConvertRange(
            int originalStart, int originalEnd, // original range
            int newStart, int newEnd, // desired range
            double value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (double)(newStart + ((value - originalStart) * scale));
        }


        //Reverses angles greater than minMag to a range between minMag and 0
        private float LoopAngle(float angle, float minMag)
        {

            float absAngle = Math.Abs(angle);

            if (absAngle <= minMag)
            {
                return angle;
            }

            float direction = angle / absAngle;

            //(180.0f * 1) - 135 = 45
            //(180.0f *-1) - -135 = -45
            float loopedAngle = (180.0f * direction) - angle;

            return loopedAngle;
        }

        private float recalculateAngles(float eulerValue)
        {
            var tmpData = (eulerValue * (180.0 / Math.PI) / 360.0 - Math.Truncate(eulerValue * (180.0 / Math.PI) / 360.0)) * 360;
            if (tmpData > 180)
            {
                var eulYNeg = eulerValue * -1;
                tmpData = ((eulYNeg * (180.0 / Math.PI) / 360.0 - Math.Truncate(eulYNeg * (180.0 / Math.PI) / 360.0)) * 360) + 360;
                tmpData = tmpData * -1;
            }

            return (float) tmpData;
        }

        public double Pitch => LoopAngle(recalculateAngles(Device.Euler.y),90);

        public double Roll => LoopAngle(recalculateAngles(Device.Euler.z), 90);
        

        public double Yaw => Device.CenteredYaw;

        public double RPM => Vehicle.EngineRpm;
    }
}