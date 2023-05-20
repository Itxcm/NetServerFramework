using System;
using System.Runtime.InteropServices;

namespace ITXCM
{
    public class TimeUtil
    {
        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter([In, Out] ref long lpPerformanceCount);

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceFrequency([In, Out] ref long lpFrequency);

        static TimeUtil()
        {
            startupTicks = ticks;
        }

        private static long _frameCount = 0;

        /// <summary>
        /// The total number of frames that have passed (Read Only).
        /// </summary>
        public static long frameCount
        { get { return _frameCount; } }

        private static long startupTicks = 0;

        private static long freq = 0;

        /// <summary>
        /// Tick count
        /// </summary>
        public static long ticks
        {
            get
            {
                long f = freq;

                if (f == 0)
                {
                    if (QueryPerformanceFrequency(ref f))
                    {
                        freq = f;
                    }
                    else
                    {
                        freq = -1;
                    }
                }
                if (f == -1)
                {
                    return Environment.TickCount * 10000;
                }
                long c = 0;
                QueryPerformanceCounter(ref c);
                return (long)(((double)c) * 1000 * 10000 / ((double)f));
            }
        }

        private static long lastTick = 0;
        private static float _deltaTime = 0;

        /// <summary>
        /// The time in seconds it took to complete the last frame (Read Only).
        /// </summary>
        public static float deltaTime
        {
            get
            {
                return _deltaTime;
            }
        }

        private static float _time = 0;

        /// <summary>
        ///  The time at the beginning of this frame (Read Only). This is the time in seconds
        ///  since the start of the game.
        /// </summary>
        public static float time
        {
            get
            {
                return _time;
            }
        }

        /// <summary>
        /// The real time in seconds since the started (Read Only).
        /// </summary>
        public static float realtimeSinceStartup
        {
            get
            {
                long _ticks = ticks;
                return (_ticks - startupTicks) / 10000000f;
            }
        }

        public static void Tick()
        {
            long _ticks = ticks;

            _frameCount++;
            if (_frameCount == long.MaxValue)
                _frameCount = 0;

            if (lastTick == 0) lastTick = _ticks;
            _deltaTime = (_ticks - lastTick) / 10000000f;
            _time = (_ticks - startupTicks) / 10000000f;
            lastTick = _ticks;
        }

        // 当前时间戳
        public static double timeStamp => GetTimestamp(DateTime.Now);

        // 根据指定时间戳 获取时间
        public static DateTime GetTime(long timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = timeStamp * 10000000;
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }

        // 根据时间 获取指定时间戳
        public static double GetTimestamp(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (time - startTime).TotalSeconds;
        }
    }
}
