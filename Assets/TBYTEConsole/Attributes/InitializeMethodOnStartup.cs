using System;

namespace TBYTEConsole
{
    // HACK: The things I do to maintain compatiblility with the editor...

#if UNITY_EDITOR
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class InitializeMethodOnStartup : UnityEditor.InitializeOnLoadMethodAttribute
    {
        public InitializeMethodOnStartup() : base()
        {

        }
    }
#else
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class InitializeMethodOnStartup : UnityEngine.RuntimeInitializeOnLoadMethodAttribute
    {
        public InitializeMethodOnStartup() : base()
        {

        }
    }
#endif
}
