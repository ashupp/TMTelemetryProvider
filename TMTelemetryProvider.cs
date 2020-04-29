using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using ILogger = SimFeedback.log.ILogger;
using System.Web.Script.Serialization;

namespace SimFeedback.telemetry
{
    public sealed class TMTelemetryProvider : AbstractTelemetryProvider
    {

        private const string sharedMemoryFile = @"Local\ManiaPlanet_Telemetry";
        private bool isStopped = true;
        private Thread t;


        public TMTelemetryProvider()
        {
            Author = "ashupp / ashnet GmbH";
            Version = "0.0.1.0";
            BannerImage = @"img\banner_tm.png";
            IconImage = @"img\icon_tm.png";
            TelemetryUpdateFrequency = 100;
        }

        private static float Rad2Deg(float v) { return (float)(v * 180 / Math.PI); }

        public override string Name => "tm";

        public override void Init(ILogger logger)
        {
            base.Init(logger);
            Log("Initializing TMTelemetryProvider");

        }

        public override string[] GetValueList()
        {
            return GetValueListByReflection(typeof(TMData));
        }

        public override void Stop()
        {
            if (isStopped) return;
            LogDebug("Stopping TMTelemetryProvider");
            isStopped = true;
            if (t != null) t.Join();
        }

        public override void Start()
        {
            if (isStopped)
            {
                LogDebug("Starting TMTelemetryProvider");
                isStopped = false;
                t = new Thread(Run);
                t.Start();
            }
        }

        private byte[] ReadBuffer(Stream memoryMappedViewStream, int size)
        {
            using (BinaryReader binaryReader = new BinaryReader(memoryMappedViewStream))
                return binaryReader.ReadBytes(size);
        }


        private void Run()
        {
            TMData lastTelemetryData = new TMData();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (!isStopped)
            {
                try
                {
                    TMData telemetryData = (TMData)readSharedMemory(typeof(TMData), sharedMemoryFile);
                    IsConnected = true;

                    if (telemetryData.Object.Timestamp != lastTelemetryData.Object.Timestamp){

                        //Debuglog("Get Data");
                        IsRunning = true;

                        sw.Restart();

                        TelemetryEventArgs args = new TelemetryEventArgs(new TMTelemetryInfo(telemetryData, lastTelemetryData));

                        RaiseEvent(OnTelemetryUpdate, args);
                        lastTelemetryData = telemetryData;

                    }
                    else if (sw.ElapsedMilliseconds > 500)
                    {
                        IsRunning = false;
                    }

                    Thread.Sleep(SamplePeriod);
                }
                catch (Exception e)
                {
                    LogError("TMTelemetryProvider Exception while processing data", e);
                    IsConnected = false;
                    IsRunning = false;
                    Thread.Sleep(1000);
                }
            }

            IsConnected = false;
            IsRunning = false;
        }
    }
}