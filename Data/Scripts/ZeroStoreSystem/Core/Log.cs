using VRage.Utils;

namespace ZeroStoreSystem.Core
{
    public static class Log
    {
        private const string Prefix = "[ZERO Store System] ";

        public static void Info(string text)
        {
            MyLog.Default.WriteLine(Prefix + text);
        }

        public static void Error(string text)
        {
            MyLog.Default.WriteLine(Prefix + "ERROR: " + text);
        }
    }
}
