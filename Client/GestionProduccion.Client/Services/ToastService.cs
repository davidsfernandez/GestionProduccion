using System;
using System.Collections.Generic;
using System.Timers;

namespace GestionProduccion.Client.Services
{
    public class ToastService
    {
        public event Action? OnShow;
        public List<ToastMessage> Toasts { get; } = new();

        public void ShowToast(string message, ToastLevel level)
        {
            var toast = new ToastMessage
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.Now
            };

            Toasts.Add(toast);
            OnShow?.Invoke();

            // Auto-remove Success/Info after 5 seconds. Errors require manual close.
            if (level != ToastLevel.Error)
            {
                var timer = new System.Timers.Timer(5000);
                timer.Elapsed += (s, e) => {
                    RemoveToast(toast);
                    timer.Dispose();
                };
                timer.AutoReset = false;
                timer.Start();
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
