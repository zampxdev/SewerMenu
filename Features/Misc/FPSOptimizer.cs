using System;
using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Core.Logging;
using SewerMenu.Features.Base;

namespace SewerMenu.Features.Misc
{
    public class FPSOptimizer : FeatureBase
    {
        public override string Id => "fpsoptimizer";
        public override string Name => "FPS Optimizer";
        public override string Description => "Improves performance without heavy visual downgrades";
        public override FeatureCategory Category => FeatureCategory.Misc;

        public bool UseFrameCap { get; set; } = false;
        public float TargetFrameRate { get; set; } = 120f;
        public bool OptimizeShadows { get; set; } = true;
        public float MaxShadowDistance { get; set; } = 140f;
        public bool DisableRealtimeReflections { get; set; } = true;
        public bool ReduceParticleRaycasts { get; set; } = true;
        public bool UseDistanceCulling { get; set; } = false;
        public float MaxViewDistance { get; set; } = 900f;
        public bool UseOcclusionCulling { get; set; } = true;
        public bool OptimizeLod { get; set; } = false;
        public float LodBias { get; set; } = 1f;
        public bool ReduceExpensiveQuality { get; set; } = true;

        private bool _hasOriginalSettings;
        private int _originalVSyncCount;
        private int _originalTargetFrameRate;
        private float _originalShadowDistance;
        private int _originalShadowCascades;
        private bool _originalRealtimeReflectionProbes;
        private int _originalParticleRaycastBudget;
        private float _originalLodBias;
        private int _originalPixelLightCount;
        private int _originalAntiAliasing;
        private float _originalResolutionScalingFixedDPIFactor;
        private readonly Dictionary<int, CameraSettings> _cameraSettings = new Dictionary<int, CameraSettings>();
        private float _nextApplyTime;

        public override void OnEnable()
        {
            CaptureOriginalSettings();
            ApplySettings();
        }

        public override void OnDisable()
        {
            RestoreSettings();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            if (Time.unscaledTime >= _nextApplyTime)
            {
                SafeExecute(ApplySettings, "applying FPS optimizer settings");
                _nextApplyTime = Time.unscaledTime + 3f;
            }
        }

        private void CaptureOriginalSettings()
        {
            if (_hasOriginalSettings) return;

            try
            {
                _originalVSyncCount = QualitySettings.vSyncCount;
                _originalTargetFrameRate = Application.targetFrameRate;
                _originalShadowDistance = QualitySettings.shadowDistance;
                _originalShadowCascades = QualitySettings.shadowCascades;
                _originalRealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
                _originalParticleRaycastBudget = QualitySettings.particleRaycastBudget;
                _originalLodBias = QualitySettings.lodBias;
                _originalPixelLightCount = QualitySettings.pixelLightCount;
                _originalAntiAliasing = QualitySettings.antiAliasing;
                _originalResolutionScalingFixedDPIFactor = QualitySettings.resolutionScalingFixedDPIFactor;
                _hasOriginalSettings = true;
            }
            catch (Exception ex)
            {
                SewerLogger.Warning($"FPS Optimizer could not capture original settings: {ex.Message}");
            }
        }

        private void ApplySettings()
        {
            try
            {
                if (UseFrameCap)
                {
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = Mathf.RoundToInt(Mathf.Clamp(TargetFrameRate, 30f, 240f));
                }

                if (OptimizeShadows)
                {
                    QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, MaxShadowDistance);
                    if (QualitySettings.shadowDistance > 120f)
                    {
                        QualitySettings.shadowCascades = Mathf.Min(QualitySettings.shadowCascades, 2);
                    }
                }

                if (DisableRealtimeReflections)
                {
                    QualitySettings.realtimeReflectionProbes = false;
                }

                if (ReduceParticleRaycasts)
                {
                    QualitySettings.particleRaycastBudget = Mathf.Min(QualitySettings.particleRaycastBudget, 512);
                }

                if (OptimizeLod)
                {
                    QualitySettings.lodBias = Mathf.Min(QualitySettings.lodBias, Mathf.Clamp(LodBias, 0.75f, 1.15f));
                }

                if (ReduceExpensiveQuality)
                {
                    QualitySettings.pixelLightCount = Mathf.Min(QualitySettings.pixelLightCount, 2);

                    if (QualitySettings.antiAliasing > 4)
                    {
                        QualitySettings.antiAliasing = 4;
                    }

                    if (QualitySettings.resolutionScalingFixedDPIFactor > 1f)
                    {
                        QualitySettings.resolutionScalingFixedDPIFactor = 1f;
                    }
                }

                ApplyCameraCulling();
            }
            catch (Exception ex)
            {
                SewerLogger.Warning($"FPS Optimizer apply failed: {ex.Message}");
            }
        }

