using System;
using System.Collections.Generic;

namespace MistInteractive.ThirdPerson
{
    public static class EventManager
    {
        private static readonly Dictionary<string, Delegate> events = new Dictionary<string, Delegate>();
        private static readonly object lockObj = new object();

        public static void StartListening(string eventName, Action listener)
        {
            lock (lockObj)
            {
                if (events.TryGetValue(eventName, out var existing))
                {
                    events[eventName] = Delegate.Combine(existing, listener);
                }
                else
                {
                    events.Add(eventName, listener);
                }
            }
        }

        public static void StopListening(string eventName, Action listener)
        {
            lock (lockObj)
            {
                if (events.TryGetValue(eventName, out var existing))
                {
                    var updated = Delegate.Remove(existing, listener);
                    if (updated == null)
                        events.Remove(eventName);
                    else
                        events[eventName] = updated;
                }
            }
        }

        public static void TriggerEvent(string eventName)
        {
            Delegate del;
            lock (lockObj)
            {
                events.TryGetValue(eventName, out del);
            }
            (del as Action)?.Invoke();
        }

        public static void StartListening<T>(string eventName, Action<T> listener)
        {
            lock (lockObj)
            {
                if (events.TryGetValue(eventName, out var existing))
                {
                    events[eventName] = Delegate.Combine(existing, listener);
                }
                else
                {
                    events.Add(eventName, listener);
                }
            }
        }

        public static void StopListening<T>(string eventName, Action<T> listener)
        {
            lock (lockObj)
            {
                if (events.TryGetValue(eventName, out var existing))
                {
                    var updated = Delegate.Remove(existing, listener);
                    if (updated == null)
                        events.Remove(eventName);
                    else
                        events[eventName] = updated;
                }
            }
        }

        public static void TriggerEvent<T>(string eventName, T param)
        {
            Delegate del;
            lock (lockObj)
            {
                events.TryGetValue(eventName, out del);
            }
            (del as Action<T>)?.Invoke(param);
        }

        public static void ClearAllListeners()
        {
            lock (lockObj)
            {
                events.Clear();
            }
        }
    }
}