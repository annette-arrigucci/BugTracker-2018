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
    public class AdminUserViewController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AdminUserView
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var userRoles = new List<AdminUserViewModel>();
            foreach (ApplicationUser user in db.Users)
            {
                var myUser = new AdminUserViewModel();
                UserRolesHelper helper = new UserRolesHelper();
                myUser.UserId = user.Id;
                myUser.UserName = user.FirstName + " " + user.LastName;
                myUser.FirstName = user.FirstName;
                myUser.LastName = user.LastName;
                myUser.UserEmail = user.Email;
                myUser.CurrentRoles = helper.ListUserRoles(user.Id);
                userRoles.Add(myUser);
            }
            var sortedUsers = userRoles.OrderBy(x => x.LastName).ThenBy(p => p.FirstName).ToList();
            return View(sortedUsers);
        }
        //get method
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            var user = db.Users.Find(id);
            AdminUserViewModel AdminModel = new AdminUserViewModel();
            UserRolesHelper helper = new UserRolesHelper();

            //AdminModel.Roles = new MultiSelectList(db.Roles, "Name", "Name", selected);
            var allRoles = new List<string>();

            allRoles.Add("Submitter");
            allRoles.Add("Developer");
            allRoles.Add("Project Manager");
            allRoles.Add("Admin");

            AdminModel.RolesToSelect = new RoleCheckBox[4];
            //build an array that has all user roles and whether the user is in that role
            //this will be sent to the view
            int i = 0;
            foreach (var role in allRoles)
            {
                var checkBox = new RoleCheckBox();
                checkBox.RoleName = role;
                if (helper.IsUserInRole(id, role))
                {
                    checkBox.Checked = true;
                }
                else
                {
                    checkBox.Checked = false;
                }
                AdminModel.RolesToSelect[i] = checkBox;
                i++;
            }
            AdminModel.UserId = user.Id;
            AdminModel.UserName = user.FirstName + " " + user.LastName;
            AdminModel.UserEmail = user.Email;

            return View(AdminModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,RolesToSelect")] AdminUserViewModel admModel)
        {
            var user = db.Users.Find(admModel.UserId);
            var id = admModel.UserId;
            var allRoles = new List<string>();
            UserRolesHelper helper = new UserRolesHelper();

            allRoles.Add("Submitter");
            allRoles.Add("Developer");
            allRoles.Add("Project Manager");
            allRoles.Add("Admin");

            //build a list of selected roles based on the array in the model that was returned
            var selectedRoles = new List<string>();
            for (int i = 0; i < admModel.RolesToSelect.Length; i++)
            {
                if (admModel.RolesToSelect[i].Checked == true)
                {
                    selectedRoles.Add(admModel.RolesToSelect[i].RoleName);
                }
            }

            //if no roles have been selected, remove user from all roles
            if (selectedRoles == null)
            {
                foreach (var rRole in allRoles)
                {
                    if (helper.IsUserInRole(admModel.UserId, rRole))
                    {
                        helper.RemoveUserFromRole(admModel.UserId, rRole);
                    }
                }
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var sRole in selectedRoles)
                {
                    if (!helper.IsUserInRole(admModel.UserId, sRole))
                    {
                        helper.AddUserToRole(admModel.UserId, sRole);
                    }
                }

                var rolesToRemove = allRoles.Except(selectedRoles);
                foreach (var rRole in rolesToRemove)
                {
                    if (helper.IsUserInRole(admModel.UserId, rRole))
                    {
                        helper.RemoveUserFromRole(admModel.UserId, rRole);
                    }
                }
                return RedirectToAction("Index");
            }
        }
    }
}