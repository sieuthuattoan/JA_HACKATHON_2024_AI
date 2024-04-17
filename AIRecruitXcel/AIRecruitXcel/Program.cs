using AIRecruitXcel.Core;

namespace AIRecruitXcel.App
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      var p = new ResumeParser();

      var r = await p.ParseResumeAsync("Resumes/MahafujAnsari.pdf");
      r.ForEach(line => Console.WriteLine(line));
    }
  }
}
