using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    //Each ticket has a list of TicketHistory objects associated with it describing changes to the ticket
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string PropertyChanged { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTimeOffset ChangeDate { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
    }
}