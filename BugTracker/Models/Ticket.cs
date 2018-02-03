using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketPriorityId { get; set; }
        public int TicketStatusId { get; set; }
        public string OwnerUserId { get; set; }
        public string AssignedToUserId { get; set; }

        public Ticket()
        {
            this.TicketAttachments = new HashSet<TicketAttachment>();
            this.TicketComments = new HashSet<TicketComment>();
            this.TicketHistories = new HashSet<TicketHistory>();
            this.TicketNotifications = new HashSet<TicketNotification>();
        }

        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; }
        public virtual ICollection<TicketComment> TicketComments { get; set; }
        public virtual ICollection<TicketHistory> TicketHistories { get; set; }
        public virtual ICollection<TicketNotification> TicketNotifications { get; set; }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to TicketDetailsViewModel return false.
            Ticket p = obj as Ticket;
            if ((System.Object)p == null)
            {
                return false;
            }

            //Check whether the products' properties are equal. 
            return Id.Equals(p.Id) && Title.Equals(p.Title);
        }

        public bool Equals(Ticket p)
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