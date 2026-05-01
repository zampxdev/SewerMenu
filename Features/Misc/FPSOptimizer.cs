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

        private bool _useFrameCap = true;
        private float _targetFrameRate = 120f;
        private bool _optimizeShadows = true;
        private float _maxShadowDistance = 140f;
        private bool _disableRealtimeReflections = true;
        private bool _reduceParticleRaycasts = true;
        private bool _useDistanceCulling = false;
        private float _maxViewDistance = 900f;
        private bool _useOcclusionCulling = true;
        private bool _optimizeLod = false;
        private float _lodBias = 1f;
        private bool _reduceExpensiveQuality = true;

        public bool UseFrameCap { get => _useFrameCap; set => SetOption(ref _useFrameCap, value); }
        public float TargetFrameRate { get => _targetFrameRate; set => SetOption(ref _targetFrameRate, value); }
        public bool OptimizeShadows { get => _optimizeShadows; set => SetOption(ref _optimizeShadows, value); }
        public float MaxShadowDistance { get => _maxShadowDistance; set => SetOption(ref _maxShadowDistance, value); }
        public bool DisableRealtimeReflections { get => _disableRealtimeReflections; set => SetOption(ref _disableRealtimeReflections, value); }
        public bool ReduceParticleRaycasts { get => _reduceParticleRaycasts; set => SetOption(ref _reduceParticleRaycasts, value); }
        public bool UseDistanceCulling { get => _useDistanceCulling; set => SetOption(ref _useDistanceCulling, value); }
        public float MaxViewDistance { get => _maxViewDistance; set => SetOption(ref _maxViewDistance, value); }
        public bool UseOcclusionCulling { get => _useOcclusionCulling; set => SetOption(ref _useOcclusionCulling, value); }
        public bool OptimizeLod { get => _optimizeLod; set => SetOption(ref _optimizeLod, value); }
        public float LodBias { get => _lodBias; set => SetOption(ref _lodBias, value); }
        public bool ReduceExpensiveQuality { get => _reduceExpensiveQuality; set => SetOption(ref _reduceExpensiveQuality, value); }

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
            _cameraSettings.Clear();
            _hasOriginalSettings = false;
            _nextApplyTime = 0f;
            CaptureOriginalSettings();
            ApplySettings();
        }

        public override void OnDisable()
        {
            RestoreSettings();
            _cameraSettings.Clear();
            _hasOriginalSettings = false;
            _nextApplyTime = 0f;
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
                if (!_hasOriginalSettings)
                {
                    CaptureOriginalSettings();
                }

                if (!_hasOriginalSettings)
                {
                    return;
                }

                if (UseFrameCap)
                {
                    if (QualitySettings.vSyncCount == 0)
                    {
                        Application.targetFrameRate = Mathf.RoundToInt(Mathf.Clamp(TargetFrameRate, 30f, 240f));
                    }
                }
                else
                {
                    RestoreFrameSettings();
                }

                if (OptimizeShadows)
                {
                    QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, MaxShadowDistance);
                    if (QualitySettings.shadowDistance > 120f)
                    {
                        QualitySettings.shadowCascades = Mathf.Min(QualitySettings.shadowCascades, 2);
                    }
                }
                else
                {
                    RestoreShadowSettings();
                }

                if (DisableRealtimeReflections)
                {
                    QualitySettings.realtimeReflectionProbes = false;
                }
                else
                {
                    QualitySettings.realtimeReflectionProbes = _originalRealtimeReflectionProbes;
                }

                if (ReduceParticleRaycasts)
                {
                    QualitySettings.particleRaycastBudget = Mathf.Min(QualitySettings.particleRaycastBudget, 512);
                }
                else
                {
                    QualitySettings.particleRaycastBudget = _originalParticleRaycastBudget;
                }

                if (OptimizeLod)
                {
                    QualitySettings.lodBias = Mathf.Min(QualitySettings.lodBias, Mathf.Clamp(LodBias, 0.75f, 1.15f));
                }
                else
                {
                    QualitySettings.lodBias = _originalLodBias;
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
                else
                {
                    RestoreExpensiveQualitySettings();
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
                    else
                    {
                        camera.useOcclusionCulling = _cameraSettings[id].UseOcclusionCulling;
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
                    else
                    {
                        var settings = _cameraSettings[id];
                        camera.farClipPlane = settings.FarClipPlane;
                        camera.layerCullSpherical = settings.LayerCullSpherical;
                        camera.layerCullDistances = CopyLayerCullDistances(settings.LayerCullDistances);
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

        private void RestoreFrameSettings()
        {
            if (!_hasOriginalSettings) return;

            QualitySettings.vSyncCount = _originalVSyncCount;
            Application.targetFrameRate = _originalTargetFrameRate;
        }

        private void RestoreShadowSettings()
        {
            if (!_hasOriginalSettings) return;

            QualitySettings.shadowDistance = _originalShadowDistance;
            QualitySettings.shadowCascades = _originalShadowCascades;
        }

        private void RestoreExpensiveQualitySettings()
        {
            if (!_hasOriginalSettings) return;

            QualitySettings.pixelLightCount = _originalPixelLightCount;
            QualitySettings.antiAliasing = _originalAntiAliasing;
            QualitySettings.resolutionScalingFixedDPIFactor = _originalResolutionScalingFixedDPIFactor;
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

        private void RequestApply()
        {
            if (IsEnabled)
            {
                _nextApplyTime = 0f;
            }
        }

        private void SetOption(ref bool field, bool value)
        {
            if (field == value) return;
            field = value;
            RequestApply();
        }

        private void SetOption(ref float field, float value)
        {
            if (Mathf.Approximately(field, value)) return;
            field = value;
            RequestApply();
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
