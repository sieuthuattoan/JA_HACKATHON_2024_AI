using Microsoft.SemanticKernel;
using System.Reflection;

namespace AIRecruitXcel.Core;

public class InterviewSupporter : IInterviewSupporter
{

  private readonly Kernel _kernel;
  public InterviewSupporter(string model, string aIKey)
  {
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion(model, aIKey);

    builder.Plugins.AddFromPromptDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins", "InterviewPlugin"));

    _kernel = builder.Build();
  }

  public async Task<List<string>> GetSampleQuestions(string jd, string resume, string level)
  {
    try
    {
      const string questionsSeparater = "*question_end*";
      var arguments = new KernelArguments()
      {
        ["jd"] = jd,
        ["number_of_questions"] = 4,
        ["level"] = level,
        ["resume"] = resume,
        ["question_separator"] = questionsSeparater,
        ["hint_separator"] = "||"
      };

      var plugin = _kernel.Plugins["InterviewPlugin"];
      var genQuestionsfunction = plugin["GenQuestions"];

      var result = await _kernel.InvokeAsync(genQuestionsfunction, arguments);

      var questions = result.GetValue<string>().Split(questionsSeparater);

      return questions.ToList();

    }
    catch (Exception ex)
    {
      throw ex;
    }
  }

  public async Task<string> EvaluateAnswers(string jd, string resume, string question, string answer)
  {
    var arguments = new KernelArguments()
    {
      ["jd"] = jd,
      ["resume"] = resume,
      ["question"] = question,
      ["answer"] = answer
    };

    var plugin = _kernel.Plugins["InterviewPlugin"];
    var evalAnswerfunction = plugin["EvaluateAnswers"];

    var result = await _kernel.InvokeAsync(evalAnswerfunction, arguments);

    var eval = result.GetValue<string>();

    return eval;
  }
}

