using System.Collections.Generic;
using UnityEngine;

namespace SewerMenu.UI
{
    public static class ToastManager
    {
        private class Toast
        {
            public string Message;
            public SewerSkin.StatusType Type;
            public float CreatedAt;
            public float Duration;
        }

        private static readonly List<Toast> _toasts = new List<Toast>();

        public static void Show(string message, SewerSkin.StatusType type = SewerSkin.StatusType.Normal, float duration = 2.6f)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            if (_toasts.Count >= 4)
            {
                _toasts.RemoveAt(0);
            }

            _toasts.Add(new Toast
            {
                Message = message,
                Type = type,
                CreatedAt = Time.unscaledTime,
                Duration = Mathf.Clamp(duration, 1.2f, 5f)
            });
        }

        public static void Draw()
        {
            if (_toasts.Count == 0) return;

            float width = 330f;
            float height = 42f;
            float x = Screen.width - width - 24f;
            float y = 72f;

            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                var toast = _toasts[i];
                float age = Time.unscaledTime - toast.CreatedAt;
                if (age >= toast.Duration)
                {
                    _toasts.RemoveAt(i);
                    continue;
                }

                float inAlpha = Mathf.Clamp01(age * 8f);
                float outAlpha = Mathf.Clamp01((toast.Duration - age) * 4f);
                float alpha = Mathf.Min(inAlpha, outAlpha);
                float slide = (1f - alpha) * 18f;

                Rect rect = new Rect(x + slide, y, width, height);
                DrawToast(rect, toast, alpha);
                y += height + 8f;
            }
        }

        private static void DrawToast(Rect rect, Toast toast, float alpha)
        {
            Color accent = GetAccent(toast.Type);
            SewerSkin.DrawRoundedRect(rect, new Color(0.035f, 0.046f, 0.043f, 0.96f * alpha), new Color(accent.r, accent.g, accent.b, 0.52f * alpha), 7, 1);
            SewerSkin.DrawRoundedRect(new Rect(rect.x + 7f, rect.y + 8f, 3f, rect.height - 16f), new Color(accent.r, accent.g, accent.b, 0.85f * alpha), Color.clear, 2, 0);

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = new Color(SewerSkin.TextColor.r, SewerSkin.TextColor.g, SewerSkin.TextColor.b, alpha);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 5f, rect.width - 28f, rect.height - 10f), toast.Message, SewerSkin.GetLabelStyle(11, FontStyle.Bold, TextAnchor.MiddleLeft));
            GUI.contentColor = oldContentColor;
        }

        private static Color GetAccent(SewerSkin.StatusType type)
        {
            switch (type)
            {
                case SewerSkin.StatusType.Success:
                    return SewerSkin.SuccessColor;
                case SewerSkin.StatusType.Warning:
                    return SewerSkin.WarningColor;
                case SewerSkin.StatusType.Error:
                    return SewerSkin.ErrorColor;
                default:
                    return SewerSkin.AccentColor;
            }
        }
    }
}
