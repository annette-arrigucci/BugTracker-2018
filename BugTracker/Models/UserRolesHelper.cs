using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class UserRolesHelper
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public UserRolesHelper()
        {
            this.db = new ApplicationDbContext();
            this.userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            this.roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }

        public bool IsUserInRole(string userId, string roleName)
        {
            return userManager.IsInRole(userId, roleName);
        }
        public IList<string> ListUserRoles(string userId)
        {
            return userManager.GetRoles(userId);
        }
        public bool AddUserToRole(string userId, string roleName)
        {
            var result = userManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }
        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = userManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }
        public IList<ApplicationUser> UsersInRole(string roleName)
        {
            var userIds = roleManager.FindByName(roleName).Users.Select(r => r.UserId);
            return userManager.Users.Where(u => userIds.Contains(u.Id)).ToList();
        }
        public IList<ApplicationUser> UsersNotInRole(string roleName)
        {
            var userIds = System.Web.Security.Roles.GetUsersInRole(roleName);
            return userManager.Users.Where(u => !userIds.Contains(u.Id)).ToList();
        }
        public string GetUserName (string uId)
        {
            var appUser = db.Users.Find(uId);
            return appUser.FirstName + " " + appUser.LastName;
        }
    }
}