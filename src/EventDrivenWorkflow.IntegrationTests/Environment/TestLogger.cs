using System.Diagnostics;

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    public static class TestLogger
    {
        public static void Log(string component, string action, string text)
        {
            Trace.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")} [{component}] {action} {text}");
        }
    }
}
