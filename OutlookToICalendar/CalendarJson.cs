using System;
using System.Collections.Generic;

namespace OutlookToICalendar
{
    public class Value
    {
        public string subject { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime startWithTimeZone { get; set; }
        public DateTime endWithTimeZone { get; set; }
        public string body { get; set; }
        public bool isHtml { get; set; }
        public string responseType { get; set; }
        public DateTime responseTime { get; set; }
        public string id { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string organizer { get; set; }
        public string timeZone { get; set; }
        public object seriesMasterId { get; set; }
        public string iCalUId { get; set; }
        public List<string> categories { get; set; }
        public string webLink { get; set; }
        public string requiredAttendees { get; set; }
        public string optionalAttendees { get; set; }
        public string resourceAttendees { get; set; }
        public string location { get; set; }
        public string importance { get; set; }
        public bool isAllDay { get; set; }
        public string recurrence { get; set; }
        public object recurrenceEnd { get; set; }
        public object numberOfOccurences { get; set; }
        public int reminderMinutesBeforeStart { get; set; }
        public bool isReminderOn { get; set; }
        public string showAs { get; set; }
        public bool responseRequested { get; set; }
        public string sensitivity { get; set; }
    }

    public class Root
    {
        public List<Value> value { get; set; }
    }
}