using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.World
{
    /// <summary>
    /// Controls the in-game time.
    /// Uses direct IL2CPP access via GameTypes.Time.
    /// 
    /// Key properties:
    /// - CurrentTime (get/set) - Time in minutes (0-1440)
    /// - ElapsedDays (get/set) - Total days elapsed
    /// - CurrentDay (get) - Day of week (EDay enum)
    /// - IsNight (get) - Whether it's nighttime
    /// - TimeProgressionMultiplier (get/set) - Speed of time
    /// 
    /// Key methods:
    /// - SetTime(int time, bool local) - Sets time in minutes
    /// - SetElapsedDays(int days) - Sets elapsed days
    /// - ForceSleep() - Forces sleep
    /// </summary>
    public class TimeController : FeatureBase
    {
        public override string Id => "timecontroller";
        public override string Name => "Time Controller";
        public override string Description => "Control the time of day";
        public override FeatureCategory Category => FeatureCategory.World;
        public override bool IsToggleable => false;

        public float TargetTime { get; set; } = 12f; // 12:00 noon (in hours)
        public bool FreezeTime { get; set; } = false;
        
        private float _originalTimeMultiplier = 1f;
        private bool _wasTimeFrozen = false;

        /// <summary>
        /// Gets the current time of day (0-24 hours).
        /// </summary>
        public float GetCurrentTime()
        {
            var time = GameTypes.Time;
            if (time == null) return 12f;

            try
            {
                // CurrentTime is in minutes (0-1440), convert to hours
                return time.CurrentTime / 60f;
            }
            catch
            {
                return 12f;
            }
        }

        /// <summary>
        /// Gets the current time in minutes (0-1440).
        /// </summary>
        public int GetCurrentTimeMinutes()
        {
            var time = GameTypes.Time;
            if (time == null) return 720;

            try
            {
                return time.CurrentTime;
            }
            catch
            {
                return 720;
            }
        }

        /// <summary>
        /// Gets the current day number (elapsed days).
        /// </summary>
        public int GetCurrentDay()
        {
            var time = GameTypes.Time;
            if (time == null) return 1;

            try
            {
                return time.ElapsedDays;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets the current day of the week.
        /// </summary>
        public string GetDayOfWeek()
        {
            var time = GameTypes.Time;
            if (time == null) return "Unknown";

            try
            {
                return time.CurrentDay.ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Checks if it's currently night.
        /// </summary>
        public bool IsNight()
        {
            var time = GameTypes.Time;
            if (time == null) return false;

            try
            {
                return time.IsNight;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the time of day (0-24 hours).
        /// </summary>
        public void SetTime(float hours)
        {
            SafeExecute(() =>
            {
                var time = GameTypes.Time;
                if (time == null)
                {
                    SewerLogger.Warning("TimeManager not found");
                    return;
                }

                // Clamp time to valid range and convert to minutes
                hours = Mathf.Clamp(hours, 0f, 24f);
                int minutes = Mathf.RoundToInt(hours * 60f);
                
                // SetTime(int time, bool local)
                time.SetTime(minutes, false);
                SewerLogger.Success($"Set time to {FormatTime(hours)}");
            }, "setting time");
        }

        /// <summary>
        /// Sets the time in minutes (0-1440).
        /// </summary>
        public void SetTimeMinutes(int minutes)
        {
            SafeExecute(() =>
            {
                var time = GameTypes.Time;
                if (time == null)
                {
                    SewerLogger.Warning("TimeManager not found");
                    return;
                }

                minutes = Mathf.Clamp(minutes, 0, 1440);
                time.SetTime(minutes, false);
                SewerLogger.Success($"Set time to {FormatTime(minutes / 60f)}");
            }, "setting time");
        }

        /// <summary>
        /// Advances time by the specified hours.
        /// </summary>
        public void AdvanceTime(float hours)
        {
            var currentTime = GetCurrentTime();
            var newTime = (currentTime + hours) % 24f;
            SetTime(newTime);
        }

        /// <summary>
        /// Sets time to morning (6:00).
        /// </summary>
        public void SetMorning() => SetTime(6f);

        /// <summary>
        /// Sets time to noon (12:00).
        /// </summary>
        public void SetNoon() => SetTime(12f);

        /// <summary>
        /// Sets time to evening (18:00).
        /// </summary>
        public void SetEvening() => SetTime(18f);

        /// <summary>
        /// Sets time to midnight (0:00).
        /// </summary>
        public void SetMidnight() => SetTime(0f);

        /// <summary>
        /// Skips to the next day.
        /// </summary>
        public void SkipDay()
        {
            SafeExecute(() =>
            {
                var time = GameTypes.Time;
                if (time == null) return;

                try
                {
                    int newDays = time.ElapsedDays + 1;
                    time.SetElapsedDays(newDays);
                    SewerLogger.Success($"Skipped to day {newDays}");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to skip day", ex);
                }
            }, "skipping day");
        }

        /// <summary>
        /// Gets the time progression multiplier.
        /// </summary>
        public float GetTimeMultiplier()
        {
            var time = GameTypes.Time;
            if (time == null) return 1f;

            try
            {
                return time.TimeProgressionMultiplier;
            }
            catch
            {
                return 1f;
            }
        }

        /// <summary>
        /// Sets the time progression multiplier.
        /// </summary>
        public void SetTimeMultiplier(float multiplier)
        {
            SafeExecute(() =>
            {
                var time = GameTypes.Time;
                if (time == null) return;

                try
                {
                    time.TimeProgressionMultiplier = multiplier;
                    SewerLogger.Success($"Set time multiplier to {multiplier:F1}x");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to set time multiplier", ex);
                }
            }, "setting time multiplier");
        }

        /// <summary>
        /// Freezes or unfreezes time.
        /// </summary>
        public void SetTimeFrozen(bool frozen)
        {
            SafeExecute(() =>
            {
                var time = GameTypes.Time;
                if (time == null) return;

                try
                {
                    if (frozen && !_wasTimeFrozen)
                    {
                        _originalTimeMultiplier = time.TimeProgressionMultiplier;
                        time.TimeProgressionMultiplier = 0f;
                        _wasTimeFrozen = true;
                        SewerLogger.Success("Time frozen");
                    }
                    else if (!frozen && _wasTimeFrozen)
                    {
                        time.TimeProgressionMultiplier = _originalTimeMultiplier;
                        _wasTimeFrozen = false;
                        SewerLogger.Success("Time unfrozen");
                    }
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to freeze/unfreeze time", ex);
                }
            }, frozen ? "freezing time" : "unfreezing time");
        }

        /// <summary>
        /// Formats time as HH:MM string.
        /// </summary>
        public static string FormatTime(float hours)
        {
            int h = Mathf.FloorToInt(hours);
            int m = Mathf.FloorToInt((hours - h) * 60);
            return $"{h:D2}:{m:D2}";
        }

        public override void Execute()
        {
            SetTime(TargetTime);
        }
    }
}
