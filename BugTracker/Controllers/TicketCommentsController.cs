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

namespace BugTracker.Controllers
{
    public class TicketCommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //not using many of the scaffolded functions in this version of BugTracker
        //commenting out the code instead of deleting it in case I need to add features later

        // GET: TicketComments
        //[Authorize(Roles ="Admin")]
        //public ActionResult Index()
        //{
        //    return View(db.TicketComments.ToList());
        //}

        // GET: TicketComments/Details/5
        //[Authorize(Roles = "Admin")]
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketComment ticketComment = db.TicketComments.Find(id);
        //    if (ticketComment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticketComment);
        //}

        // GET: TicketComments/Create
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        public ActionResult Create(int ticketId)
        {
            //Make sure the user is authorized to comment on this ticket
            var helper = new ProjectUserHelper();
            var userId = User.Identity.GetUserId();
            var ticket = db.Tickets.Find(ticketId);

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
                else
                {
                    return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });

                }
            }

            var model = new TicketComment();
            model.TicketId = ticketId;
            ViewBag.ticketTitle = ticket.Title;
            return View(model);
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Comment,TicketId")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                var user = User.Identity.GetUserId();
                ticketComment.Created = DateTimeOffset.Now;
                ticketComment.UserId = user;
                var ticket = db.Tickets.Find(ticketComment.TicketId);

                //if user adding the comment is not the developer assigned to the ticket, 
                //create a ticket notification and send an email to the developer
                if (!user.Equals(ticket.AssignedToUserId))
                {
                    var commentNotification = new TicketNotification(ticket.Id, ticket.AssignedToUserId, "Comment");
                    commentNotification.AddTicketNotification();
                    commentNotification.SendNotificationEmail();
                }

                db.TicketComments.Add(ticketComment);
                db.SaveChanges();
                return RedirectToAction("Details","Tickets", new { id = ticketComment.TicketId });
            }
            return View(ticketComment);
        }

        // GET: TicketComments/Edit/5
        //[Authorize(Roles = "Admin")]
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketComment ticketComment = db.TicketComments.Find(id);
        //    if (ticketComment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticketComment);
        //}

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(ticketComment).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(ticketComment);
        //}

        // GET: TicketComments/Delete/5
        //[Authorize(Roles = "Admin")]
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketComment ticketComment = db.TicketComments.Find(id);
        //    if (ticketComment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticketComment);
        //}

        // POST: TicketComments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    TicketComment ticketComment = db.TicketComments.Find(id);
        //    db.TicketComments.Remove(ticketComment);
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
    }
}
