using Microsoft.SemanticKernel;
namespace AIRecruitXcel.Core;

public interface IInterviewSupporter
{
    Task<List<string>> GetSampleQuestions(string jd, string resume, string level);
    Task<string> EvaluateAnswers(string jd, string cv, string question, string answer);
}

