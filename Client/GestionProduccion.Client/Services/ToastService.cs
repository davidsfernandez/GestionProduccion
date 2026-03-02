using System;
using System.Collections.Generic;
using System.Timers;

namespace GestionProduccion.Client.Services
{
    public class ToastService
    {
        private readonly AudioService _audio;
        public event Action? OnShow;
        public List<ToastMessage> Toasts { get; } = new();

        public ToastService(AudioService audio)
        {
            _audio = audio;
        }

        public void ShowToast(string message, ToastLevel level, string? title = null)
        {
            var toast = new ToastMessage
            {
                Message = message,
                Level = level,
                Title = title,
                Timestamp = DateTime.Now
            };

            Toasts.Add(toast);
            OnShow?.Invoke();

            // Play Sound Feedback
            _ = PlayToastSound(level);

            // Auto-remove Success/Info after 5 seconds. Errors require manual close.
            if (level != ToastLevel.Error)
            {
                var timer = new System.Timers.Timer(5000);
                timer.Elapsed += (s, e) =>
                {
                    RemoveToast(toast);
                    timer.Dispose();
                };
                timer.AutoReset = false;
                timer.Start();
            }
        }

        private async Task PlayToastSound(ToastLevel level)
        {
            switch (level)
            {
                case ToastLevel.Success: await _audio.PlaySuccess(); break;
                case ToastLevel.Error: await _audio.PlayError(); break;
                default: await _audio.PlayNotify(); break;
            }
        }

        public void RemoveToast(ToastMessage toast)
        {
            if (Toasts.Contains(toast))
            {
                Toasts.Remove(toast);
                OnShow?.Invoke();
            }
        }
    }

    public class ToastMessage
    {
        public string Message { get; set; } = string.Empty;
        public string? Title { get; set; }
        public ToastLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }
}
