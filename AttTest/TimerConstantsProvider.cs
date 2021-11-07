using System;
using System.IO;
using System.Text.Json;

namespace AttTest
{
    public class TimerConstantsProvider
    {
        public static TimerConstants? GetTimerConstants(string path)
        {
            var conf = File.ReadAllText(path);
            try
            {
                return JsonSerializer.Deserialize<TimerConstants>(conf);
            }
            catch (JsonException e)
            {
                return null;
            }
        }
    }
}