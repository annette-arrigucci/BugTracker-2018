using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    //this is a class to display ticket information to a user
    public class TicketDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        [Display(Name = "Project")]
        public string ProjectTitle { get; set; }
        [Display(Name = "Type")]
        public string TicketType { get; set; }
        [Display(Name = "Priority")]
        public string TicketPriority { get; set; }
        [Display(Name = "Status")]
        public string TicketStatus { get; set; }
        [Display(Name = "Owner")]
        public string OwnerName { get; set; }
        [Display(Name = "Assigned To")]
        public string AssignedToUserName { get; set; }
        public ApplicationDbContext db = new ApplicationDbContext();

        public TicketDetailsViewModel(Ticket ticket)
        {
            this.Id = ticket.Id;
            this.Title = ticket.Title;
            this.Description = ticket.Description;
            this.Created = ticket.Created;
            this.Updated = ticket.Updated;
            //on initialization, translate Id numbers into strings for user to view
            this.ProjectTitle = getProjectTitle(ticket.ProjectId);
            this.TicketType = getType(ticket.TicketTypeId);
            this.TicketPriority = getPriority(ticket.TicketPriorityId);
            this.TicketStatus = getStatus(ticket.TicketStatusId);
            this.OwnerName = getName(ticket.OwnerUserId);
            //tickets are initially not assigned, so this field can be null
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
            {
                this.AssignedToUserName = getName(ticket.AssignedToUserId);
            }
            else
            {
                this.AssignedToUserName = "Unassigned";
            }
        }

        public string getProjectTitle(int projectId)
        {
            var project = db.Projects.FirstOrDefault(x => x.Id == projectId);
            return project.Name;
        }

        public string getType(int typeId)
        {
            var tType = db.TicketTypes.FirstOrDefault(x => x.Id == typeId);
            return tType.Name;
        }

        public string getPriority(int priorityId)
        {
            var tPriority = db.TicketPriorities.FirstOrDefault(x => x.Id == priorityId);
            return tPriority.Name;
        }

        public string getStatus(int statusId)
        {
            var tStatus = db.TicketStatuses.FirstOrDefault(x => x.Id == statusId);
            return tStatus.Name;
        }

        public string getName(string userId)
        {
            var user = db.Users.Find(userId);
            return user.FirstName + " " + user.LastName;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to TicketDetailsViewModel return false.
            TicketDetailsViewModel p = obj as TicketDetailsViewModel;
            if ((System.Object)p == null)
            {
                return false;
            }

            //Check whether the products' properties are equal. 
            return Id.Equals(p.Id) && Title.Equals(p.Title);
        }

        public bool Equals(TicketDetailsViewModel p)
        {
            // If parameter is null return false
            if ((object)p == null)
            {
                return false;
            }

            //Check whether the products' properties are equal. 
            return Id.Equals(p.Id) && Title.Equals(p.Title);
        }

        public override int GetHashCode()
        {
            //Calculate the hash code for the product. 
            return Id ^ Id;
        }

    }
}