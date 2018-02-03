using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    //create object to pass into Create view for Tickets
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        [Display(Name ="Project")]
        public SelectList Projects { get; set; }
        [Display(Name = "Type")]
        public SelectList TicketTypes { get; set; }
        [Display(Name = "Priority")]
        public SelectList TicketPriorities { get; set; }
        //status isn't editable for creating tickets

        [Required(ErrorMessage = "Please select a project")]
        public int SelectedProject { get; set; }
        [Required(ErrorMessage = "Please select a type")]
        public int SelectedType { get; set; }
        [Required(ErrorMessage = "Please select a priority")]
        public int SelectedPriority { get; set; }
    }
}