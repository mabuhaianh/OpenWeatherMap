using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OpenWeatherMapAutomation {

    public class PerformanceCounter {

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public Int64 mStart;
        public Int64 mStop;
        public Int64 mFrequency;
		public Int64 mLastKnownValue = 0;

        public PerformanceCounter(Boolean autoStart = false) {

            if (autoStart) {

                StartCounter();
            }
        }

		public Double GetTimePassed() {

			QueryPerformanceCounter(out mLastKnownValue);
			return Convert.ToDouble(mLastKnownValue - mStart) / Convert.ToDouble(mFrequency);
		}

        public void StartCounter() {

            if (QueryPerformanceFrequency(out mFrequency) == false) {

                throw new Win32Exception();
            }

            QueryPerformanceCounter(out mStart);
        }

        public void StopCounter() {

            QueryPerformanceCounter(out mStop);
        }

		public Double StopCounterAndGetTimeInMS() {

			StopCounter();
            return GetTime() * 1000;
		}

        public Double StopCounterAndGetTimeInSeconds() {

            StopCounter();
            return GetTime();
        }

        public Double GetTime() {

            return Convert.ToDouble(mStop - mStart) / Convert.ToDouble(mFrequency);
        }

        public Int64 GetTicks() {

            return mStop - mStart;
        }
    }
}