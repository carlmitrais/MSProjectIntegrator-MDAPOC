using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Web;
using System.Web.Mvc;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using MSProjectIntegrator.Models;
using MSProjectIntegrator.Helper;
using System.IO;
using Newtonsoft.Json;

namespace MSProjectIntegrator.Controllers
{
    public class HomeController : Controller
    {
        const int PROJECT_BLOCK_SIZE = 20;

        private static ProjectContext projContext;
        //new ProjectContext("https://mitraistes.sharepoint.com/sites/pwa");      
        //new ProjectContext("http://home.mitrais.com/PWA");
        private static string user;
        private static string pass;
        private static string dom;
        private static bool isOnPremise;

        static Dictionary<String, CustomField> pwaECF = new Dictionary<string, CustomField>();

        public ActionResult Index()
        {
            Connection conn = new Connection() {
                Url = string.Empty,
                UserName = string.Empty,
                Password = string.Empty,
                Domain = string.Empty,
                IsOnpremise = false
            };
            return View(conn);
        }

        public ActionResult PostConn(Connection model)
        {
            user = model.UserName;
            projContext = new ProjectContext(model.Url);
            pass = model.Password;
            dom = model.Domain;
            isOnPremise = model.IsOnpremise;

            if (!model.IsOnpremise)
            {
                string stringPass = pass;
                SecureString secpassword = new SecureString();
                foreach (char c in stringPass.ToCharArray()) secpassword.AppendChar(c);
                projContext.Credentials = new SharePointOnlineCredentials(user, secpassword);
            }
            else if (model.IsOnpremise)
            {
                projContext.Credentials = new NetworkCredential(user, pass, dom);
            }

            projContext.Load(projContext.Projects);
            projContext.ExecuteQuery();

            if (projContext.Projects.Count > 0)
            {
                return RedirectToAction("ProjectList");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Close()
        {
            projContext = null;
            user = string.Empty;
            pass = string.Empty;
            dom = string.Empty;
            isOnPremise = false;           

            return RedirectToAction("Index");
        }

        public ActionResult ProjectList()
        {
            List<ProjectModel> projectList = new List<ProjectModel>();

            if(projContext == null)
            {
                return View("Error");
            }

            //using (projContext)
            //{
            if (!isOnPremise)
            {
                //online
                string stringPass = pass;
                SecureString secpassword = new SecureString();
                foreach (char c in stringPass.ToCharArray()) secpassword.AppendChar(c);
                projContext.Credentials = new SharePointOnlineCredentials(user, secpassword);
            }
            else if (isOnPremise)
            {
                // onpremise
                projContext.Credentials = new NetworkCredential(user, pass, dom);
            }

            projContext.Load(projContext.Projects);
            projContext.ExecuteQuery();

            foreach (PublishedProject pubProj in projContext.Projects)
            {
                ProjectModel project = new ProjectModel()
                {
                    Id = pubProj.Id,
                    Name = pubProj.Name,
                    Created = pubProj.CreatedDate
                };

                projectList.Add(project);
            }
            //}

            return View(projectList);

        }

        public ActionResult ProjectDetail(string guid)
        {
            ProjectModel project = new ProjectModel();

            if(TempData["project"] == null)
            {
                using (projContext)
                {
                    if (!isOnPremise)
                    {
                        //online
                        string stringPass = pass;
                        SecureString secpassword = new SecureString();
                        foreach (char c in stringPass.ToCharArray()) secpassword.AppendChar(c);
                        projContext.Credentials = new SharePointOnlineCredentials(user, secpassword);
                    }
                    else if (isOnPremise)
                    {
                        // onpremise
                        projContext.Credentials = new NetworkCredential(user, pass, dom);
                    }

                    var proj = projContext.Projects.First(p => p.Id == new Guid(guid));

                    projContext.Load(proj, p => p.Id, p => p.Name, p => p.CreatedDate, p => p.Tasks, p => p.Assignments.Include(
                         a => a.Task,
                         a => a.Resource));
                    projContext.ExecuteQuery();

                    project.Id = proj.Id;
                    project.Name = proj.Name;
                    project.Created = proj.CreatedDate;

                    foreach (PublishedTask pubTask in proj.Tasks)
                    {
                        var pubAss = proj.Assignments.FirstOrDefault(a => a.Task.Id == pubTask.Id);
                        ResourceModel assUser = new ResourceModel();
                        if (pubAss != null)
                        {
                            assUser.Id = pubAss.Resource.Id;
                            assUser.Name = pubAss.Resource.Name;
                        }

                        var loadTask = pubTask;
                        projContext.Load(loadTask, t => t.Predecessors, t => t.Successors);
                        projContext.ExecuteQuery();

                        TaskModel assTask = new TaskModel()
                        {
                            Id = pubTask.Id,
                            Task_Name = pubTask.Name,
                            Duration = pubTask.Duration,
                            Act_Dur = pubTask.ActualDuration,
                            Rem_Dur = pubTask.RemainingDuration,
                            Percent_Comp = pubTask.PercentComplete,
                            Actual_Work = pubTask.ActualWork,
                            Remaining_Work = pubTask.RemainingWork,
                            Scheduled_Work = pubTask.Work,
                            Act_Start = pubTask.ActualStart,
                            Act_Finish = pubTask.ActualFinish,
                            Actual_Cost = pubTask.ActualCost,
                            Start_Date = pubTask.Start,
                            Finish_Date = pubTask.Finish,
                            Cost = pubTask.Cost,
                            User = assUser,
                            Constraint_Type = pubTask.ConstraintType.ToString(),
                            Predecessors = loadTask.Predecessors.Count(),
                            Successors = loadTask.Successors.Count(),
                            Summary = pubTask.IsSummary ? "Yes" : "No",
                            Notes = pubTask.Notes
                        };

                        project.TaskList.Add(assTask);
                    }
                }

                TempData.Clear();
                TempData["Project"] = project;
                TempData.Keep("project");
            }

            else if(TempData["project"] != null)
            {
                project = TempData["project"] as ProjectModel;
                TempData["Project"] = project;
                TempData.Keep("project");
            }            

            return View(project);
        }

        public ActionResult TaskDetail(string taskGuid)
        {
            ProjectModel project = new ProjectModel();
            project = TempData["project"] as ProjectModel;

            TaskModel task = project.TaskList.Where(t => t.Id == new Guid(taskGuid)).FirstOrDefault();

            List<ResourceModel> resources = new List<ResourceModel>();

            using (projContext)
            {
                if (!isOnPremise)
                {
                    //online
                    string stringPass = pass;
                    SecureString secpassword = new SecureString();
                    foreach (char c in stringPass.ToCharArray()) secpassword.AppendChar(c);
                    projContext.Credentials = new SharePointOnlineCredentials(user, secpassword);
                }
                else if (isOnPremise)
                {
                    // onpremise
                    projContext.Credentials = new NetworkCredential(user, pass, dom);
                }

                var proj = projContext.Projects.First(p => p.Id == project.Id);
                projContext.Load(proj, p => p.ProjectResources);
                projContext.ExecuteQuery();

                foreach(PublishedProjectResource r in proj.ProjectResources)
                {
                    ResourceModel user = new ResourceModel()
                    {
                        Id = r.Id,
                        Name = r.Name
                    };

                    resources.Add(user);
                }
            }

            ViewBag.ResourcesList = resources;

            TempData.Clear();
            TempData["Project"] = project;
            TempData.Keep("project");

            return View(task);
        }

        public ActionResult PostTaskItem(TaskModel model)
        {
            ProjectModel project = new ProjectModel();
            project = TempData["project"] as ProjectModel;

            var stuffToRemove = project.TaskList.SingleOrDefault(s => s.Id == model.Id);
            if(stuffToRemove.Id != Guid.Empty)
            {
                project.TaskList.Remove(stuffToRemove);

                project.TaskList.Add(model);
            }

            TempData.Clear();
            TempData["Project"] = project;
            TempData.Keep("project");           

            return RedirectToAction("ProjectDetail", new { guid = project.Id.ToString() });
        }

        public ActionResult CreateCSV(string model)
        {
            //var Model = JsonConvert.DeserializeObject<IEnumerable<TaskModel>>(model);

            ProjectModel project = new ProjectModel();
            project = TempData["project"] as ProjectModel;

            IEnumerable<string> col = new List<string>() {
                "Id",
                "Task_Name",
                "Duration",
                "Act_Dur",
                "Rem_Dur",
                "Percent_Comp",
                "Actual_Work",
                "Remaining_Work",
                "Scheduled_Work",
                "Act_Start",
                "Act_Finish",
                "Actual_Cost",
                "Start_Date",
                "Finish_Date",
                "Cost",

                "Constraint_Type",
                "Predecessors",
                "Successors",
                "Summary",
                "Notes",

            };
            
            var csvDefinition = new CsvDefinition
            {
                Header = "",
                FieldSeparator = ',',
                Columns = col
            };

            try
            {
                string path = string.Format(Server.MapPath("~/APP_Data/csvdraft"));

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = project.Id.ToString() + "#" + DateTime.Now.ToString("yyyymmdd-hhss");
                string csvPath = string.Format("{0}/{1}", path, fileName + ".csv");
                project.TaskList.ToCsv(csvPath, csvDefinition);

            }
            catch (Exception ex)
            {
                TempData["Project"] = project;
                TempData.Keep("project");
            }

            TempData["Project"] = project;
            TempData.Keep("project");

            return Json(new { result = "Ok"}, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult SyncProject()
        {
            if (projContext == null)
            {
                return View("Error");
            }

            List<ProjectModel> projectList = new List<ProjectModel>();

            if (!isOnPremise)
            {
                //online
                string stringPass = pass;
                SecureString secpassword = new SecureString();
                foreach (char c in stringPass.ToCharArray()) secpassword.AppendChar(c);
                projContext.Credentials = new SharePointOnlineCredentials(user, secpassword);
            }
            else if (isOnPremise)
            {
                // onpremise
                projContext.Credentials = new NetworkCredential(user, pass, dom);
            }

            List<Guid> ids = new List<Guid>();

            string path = string.Format(Server.MapPath("~/APP_Data/csvdraft"));

            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles("*.csv");
            foreach (FileInfo file in Files)
            {
                string fileName = file.Name;

                int index = fileName.IndexOf('#');
                string sub = string.Empty;
                if (index >= 0)
                {
                    sub = fileName.Substring(0, index);
                }

                Guid id = new Guid(sub);
                ids.Add(id);                  
            }

            projContext.LoadQuery(projContext.Projects);
            projContext.ExecuteQuery();

            var coll = projContext.Projects.Where(p => ids.Contains(p.Id));

            foreach (PublishedProject pubProj in projContext.Projects)
            {
                if (ids.Contains(pubProj.Id))
                {
                    ProjectModel project = new ProjectModel()
                    {
                        Id = pubProj.Id,
                        Name = pubProj.Name,
                        Created = pubProj.CreatedDate
                    };

                    projectList.Add(project);
                }                    
            }

            return View(projectList);
        }

        public ActionResult CSVProjectDetail(string guid)
        {
            string path = string.Format(Server.MapPath("~/APP_Data/csvdraft/"));

            IEnumerable<string> col = new List<string>() {
                "Id",
                "Task_Name",
                "Duration",
                "Act_Dur",
                "Rem_Dur",
                "Percent_Comp",
                "Actual_Work",
                "Remaining_Work",
                "Scheduled_Work",
                "Act_Start",
                "Act_Finish",
                "Actual_Cost",
                "Start_Date",
                "Finish_Date",
                "Cost",

                "Constraint_Type",
                "Predecessors",
                "Successors",
                "Summary",
                "Notes",

            };
           
            var csvDefinition = new CsvDefinition
            {
                FieldSeparator = ',',
                Columns = col
            };

            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles("*.csv");

            ProjectModel project = new ProjectModel();

            foreach (FileInfo f in Files)
            {
                string fileName = f.Name;

                int index = fileName.IndexOf('#');
                string sub = string.Empty;
                if (index >= 0)
                {
                    sub = fileName.Substring(0, index);

                    if(sub == guid)
                    {                    

                        using (projContext)
                        {
                            if (!isOnPremise)
                            {
                                //online
                                string stringPass = pass;
                                SecureString secpassword = new SecureString();
                                foreach (char c in stringPass.ToCharArray()) secpassword.AppendChar(c);
                                projContext.Credentials = new SharePointOnlineCredentials(user, secpassword);
                            }
                            else if (isOnPremise)
                            {
                                // onpremise
                                projContext.Credentials = new NetworkCredential(user, pass, dom);
                            }

                            var proj = projContext.Projects.First(p => p.Id == new Guid(guid));

                            projContext.Load(proj);
                            projContext.ExecuteQuery();

                            project.Id = proj.Id;
                            project.Name = proj.Name;
                            project.Created = proj.CreatedDate;
                        }

                        // reader = new CsvFileReader<TaskModel>(path + fileName, csvDefinition);
                        CsvFile.DefaultCsvDefinition = csvDefinition;
                        foreach (var c in CsvFile.Read<TaskModel>(path + fileName))
                        {
                           project.TaskList.Add(c);                            
                        }
                    }
                }
            }

            TempData.Clear();
            TempData["Project"] = project;
            TempData.Keep("project");

            return View(project);
        }

        public ActionResult SyncTask(string taskGuid, string procGuid)
        {
            ProjectModel project = new ProjectModel();
            project = TempData["project"] as ProjectModel;

            TaskModel updatedtask = project.TaskList.Where(t => t.Id == new Guid(taskGuid)).SingleOrDefault();

            if(updatedtask.Id != Guid.Empty)
            {
                if (!isOnPremise)
                {
                    //online
                    string stringPass = pass;
                    SecureString secpassword = new SecureString();
                    foreach (char c in stringPass.ToCharArray()) secpassword.AppendChar(c);
                    projContext.Credentials = new SharePointOnlineCredentials(user, secpassword);
                }
                else if (isOnPremise)
                {
                    // onpremise
                    projContext.Credentials = new NetworkCredential(user, pass, dom);
                }

                PublishedProject proc = projContext.Projects.GetByGuid(new Guid(procGuid));
                DraftProject draft = proc.CheckOut();

                projContext.Load(draft.Tasks, dt => dt.Where(a => a.Id == new Guid(taskGuid)));
                projContext.Load(draft.Assignments, da => da.Where(a => a.Task.Id == new Guid(taskGuid)));

                //projContext.Load(draft.ProjectResources, dr => dr.Where(a => a.Name == updatedtask.User.Name));
                projContext.ExecuteQuery();

                DraftTask task = draft.Tasks.First();
                DraftAssignment ass = draft.Assignments.First();

                task.Duration = updatedtask.Duration;
                task.Name = updatedtask.Task_Name;
                ass.PercentWorkComplete = updatedtask.Percent_Comp;

                JobState state = projContext.WaitForQueue(draft.Publish(true), 100);
                
            }

            TempData.Clear();
            TempData["Project"] = project;
            TempData.Keep("project");

            return RedirectToAction("CSVProjectDetail", new { guid = project.Id.ToString() });
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}