using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    //creates objects to pass into Edit view of Tickets controller
    public class TicketEditViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        [Display(Name = "Project")]
        public string ProjectName { get; set; }

        [Display(Name = "Assigned to")]
        public string AssignedToUserName { get; set; }

        //pass SelectLists in for dropdown menus
        //project isn't editable so isn't included as a SelectList
        [Display(Name = "Type")]
        public SelectList TicketTypes { get; set; }
        [Display(Name = "Priority")]
        public SelectList TicketPriorities { get; set; }
        [Display(Name = "Status")]
        public SelectList TicketStatuses { get; set; }

        [Required(ErrorMessage = "Please select a type")]
        public int SelectedType { get; set; }
        [Required(ErrorMessage = "Please select a priority")]
        public int SelectedPriority { get; set; }
        [Required(ErrorMessage = "Please select a status")]
        public int SelectedStatus { get; set; }
    }
}