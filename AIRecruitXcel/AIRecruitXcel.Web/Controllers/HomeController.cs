using AIRecruitXcel.Core;
using AIRecruitXcel.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;

namespace AIRecruitXcel.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IInterviewSupporter _sKernel;

        public HomeController(ILogger<HomeController> logger, IInterviewSupporter sKernel)
        {
            _sKernel = sKernel;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new InterviewViewModel();
            return View(model);
        }

        [Route("jobapplication/demoid")]
        public async Task<IActionResult> Demo()
        {
            var model = new InterviewViewModel
            {
                JobDescription = "Angular developers oversee the design and development of websites and of single-page, advanced, enterprise web, server-side rendered, graphic-rich and mobile applications. They may also create user-friendly and intuitive websites. Here are some of the daily responsibilities an Angular developer may perform:\r\n- Building dynamic and adaptable web applications\r\n- Performing analysis of product development\r\n- Delivering comprehensive front-end software\r\n- Working with back-end programmers to create REST API\r\n- Developing front-end application and asset infrastructure\r\n- Configuring, constructing and testing scripts within an ongoing integration environment\r\n- Using advanced techniques such as multi-threading to generate non-blocking code\r\n- Using Angular command-line interface to allow developers to do web application coding and configuration\r\n- Interacting with external website services\r\n- Assisting with workflow coordination between HTML programmers and graphic designers\r\n- Writing understandable HTML, JavaScript and cascading style sheets (CSS) code\r\n- Making decisions regarding technical and design aspects of the Angular project",
                Resume = "Mia Zox\r\n\r\nLevel 1, Bond StreetSydney NSW 2000\r\n+61 2 8005 5711\r\nmiazox@gmail.com\r\n\r\nPersonal Information:\r\n\r\nDate of Birth: 26/07/1991\r\nNationality: Australia\r\nMarital Status: Single\r\nCareer Objective:\r\nI am a Senior Angular Developer with over 10 years of experience in developing front-end web applications. I enjoy creating complex, multi-functional solutions using Angular and related technologies. My goal is to continue contributing to exciting projects and advancing my career in a dynamic and challenging environment.\r\n\r\nKey Skills:\r\n\r\nExtensive experience with Angular (Angular 2+), capable of building complex web applications.\r\nProficient in TypeScript, HTML5, CSS3, and other web technologies.\r\nDeep understanding of application architecture and design patterns.\r\nExperience working with RESTful APIs and integrating external services.\r\nStrong skills in performance optimization and user interaction.\r\nAbility to work effectively in a team development environment as well as independently.\r\nExperience in deploying and maintaining web applications in production environments.\r\nWork Experience:\r\n\r\nSenior Angular Developer\r\nJob Adder Company, City Sydney\r\nDuration: 05-2015 - Present\r\n\r\nDeveloped and maintained front-end web applications using Angular and TypeScript.\r\nCollaborated closely with UI/UX design team to ensure the best user interface.\r\nOptimized performance and scalability of applications through technical enhancements.\r\nConducted unit testing and continuous integration to ensure software quality.\r\nAngular Developer\r\nDEF Company, City LMN\r\nDuration: Month Year - Month Year\r\n\r\nDeveloped new features and bug fixes for web applications using Angular.\r\nInteracted with other development team members to achieve project goals.\r\nConducted analysis and design to implement business requirements into features.\r\nEducation:\r\n\r\nBachelor of Science in Information Technology\r\nABC University, City XYZ\r\nDuration: Month Year - Month Year\r\nCertification:\r\n\r\nAngular Certified Developer (ACD)\r\nLanguage:\r\n\r\nEnglish: Fluent\r\nInterests:\r\n\r\nTraveling\r\nReading\r\nSports\r\nReferences:\r\nAvailable upon request",
                HintDisabled = true
            };
            return View("Index", model);
        }

        [Route("jobapplication/demoid")]
        [Route("")]
        [HttpPost]
        public async Task<IActionResult> Index(InterviewViewModel model)
        {
            if (model.Action == "Upload" && model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                model.Resume = await ParseResumeAsync(model);
                model.Questions = new List<QuestionViewModel>();
            }

            if (model.Action == "Start" && !string.IsNullOrEmpty(model.JobDescription) && !string.IsNullOrEmpty(model.Resume))
            {
                model = await GenerateQuestions(model);
            }

            if (model.Action == "Finish")
            {
                model.IsFinishedReview = true;
                model = await EvaluateAnswers(model);
            }

            return View(model);
        }


        private async Task<InterviewViewModel> GenerateQuestions(InterviewViewModel model)
        {
            var questions = await _sKernel.GetSampleQuestions(model.JobDescription, model.Resume, "Senior");
            model.Questions = new List<QuestionViewModel>();
            foreach (var question in questions)
            {
                if (!string.IsNullOrEmpty(question))
                {
                    var questionAndHint = question.Split("||", StringSplitOptions.RemoveEmptyEntries);
                    var q = new QuestionViewModel
                    {
                        Question = questionAndHint[0]
                    };
                    if (questionAndHint.Length > 1)
                    {
                        q.Hint = questionAndHint[1];
                    }

                    model.Questions.Add(q);
                }

            }

            return model;
        }

        private async Task<InterviewViewModel> EvaluateAnswers(InterviewViewModel model)
        {
            foreach (var question in model.Questions.Where(x => !string.IsNullOrEmpty(x.Answer)))
            {
                question.AIFeedback = await _sKernel.EvaluateAnswers(model.JobDescription, model.Resume, question.Question, question.Answer);
                question.AIFeedback = question.AIFeedback.Replace("*separator*", "<br>");
            }
            return model;
        }

        private async Task<string> ParseResumeAsync(InterviewViewModel model)
        {
            string tempFilePath = Path.GetTempFileName();
            using (var fileStream = System.IO.File.Create(tempFilePath))
            {
                model.ResumeFile.CopyTo(fileStream);
            }
            var parser = new ResumeParser();
            var data = await parser.ParseResumeAsync(tempFilePath);
            System.IO.File.Delete(tempFilePath);
            return string.Join(Environment.NewLine, data);
        }

    }
}
