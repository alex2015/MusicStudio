using System;
using System.ComponentModel;

namespace Tools
{
    public static class EventsHelper
    {
        private static void UnsafeRaise(Delegate del, params object[] args)
        {
            if ((object)del == null)
                return;
            foreach (Delegate invocation in del.GetInvocationList())
            {
                try
                {
                    invocation.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public static void Raise(GenericEventHandler del)
        {
            UnsafeRaise(del);
        }

        public static void Raise<T>(GenericEventHandler<T> del, T t)
        {
            UnsafeRaise(del, t);
        }

        public static void Raise<T, U>(GenericEventHandler<T, U> del, T t, U u)
        {
            UnsafeRaise(del, t, u);
        }

        public static void Raise<T, U, V>(GenericEventHandler<T, U, V> del, T t, U u, V v)
        {
            UnsafeRaise(del, t, u, v);
        }

        public static void Raise(EventHandler del, object sender, EventArgs args)
        {
            UnsafeRaise(del, sender, args);
        }

        public static void Raise<T>(EventHandler<T> del, object sender, T args) where T : EventArgs
        {
            UnsafeRaise(del, sender, args);
        }

        public static void Raise(PropertyChangedEventHandler del, object sender, PropertyChangedEventArgs args)
        {
            UnsafeRaise(del, sender, args);
        }
    }
}
