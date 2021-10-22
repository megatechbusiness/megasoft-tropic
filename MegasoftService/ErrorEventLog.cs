﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegasoftService
{
    class ErrorEventLog
    {
        public static void WriteErrorLog(string EventType, string Message)
        {
            try
            {
                // Create the source and log, if it does not already exist.
                if (!EventLog.SourceExists("Megasoft"))
                {
                    EventLog.CreateEventSource("Megasoft", "MegasoftPostingService");
                }
                // Create an EventLog instance and assign its source.
                EventLog eventLog = new EventLog();
                // Setting the source
                eventLog.Source = "Megasoft";

                // Write an entry to the event log.
                if (EventType == "E")
                {
                    eventLog.WriteEntry(Message, EventLogEntryType.Error);
                }
                else if (EventType == "I")
                {
                    eventLog.WriteEntry(Message, EventLogEntryType.Information);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
