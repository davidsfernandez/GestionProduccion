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

            // Auto-remove after 5 seconds
            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (s, e) => {
                Toasts.Remove(toast);
                OnShow?.Invoke();
                timer.Dispose();
            };
            timer.Start();
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
