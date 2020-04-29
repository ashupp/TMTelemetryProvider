using System;
using System.Runtime.InteropServices;

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

        public double Pitch => (Device.Euler.y) * (180 / Math.PI);

        public double Roll => (Device.Euler.z) * (180 / Math.PI);

        public double Yaw => Device.CenteredYaw;

        public double RPM => Vehicle.EngineRpm;
    }
}