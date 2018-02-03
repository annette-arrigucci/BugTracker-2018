using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class ProjectUserViewController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ProjectUserView
        [Authorize(Roles ="Admin, Project Manager")]
        public ActionResult Edit(int? projectId)
        {
            if (projectId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(projectId);
            var helper = new ProjectUserHelper();

            if (project == null)
            {
                return HttpNotFound();
            }
            else
            {
                //if the user is not an admin, check that this is one of the PM's assigned projects
                if (!User.IsInRole("Admin"))
                {                 
                    var userId = User.Identity.GetUserId();
                    if (!helper.IsUserInProject(userId, (int)projectId))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
            }
            var projectModel = new ProjectUserViewModel();
            // make sure project exists
            projectModel.ProjectId = project.Id;
            projectModel.ProjectName = project.Name;
            //get the user Ids that are associated with the project
            var userIdList = helper.UsersInProject((int) projectId);
            var userInfoList = helper.getUserInfo(userIdList);
            projectModel.UsersAssignedtoProject = new MultiSelectList(userInfoList, "UserId", "UserName");

            //get the user Ids not associated with the project
            var nonUserIdList = helper.UsersNotInProject((int) projectId);
            var nonUserInfoList = helper.getUserInfo(nonUserIdList);
            projectModel.UsersNotAssignedToProject = new MultiSelectList(nonUserInfoList, "UserId", "UserName");
            return View(projectModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUsers(int pId, string[] usersToAdd)
        {
            var helper = new ProjectUserHelper();
            
            if (usersToAdd == null)
            {
                return RedirectToAction("Edit", new { projectId = pId });
            }
            else
            {
                for (var i = 0; i <usersToAdd.Length; i++)
                {
                    if (!helper.IsUserInProject(usersToAdd[i], pId))
                    {
                        helper.AddUserToProject(usersToAdd[i], pId);
                    }
                }
                return RedirectToAction("Edit", new { projectId = pId });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUsers(int pId, string[] usersToRemove)
        {
            var helper = new ProjectUserHelper();
            
            if (usersToRemove == null)
            {
                return RedirectToAction("Edit", new { projectId = pId });
            }
            else
            {
                for (var i = 0; i < usersToRemove.Length; i++)
                {
                    if (helper.IsUserInProject(usersToRemove[i], pId))
                    {
                        helper.RemoveUserFromProject(usersToRemove[i], pId);
                    }
                }
                return RedirectToAction("Edit", new { projectId = pId });
            }
        }

        
    }
}