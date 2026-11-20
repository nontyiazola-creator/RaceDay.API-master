
namespace RaceDay.Mvc.ViewModels.Participant;

public class ParticipantEventViewModel
{
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int OrganiserID { get; set; }
}

public class ParticipantCategoryViewModel
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ParticipantEventDetailsViewModel
{
    public ParticipantEventViewModel Event { get; set; } = new();
    public List<ParticipantCategoryViewModel> Categories { get; set; } = new();
}

public class ParticipantEnrolmentViewModel
{
    public int EnrolmentID { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ParticipantResultViewModel
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
    public DateTime RecordedAt { get; set; }
}
