using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class TicketAssignViewModel
    {
        public TicketDetailsViewModel TicketDetails { get; set; }
        [Display(Name = "Assigned User")]
        public SelectList ProjUsersList { get; set; }
        [Required(ErrorMessage = "No user selected")]
        public string SelectedUser { get; set; }
    }
}