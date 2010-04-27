//
// EventRecurrence.cs:
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
	/// <summary>
	/// This class describes a recurrence pattern of an Event
	/// </summary>
	/// <remarks>
	/// Start and end date patterns are specified without time; the time is
	/// taken by the event including this recurrence pattern.
	/// </remarks>
	public sealed class EventRecurrence {
		#region .ctor
		private EventRecurrence(RecurrenceType type, int interval, short month, short dayOfMonth,
								   RecurrenceDays dayOfWeekMask, short instance, string startDatePattern,
								   string endDatePattern, bool noEndDate) {
			this.type = type;
			this.interval = interval;
			this.month = month;
			this.dayOfMonth = dayOfMonth;
			this.dayOfWeekMask = dayOfWeekMask;
			this.instance = instance;
			this.startDatePattern = startDatePattern;
			this.endDatePattern = endDatePattern;
			this.noEndDate = noEndDate;
		}
		#endregion
		
		#region Fields
		private RecurrenceType type;
		private int interval;
		private short month;
		private short dayOfMonth;
		private RecurrenceDays dayOfWeekMask;
		private short instance;
		private string startDatePattern;
		private bool noEndDate;
		private string endDatePattern;
		#endregion

		#region Properties
		public RecurrenceType Type {
			get { return type; }
		}

		public int Interval {
			get { return interval; }
		}

		public short Month {
			get { return month; }
		}

		public RecurrenceDays DayOfWeekMask {
			get { return dayOfWeekMask; }
		}

		public short DayOfMonth {
			get { return dayOfMonth; }
		}

		public short Instance {
			get { return instance; }
		}

		public string StartDatePattern {
			get { return startDatePattern; }
		}

		public bool IsNoEndDate {
			get { return noEndDate; }
			set { noEndDate = value; }
		}

		public string EndDatePattern {
			get { return endDatePattern; }
		}
		#endregion

		#region Private Static Methods
		private static void ValidateMonth(short month) {
			if (month < 1 && month > 12)
				throw new ArgumentOutOfRangeException("month", month, "Month is outside the range (1, 12)");
		}

		private static void ValidateInstance(int instance) {
			if (instance < 1)
				throw new RecurrencePatternException("instance is not greater than 0: " + instance);
		}

		private static void ValidateInterval(int interval) {
			if (interval < 0)
				throw new RecurrencePatternException("interval is not greater or equals than 0: " + interval);
		}

		private static void ValidateDate(string datePattern) {
			if (datePattern == null || datePattern.Length == 0)
				throw new RecurrencePatternException("datePattern is empty: " + datePattern);
		}

		private static void ValidateDayOfMonth(short dayOfMonth) {
			if (dayOfMonth < 1 && dayOfMonth > 31)
				throw new RecurrencePatternException("dayOfMonth outside range (1, 31): " + dayOfMonth);
		}
		#endregion

		#region Public Static Methods
		/// <summary>
		/// Creates a daily recurrence with start and end dates.
		/// </summary>
		/// <param name="interval">The number of days between two recurrences.</param>
		/// <param name="startDatePattern">The start date pattern.</param>
		/// <param name="endDatePattern">The end date pattern.</param>
		/// <exception cref="RecurrencePatternException">
		/// Thrown in case of error specifying the parameters.
		/// </exception>
		public static EventRecurrence CreateDailyRecurrence(int interval, string startDatePattern, string endDatePattern) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Daily, interval, 0, 0, RecurrenceDays.Unspecified, 0,
			                             startDatePattern, endDatePattern, false);
		}
		
		/// <summary>
		/// Creates a daily recurrence with start, end dates and a flag for indicating
		/// if it has an end date.
		/// </summary>
		/// <param name="interval">The number of days between two recurrences.</param>
		/// <param name="startDatePattern">The start date pattern.</param>
		/// <param name="endDatePattern">The end date pattern.</param>
		/// <param name="noEndDate">The flag indicating whethe the instance created has 
		/// an end date or not.</param>
		/// <exception cref="RecurrencePatternException">
		/// Thrown in case of error specifying the parameters.
		/// </exception>
		public static EventRecurrence CreateDailyRecurrence(int interval, string startDatePattern, string endDatePattern, bool noEndDate) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Daily, interval, 0, 0, RecurrenceDays.Unspecified,
			                             0, startDatePattern, endDatePattern, noEndDate);
		}
		
		/// <summary>
		/// Creates the inifinite daily recurrence.
		/// </summary>
		/// <param name="interval">The number of days between two recurrences.</param>
		/// <param name="startDatePattern">The start date pattern.</param>
		/// <exception cref="RecurrencePatternException">
		/// Thrown in case of error specifying the parameters.
		/// </exception>
		public static EventRecurrence CreateDailyRecurrence(int interval, string startDatePattern) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Daily, interval, 0, 0, RecurrenceDays.Unspecified,
			                             0, startDatePattern, null, false);
		}

		/// <summary>
		/// Creates the inifinite daily recurrence.
		/// </summary>
		/// <param name="interval">The number of days between two recurrences.</param>
		/// <param name="startDatePattern">The start date pattern.</param>
		/// <param name="noEndDate">The flag indicating whethe the instance created has 
		/// an end date or not.</param>
		/// <exception cref="RecurrencePatternException">
		/// Thrown in case of error specifying the parameters.
		/// </exception>
		public static EventRecurrence CreateDailyRecurrence(int interval, string startDatePattern, bool noEndDate) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Daily, interval, 0, 0, RecurrenceDays.Unspecified,
			                             0, startDatePattern, null, noEndDate);
		}

		public static EventRecurrence CreateWeeklyRecurrence(int interval, RecurrenceDays dayOfWeekMask,
		                                                       string startDatePattern, string endDatePattern) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Weekly, interval, 0, 0, dayOfWeekMask, 0,
			                             startDatePattern, endDatePattern, false);
		}

		public static EventRecurrence CreateWeeklyRecurrence(int interval, RecurrenceDays dayOfWeekMask,
		                                                       string startDatePattern, string endDatePattern,
		                                                       bool noEndDate) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Weekly, interval, 0, 0, dayOfWeekMask, 0, 
			                             startDatePattern, endDatePattern, noEndDate);
		}

		public static EventRecurrence CreateWeeklyRecurrence(int interval, RecurrenceDays dayOfWeekMask, string startDatePattern) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Weekly, interval, 0, 0, dayOfWeekMask, 0,
			                             startDatePattern, null, false);
		}

		public static EventRecurrence CreateWeeklyRecurrence(int interval, RecurrenceDays dayOfWeekMask,
		                                                       string startDatePattern, bool noEndDate) {
			ValidateInterval(interval);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Weekly, interval, 0, 0, dayOfWeekMask, 0, 
			                             startDatePattern, null, noEndDate);
		}

		public static EventRecurrence CreateMonthlyRecurrence(int interval, short dayOfMonth,
		                                                        string startDatePattern, string endDatePattern) {
			ValidateInterval(interval);
			ValidateDayOfMonth(dayOfMonth);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Monthly, interval, 0, dayOfMonth, RecurrenceDays.Unspecified, 
			                             0, startDatePattern, endDatePattern, false);
		}

		public static EventRecurrence CreateMonthlyRecurrence(int interval, short dayOfMonth, string startDatePattern,
		                                                        string endDatePattern, bool noEndDate) {
			ValidateInterval(interval);
			ValidateDayOfMonth(dayOfMonth);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Monthly, interval, 0, dayOfMonth, RecurrenceDays.Unspecified,
			                             0, startDatePattern, endDatePattern, noEndDate);
		}

		public static EventRecurrence CreateMonthlyRecurrence(int interval, short dayOfMonth, string startDatePattern) {
			ValidateInterval(interval);
			ValidateDayOfMonth(dayOfMonth);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Monthly, interval, 0, dayOfMonth, RecurrenceDays.Unspecified, 
			                             0, startDatePattern, null, false);
		}

		public static EventRecurrence CreateMonthlyRecurrence(int interval, short dayOfMonth, string startDatePattern,
		                                                        bool noEndDate) {
			ValidateInterval(interval);
			ValidateDayOfMonth(dayOfMonth);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Monthly, interval, 0, dayOfMonth, RecurrenceDays.Unspecified,
			                             0, startDatePattern, null, noEndDate);
		}

		public static EventRecurrence CreateMonthNthRecurrence(int interval, RecurrenceDays dayOfWeekMask, 
		                                                         short instance, string startDatePattern,
		                                                         string endDatePattern) {
			ValidateInterval(interval);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.MonthNth, interval, 0, 0, dayOfWeekMask, instance,
			                             startDatePattern, endDatePattern, false);
		}

		public static EventRecurrence CreateMonthNthRecurrence(int interval, RecurrenceDays dayOfWeekMask,
		                                                         short instance, string startDatePattern, 
		                                                         string endDatePattern, bool noEndDate) {
			ValidateInterval(interval);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.MonthNth, interval, 0, 0, dayOfWeekMask, instance,
			                             startDatePattern, endDatePattern, noEndDate);
		}

		public static EventRecurrence CreateMonthNthRecurrence(int interval, RecurrenceDays dayOfWeekMask,
		                                                         short instance, string startDatePattern) {
			ValidateInterval(interval);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.MonthNth, interval, 0, 0, dayOfWeekMask, instance,
			                             startDatePattern, null, false);
		}

		public static EventRecurrence CreateMonthNthRecurrence(int interval, RecurrenceDays dayOfWeekMask,
		                                                         short instance, string startDatePattern,
		                                                         bool noEndDate) {
			ValidateInterval(interval);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.MonthNth, interval, 0, 0, dayOfWeekMask, instance,
			                             startDatePattern, null, noEndDate);
		}

		public static EventRecurrence CreateYearlyRecurrence(short dayOfMonth, short month, string startDatePattern,
		                                                       string endDatePattern, bool noEndDate) {
			ValidateDayOfMonth(dayOfMonth);
			ValidateMonth(month);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Yearly, 0, month, dayOfMonth, RecurrenceDays.Unspecified,
			                             0, startDatePattern, endDatePattern, noEndDate);
		}

		public static EventRecurrence CreateYearlyRecurrence(short dayOfMonth, short month, string startDatePattern,
		                                                       string endDatePattern) {
			ValidateDayOfMonth(dayOfMonth);
			ValidateMonth(month);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.Yearly, 0, month, dayOfMonth, 
			                             RecurrenceDays.Unspecified, 0, startDatePattern, endDatePattern,
			                             false);
		}

		public static EventRecurrence CreateYearlyRecurrence(short dayOfMonth, short month, string startDatePattern) {
			ValidateDayOfMonth(dayOfMonth);
			ValidateMonth(month);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Yearly, 0, month, dayOfMonth, 
			                             RecurrenceDays.Unspecified, 0, startDatePattern, null, false);
		}

		public static EventRecurrence CreateYearlyRecurrence(short dayOfMonth, short month, 
		                                                       string startDatePattern, bool noEndDate) {
			ValidateDayOfMonth(dayOfMonth);
			ValidateMonth(month);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.Yearly, 0, month, dayOfMonth, RecurrenceDays.Unspecified,
			                             0, startDatePattern, null, noEndDate);
		}

		public static EventRecurrence CreateYearNthRecurrence(RecurrenceDays dayOfWeekMask, short month, 
		                                                        short instance, string startDatePattern, 
		                                                        string endDatePattern, bool noEndDate) {
			ValidateMonth(month);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.YearNth, 0, month, 0, dayOfWeekMask, instance,
			                             startDatePattern, endDatePattern, noEndDate);
		}

		public static EventRecurrence CreateYearNthRecurrence(RecurrenceDays dayOfWeekMask, short month,
		                                                        short instance, string startDatePattern, string endDatePattern) {
			ValidateMonth(month);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);
			ValidateDate(endDatePattern);

			return new EventRecurrence(RecurrenceType.YearNth, 0, month, 0, dayOfWeekMask, instance,
			                             startDatePattern, endDatePattern, false);
		}

		public static EventRecurrence CreateYearNthRecurrence(RecurrenceDays dayOfWeekMask, short month,
		                                                        short instance, string startDatePattern) {
			ValidateMonth(month);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.YearNth, 0, month, 0, dayOfWeekMask, instance,
			                             startDatePattern, null, false);
		}

		public static EventRecurrence CreateYearNthRecurrence(RecurrenceDays dayOfWeekMask, short month,
		                                                        short instance, string startDatePattern, bool noEndDate) {
			ValidateMonth(month);
			ValidateInstance(instance);
			ValidateDate(startDatePattern);

			return new EventRecurrence(RecurrenceType.YearNth, 0, month, 0, dayOfWeekMask, instance,
			                             startDatePattern, null, noEndDate);
		}
		#endregion
	}
}