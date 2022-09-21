public class BuildSettings
{
    public string Owner { get; set; } = string.Empty;

    public string MainProjectName { get; set; } = string.Empty;

    public string? MainProjectFileName { get; set; }

    public string? DocumentationDirName { get; set; }

    public string? ReleaseNotesDirName { get; set; }

    public bool AnnounceOnTwitter { get; set; }
}
