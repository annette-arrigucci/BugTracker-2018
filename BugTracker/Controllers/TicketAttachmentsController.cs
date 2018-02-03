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
using System.IO;

namespace BugTracker.Controllers
{
    public class TicketAttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //not using many of the scaffolded functions in this version of BugTracker
        //commenting out the code instead of deleting it in case I need to add features later

        //// GET: TicketAttachments
        //[Authorize(Roles = "Admin")]
        //public ActionResult Index()
        //{
        //    return View(db.TicketAttachments.ToList());
        //}

        //not using this

        // GET: TicketAttachments/Details/5
        //[Authorize(Roles = "Admin")]
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
        //    if (ticketAttachment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticketAttachment);
        //}

        // GET: TicketAttachments/Create
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        public ActionResult Create(int ticketId)
        {
            //Make sure the user is authorized to add an attachment on this ticket
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
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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

            var model = new TicketAttachment();
            model.TicketId = ticketId;
            ViewBag.ticketTitle = ticket.Title;
            return View(model);
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,Description")] TicketAttachment ticketAttachment, HttpPostedFileBase document)
        {
            if (document != null && document.ContentLength > 0)
            {  //check the file name to make sure it's a file that we want                 
                var ext = Path.GetExtension(document.FileName).ToLower();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".bmp" && ext != ".doc" && ext != ".docx" && ext != ".pdf")
                {
                    ModelState.AddModelError("document", "Invalid Format.");
                }            
            }
            if (ModelState.IsValid)
            {
                var user = User.Identity.GetUserId();
                ticketAttachment.FileName = document.FileName;
                ticketAttachment.Created = DateTimeOffset.Now;
                ticketAttachment.UserId = user;
                var ticket = db.Tickets.Find(ticketAttachment.TicketId);

                if (document != null)
                {
                    var filePath = "/Uploads/";      //relative server path  - to where my machine is                   
                    var absPath = Server.MapPath("~" + filePath);    // path on physical drive on server                      
                    ticketAttachment.FileUrl = filePath + document.FileName;    // media url for relative path                         
                    document.SaveAs(Path.Combine(absPath, document.FileName)); //save image
                }

                //if user adding the attachment is not the developer assigned to the ticket, 
                //create a ticket notification and send an email to the developer
                if (!user.Equals(ticket.AssignedToUserId))
                {
                    var attachNotification = new TicketNotification(ticket.Id, ticket.AssignedToUserId, "Attachment");
                    attachNotification.AddTicketNotification();
                    attachNotification.SendNotificationEmail();
                }

                //add the attachment
                db.TicketAttachments.Add(ticketAttachment);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
            }
            return View(ticketAttachment);
        }

        //not using this

        // GET: TicketAttachments/Edit/5
        //[Authorize(Roles = "Admin")]
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
        //    if (ticketAttachment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticketAttachment);
        //}

        //not using this

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,TicketId,FilePath,Description,Created,UserId,FileUrl")] TicketAttachment ticketAttachment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(ticketAttachment).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(ticketAttachment);
        //}

        //not using this

        // GET: TicketAttachments/Delete/5
        //[Authorize(Roles = "Admin")]
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
        //    if (ticketAttachment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticketAttachment);
        //}

        // POST: TicketAttachments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
        //    db.TicketAttachments.Remove(ticketAttachment);
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
