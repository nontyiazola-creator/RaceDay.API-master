
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels.Organiser;

public class EventViewModel
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }

    [Required]
    public string EventName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

    [Required]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 99999)]
    public decimal Distance { get; set; }

    [Required]
    public string EventType { get; set; } = "Run";
}

public class CategoryViewModel
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }

    [Required]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class EnrolmentViewModel
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ResultViewModel
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class CaptureResultViewModel
{
    public int EnrolmentID { get; set; }

    public string? FinishTime { get; set; }

    [Range(1, 999999)]
    public int? FinishPosition { get; set; }
}
