using System;
using System.Collections.Generic;

namespace MistInteractive.ThirdPerson.Utils
{
    /// <summary>
    /// A global, thread-safe event broadcaster using string tokens,
    /// supporting both parameterless and parameterized events.
    /// </summary>
    public static class EventManager
    {
        // Store delegates (Action and Action<T>) keyed by event name
        private static readonly Dictionary<string, Delegate> _events = new Dictionary<string, Delegate>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Removes all registered listeners for all events.
        /// </summary>
        public static void ClearAllListeners()
        {
            lock (_lock)
            {
                _events.Clear();
            }
        }

        // -------- Parameterless events --------

        public static void StartListening(string eventName, Action listener)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_events.TryGetValue(eventName, out var existing))
                {
                    existing = Delegate.Combine(existing, listener);
                    _events[eventName] = existing;
                }
                else
                {
                    _events.Add(eventName, listener);
                }
            }
        }

        public static void StopListening(string eventName, Action listener)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (!_events.TryGetValue(eventName, out var existing))
                    return;

                var updated = Delegate.Remove(existing, listener);
                if (updated == null)
                    _events.Remove(eventName);
                else
                    _events[eventName] = updated;
            }
        }

        public static void TriggerEvent(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));

            Delegate del;
            lock (_lock)
            {
                _events.TryGetValue(eventName, out del);
            }

            (del as Action)?.Invoke();
        }

        // -------- Parameterized events --------

        public static void StartListening<T>(string eventName, Action<T> listener)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_events.TryGetValue(eventName, out var existing))
                {
                    existing = Delegate.Combine(existing, listener);
                    _events[eventName] = existing;
                }
                else
                {
                    _events.Add(eventName, listener);
                }
            }
        }

        public static void StopListening<T>(string eventName, Action<T> listener)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (!_events.TryGetValue(eventName, out var existing))
                    return;

                var updated = Delegate.Remove(existing, listener);
                if (updated == null)
                    _events.Remove(eventName);
                else
                    _events[eventName] = updated;
            }
        }

        public static void TriggerEvent<T>(string eventName, T param)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));

            Delegate del;
            lock (_lock)
            {
                _events.TryGetValue(eventName, out del);
            }

            (del as Action<T>)?.Invoke(param);
        }
    }
}
