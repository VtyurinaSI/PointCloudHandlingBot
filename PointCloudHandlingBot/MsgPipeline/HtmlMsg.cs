using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PointCloudHandlingBot.MsgPipeline
{
    internal class HtmlMsg : IMsgPipelineSteps
    {
        public async Task Send(ITelegramBotClient bot, UserData user)
        {
            var html = GenerateHtml(user.CurrentPcl.PointCloud, user.CurrentPcl.Colors);

            await using var ms = new MemoryStream(Encoding.UTF8.GetBytes(html));

            await bot.SendDocument(
                user.ChatId,
                InputFile.FromStream(ms, fileName: "cloud.html"),
                caption: "Открой этот файл, тут можно посмотреть облако точек в 3D");

        }
        string HtmlTemplate = @"
<!DOCTYPE html>
<html lang=""ru"">
<head>
<meta charset=""UTF-8"">
<title>Cloud</title>
<script src=""https://cdn.plot.ly/plotly-2.32.0.min.js""></script>
<style>
  html,body{{margin:0;padding:0;width:100%;height:100%}}
  #plot{{width:100%;height:100%}}
</style>
</head>
<body>
<div id=""plot""></div>
<script>
  const xs = [{0}];
  const ys = [{1}];
  const zs = [{2}];
  const cs = [{3}];
 const camera = {4};
  Plotly.newPlot(
    'plot',
    [{{                      
      x: xs,
      y: zs,
      z: ys,
      mode: 'markers',
      type: 'scatter3d',
      marker: {{ size: 3, color: cs, opacity: 1 }}
    }}],
    {{                      
      scene: {{
        camera: camera, 
        xaxis: {{ title: 'X' }},
        yaxis: {{ title: 'Z' }},
        zaxis: {{ title: 'Y' }},
        aspectmode: 'data'
      }},
      margin: {{ l: 0, r: 0, b: 0, t: 0 }}
    }},
    {{ responsive: true }}
  );
</script>
</body>
</html>";


        string GenerateHtml(
                IReadOnlyList<Vector3> pts,
                IReadOnlyList<OxyColor> cols)
        {
            if (pts.Count != cols.Count)
                throw new ArgumentException("Points and colors length mismatch");

            var ci = CultureInfo.InvariantCulture;

            string toCsv<T>(IEnumerable<T> source) => string.Join(",", source);

            var xs = toCsv(pts.Select(p => p.X.ToString("G", ci)));
            var ys = toCsv(pts.Select(p => p.Y.ToString("G", ci)));
            var zs = toCsv(pts.Select(p => p.Z.ToString("G", ci)));

            var cs = toCsv(cols.Select(c => $"\"#{c.R:X2}{c.G:X2}{c.B:X2}\""));
            Vector3 cameraEye = new(0f, -1.5f, 0f);
            string cameraJson =
       $"{{ eye: {{ x:{cameraEye.X.ToString("G", ci)}, y:{cameraEye.Y.ToString("G", ci)}, z:{cameraEye.Z.ToString("G", ci)} }} }}";

            return string.Format(HtmlTemplate, xs, ys, zs, cs, cameraJson);
        }

    }
}
