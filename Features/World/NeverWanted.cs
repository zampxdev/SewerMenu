using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.World
{
    /// <summary>
    /// Prevents wanted level from ever increasing.
    /// Continuously resets police suspicion to 0.
    /// </summary>
    public class NeverWanted : FeatureBase
    {
        public override string Id => "neverwanted";
        public override string Name => "Never Wanted";
        public override string Description => "Prevents wanted level from increasing";
        public override FeatureCategory Category => FeatureCategory.World;

        // Update more frequently than PoliceDisable since we want to catch suspicion before it builds
        private float _lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.25f; // 4 times per second

        public override void OnEnable()
        {
            ClearAllSuspicion();
            _lastUpdateTime = Time.time;
            SewerLogger.Debug("NeverWanted enabled");
        }

        public override void OnDisable()
        {
            SewerLogger.Debug("NeverWanted disabled");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            // Run frequently to catch suspicion before it builds
            if (Time.time - _lastUpdateTime < UPDATE_INTERVAL) return;
            _lastUpdateTime = Time.time;

            SafeExecute(() =>
            {
                ClearAllSuspicion();
            }, "maintaining never wanted");
        }

        /// <summary>
        /// Clears all police suspicion.
        /// </summary>
        private void ClearAllSuspicion()
        {
            var police = GameTypes.GetAllPolice();
            if (police == null || police.Length == 0) return;

            foreach (var officer in police)
            {
                if (officer == null) continue;

                try
                {
                    // Reset suspicion to 0
                    if (officer.Suspicion > 0f)
                    {
                        officer.Suspicion = 0f;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Gets the current maximum suspicion level across all officers.
        /// </summary>
        public float GetMaxSuspicion()
        {
            float maxSuspicion = 0f;
            
            try
            {
                var police = GameTypes.GetAllPolice();
                if (police == null || police.Length == 0) return 0f;

                foreach (var officer in police)
                {
                    if (officer == null) continue;

                    try
                    {
                        if (officer.Suspicion > maxSuspicion)
                        {
                            maxSuspicion = officer.Suspicion;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return maxSuspicion;
        }
    }
}