        private void ApplyCameraCulling()
        {
            try
            {
                var cameras = Camera.allCameras;
                if (cameras == null) return;

                foreach (var camera in cameras)
                {
                    if (camera == null) continue;

                    var id = camera.GetInstanceID();
                    if (!_cameraSettings.ContainsKey(id))
                    {
                        _cameraSettings[id] = new CameraSettings
                        {
                            FarClipPlane = camera.farClipPlane,
                            UseOcclusionCulling = camera.useOcclusionCulling,
                            LayerCullSpherical = camera.layerCullSpherical,
                            LayerCullDistances = CopyLayerCullDistances(camera.layerCullDistances)
                        };
                    }

                    if (UseOcclusionCulling)
                    {
                        camera.useOcclusionCulling = true;
                    }

                    if (UseDistanceCulling)
                    {
                        var maxDistance = Mathf.Clamp(MaxViewDistance, 150f, 1500f);
                        camera.farClipPlane = Mathf.Min(camera.farClipPlane, maxDistance);
                        camera.layerCullSpherical = true;

                        var distances = camera.layerCullDistances;
                        if (distances == null || distances.Length != 32)
                        {
                            distances = new float[32];
                        }

                        for (var i = 0; i < distances.Length; i++)
                        {
                            distances[i] = 0f;
                        }

                        distances[0] = maxDistance;
                        distances[1] = maxDistance;
                        distances[2] = maxDistance;
                        distances[4] = maxDistance;
                        distances[5] = maxDistance;
                        distances[8] = maxDistance;
                        distances[9] = maxDistance;

                        camera.layerCullDistances = distances;
                    }
                }
            }
            catch (Exception ex)
            {
                SewerLogger.Warning($"FPS Optimizer camera culling failed: {ex.Message}");
            }
        }

        private void RestoreSettings()
        {
            if (!_hasOriginalSettings) return;

            try
            {
                QualitySettings.vSyncCount = _originalVSyncCount;
                Application.targetFrameRate = _originalTargetFrameRate;
                QualitySettings.shadowDistance = _originalShadowDistance;
                QualitySettings.shadowCascades = _originalShadowCascades;
                QualitySettings.realtimeReflectionProbes = _originalRealtimeReflectionProbes;
                QualitySettings.particleRaycastBudget = _originalParticleRaycastBudget;
                QualitySettings.lodBias = _originalLodBias;
                QualitySettings.pixelLightCount = _originalPixelLightCount;
                QualitySettings.antiAliasing = _originalAntiAliasing;
                QualitySettings.resolutionScalingFixedDPIFactor = _originalResolutionScalingFixedDPIFactor;
                RestoreCameraSettings();
            }
            catch (Exception ex)
            {
                SewerLogger.Warning($"FPS Optimizer restore failed: {ex.Message}");
            }
        }

        private void RestoreCameraSettings()
        {
            try
            {
                var cameras = Camera.allCameras;
                if (cameras == null) return;

                foreach (var camera in cameras)
                {
                    if (camera == null) continue;
                    var id = camera.GetInstanceID();
                    if (!_cameraSettings.TryGetValue(id, out var settings)) continue;

                    camera.farClipPlane = settings.FarClipPlane;
                    camera.useOcclusionCulling = settings.UseOcclusionCulling;
                    camera.layerCullSpherical = settings.LayerCullSpherical;
                    camera.layerCullDistances = CopyLayerCullDistances(settings.LayerCullDistances);
                }
            }
            catch { }
        }

        private static float[] CopyLayerCullDistances(float[] source)
        {
            if (source == null) return null;

            var copy = new float[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        private struct CameraSettings
        {
            public float FarClipPlane;
            public bool UseOcclusionCulling;
            public bool LayerCullSpherical;
            public float[] LayerCullDistances;
        }
    }
}
