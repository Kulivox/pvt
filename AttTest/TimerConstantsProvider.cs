using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
                var result = JsonSerializer.Deserialize<TimerConstants>(conf);

                if (result is null)
                {
                    return result;
                }
                
                var validationCtx = new ValidationContext(result);

                var valResults = new List<ValidationResult>();

                if (Validator.TryValidateObject(result, validationCtx, valResults))
                {
                    return result;
                }

                return null;
            }
            catch (JsonException e)
            {
                return null;
            }
        }
    }
}