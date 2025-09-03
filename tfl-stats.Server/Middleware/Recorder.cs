using System.Collections.Concurrent;
using System.Text.Json;

namespace tfl_stats.Server.Middleware
{
    public class Recorder
    {
        private readonly RequestDelegate _next;

        ConcurrentDictionary<string, string> _records = new();

        private bool _record = false;
        private bool _playback = false;
        public Recorder(RequestDelegate next) // delegate injected at startup
        {
            _next = next;
        }

        // called every request
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.ToString() == "/api/recorder") // url should look like /api/recorder?mode=record_on
            {
                await ChangeSettings(context);
                return;
            }
            // the key is the url, e.g. /lineDiagram?line=xyz
            string key = context.Request.Path.ToString() + context.Request.QueryString.ToString();

            // Here we can see if the request is already saved and replay that instead of
            // calling _next()
            if (_playback)
            {
                if (_records.TryGetValue(key, out var recordedResponse)) {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(recordedResponse);
                    return;
                }
            }

            using MemoryStream buffer = new MemoryStream();
            var originalBody = context.Response.Body;
            context.Response.Body = buffer; // give it an empty buffer

            await _next(context); // in here, the response data will fill the buffer
            // Here, after we get the response, we can record the request/response key/values
            if (_record && context.Response.StatusCode == 200) // only record 200=success results maybe that's enough.
            {
                buffer.Position = 0;
                using var reader = new StreamReader(buffer, leaveOpen: true);
                string response = await reader.ReadToEndAsync();
                _records[key] = response;
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }

        private async Task ChangeSettings(HttpContext context)
        {
            var mode = context.Request.Query["mode"]; // mode will be one of "record_on, record_off, etc"

            switch (mode)
            {
                case "record_on":
                    {
                        _record = true;
                        break;
                    }
                case "record_off":
                    {
                        _record = false;
                        break;
                    }
                case "playback_on":
                    {
                        _playback = true;
                        break;
                    }
                case "playback_off":
                    {
                        _playback = false;
                        break;
                    }
                case "save":
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(_records, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        await File.WriteAllTextAsync("recorder.json", json);
                        break;
                    }
                case "load":
                    {
                        var json = await File.ReadAllTextAsync("recorder.json");
                        var temp = System.Text.Json.JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(json);
                        _records = temp!;
                        break;
                    }

            }
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync($"<h2>Mode = {mode}</h2>");
            return;
        }
    }
}

