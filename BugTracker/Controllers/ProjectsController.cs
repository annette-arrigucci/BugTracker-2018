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
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        [Authorize]
        public ActionResult Index()
        {
            //perform a join on the Projects table with the ProjectUsers lookup table
            //to retrieve all the projects a user is associated with
            var id = User.Identity.GetUserId();
            if (User.IsInRole("Admin"))
            {
                var myList = db.Projects.Where(x => x.IsActive == true).ToList();
                //reverse the list so most recent projects on top of list
                return View(quickReverse(myList));
            }
            else
            {
                var projects = db.Projects.Where(x => x.ProjectUsers.Any(y => y.UserId == id));
                var activeProjects = projects.Where(x => x.IsActive == true).ToList();
                return View(quickReverse(activeProjects.ToList()));
            }
        }

        public List<Project> quickReverse(List<Project> oldList)
        {
            oldList.Reverse();
            return oldList;
        }

        // GET: Projects/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }
            if (!User.IsInRole("Admin"))
            {
                var helper = new ProjectUserHelper();
                var userId = User.Identity.GetUserId();

                if (!helper.IsUserInProject(userId, (int)id))
                {
                    return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                }
            }
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.IsActive = true;
                db.Projects.Add(project);
                db.SaveChanges();

                ProjectUser projUser = new ProjectUser();
                projUser.UserId = User.Identity.GetUserId();
                projUser.ProjectId = project.Id;

                db.ProjectUsers.Add(projUser);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }
            else
            {
                //if the user is not an admin, check that this is one of the PM's assigned projects
                if (!User.IsInRole("Admin"))
                {
                    var helper = new ProjectUserHelper();
                    var userId = User.Identity.GetUserId();

                    if (!helper.IsUserInProject(userId, (int)id))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,IsActive")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Archive(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }           
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }
            else
            {
                //if the user is not an admin, check that this is one of the PM's assigned projects
                if (!User.IsInRole("Admin"))
                {               
                    var helper = new ProjectUserHelper();
                    var userId = User.Identity.GetUserId();
                    //if PM isn't assigned to project, return a bad request
                    if (!helper.IsUserInProject(userId, (int)id))
                    {
                        return RedirectToAction("Index", "Error", new { errorMessage = "Not Authorized" });
                    }
                }
            }
            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ArchiveConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            project.IsActive = false;
            db.Entry(project).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult ArchivedProjects()
        {
            
            var id = User.Identity.GetUserId();
            //Admin can view all inactive projects
            if (User.IsInRole("Admin"))
            {
                var myList = db.Projects.Where(x => x.IsActive == false).ToList();
                return View(quickReverse(myList));
            }
            else
            //perform a join on the Projects table with the ProjectUsers lookup table
            //to retrieve the inactive projects a user is associated with
            {
                var projects = db.Projects.Where(x => x.ProjectUsers.Any(y => y.UserId == id));
                var myArchivedProjects = projects.Where(x => x.IsActive == false).ToList();
                return View(quickReverse(myArchivedProjects.ToList()));
            }
        }

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
