using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.World
{
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

        public float GetCurrentTime()
        {
            var time = GameTypes.Time;
            if (time == null) return 12f;

            try
            {
                return time.CurrentTime / 60f; // CurrentTime is in minutes (0-1440)
            }
            catch
            {
                return 12f;
            }
        }

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

                hours = Mathf.Clamp(hours, 0f, 24f);
                int minutes = Mathf.RoundToInt(hours * 60f);
                time.SetTimeAndSync(minutes);
                SewerLogger.Success($"Set time to {FormatTime(hours)}");
            }, "setting time");
        }

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
                time.SetTimeAndSync(minutes);
                SewerLogger.Success($"Set time to {FormatTime(minutes / 60f)}");
            }, "setting time");
        }

        public void AdvanceTime(float hours)
        {
            var currentTime = GetCurrentTime();
            var newTime = (currentTime + hours) % 24f;
            SetTime(newTime);
        }

        public void SetMorning() => SetTime(6f);
        public void SetNoon() => SetTime(12f);
        public void SetEvening() => SetTime(18f);
        public void SetMidnight() => SetTime(0f);

        public void SkipDay()
        {
            SafeExecute(() =>
            {
                var time = GameTypes.Time;
                if (time == null) return;

                try
                {
                    int newDays = time.ElapsedDays + 1;
                    time.ElapsedDays = newDays;
                    SewerLogger.Success($"Skipped to day {newDays}");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to skip day", ex);
                }
            }, "skipping day");
        }

        public float GetTimeMultiplier()
        {
            var time = GameTypes.Time;
            if (time == null) return 1f;

            try
            {
                return time.TimeSpeedMultiplier;
            }
            catch
            {
                return 1f;
            }
        }

        public void SetTimeMultiplier(float multiplier)
        {
            SafeExecute(() =>
            {
                var time = GameTypes.Time;
                if (time == null) return;

                try
                {
                    time.SetTimeSpeedMultiplier(multiplier);
                    SewerLogger.Success($"Set time multiplier to {multiplier:F1}x");
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("Failed to set time multiplier", ex);
                }
            }, "setting time multiplier");
        }

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
                        _originalTimeMultiplier = time.TimeSpeedMultiplier;
                        time.SetTimeSpeedMultiplier(0f);
                        _wasTimeFrozen = true;
                        SewerLogger.Success("Time frozen");
                    }
                    else if (!frozen && _wasTimeFrozen)
                    {
                        time.SetTimeSpeedMultiplier(_originalTimeMultiplier);
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
