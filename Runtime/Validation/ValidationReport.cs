using System.Collections.Generic;
using System.Text;

namespace SkillCreator
{
    public enum ValidationSeverity
    {
        Error,
        Warning
    }

    public readonly struct ValidationIssue
    {
        public readonly ValidationSeverity Severity;
        public readonly string Context;
        public readonly string Message;

        public ValidationIssue(ValidationSeverity severity, string context, string message)
        {
            Severity = severity;
            Context = context;
            Message = message;
        }

        public override string ToString()
        {
            string tag = Severity == ValidationSeverity.Error ? "ERROR" : "WARN";
            return $"[{tag}] {Context}: {Message}";
        }
    }

    /// <summary>데이터 검증 결과 모음.</summary>
    public sealed class ValidationReport
    {
        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();

        public IReadOnlyList<ValidationIssue> Issues => _issues;

        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }

        public bool HasErrors => ErrorCount > 0;
        public bool IsValid => ErrorCount == 0;

        public void Error(string context, string message)
        {
            _issues.Add(new ValidationIssue(ValidationSeverity.Error, context, message));
            ErrorCount++;
        }

        public void Warning(string context, string message)
        {
            _issues.Add(new ValidationIssue(ValidationSeverity.Warning, context, message));
            WarningCount++;
        }

        public override string ToString()
        {
            if (_issues.Count == 0)
                return "검증 통과: 문제 없음.";

            var sb = new StringBuilder();
            sb.AppendLine($"검증 결과: 오류 {ErrorCount}개, 경고 {WarningCount}개");
            for (int i = 0; i < _issues.Count; i++)
                sb.AppendLine(_issues[i].ToString());

            return sb.ToString();
        }
    }
}
