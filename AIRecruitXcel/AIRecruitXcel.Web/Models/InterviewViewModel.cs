namespace AIRecruitXcel.Web.Models
{
  public class InterviewViewModel
  {
    public bool IsFinishedReview { get; set; }
    public bool HintDisabled { get; set; }

    public string Action { get; set; }

    public string? JobDescription { get; set; }
    public string? Resume { get; set; }
    public IFormFile? ResumeFile { get; set; }
    public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
  }

  public class QuestionViewModel
  {
    public string? Question { get; set; }
    public string? Hint { get; set; }
    public string? Answer { get; set; }
    public string? AIFeedback { get; set; }
  }
}
