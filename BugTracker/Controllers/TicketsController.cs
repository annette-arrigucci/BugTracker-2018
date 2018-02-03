using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
//using PagedList;
//using PagedList.Mvc;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //mapping numbers to strings with priorities, statuses and types so I don't have to look up in database
        //this could change if new priorities, statuses or types are added
        string[] Priorities = { "High", "Medium", "Low", "Critical" };
        string[] Statuses = { "Waiting for support", "Waiting for customer", "Resolved", "On hold", "New" };
        string[] Types = { "Error report", "Feature request", "Service request", "Other" };

        // GET: Get the user's dashboard view
        [Authorize]
        public ActionResult Dashboard()
        {
            var id = User.Identity.GetUserId();
            var recentTicketList = new List<Ticket>();
            var recentTicketDetailsList = new List<TicketDetailsViewModel>();

            // if admin, view all tickets
            if (User.IsInRole("Admin"))       
            {
                var myTickets = db.Tickets.ToList();
                var recentList = GetMostRecent(myTickets);
                //get the first five tickets in the list if the list has more than five items
                //otherwise get details for the whole list
                if(recentList.Count > 5)
                {
                    recentTicketList = recentList.Take(5).ToList();
                }
                else
                {
                    recentTicketList = recentList;
                }
                recentTicketDetailsList = TransformTickets(recentTicketList);
                return View(recentTicketDetailsList);
            }
                //otherwise, go through each role a user can be in and add the tickets that can be viewed in each
                //The Union method is used to eliminate duplicate entries in which user both owns the ticket is 
                //assigned the ticket or is PM for the ticket - the Equals method is overriden in the TicketDetailsViewModel
                else if (User.IsInRole("Project Manager"))
                {
                    var query = db.Projects.Where(x => x.ProjectUsers.Any(y => y.UserId == id));
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
                    recentTicketList = pmTicketList;
                }
                if (User.IsInRole("Developer"))
                {
                    var devTicketList = db.Tickets.Where(x => x.AssignedToUserId == id).ToList();
                    recentTicketList = recentTicketList.Union(devTicketList).ToList();
                }
                if (User.IsInRole("Submitter"))
                {
                    var subTicketList = db.Tickets.Where(x => x.OwnerUserId == id).ToList();
                    recentTicketList = recentTicketList.Union(subTicketList).ToList();
                }
            //now that we have the ticket list, order it by recency
            var recencyList = GetMostRecent(recentTicketList);
            //trim the list to the top 5 if we have enough items
            if (recencyList.Count > 5)
            {
                recentTicketList = recencyList.Take(5).ToList();
            }
            else
            {
                recentTicketList = recencyList;
            }
            recentTicketDetailsList = TransformTickets(recentTicketList);
            return View(recentTicketDetailsList);
         }

        //return a list of tickets ordered by recency
        public List<Ticket> GetMostRecent(List<Ticket> tickets)
        {
            //calculate the recency of each ticket that we can see
            var ticketRecencyList = new List<TicketTimeDifference>();
            foreach (var ticket in tickets)
            {
                var ttSpan = new TicketTimeDifference();
                ttSpan.TicketId = ticket.Id;
                //if the ticket was updated, use that value for recency
                if (ticket.Updated != null)
                {
                    ttSpan.TimeDifferenceFromNow = DateTimeOffset.Now.Subtract((DateTimeOffset)ticket.Updated);
                }
                //otherwise use the created date
                else
                {
                    ttSpan.TimeDifferenceFromNow = DateTimeOffset.Now.Subtract((DateTimeOffset)ticket.Created);
                }
                ticketRecencyList.Add(ttSpan);
            }
            //based on the
            var mostRecent = ticketRecencyList.OrderBy(x => x.TimeDifferenceFromNow).ToList();
            var recentList = new List<Ticket>();
            for (int i = 0; i < mostRecent.Count; i++)
            {
                var ticket = db.Tickets.Find(mostRecent[i].TicketId);
                recentList.Add(ticket);
            }
            return recentList;
        }

        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            var id = User.Identity.GetUserId();
            var ticketDetailsList = new List<TicketDetailsViewModel>();

            // if admin, view all tickets
            if (User.IsInRole("Admin"))
            {
                var tickets = db.Tickets;
                ticketDetailsList = TransformTickets(db.Tickets.ToList());
                ticketDetailsList = ticketDetailsList.OrderByDescending(x => x.Created).ToList();
                return View(ticketDetailsList);
            }
            //otherwise, go through each role a user can be in and add the tickets that can be viewed in each
            //The Union method is used to eliminate duplicate entries in which user both owns the ticket is 
            //assigned the ticket or is PM for the ticket - the Equals method is overriden in the TicketDetailsViewModel
            else if (User.IsInRole("Project Manager"))
            {
                var query = db.Projects.Where(x => x.ProjectUsers.Any(y => y.UserId == id));
                var projects = query.ToList();
                var ticketList = new List<Ticket>();
                if (projects.Count > 0)
                { 
                    foreach (Project p in projects)
                    {
                        var projTickets = p.Tickets;
                        ticketList.AddRange(projTickets);
                    }
                }             
                var pmTicketDetailsList = TransformTickets(ticketList);
                ticketDetailsList = ticketDetailsList.Union(pmTicketDetailsList).ToList();
            }
            if (User.IsInRole("Developer"))
            {
                var tickets = db.Tickets.Where(x => x.AssignedToUserId == id);
                var devDetailsList = TransformTickets(tickets.ToList());
                ticketDetailsList = ticketDetailsList.Union(devDetailsList).ToList();
            }
            if (User.IsInRole("Submitter"))
            {
                var tickets = db.Tickets.Where(x => x.OwnerUserId == id);
                var subDetailsList = TransformTickets(tickets.ToList());
                ticketDetailsList = ticketDetailsList.Union(subDetailsList).ToList();
            }
            //the order gets overriden when datatables.net script is applied
            ticketDetailsList = ticketDetailsList.OrderByDescending(x => x.Created).ToList();
            return View(ticketDetailsList);
        }

        //this calls a ticket directory view where all fields of tickets can be viewed on one page
        //same code as Index view but displays differently in view
        [Authorize]
        public ActionResult All()
        {
            var id = User.Identity.GetUserId();
            var ticketDetailsList = new List<TicketDetailsViewModel>();

            // if admin, view all tickets
            if (User.IsInRole("Admin"))
            {
                var tickets = db.Tickets;
                ticketDetailsList = TransformTickets(db.Tickets.ToList());
                ticketDetailsList = ticketDetailsList.OrderByDescending(x => x.Created).ToList();
                return View(ticketDetailsList);
            }
            //otherwise, go through each role a user can be in and add the tickets that can be viewed in each
            //duplicates eliminated with Union method
            else if (User.IsInRole("Project Manager"))
            {
                var query = db.Projects.Where(x => x.ProjectUsers.Any(y => y.UserId == id));
                var projects = query.ToList();
                var ticketList = new List<Ticket>();
                if (projects.Count > 0)
                {
                    foreach (Project p in projects)
                    {
                        var projTickets = p.Tickets;
                        ticketList.AddRange(projTickets);
                    }
                }
                var pmTicketDetailsList = TransformTickets(ticketList);
                ticketDetailsList = ticketDetailsList.Union(pmTicketDetailsList).ToList();
            }
            if (User.IsInRole("Developer"))
            {
                var tickets = db.Tickets.Where(x => x.AssignedToUserId == id);
                var devDetailsList = TransformTickets(tickets.ToList());
                ticketDetailsList = ticketDetailsList.Union(devDetailsList).ToList();
            }
            if (User.IsInRole("Submitter"))
            {
                var tickets = db.Tickets.Where(x => x.OwnerUserId == id);
                var subDetailsList = TransformTickets(tickets.ToList());
                ticketDetailsList = ticketDetailsList.Union(subDetailsList).ToList();
            }
            ticketDetailsList = ticketDetailsList.OrderByDescending(x => x.Created).ToList();
            return View(ticketDetailsList);
        }

        // GET: Tickets/Details/5
        [Authorize(Roles = "Submitter, Developer, Project Manager, Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }

            var userId = User.Identity.GetUserId();
            var helper = new ProjectUserHelper();

            //if user is not an admin, who is able to view all tickets, or the creator of the ticket, who is able to view and edit the ticket,
            //check if they are a project manager or developer who is authorized to view and edit the ticket 
            //If not, redirect them to a "bad request" page
            if (!User.IsInRole("Admin") && !ticket.OwnerUserId.Equals(userId))
            {
                //for PM, verify it is in one of their assigned projects
                if (User.IsInRole("Project Manager"))
                {
                    if (!helper.IsUserInProject(userId, ticket.ProjectId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
                //for developer - verify that they have been assigned this ticket
                else if (User.IsInRole("Developer"))
                {
                    //if the ticket is unassigned, return a bad request
                    if (string.IsNullOrEmpty(ticket.AssignedToUserId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                    else if (!ticket.AssignedToUserId.Equals(userId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
                //if the user is not a PM, developer or submitter, then they are unassigned and not authorized to view any tickets
                else
                {
                    return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                }
            }
            var model = new TicketDetailsViewModel(ticket);

            //pass in the TicketComments with the ticket details
            ViewBag.Comments = ticket.TicketComments;
            ViewBag.Attachments = ticket.TicketAttachments;
            return View(model);
        }

        [Authorize(Roles = "Submitter, Developer, Project Manager, Admin")]
        public ActionResult History(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }

            var userId = User.Identity.GetUserId();
            var helper = new ProjectUserHelper();

            //if user is not an admin, who is able to view all tickets, or the creator of the ticket, who is able to view and edit the ticket,
            //check if they are a project manager or developer who is authorized to view and edit the ticket 
            //If not, redirect them to a "bad request" page
            if (!User.IsInRole("Admin") && !ticket.OwnerUserId.Equals(userId))
            {
                //for PM, verify it is in one of their assigned projects
                if (User.IsInRole("Project Manager"))
                {
                    if (!helper.IsUserInProject(userId, ticket.ProjectId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
                //for developer - verify that they have been assigned this ticket
                else if (User.IsInRole("Developer"))
                {
                    //if the ticket is unassigned, return a bad request
                    if (string.IsNullOrEmpty(ticket.AssignedToUserId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                    else if (!ticket.AssignedToUserId.Equals(userId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
               
                //if the user is not a PM, developer or submitter, then they are unassigned and not authorized to view any tickets
                else
                {
                    //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                }
            }
            ViewBag.ticketTitle = ticket.Title;
            ViewBag.ticketId = ticket.Id;
            var model = ticket.TicketHistories.OrderByDescending(x => x.ChangeDate);
            return View(model);
        }

        // GET: Tickets/AssignUser/5
        [Authorize(Roles ="Project Manager")]
        public ActionResult AssignUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //check that project manager is assigning a ticket that is in one of their projects
            var helper = new ProjectUserHelper();
            var userId = User.Identity.GetUserId();
            Ticket ticket = db.Tickets.Find(id);

            if (!helper.IsUserInProject(userId, ticket.ProjectId))
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
            }

            var model = new TicketAssignViewModel();
            
            model.TicketDetails = new TicketDetailsViewModel(ticket);
            
            if(!string.IsNullOrEmpty(ticket.AssignedToUserId))
            {
                model.SelectedUser = ticket.AssignedToUserId;
            }
            //find only the developers assigned to a project - only developers can be assigned a ticket        
            var developerIDList = helper.DevelopersInProject(ticket.ProjectId);
            var developerInfoList = helper.getUserInfo(developerIDList);
            if (!string.IsNullOrEmpty(model.SelectedUser))
            {
                model.ProjUsersList = new SelectList(developerInfoList, "UserId", "UserName", model.SelectedUser);
            }
            else
            {
                model.ProjUsersList = new SelectList(developerInfoList, "UserId", "UserName");
            }
            if (ticket == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }
            return View(model);
        }

        // POST: Tickets/AssignUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignUser(int tId, string SelectedUser)
        {
            if (ModelState.IsValid)
            {
                var ticket = db.Tickets.Find(tId);
                //if there is already a user assigned, check if it is the same user
                //if it is, we won't create another ticket notification
                if (ticket.AssignedToUserId != null && ticket.AssignedToUserId.Equals("Selected User"))
                {
                        return RedirectToAction("Index");
                }
                //double check that user is a developer; if not redirect back to form
                var helper = new UserRolesHelper();
                if (!helper.IsUserInRole(SelectedUser, "Developer"))
                {
                    return RedirectToAction("AssignUser", new { id = tId });
                }
                //otherwise,              
                //- create entry in ticket history table
                //- create an entry in ticket notification table
                //- send an email to the developer who has been assigned the ticket
                //- update the ticket: if status is New, change to "Waiting for Support" and change its assigned user
                //- update the database

                else
                {
                    //get userId and name of assigner
                    var assignerId = User.Identity.GetUserId();

                    //get name of SelectedUser
                    var selected = db.Users.Find(SelectedUser);
                    var selectedName = selected.FirstName + " " + selected.LastName;

                    //create the ticket history entry
                    //if this is the first assignment of the ticket, the "Assigned To" field will be null
                    if (ticket.AssignedToUserId == null)
                    {
                        CreateTicketHistory(tId, assignerId, "Assigned To", "None", selectedName);
                        //create a new Ticket Notification object
                        var tn = new TicketNotification(tId, SelectedUser, "Assign");
                        tn.AddTicketNotification();
                        tn.SendNotificationEmail();
                    }
                    //otherwise create a ticket for the change of assignment
                    else
                    {
                        //get the name of the user who was previously assigned to the ticket
                        var oldAssigned = db.Users.Find(ticket.AssignedToUserId);
                        var oldAssignedName = oldAssigned.FirstName + " " + oldAssigned.LastName;
                        CreateTicketHistory(tId, assignerId, "Assigned To", oldAssignedName, selectedName);
                        
                        //notify the old developer that the ticket has been reassigned
                        var trn1 = new TicketNotification(tId, ticket.AssignedToUserId, "Reassign");
                        trn1.AddTicketNotification();
                        trn1.SendNotificationEmail();

                        //notify the new developer they have been assigned to the ticket
                        var trn2 = new TicketNotification(tId, SelectedUser, "Assign");
                        trn2.AddTicketNotification();
                        trn2.SendNotificationEmail();
                    }

                    ticket.AssignedToUserId = SelectedUser;                

                    UpdateTicketStatusIfNew(tId);

                    db.Entry(ticket).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("ConfirmAssignment");
                } 
            }
            else
            {
                return RedirectToAction("AssignUser", new { id = tId });
            }
        }
       

        public void UpdateTicketStatusIfNew(int tId)
        {
            var ticket = db.Tickets.Find(tId);
            var status = db.TicketStatuses.Find(ticket.TicketStatusId);
         
            if(status.Name == "New")
            {
                var newStatus = db.TicketStatuses.FirstOrDefault(x => x.Name == "Waiting for support");
                ticket.TicketStatusId = newStatus.Id;
                CreateTicketHistory(tId, "System", "Status", "New", "Waiting for support");
            }
        }

        [Authorize(Roles = "Project Manager")]
        public ActionResult ConfirmAssignment()
        {
            return View();
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            var ticketView = new TicketCreateViewModel();

            var myHelper = new ProjectUserHelper();
            var userId = User.Identity.GetUserId();
            //set it so that a user creates a ticket he/she can only select a project he/she is assigned to  
            var projectList = new List<Project>();
            if (!User.IsInRole("Admin"))
            {
                var projects = myHelper.ListUserProjects(userId);
                //only allow the user to select from active projects
                projectList = projects.Where(x => x.IsActive == true).ToList();
            }
            //if user as an admin, display all projects
            else
            {
                var allProjects = myHelper.GetAllProjects();
                projectList = allProjects.Where(x => x.IsActive == true).ToList();
            }
            ticketView.Projects = new SelectList(projectList, "Id", "Name");
            ticketView.TicketTypes = new SelectList(db.TicketTypes, "Id", "Name");
            ticketView.TicketPriorities = new SelectList(db.TicketPriorities, "Id", "Name");
            return View(ticketView);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description,SelectedProject,SelectedType,SelectedPriority")] TicketCreateViewModel tvm)
        {
            if (ModelState.IsValid)
            {
                var ticket = new Ticket();
                ticket.Title = tvm.Title;
                ticket.Description = tvm.Description;
                ticket.Created = DateTimeOffset.Now;
                ticket.ProjectId = tvm.SelectedProject;
                ticket.TicketTypeId = tvm.SelectedType;
                ticket.TicketPriorityId = tvm.SelectedPriority;
                var query = from p in db.TicketStatuses
                            where p.Name == "New"
                            select p.Id;
                ticket.TicketStatusId = query.First();
                ticket.OwnerUserId = User.Identity.GetUserId();

                db.Tickets.Add(ticket);
                db.SaveChanges();

                //update TicketHistories table
                AddTicketHistoryForCreate(ticket);

                return RedirectToAction("Index");
            }
            return View(tvm);
        }

       

        // GET: Tickets/Edit/5
        [Authorize( Roles = "Submitter, Developer, Project Manager, Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }

            Ticket ticket = db.Tickets.Find(id);
            var userId = User.Identity.GetUserId();
            var helper = new ProjectUserHelper();

            if (ticket == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }

            //if user is not an admin, who is able to view all tickets, or the creator of the ticket, who is able to view and edit the ticket,
            //check if they are a project manager or developer who is authorized to view and edit the ticket 
            //If not, redirect them to a "bad request" page
            if (!User.IsInRole("Admin") && !ticket.OwnerUserId.Equals(userId))
            {
                //for PM, verify it is in one of their assigned projects
                if (User.IsInRole("Project Manager"))
                {
                    if (!helper.IsUserInProject(userId, ticket.ProjectId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
                //for developer - verify that they have been assigned this ticket
                else if (User.IsInRole("Developer"))
                {
                    //if the ticket is unassigned, return a bad request
                    if (string.IsNullOrEmpty(ticket.AssignedToUserId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                    else if (!ticket.AssignedToUserId.Equals(userId))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
               
                //if the user is not a PM, developer or submitter, then they are unassigned and not authorized to view any tickets
                else
                {
                    return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                }
            }

            var ticketEdit = new TicketEditViewModel();
            ticketEdit.Id = ticket.Id;
            ticketEdit.Title = ticket.Title;
            ticketEdit.Created = ticket.Created;
            ticketEdit.Updated = ticket.Updated;
            ticketEdit.Description = ticket.Description;

            //setting the default selected values of the TicketEditViewModel to the current values in the ticket
            //the project field is not editable
            var project = db.Projects.Find(ticket.ProjectId);
            ticketEdit.ProjectName = project.Name;
            ticketEdit.SelectedType = ticket.TicketTypeId;
            ticketEdit.SelectedPriority = ticket.TicketPriorityId;
            ticketEdit.SelectedStatus = ticket.TicketStatusId;
            //form a username if the ticket has been assigned - otherwise set to "Unassigned"
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
            {
               var assignedTo = db.Users.Find(ticket.AssignedToUserId);
               ticketEdit.AssignedToUserName = assignedTo.FirstName + " " + assignedTo.LastName;
            }
            else
            {
               ticketEdit.AssignedToUserName = "Unassigned";
            }

            
            ticketEdit.TicketTypes = new SelectList(db.TicketTypes, "Id", "Name", ticketEdit.SelectedType);//ticket.TicketTypeId
            ticketEdit.TicketPriorities = new SelectList(db.TicketPriorities, "Id", "Name", ticketEdit.SelectedPriority);//ticket.TicketPriorityId
            ticketEdit.TicketStatuses = new SelectList(db.TicketStatuses, "Id", "Name", ticketEdit.SelectedStatus);//ticket.TicketStatusId

            return View(ticketEdit);         
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,SelectedType,SelectedPriority,SelectedStatus")] TicketEditViewModel tevModel)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = db.Tickets.Find(tevModel.Id);

                //check the differences between the original ticket and the edited ticket
                //get the user Id of the editor of the ticket
                //add the ticket history or histories to the database
                var editor = User.Identity.GetUserId();
                var wasEdited = CheckTicketHistoryEdit(ticket, tevModel, editor);

                //if the editor of the ticket is not the developer assigned to the ticket, and there were
                //changes to the ticket, create a ticket notification and send an email to the developer
                if(!editor.Equals(ticket.AssignedToUserId) && wasEdited == true)
                {
                    var teNotification = new TicketNotification(ticket.Id, ticket.AssignedToUserId, "Edited");
                    teNotification.AddTicketNotification();
                    teNotification.SendNotificationEmail();
                }
                ticket.Title = tevModel.Title;
                ticket.Description = tevModel.Description;
                ticket.Created = tevModel.Created;
                ticket.Updated = DateTimeOffset.Now;
                ticket.TicketTypeId = tevModel.SelectedType;
                ticket.TicketPriorityId = tevModel.SelectedPriority;
                ticket.TicketStatusId = tevModel.SelectedStatus;

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tevModel);
        }

        //Tickets are not supposed to be deleted - their final state is "resolved"
        //Commenting out code in case I need to add it back later

        // GET: Tickets/Delete/5
        //[Authorize(Roles = "Admin, Project Manager")]
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticket);
        //}

        // POST: Tickets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Ticket ticket = db.Tickets.Find(id);
        //    db.Tickets.Remove(ticket);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //take a list of Tickets and change them into TicketDetailsViewModel objects that will be passed to
        //a human-readable Index view
        public List<TicketDetailsViewModel> TransformTickets(List<Ticket> ticketList)
        {
            var ticketDetailsList = new List<TicketDetailsViewModel>();
            foreach (var ticket in ticketList)
            {
                var tdTicket = new TicketDetailsViewModel(ticket);
                ticketDetailsList.Add(tdTicket);
            }
            return ticketDetailsList;
        }

        //create a ticket history entry upon creation of a new ticket
        public void AddTicketHistoryForCreate(Ticket ticket)
        {
            var ticketHistory = new TicketHistory();
            ticketHistory.TicketId = ticket.Id;
            ticketHistory.PropertyChanged = "Created";
            ticketHistory.ChangeDate = DateTimeOffset.Now;
            var priority = Priorities[ticket.TicketPriorityId - 1];
            var type = Types[ticket.TicketTypeId - 1];
            var status = Statuses[ticket.TicketStatusId - 1];
            var project = db.Projects.Find(ticket.ProjectId);
            var pName = project.Name;

            ticketHistory.NewValue = ticket.Created.ToString();
            ticketHistory.Description = "Title: " + ticket.Title + "<br>" + "Project: " + pName + "<br>" + "Description: " + ticket.Description + "<br>Priority: " + priority + "; Type: " + type + "; Status: " + status;
            ticketHistory.UserId = ticket.OwnerUserId;

            db.TicketHistories.Add(ticketHistory);
            db.SaveChanges();
        }

        //create a ticket history entry in the database with every field that was edited in the ticket
        //if ticket had any edits, return true
        public bool CheckTicketHistoryEdit(Ticket ticket, TicketEditViewModel editedTicket, string userId)
        {
            bool wasEdited = false;
            if (!ticket.Title.Equals(editedTicket.Title))
            {
                var old = ticket.Title;
                var newVal = editedTicket.Title;
                CreateTicketHistory(ticket.Id, userId, "Title", old, newVal);
                wasEdited = true;
            }
            if (!ticket.Description.Equals(editedTicket.Description))
            {
                var old = ticket.Description;
                var newVal = editedTicket.Description;
                CreateTicketHistory(ticket.Id, userId, "Description", old, newVal);
                wasEdited = true;
            }
            if (!ticket.TicketTypeId.Equals(editedTicket.SelectedType))
            {
                var old = Types[ticket.TicketTypeId - 1];
                var newVal = Types[editedTicket.SelectedType - 1];
                CreateTicketHistory(ticket.Id, userId, "Type", old, newVal);
                wasEdited = true;
            }
            if (!ticket.TicketPriorityId.Equals(editedTicket.SelectedPriority))
            {
                var old = Priorities[ticket.TicketPriorityId - 1];
                var newVal = Priorities[editedTicket.SelectedPriority - 1];
                CreateTicketHistory(ticket.Id, userId, "Priority", old, newVal);
                wasEdited = true;
            }
            if (!ticket.TicketStatusId.Equals(editedTicket.SelectedStatus))
            {
                var old = Statuses[ticket.TicketStatusId - 1];
                var newVal = Statuses[editedTicket.SelectedStatus - 1];
                CreateTicketHistory(ticket.Id, userId, "Status", old, newVal);
                wasEdited = true;
            }
            return wasEdited;
        }

        //create a new ticket history object in the database
        public void CreateTicketHistory(int ticketId, string userId, string property, string oldValue, string newValue)
        {
            var ticketHistory = new TicketHistory();
            ticketHistory.TicketId = ticketId;
            ticketHistory.UserId = userId;
            ticketHistory.PropertyChanged = property;
            ticketHistory.ChangeDate = DateTimeOffset.Now;
            ticketHistory.OldValue = oldValue;
            ticketHistory.NewValue = newValue;      
            
            if (property.Equals("Description"))
            {
                ticketHistory.Description = "Ticket description changed.<br>New description:<br>" + newValue + "<br>" + "Old description:<br>" + oldValue;
            }
            else if(property.Equals("Title") || property.Equals("Type") || property.Equals("Priority") || property.Equals("Status"))
            {
                ticketHistory.Description = property + " changed from \"" + oldValue + "\" to \"" + newValue + ".\"";
            }
            else if(property.Equals("Assigned To"))
            {
                if (oldValue.Equals("None"))
                {
                    ticketHistory.Description = "Ticket assigned to " + newValue + "."; 
                }
                else
                {
                    ticketHistory.Description = "Ticket assignment changed from " + oldValue + " to " + newValue + ".";
                }
            }
            db.TicketHistories.Add(ticketHistory);
            db.SaveChanges();
        }

        //protected override void OnException(ExceptionContext filterContext)
        //{
            //filterContext.ExceptionHandled = true;

            // Redirect on error:
            //filterContext.Result = RedirectToAction("Index", "Error");

            // OR set the result without redirection:
            //filterContext.Result = new ViewResult
            //{
            //    ViewName = "~/Views/Error/Index.cshtml"
            //};
        //}
    }
}
