using System;
using System.Reflection;

namespace HideAndSkull.Lobby.Singleton
{
    /// <summary>
    /// Singleton base
    /// </summary>
    /// <typeparam name="T">싱글톤으로 사용하려는 타입(상속클래스)</typeparam>
    public abstract class Singleton<T>
        where T : Singleton<T>
    {
        public static T instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = (T)Activator.CreateInstance(typeof(T));
                }

                return s_instance;
            }
        }

        private static T s_instance;
    }
}