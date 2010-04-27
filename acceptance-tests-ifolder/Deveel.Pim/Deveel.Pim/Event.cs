//
// Event.cs:
//
// Author:
//	Antonello Provenzano (antonello@deveel.com)
//
// (C) 2006, Deveel srl, (http://deveel.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;

namespace Deveel.Pim {
	public class Event {
		#region .ctor
		public Event() {
			categories = new Property();
			classEvent = new Property();
			description = new Property();
			latitude = new Property();
			longitude = new Property();
			location = new Property();
			lastModified = new Property();
			priority = new Property();
			dtStamp = new Property();
			sequence = new Property();
			status = new Property();
			summary = new Property();
			uid = new Property();
			url = new Property();
			dtEnd = new Property();
			dtStart = new Property();
			replyTime = new Property();
			duration = new Property();
			transp = new Property();
			organizer = new Property();
			rrule = new Property();
			contact = new Property();
			created = new Property();
			dalarm = new Property();
			palarm = new Property();

			xTags = new XTagCollection();

			recurrence = null;
			reminder = null;
		}
		#endregion

		#region Fields
		private Property dalarm;
		private Property palarm;
		private Property categories;
		private Property classEvent;
		private Property description;
		private Property latitude;
		private Property longitude;
		private Property location;
		private Property priority;
		private Property status;
		private Property summary;
		private Property dtEnd;
		private Property dtStart;
		private Property replyTime;
		private Property duration;
		private Property transp;
		private Property organizer;
		private Property url;
		private Property uid;
		private Property rrule;
		private Property contact;
		private Property created;
		private Property dtStamp;
		private Property lastModified;
		private Property sequence;
		private XTagCollection xTags;

		// Because these properties are not part of the iCal specifications,
		// we do not need to use the Property object to store them...
		private bool allDay;
		private MeetingStatus meetingStatus;
		private BusyStatus busyStatus;
		private int mileage;
		private Reminder reminder;

		private EventRecurrence recurrence;  // null if the event is not recurrent
		#endregion

		#region Properties
		/// <summary>
		/// Returns the access classification for a calendar component
		/// </summary>
		public Property ClassEvent {
			get { return classEvent; }
			set { classEvent = value; }
		}

		/// <summary>
		/// Returns the date and time that the calendar information was created.
		/// </summary>
		public Property Created {
			get { return created; }
			set { created = value; }
		}

		/// <summary>
		/// Returns the more complete description of the calendar component.
		/// </summary>
		public Property Description {
			get { return description; }
			set { description = value; }
		}

		/// <summary>
		/// Returns the start date and time for the event
		/// </summary>
		public Property DateStart {
			get { return dtStart; }
			set { dtStart = value; }
		}

		/// <summary>
		/// Returns the latitude
		/// </summary>
		public Property Latitude {
			get { return latitude; }
			set { latitude = value; }
		}

		/// <summary>
		/// Returns the longitude
		/// </summary>
		public Property Longitude {
			get { return longitude; }
			set { longitude = value; }
		}

		/// <summary>
		/// Returns the date and time of the last revised for the event
		/// </summary>
		public Property LastModified {
			get { return lastModified; }
			set { lastModified = value; }
		}

		/// <summary>
		/// Returns the location of this event
		/// </summary>
		public Property Location {
			get { return location; }
			set { location = value; }
		}

		/// <summary>
		/// Returns the organizer for the event
		/// </summary>
		public Property Organizer {
			get { return organizer; }
			set { organizer = value; }
		}

		/// <summary>
		/// Returns the relative priority for the event
		/// </summary>
		public Property Priority {
			get { return priority; }
			set { priority = value; }
		}

		/// <summary>
		/// Returns the date and time that the instance of the iCal object was created.
		/// </summary>
		public Property DateStamp {
			get { return dtStamp; }
			set { dtStamp = value; }
		}

		/// <summary>
		/// Returns the revision sequence number
		/// </summary>
		public Property Sequence {
			get { return sequence; }
			set { sequence = value; }
		}

		/// <summary>
		/// Returns the status of the event
		/// </summary>
		public Property Status {
			get { return status; }
			set { status = value; }
		}

		/// <summary>
		/// Returns the uid of this event
		/// </summary>
		public Property UID {
			get { return uid; }
			set { uid = value; }
		}

		/// <summary>
		/// Returns the url for the event
		/// </summary>
		public Property Url {
			get { return url; }
			set { url = value; }
		}

		/// <summary>
		/// Returns the end date and time for the event
		/// </summary>
		public Property DateEnd {
			get { return dtEnd; }
			set { dtEnd = value; }
		}

		/// <summary>
		/// Returns the duration for the event
		/// </summary>
		public Property Duration {
			get { return duration; }
			set { duration = value; }
		}

		public Property Summary {
			get { return summary; }
			set { summary = value; }
		}

		/// <summary>
		/// Returns the transport for a calendar component
		/// </summary>
		public Property Transport {
			get { return transp; }
			set { transp = value; }
		}

		/// <summary>
		/// Returns the categories for the event
		/// </summary>
		public Property Categories {
			get { return categories; }
			set { categories = value; }
		}

		/// <summary>
		/// Returns the contact information or alternately a reference to contact
		/// information associated with the calendar component
		/// </summary>
		public Property Contact {
			get { return contact; }
			set { contact = value; }
		}

		/// <summary>
		/// Returns the rule for recurring event
		/// </summary>
		public Property Rrule {
			get { return rrule; }
			set { rrule = value; }
		}

		public XTagCollection XTags {
			get { return xTags; }
		}

		/// <summary>
		/// Returns the dalarm property (DALARM = Display Reminder)
		/// </summary>
		public Property DisplayAlarm {
			get { return dalarm; }
			set { dalarm = value; }
		}

		/// <summary>
		/// Returns the palarm property (PALARM = Procedure Reminder)
		/// </summary>
		public Property ProcedureAlarm {
			get { return palarm; }
			set { palarm = value; }
		}

		public bool IsAllDay {
			get { return allDay; }
			set { allDay = value; }
		}

		public MeetingStatus MeetingStatus {
			get { return meetingStatus; }
			set { meetingStatus = value; }
		}

		public BusyStatus BusyStatus {
			get { return busyStatus; }
			set { busyStatus = value; }
		}

		public int Mileage {
			get { return mileage; }
			set { mileage = value; }
		}

		public Property ReplyTime {
			get { return replyTime; }
			set { replyTime = value; }
		}

		public EventRecurrence Recurrence {
			get { return recurrence; }
			set { recurrence = value; }
		}

		/// <summary>
		/// Is this event a recurrent event?
		/// </summary>
		public bool IsRecurrent {
			get {
				if (recurrence != null)
					return true;
				return false;
			}
		}

		/// <summary>
		/// Makes this event not recurrent.
		/// </summary>
		public void RemoveRecurrence() {
			recurrence = null;
		}

		public Reminder Reminder {
			get { return reminder; }
			set { reminder = value; }
		}
		#endregion
	}
}