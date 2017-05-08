using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSProjectIntegrator.Models
{
    public class ProjectModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        private List<TaskModel> _taskItems = new List<TaskModel>();
        public List<TaskModel> TaskList
        {
            get { return _taskItems; }
            set { _taskItems = value; }
        }
    }
}