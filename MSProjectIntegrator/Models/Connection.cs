using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSProjectIntegrator.Models
{
    public class Connection
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public bool IsOnpremise { get; set; }
    }
}