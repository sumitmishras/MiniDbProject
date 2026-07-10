namespace MiniDbProject.Models;

public class WizardStep
{
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }
    public WizardStepStatus Status { get; set; } = WizardStepStatus.Pending;
}

public enum WizardStepStatus
{
    Pending,
    Active,
    Completed,
    Skipped,
    Error
}

public class WizardData
{
    public string? SelectedPortalName { get; set; }
    public string? SelectedServerName { get; set; }
    public string? SourceDatabaseName { get; set; }
    public string? ScriptFilePath { get; set; }
    public string? DestinationInstance { get; set; }
    public string? DestinationDatabaseName { get; set; }
    public bool IsValidated { get; set; }
    public string? ValidationMessage { get; set; }
    public long EstimatedSizeBytes { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public bool Confirmed { get; set; }
}
