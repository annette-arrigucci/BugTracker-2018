using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class ProjectInfoViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string UserId { get; set; }
        public int NumUserTicketsForProject { get; set; }
        public int NumUserCriticalTickets { get; set; }
        public int NumUserResolvedTickets { get; set; }

        public ProjectInfoViewModel(string userId, int projectId)
        {
            var db = new ApplicationDbContext();
            this.UserId = userId;
            this.ProjectId = projectId;
            var project = db.Projects.Where(x => x.Id == projectId).First();
            this.ProjectName = project.Name;
            var tickets = FindUserTickets();
            var helper = new UserRolesHelper();
            var projTickets = tickets.Where(x => x.ProjectId == projectId).ToList();
            
            this.NumUserTicketsForProject = projTickets.Count;
            var critical = db.TicketPriorities.Where(x => x.Name.Equals("Critical")).First();
            this.NumUserCriticalTickets = projTickets.Where(x => x.TicketPriorityId == critical.Id).Count();
            var resolved = db.TicketStatuses.Where(x => x.Name.Equals("Resolved")).First();
            this.NumUserResolvedTickets = projTickets.Where(x => x.TicketStatusId == resolved.Id).Count();
        }

        public List<Ticket> FindUserTickets()
        {
            var db = new ApplicationDbContext();
            var myTickets = new List<Ticket>();
            var recentTicketDetailsList = new List<TicketDetailsViewModel>();
            var helper = new UserRolesHelper();

            // if admin, view all tickets
            if (helper.IsUserInRole(UserId, "Admin"))
            {
                myTickets = db.Tickets.ToList();
            }
            //otherwise, go through each role a user can be in and add the tickets that can be viewed in each
            //The Union method is used to eliminate duplicate entries in which user both owns the ticket is 
            //assigned the ticket or is PM for the ticket - the Equals method is overriden in the TicketDetailsViewModel
            else if (helper.IsUserInRole(UserId, "Project Manager"))
            {
                var query = db.Projects.Where(x => x.ProjectUsers.Any(y => y.UserId == UserId));
                var projects = query.ToList();
                var pmTicketList = new List<Ticket>();
                if (projects.Count > 0)
                {
                    foreach (Project p in projects)
                    {
                        var projTickets = p.Tickets;
                        pmTicketList.AddRange(projTickets);
                    }
                }
                myTickets = pmTicketList;
            }
            if (helper.IsUserInRole(UserId, "Developer"))
            {
                var devTicketList = db.Tickets.Where(x => x.AssignedToUserId == UserId).ToList();
                myTickets = myTickets.Union(devTicketList).ToList();
            }
            if (helper.IsUserInRole(UserId, "Submitter"))
            {
                var subTicketList = db.Tickets.Where(x => x.OwnerUserId == UserId).ToList();
                myTickets = myTickets.Union(subTicketList).ToList();
            }            
            return myTickets;
        }
    }
}