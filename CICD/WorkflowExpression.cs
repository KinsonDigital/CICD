namespace CICD;

public static class WorkflowExpression
{
    private const char expStart = '$';
    private const string bracketStart = "{{";
    private const string bracketEnd = "}}";

    public static string CreateSecretExpression(string name) => $"{expStart}{bracketStart} secrets.{name} {bracketEnd}";

    public static string CreateEnvExpression(string name) => $"{expStart}{bracketStart} env.{name} {bracketEnd}";
}
