using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketTimeDifference
    {
        public int TicketId { get; set; }
        public TimeSpan TimeDifferenceFromNow { get; set; }
    }
}