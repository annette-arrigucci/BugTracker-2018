using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    //this class is for when I need to pass human-readable user information into a view
    public class UserInfoViewModel
    {
        public string UserId { get; set; }
        [Display(Name = "Name")]
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Current Roles")]
        public IList<string> CurrentRoles { get; set; }
    }
}