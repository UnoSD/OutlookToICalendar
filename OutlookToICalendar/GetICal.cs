using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

// https://functionurl?response=accepted;organizer&notCategory=Webinar;Learning;Lunch%2Fpersonal

namespace OutlookToICalendar
{
    public static class GetICal
    {
        // ReSharper disable once UnusedMember.Local
        static CalendarEvent WithAttendeeFrom(this CalendarEvent ce, Value x)
        {
            ce.Attendees = x.requiredAttendees
                            .Split(';')
                            .Where(a => !string.IsNullOrWhiteSpace(a))
                            .Select(a =>
                             {
                                 try
                                 {
                                     return new Attendee(a);
                                 }
                                 catch
                                 {
                                     return null;
                                 }
                             })
                            .Concat(x.optionalAttendees
                                     .Split(';')
                                     .Where(a => !string.IsNullOrWhiteSpace(a))
                                     .Select(a =>
                                      {
                                          try
                                          {
                                              return new Attendee(a) { Role = "OPT-PARTICIPANT" };
                                          }
                                          catch
                                          {
                                              return null;
                                          }
                                      })
                                     .ToList())
                            .Where(a => a != null)
                            .ToList();

            return ce;
        }

        static List<string> GetQueryValues(this HttpRequest req, string key) =>
            req.Query[key]
               .ToString()
               .Split(';')
               .Where(x => !string.IsNullOrWhiteSpace(x))
               .ToList();

        [FunctionName("GetICal")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            // notResponded, tentativelyAccepted, organizer, accepted, none
            var responseFilter = req.GetQueryValues("response");
            var notResponseFilter = req.GetQueryValues("notResponse");
            
            var categoryFilter = req.GetQueryValues("category");
            var notCategoryFilter = req.GetQueryValues("notCategory");

            if (responseFilter.Any() && notResponseFilter.Any())
                throw new ArgumentException("Cannot filter both on include and exclude response");
            
            if (categoryFilter.Any() && notCategoryFilter.Any())
                throw new ArgumentException("Cannot filter both on include and exclude response");
            
            bool Filter(Value x) =>
                (!responseFilter.Any()    || responseFilter.Contains(x.responseType))      &&
                (!notResponseFilter.Any() || !responseFilter.Contains(x.responseType))     &&
                (!categoryFilter.Any()    || categoryFilter.Intersect(x.categories).Any()) &&
                (!notCategoryFilter.Any() || !categoryFilter.Intersect(x.categories).Any());
            
            var calendar = new Calendar();
            
            calendar.Events
                    .AddRange(JsonConvert.DeserializeObject<Root>(body).value
                                         .AsParallel()
                                         .Where(Filter)
                                         .Select(x => new CalendarEvent
                                          {
                                              Summary      = x.subject,
                                              DtStart      = new CalDateTime(x.startWithTimeZone),
                                              DtEnd        = new CalDateTime(x.endWithTimeZone),
                                              Description  = x.body,
                                              Created      = new CalDateTime(x.createdDateTime),
                                              LastModified = new CalDateTime(x.lastModifiedDateTime),
                                              Organizer    = new Organizer(x.organizer),
                                              Categories   = x.categories,
                                              Location     = x.location,
                                              IsAllDay     = x.isAllDay,
                                              Uid          = x.iCalUId
                                          }/*.WithAttendeeFrom(x)*/));
            
            return new OkObjectResult(new CalendarSerializer().SerializeToString(calendar));
        }
    }
}
