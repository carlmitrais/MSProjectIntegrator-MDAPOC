using MSProjectIntegrator.Models;
using net.sf.mpxj;
using net.sf.mpxj.mpp;
using net.sf.mpxj.reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MSProjectIntegrator.Controllers
{
    public class ReaderController : Controller
    {
        // GET: Reader
        public ActionResult Index()
        {
            string path = string.Format(Server.MapPath("~/APP_Data/mpp/b4ubuild_sample_07.mpp"));

            ProjectReader reader = new MPPReader();
            ProjectFile project = reader.read(path);

            ProjectModel projectModel = new ProjectModel();

            projectModel.Id = Guid.Empty;
            projectModel.Name = string.Empty;

            foreach (Task task in project.AllTasks)
            {
                ResourceModel assUser = new ResourceModel();
                assUser.Name = task.ResourceNames;

                TaskModel assTask = new TaskModel()
                {
                    UniqId = int.Parse(task.ID.ToString()),
                    Task_Name = task.Name,
                    Duration = task.Duration.ToString(),
                    Act_Dur = task.ActualDuration.ToString(),
                    Rem_Dur = task.RemainingDuration.ToString(),
                    Percent_Comp = int.Parse(task.PercentageWorkComplete.ToString()),
                    //Actual_Work = task.ActualWork,
                    //Remaining_Work = task.RemainingWork,
                    //Scheduled_Work = task.Work,
                    //Act_Start = task.ActualStart,
                    //Act_Finish = task.ActualFinish,
                    //Actual_Cost = task.ActualCost,
                    //Start_Date = task.Start,
                    //Finish_Date = task.Finish,
                    //Cost = task.Cost,
                    User = assUser,
                    Constraint_Type = task.ConstraintType.ToString(),
                    //Predecessors = task.Predecessors.Count(),
                    // = task.Successors.Count(),
                    //Summary = task.IsSummary ? "Yes" : "No",
                    Notes = task.Notes
                };

                projectModel.TaskList.Add(assTask);
            }

            TempData.Clear();
            TempData["Project"] = project;
            TempData.Keep("project");

            return View(project);
            // return View();
        }

        public ActionResult Read()
        {
            string path = string.Format(Server.MapPath("~/APP_Data/mpp/b4ubuild_sample_07.mpp"));

            ProjectReader reader = new MPPReader();
            ProjectFile project = reader.read(path);

            ProjectModel projectModel = new ProjectModel();

            projectModel.Id = Guid.Empty;
            projectModel.Name = string.Empty;

            foreach(Task task in project.AllTasks)
            {
                ResourceModel assUser = new ResourceModel();
                assUser.Name = task.ResourceNames;

                TaskModel assTask = new TaskModel()
                {
                    UniqId = int.Parse(task.ID.ToString()),
                    Task_Name = task.Name,
                    Duration = task.Duration.ToString(),
                    Act_Dur = task.ActualDuration.ToString(),
                    Rem_Dur = task.RemainingDuration.ToString(),
                    Percent_Comp = int.Parse(task.PercentageWorkComplete.ToString()),
                    //Actual_Work = task.ActualWork,
                    //Remaining_Work = task.RemainingWork,
                    //Scheduled_Work = task.Work,
                    //Act_Start = task.ActualStart,
                    //Act_Finish = task.ActualFinish,
                    //Actual_Cost = task.ActualCost,
                    //Start_Date = task.Start,
                    //Finish_Date = task.Finish,
                    //Cost = task.Cost,
                    User = assUser,
                    Constraint_Type = task.ConstraintType.ToString(),
                    //Predecessors = task.Predecessors.Count(),
                    // = task.Successors.Count(),
                    //Summary = task.IsSummary ? "Yes" : "No",
                    Notes = task.Notes
                };

                projectModel.TaskList.Add(assTask);
            }

            TempData.Clear();
            TempData["Project"] = project;
            TempData.Keep("project");

            return View(project);

        }
    }
}