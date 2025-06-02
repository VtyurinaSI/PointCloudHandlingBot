using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace PointCloudHandlingBot.Commands
{
    internal class CutCmd : CommandBase
    {
        public CutCmd(Logger logger)
            : base("/cut", logger, 1,
                  ["Введи неравнствами интересующую тебя область через ';'"])
        { }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Обрезка облака точек. Условия: {string.Join(';', conds)}",
                LogLevel.Information, user, "Попробую");
            ROIind = [.. Enumerable.Range(0, user.CurrentPcl.PointCloud.Count)];
            foreach (var cnd in conds)
                Evaluate(cnd, user.CurrentPcl.PointCloud);
            List<Vector3> newPcl = [];
            foreach (var ind in ROIind)
                newPcl.Add(user.CurrentPcl.PointCloud[ind]);
            user.CurrentPcl.PointCloud = newPcl;
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            logger.LogBot($"Облако точек обрезано",
                LogLevel.Information, user, "Вот что получилось");
            return [new TextMsg("Обрезал"),
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)];

        }

        public override string SetParseParts(string textMsg)
        {
            textMsg = textMsg.Replace(" ", "");
            textMsg = textMsg.Replace(".", ",");
            conds = textMsg.Split(';').ToList();
            ParseParts = [0];
            return "Записал";
        }

        private List<string> conds = [];
        private HashSet<int> ROIind = [];
        public void Evaluate(string conditionStr, List<Vector3> pcl)
        {
            if (string.IsNullOrWhiteSpace(conditionStr))
                throw new ArgumentException("Пустая строка условия", nameof(conditionStr));

            string pattern = @"^\s*(?<var>\w+?)\s*(?<op><=|>=|==|!=|<|>)\s*(?<val>.+?)\s*$";
            var regex = new Regex(pattern);
            var match = regex.Match(conditionStr);
            if (!match.Success)
                throw new ArgumentException($"Строка не соответствует формату 'имя ОПЕРАТОР значение': \"{conditionStr}\"");

            string varName = match.Groups["var"].Value.ToLower();
            string op = match.Groups["op"].Value;
            string valStr = match.Groups["val"].Value;

            valStr = valStr.Trim();

            if (!double.TryParse(valStr, out double literalValue))
            {
                throw new ArgumentException($"Невозможно распарсить значение '{valStr}' в число");
            }
            List<float> axValues = [];

            switch (varName)
            {
                case "x":
                    axValues = pcl.Select(point => point.X).ToList(); break;
                case "y":
                    axValues = pcl.Select(point => point.Y).ToList(); break;
                case "z":
                    axValues = pcl.Select(point => point.Z).ToList(); break;
            }
            int count = axValues.Count;
            HashSet<int> curROIind = [];
            switch (op)
            {
                case "<":
                    foreach (var ind in ROIind)
                        if (axValues[ind] < literalValue)
                            curROIind.Add(ind);
                    break;
                case "<=":
                    foreach (var ind in ROIind)
                        if (axValues[ind] <= literalValue)
                            curROIind.Add(ind);
                    break;
                case ">":
                    foreach (var ind in ROIind)
                        if (axValues[ind] > literalValue)
                            curROIind.Add(ind);
                    break;
                case ">=":
                    foreach (var ind in ROIind)
                        if (axValues[ind] >= literalValue)
                            curROIind.Add(ind);
                    break;
                case "==":
                    foreach (var ind in ROIind)
                        if (axValues[ind] == literalValue)
                            curROIind.Add(ind);
                    break;
                case "!=":
                    foreach (var ind in ROIind)
                        if (axValues[ind] != literalValue)
                            curROIind.Add(ind);
                    break;
                default:
                    throw new InvalidOperationException($"Неизвестный оператор '{op}'");
            }
            ROIind = curROIind;
        }
    }
}
