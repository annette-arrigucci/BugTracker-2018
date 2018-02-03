using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BugTracker.Models
{
    //a TicketNotification object is created whenever a notification email is sent to a developer
    //has methods to add the notification to the database and send the email
    public class TicketNotification
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public string NotificationType { get; set; }

        public TicketNotification(int tId, string uId, string nType)
        {
            this.TicketId = tId;
            this.UserId = uId;
            this.NotificationType = nType;
        }

        public void AddTicketNotification()
        {
            var db = new ApplicationDbContext();
            db.TicketNotifications.Add(this);
            db.SaveChanges();
        }

        //Email a developer a notification
        public async Task SendNotificationEmail()
        {
            var db = new ApplicationDbContext();
            var callbackUrl = "http://aarrigucci-bugtracker.azurewebsites.net/Tickets/Details/" + TicketId.ToString();
            ApplicationUser user = db.Users.Find(UserId);
            var ticket = db.Tickets.Find(TicketId);
            var es = new EmailService();
            string body = "";
            string subject = "";

            //depending on the type of notification, set a different subject line and body for the email
            switch (NotificationType)
            {
                case "Assign":      subject = "Ticket assigned - " + ticket.Title;
                                    body = "You have been assigned a new ticket in the Bug Tracker. Click <a href=\"" + callbackUrl + "\">here</a> to view ticket details.";
                                    break;
                case "Reassign":    subject = "Ticket reassigned - " + ticket.Title;
                                    body = "The ticket \""+ ticket.Title + "\" has been reassigned in the Bug Tracker. View your updated tickets list <a href=\"http://aarrigucci-bugtracker.azurewebsites.net/Tickets\">here</a>.";
                                    break;
                case "Edited":      subject = "Ticket edited - " + ticket.Title;
                                    body = "The ticket \"" + ticket.Title + "\" was edited in the Bug Tracker. Click <a href=\"" + callbackUrl + "\">here</a> to view the updated ticket details.";
                                    break;
                case "Attachment":  subject = "Attachment added - " + ticket.Title;
                                    body = "An attachment was add to the ticket \"" + ticket.Title + "\" in the Bug Tracker. Click <a href=\"" + callbackUrl + "\">here</a> to view the attachment and other ticket details.";
                                    break;
                case "Comment":     subject = "Comment added - " + ticket.Title;
                                    body = "A comment was add to the ticket \"" + ticket.Title + "\" in the Bug Tracker. Click <a href=\"" + callbackUrl + "\">here</a> to view the comment and other ticket details.";
                                    break;
            }
            es.SendAsync(new IdentityMessage
            {
                Destination = user.Email,
                Subject = subject,
                Body = body
            });
        }
    }
}