using Textkernel.Tx;
using Textkernel.Tx.Models;
using Textkernel.Tx.Models.API.Parsing;
using Textkernel.Tx.Models.Resume.ContactInfo;

namespace AIRecruitXcel.Core
{
  public class ResumeParser
  {
    static readonly HttpClient httpClient = new HttpClient();
    private readonly TxClient _client;

    public ResumeParser()
    {
      _client = new TxClient(httpClient, new TxClientSettings
      {
        AccountId = "change-me",
        ServiceKey = "change-me",
        DataCenter = DataCenter.US
      });
    }

    /// <summary>
    /// https://github.com/textkernel/tx-dotnet/blob/master/examples/Parsing/Basic%20Parsing.md
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<List<string>> ParseResumeAsync(string path)
    {
      var resume = new List<string>();

      // A Document is an unparsed File(PDF, Word Doc, etc)
      Document doc = new Document(path);

      //when you create a ParseRequest, you can specify many configuration settings
      //in the ParseOptions. See https://developer.textkernel.com/tx-platform/v10/resume-parser/api/
      ParseRequest request = new ParseRequest(doc, new ParseOptions());

      try
      {
        ParseResumeResponse response = await _client.ParseResume(request);
        //if we get here, it was 200-OK and all operations succeeded

        //now we can use the response to output some of the data from the resume
        resume = PrintBasicResumeInfo(response);
      }
      catch (TxException e)
      {
        //the document could not be parsed, always try/catch for TxExceptions when using TxClient
        Console.WriteLine($"Error: {e.TxErrorCode}, Message: {e.Message}");
      }

      return resume;

    }

    static List<string> PrintBasicResumeInfo(ParseResumeResponse response)
    {
      var resume = new List<string>();

      resume.AddRange(PrintContactInfo(response));
      resume.AddRange(PrintPersonalInfo(response));
      resume.AddRange(PrintWorkHistory(response.Value));
      resume.AddRange(PrintEducation(response.Value));
      return resume;
    }

    static List<string> PrintHeader(string headerName)
    {
      var header = new List<string>
      {
        "",
        "",
        $"--------------- {headerName} ---------------"
      };
      return header;
    }

    static List<string> PrintContactInfo(ParseResumeResponse response)
    {
      var lines = new List<string>();
      //general contact information (only some examples listed here, there are many others)
      lines.AddRange(PrintHeader("CONTACT INFORMATION"));
      lines.Add("Name: " + response.EasyAccess().GetCandidateName()?.FormattedName);
      lines.Add("Email: " + response.EasyAccess().GetEmailAddresses()?.FirstOrDefault());
      lines.Add("Phone: " + response.EasyAccess().GetPhoneNumbers()?.FirstOrDefault());
      lines.Add("City: " + response.EasyAccess().GetContactInfo()?.Location?.Municipality);
      lines.Add("Region: " + response.EasyAccess().GetContactInfo()?.Location?.Regions?.FirstOrDefault());
      lines.Add("Country: " + response.EasyAccess().GetContactInfo()?.Location?.CountryCode);
      lines.Add("LinkedIn: " + response.EasyAccess().GetWebAddress(WebAddressType.LinkedIn));

      return lines;
    }

    static List<string> PrintPersonalInfo(ParseResumeResponse response)
    {
      var lines = new List<string>();
      //personal information (only some examples listed here, there are many others)
      lines.AddRange(PrintHeader("PERSONAL INFORMATION"));
      lines.Add("Date of Birth: " + response.EasyAccess().GetDateOfBirth()?.Date.ToShortDateString());
      lines.Add("Driving License: " + response.EasyAccess().GetDrivingLicense());
      lines.Add("Nationality: " + response.EasyAccess().GetNationality());
      lines.Add("Visa Status: " + response.EasyAccess().GetVisaStatus());

      return lines;
    }

    static List<string> PrintWorkHistory(ParseResumeResponseValue response)
    {
      var lines = new List<string>();
      //basic work history display
      lines.AddRange(PrintHeader("EXPERIENCE SUMMARY"));
      lines.Add("Years Experience: " + Math.Round((response.ResumeData?.EmploymentHistory?.ExperienceSummary?.MonthsOfWorkExperience ?? 0) / 12.0, 1));
      lines.Add("Avg Years Per Employer: " + Math.Round((response.ResumeData?.EmploymentHistory?.ExperienceSummary?.AverageMonthsPerEmployer ?? 0) / 12.0, 1));
      lines.Add("Years Management Experience: " + Math.Round((response.ResumeData?.EmploymentHistory?.ExperienceSummary?.MonthsOfManagementExperience ?? 0) / 12.0, 1));

      response.ResumeData?.EmploymentHistory?.Positions?.ForEach(position =>
      {
        lines.Add($"{Environment.NewLine}POSITION '{position.Id}'");
        lines.Add($"Employer: {position.Employer?.Name?.Normalized}");
        lines.Add($"Title: {position.JobTitle?.Normalized}");
        lines.Add($"Date Range: {GetTxDateAsString(position.StartDate)} - {GetTxDateAsString(position.EndDate)}");
      });

      return lines;
    }

    static List<string> PrintEducation(ParseResumeResponseValue response)
    {
      var lines = new List<string>();
      //basic education display
      lines.AddRange(PrintHeader("EDUCATION SUMMARY"));
      lines.Add($"Highest Degree: {response.ResumeData?.Education?.HighestDegree?.Name?.Normalized}");

      response.ResumeData?.Education?.EducationDetails?.ForEach(edu =>
      {
        lines.Add($"{Environment.NewLine}EDUCATION '{edu.Id}'");
        lines.Add($"School: {edu.SchoolName?.Normalized}");
        lines.Add($"Degree: {edu.Degree?.Name?.Normalized}");
        if (edu.Majors != null)
          lines.Add($"Focus: {string.Join(", ", edu.Majors)}");
        if (edu.GPA != null)
          lines.Add($"GPA: {edu.GPA?.NormalizedScore}/1.0 ({edu.GPA?.Score}/{edu.GPA?.MaxScore})");
        //string endDateRepresents = edu.Graduated.Value ? "Graduated" : "Last Attended";
        //Console.WriteLine($"{endDateRepresents}: {GetTxDateAsString(edu.EndDate)}");
      });

      return lines;
    }

    static string GetTxDateAsString(TxDate date)
    {
      //a TxDate represents a date found on a resume, so it can either be 
      //'current', as in "July 2018 - current"
      //a year, as in "2018 - 2020"
      //a year and month, as in "2018/06 - 2020/07"
      //a year/month/day, as in "5/4/2018 - 7/2/2020"

      if (date == null) return "";
      if (date.IsCurrentDate) return "current";

      string format = "yyyy";

      if (date.FoundMonth)
      {
        format += "-MM";//only print the month if it was actually found on the resume/job

        if (date.FoundDay) format += "-dd";//only print the day if it was actually found
      }

      return date.Date.ToString(format);
    }

  }
}
