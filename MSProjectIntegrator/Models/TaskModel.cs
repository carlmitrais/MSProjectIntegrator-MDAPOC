using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MSProjectIntegrator.Helper;

namespace MSProjectIntegrator.Models
{
    public class TaskModel
    {
        public Guid Id { get; set; }
        //Id
        public int UniqId { get; set; }

        //WBS

        //TaskName
        public string Task_Name { get; set; }
        //Duration
        public string Duration { get; set; }
        // ActualDuration
        public string Act_Dur { get; set; }
        //Rem_Dur RemainingDuration
        public string Rem_Dur { get; set; }
        // WorkComplete
        public int Percent_Comp { get; set; }
        //ActualWork
        public string Actual_Work { get; set; }
        //RemainingWork
        public string Remaining_Work { get; set; }
        // Work
        public string Scheduled_Work { get; set; }
        // ActualStart
        public DateTime Act_Start { get; set; }
        // ActualFinish
        public DateTime Act_Finish { get; set; }
        //ActualCost  -> assigment
        public double Actual_Cost { get; set; }
        //StartDate
        public DateTime Start_Date { get; set; }
        // EndDate
        public DateTime Finish_Date { get; set; }
        //Cost
        public double Cost { get; set; }

        private ResourceModel _resource = new ResourceModel();

        //Resource_Names
        public ResourceModel User
        {
            get { return _resource; }
            set { _resource = value; }
        }
        //ConstraintType
        public string Constraint_Type { get; set; }
        //Predecessors
        public int Predecessors { get; set; }
        //Successors
        public int Successors { get; set; }
        //Summary
        public string Summary { get; set; }
        //Notes
        public string Notes { get; set; }

        //IsDeleted

        //Text_1

        //Text_2

        //Text_3

    }
}