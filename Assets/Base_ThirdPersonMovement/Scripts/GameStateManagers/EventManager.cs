using System;
using System.Collections.Generic;

    /// <summary>
    /// A global, thread?safe event broadcaster using string tokens.
    /// </summary>
    public static class EventManager
    {
        // backing store for event handlers
        private static readonly Dictionary<string, Action> _events = new Dictionary<string, Action>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Removes all registered listeners.
        /// </summary>
        public static void ClearAllListeners()
        {
            lock (_lock)
            {
                _events.Clear();
            }
        }

        /// <summary>
        /// Subscribes <paramref name="listener"/> to the event keyed by <paramref name="eventName"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="eventName"/> or <paramref name="listener"/> is null or empty.
        /// </exception>
        public static void StartListening(string eventName, Action listener)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName), "Event name cannot be null or empty.");
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_events.TryGetValue(eventName, out var existing))
                {
                    existing += listener;
                    _events[eventName] = existing;
                }
                else
                {
                    _events.Add(eventName, listener);
                }
            }
        }

        /// <summary>
        /// Unsubscribes <paramref name="listener"/> from the event keyed by <paramref name="eventName"/>.
        /// If no listeners remain, the event key is removed.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="eventName"/> or <paramref name="listener"/> is null or empty.
        /// </exception>
        public static void StopListening(string eventName, Action listener)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName), "Event name cannot be null or empty.");
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (!_events.TryGetValue(eventName, out var existing))
                    return;

                existing -= listener;
                if (existing == null)
                {
                    _events.Remove(eventName);
                }
                else
                {
                    _events[eventName] = existing;
                }
            }
        }

        /// <summary>
        /// Fires the event keyed by <paramref name="eventName"/>, invoking all subscribers.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="eventName"/> is null or empty.
        /// </exception>
        public static void TriggerEvent(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName), "Event name cannot be null or empty.");

            Action handler;
            lock (_lock)
            {
                _events.TryGetValue(eventName, out handler);
            }

            // invoke outside lock to avoid deadlocks or handler-induced delays
            handler?.Invoke();
        }
    }

