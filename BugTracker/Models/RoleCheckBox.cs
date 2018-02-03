using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    //this is a class to pass to a Edit view in AdminUserView controller - select the user's role via checkbox
    public class RoleCheckBox
    {
        public string RoleName { get; set; }
        public bool Checked { get; set; }
    }
}