using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class ProjectUserViewModel
    {
        [Display(Name ="Project Name")]
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public MultiSelectList UsersAssignedtoProject { get; set; }
        public MultiSelectList UsersNotAssignedToProject { get; set; }
        public string[] UsersToAdd { get; set; }
        public string[] UsersToRemove { get; set; }
    }
}