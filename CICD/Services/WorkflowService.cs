using System;
using System.Collections.Generic;
using System.Text;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;

namespace CICD.Services;

public class WorkflowService
{
    private const string workflowName = "name:";
    private const string on = "on:";
    private const string workflowDispatch = "workflow_dispatch:";
    private const string pullRequestEvent = "pull_request:";
    private const string branchesFilter = "branches:";
    private const string jobs = "jobs:";
    private const string jobName = "name:";
    private const string runsOn = "runs-on:";
    private const string steps = "steps:";
    private const string usesCheckout = "- uses: actions/checkout@v2";
    private const string stepName = "- name:";
    private const string scriptRun = "run: ./build.cmd";
    private const string env = "env:";

    public string CreateBuildStatusCheckWorkflow()
    {
        var workflowBuilder = new StringBuilder();

        workflowBuilder.AppendLine(CreateWorkflowTitle("Build", WorkflowType.StatusCheck).AddNewLine(2));
        workflowBuilder.AppendLine(CreateEvents(true, true, "develop", "master").AddNewLine(2));
        workflowBuilder.AppendLine(CreateJob("Build Project", nameof(CICD.BuildStatusCheck), GitHubActionsImage.UbuntuLatest));
        workflowBuilder.AppendLine(CreateDefaultEnvVars());

        return workflowBuilder.ToString();
    }

    public string CreateProdReleaseCheckWorkflow()
    {
        var workflowBuilder = new StringBuilder();

        workflowBuilder.AppendLine(CreateWorkflowTitle("Production", WorkflowType.Release).AddNewLine(2));
        workflowBuilder.AppendLine(CreateEvents(true, false).AddNewLine(2));
        workflowBuilder.AppendLine(CreateJob("Production Release", nameof(CICD.ProductionRelease), GitHubActionsImage.UbuntuLatest));

        var envVars = new List<string>
        {
            env.AddIndents(3),
            CreateEnvVarWithSecretExpValue("GITHUB_TOKEN", "GITHUB_TOKEN").AddIndents(4),
            CreateEnvVarWithEnvExpValue("NugetOrgApiKey", "NUGET_ORG_API_KEY").AddIndents(4),
            CreateEnvVarWithEnvExpValue("TwitterConsumerApiKey", "TWITTER_CONSUMER_API_KEY").AddIndents(4),
            CreateEnvVarWithEnvExpValue("TwitterConsumerApiSecret", "TWITTER_CONSUMER_API_SECRET").AddIndents(4),
            CreateEnvVarWithEnvExpValue("TwitterAccessToken", "TWITTER_ACCESS_TOKEN").AddIndents(4),
            CreateEnvVarWithEnvExpValue("TwitterAccessTokenSecret", "TWITTER_ACCESS_TOKEN_SECRET").AddIndents(4),
        };

        workflowBuilder.AppendLine(string.Join(Environment.NewLine, envVars));

        return workflowBuilder.ToString();
    }

    // TODO: Set to private
    public string CreateEvents(bool manualDispatch, bool pullRequest, params string[] branches)
    {
        var result = new List<string>();

        result.Add(on);

        if (manualDispatch is false && pullRequest is false)
        {
            throw new Exception("A workflow must have at least one event.");
        }

        if (manualDispatch)
        {
            result.Add(workflowDispatch.AddIndents(1));
        }

        if (pullRequest)
        {
            result.Add(pullRequestEvent.AddIndents(1));

            result.Add(branchesFilter.AddIndents(2));

            for (var i = 0; i < branches.Length; i++)
            {
                branches[i] = $"- {branches[i]}".AddIndents(3);
            }

            result.Add(string.Join(Environment.NewLine, branches));
        }

        return string.Join(Environment.NewLine, result);
    }

    // TODO: Set to private
    public string CreateWorkflowTitle(string name, WorkflowType workflowType)
    {
        return workflowType switch
        {
            WorkflowType.StatusCheck => $"{workflowName} ✅{name} Status Check",
            WorkflowType.Release => $"{workflowName} 🚀{name} Release",
            _ => throw new ArgumentOutOfRangeException(nameof(workflowType), workflowType, null)
        };
    }

    // TODO: Set to private
    public string CreateJob(string nameOfJob, string targetName, GitHubActionsImage image)
    {
        var lines = new List<string>();

        var imageStr = image switch
        {
            GitHubActionsImage.WindowsServer2022 => "windows-2022",
            GitHubActionsImage.WindowsServer2019 => "windows-2019",
            GitHubActionsImage.Ubuntu2204 => "ubuntu-22.04",
            GitHubActionsImage.Ubuntu2004 => "ubuntu-20.04",
            GitHubActionsImage.Ubuntu1804 => "ubuntu-18.04",
            GitHubActionsImage.MacOs12 => "macos-12",
            GitHubActionsImage.MacOs11 => "macos-11",
            GitHubActionsImage.MacOs1015 => "macos-10.15",
            GitHubActionsImage.WindowsLatest => "windows-latest",
            GitHubActionsImage.UbuntuLatest => "ubuntu-latest",
            GitHubActionsImage.MacOsLatest => "macos-latest",
            _ => throw new ArgumentOutOfRangeException(nameof(image), image, null)
        };

        lines.Add(jobs);
        lines.Add(nameOfJob.ToSnakeCase().AddIndents(1));
        lines.Add($"{jobName} {nameOfJob.ToPascalCase()}".AddIndents(2));
        lines.Add($"{runsOn} {imageStr}".AddIndents(2));
        lines.Add(steps.AddIndents(2));
        lines.Add(usesCheckout.AddIndents(2));
        lines.Add($"{stepName} Run {targetName.ToSpaceDelimitedSections()}".AddIndents(2));
        lines.Add($"{scriptRun} {targetName}".AddIndents(3));

        return string.Join(Environment.NewLine, lines);
    }

    private string CreateDefaultEnvVars()
    {
        var lines = new List<string>();

        lines.Add(env.AddIndents(3));
        lines.Add($"GITHUB_TOKEN: {WorkflowExpression.CreateSecretExpression("GITHUB_TOKEN")}".AddIndents(4));

        return string.Join(Environment.NewLine, lines);
    }

    private string CreateEnvVarWithSecretExpValue(string envName, string secretName)
    {
        return $"{envName}: ${WorkflowExpression.CreateSecretExpression(secretName)}";
    }

    private string CreateEnvVarWithEnvExpValue(string envName, string envRefName)
    {
        return $"{envName}: ${WorkflowExpression.CreateEnvExpression(envRefName)}";
    }
}
