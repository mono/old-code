//
// TimeZone.cs:
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
using System.Collections;
using System.Globalization;

namespace Deveel.Pim {
	public sealed class TimeZone : System.TimeZone, ICloneable {
		#region .ctor
		private TimeZone(int rawOffset, String id) {
			this.rawOffset = rawOffset;
			this.id = id;
			useDaylight = false;
			startYear = 0;
		}

		private TimeZone(int rawOffset, String id, int startMonth,
						int startDayOfWeekInMonth, int startDayOfWeek,
						int startTime, int endMonth, int endDayOfWeekInMonth,
						int endDayOfWeek, int endTime) {
			this.rawOffset = rawOffset;
			this.id = id;
			useDaylight = true;

			SetStartRule(startMonth, startDayOfWeekInMonth, startDayOfWeek, startTime);
			SetEndRule(endMonth, endDayOfWeekInMonth, endDayOfWeek, endTime);
			if (startMonth == endMonth)
				throw new ArgumentException("startMonth and endMonth must be different");

			this.startYear = 0;
		}

		private TimeZone(int rawOffset, String id, int startMonth,
						int startDayOfWeekInMonth, int startDayOfWeek,
						int startTime, int endMonth, int endDayOfWeekInMonth,
						int endDayOfWeek, int endTime, int dstSavings)
			: this(rawOffset, id, startMonth, startDayOfWeekInMonth, startDayOfWeek,
			startTime, endMonth, endDayOfWeekInMonth, endDayOfWeek, endTime) {
			this.dstSavings = dstSavings;
		}

		private TimeZone(int rawOffset, String id, int startMonth,
							int startDayOfWeekInMonth, int startDayOfWeek,
							int startTime, int startTimeMode, int endMonth,
							int endDayOfWeekInMonth, int endDayOfWeek,
							int endTime, int endTimeMode, int dstSavings) {
			this.rawOffset = rawOffset;
			this.id = id;
			useDaylight = true;

			if (startTimeMode < WALL_TIME || startTimeMode > UTC_TIME)
				throw new ArgumentException("startTimeMode must be one of WALL_TIME, STANDARD_TIME, or UTC_TIME");
			if (endTimeMode < WALL_TIME || endTimeMode > UTC_TIME)
				throw new ArgumentException("endTimeMode must be one of WALL_TIME, STANDARD_TIME, or UTC_TIME");
			this.startTimeMode = startTimeMode;
			this.endTimeMode = endTimeMode;

			SetStartRule(startMonth, startDayOfWeekInMonth, startDayOfWeek, startTime);
			SetEndRule(endMonth, endDayOfWeekInMonth, endDayOfWeek, endTime);
			if (startMonth == endMonth)
				throw new ArgumentException("startMonth and endMonth must be different");
			this.startYear = 0;

			this.dstSavings = dstSavings;
		}
		#endregion

		#region Fields
		private int rawOffset;
		private bool useDaylight;
		private int dstSavings = 60 * 60 * 1000;
		private int startYear;
		private int startMode;
		private int startMonth;
		private int startDay;
		private int startDayOfWeek;
		private int startTime;
		private int startTimeMode = WALL_TIME;
		private int endMonth;
		private int endMode;
		private int endDay;
		private int endDayOfWeek;
		private int endTime;
		private int endTimeMode = WALL_TIME;

		private byte[] monthLength = monthArr;
		private static readonly byte[] monthArr = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
		private string id;

		private static Hashtable timezones0;

		private const int STANDARD_TIME = 1;
		private const int WALL_TIME = 0;
		private const int UTC_TIME = 2;

		private const int DOM_MODE = 1;
		private const int DOW_IN_MONTH_MODE = 2;
		private const int DOW_GE_DOM_MODE = 3;
		private const int DOW_LE_DOM_MODE = 4;

		private const int SUNDAY = 1;
		private const int MONDAY = 2;
		private const int TUESDAY = 3;
		private const int WEDNESDAY = 4;
		private const int THURSDAY = 5;
		private const int FRIDAY = 6;
		private const int SATURDAY = 7;

		private const int JANUARY = 0;
		private const int FEBRUARY = 1;
		private const int MARCH = 2;
		private const int APRIL = 3;
		private const int MAY = 4;
		private const int JUNE = 5;
		private const int JULY = 6;
		private const int AUGUST = 7;
		private const int SEPTEMBER = 8;
		private const int OCTOBER = 9;
		private const int NOVEMBER = 10;
		private const int DECEMBER = 11;
		#endregion

		#region Properties
		public override string DaylightName {
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public override string StandardName {
			get { return id; }
		}

		public static TimeZone UTC {
			get { return GetTimeZone("UTC"); }
		}

		public static TimeZone CET {
			get { return GetTimeZone("CET"); }
		}
		#endregion

		#region Private Methods
		private int checkRule(int month, int day, int dayOfWeek) {
			if (month < 0 || month > 11)
				throw new ArgumentException("month out of range");

			int daysInMonth = getDaysInMonth(month, 1);
			if (dayOfWeek == 0) {
				if (day <= 0 || day > daysInMonth)
					throw new ArgumentException("day out of range");
				return DOM_MODE;
			} else if (dayOfWeek > 0) {
				if (Math.Abs(day) > (daysInMonth + 6) / 7)
					throw new ArgumentException("dayOfWeekInMonth out of range");
				if (dayOfWeek > SATURDAY)
					throw new ArgumentException("dayOfWeek out of range");
				return DOW_IN_MONTH_MODE;
			} else {
				if (day == 0 || Math.Abs(day) > daysInMonth)
					throw new ArgumentException("day out of range");
				if (dayOfWeek < -SATURDAY)
					throw new ArgumentException("dayOfWeek out of range");
				if (day < 0)
					return DOW_LE_DOM_MODE;
				return DOW_GE_DOM_MODE;
			}
		}

		private void SetStartRule(int month, int day, int dayOfWeek, int time) {
			this.startMode = checkRule(month, day, dayOfWeek);
			this.startMonth = month;
			this.startDay = day;
			this.startDayOfWeek = Math.Abs(dayOfWeek);
			if (this.startTimeMode == WALL_TIME || this.startTimeMode == STANDARD_TIME)
				this.startTime = time;
			else
				// Convert from UTC to STANDARD
				this.startTime = time + this.rawOffset;
			useDaylight = true;
		}

		private void SetStartRule(int month, int day, int dayOfWeek, int time, bool after) {
			// FIXME: XXX: Validate that checkRule and offset processing work with on
			// or before mode.
			this.startDay = after ? Math.Abs(day) : -Math.Abs(day);
			this.startDayOfWeek = after ? Math.Abs(dayOfWeek) : -Math.Abs(dayOfWeek);
			this.startMode = (dayOfWeek != 0)
							 ? (after ? DOW_GE_DOM_MODE : DOW_LE_DOM_MODE)
							 : checkRule(month, day, dayOfWeek);
			this.startDay = Math.Abs(this.startDay);
			this.startDayOfWeek = Math.Abs(this.startDayOfWeek);

			this.startMonth = month;

			if (this.startTimeMode == WALL_TIME || this.startTimeMode == STANDARD_TIME)
				this.startTime = time;
			else
				// Convert from UTC to STANDARD
				this.startTime = time + this.rawOffset;
			useDaylight = true;
		}

		private void SetStartRule(int month, int day, int time) {
			SetStartRule(month, day, 0, time);
		}

		private void SetEndRule(int month, int day, int dayOfWeek, int time) {
			this.endMode = checkRule(month, day, dayOfWeek);
			this.endMonth = month;
			this.endDay = day;
			this.endDayOfWeek = Math.Abs(dayOfWeek);
			if (this.endTimeMode == WALL_TIME)
				this.endTime = time;
			else if (this.endTimeMode == STANDARD_TIME)
				// Convert from STANDARD to DST
				this.endTime = time + this.dstSavings;
			else
				// Convert from UTC to DST
				this.endTime = time + this.rawOffset + this.dstSavings;
			useDaylight = true;
		}

		private void SetEndRule(int month, int day, int dayOfWeek, int time, bool after) {
			// FIXME: XXX: Validate that checkRule and offset processing work with on
			// or before mode.
			this.endDay = after ? Math.Abs(day) : -Math.Abs(day);
			this.endDayOfWeek = after ? Math.Abs(dayOfWeek) : -Math.Abs(dayOfWeek);
			this.endMode = (dayOfWeek != 0)
						   ? (after ? DOW_GE_DOM_MODE : DOW_LE_DOM_MODE)
						   : checkRule(month, day, dayOfWeek);
			this.endDay = Math.Abs(this.endDay);
			this.endDayOfWeek = Math.Abs(endDayOfWeek);

			this.endMonth = month;

			if (this.endTimeMode == WALL_TIME)
				this.endTime = time;
			else if (this.endTimeMode == STANDARD_TIME)
				// Convert from STANDARD to DST
				this.endTime = time + this.dstSavings;
			else
				// Convert from UTC to DST
				this.endTime = time + this.rawOffset + this.dstSavings;
			useDaylight = true;
		}

		private void SetEndRule(int month, int day, int time) {
			SetEndRule(month, day, 0, time);
		}

		private int getDaysInMonth(int month, int year) {
			if (month == 1) {
				if ((year & 3) != 0)
					return 28;

				// Assume default Gregorian cutover, 
				// all years prior to this must be Julian
				if (year < 1582)
					return 29;

				// Gregorian rules 
				return ((year % 100) != 0 || (year % 400) == 0) ? 29 : 28;
			}

			return monthArr[month];
		}

		private bool isBefore(int calYear, int calMonth, int calDayOfMonth,
						   int calDayOfWeek, int calMillis, int mode,
						   int month, int day, int dayOfWeek, int millis) {
			// This method is called by Calendar, so we mustn't use that class.
			// We have to do all calculations by hand.
			// check the months:
			// XXX - this is not correct:
			// for the DOW_GE_DOM and DOW_LE_DOM modes the change date may
			// be in a different month.
			if (calMonth != month)
				return calMonth < month;

			// check the day:
			switch (mode) {
				case DOM_MODE:
					if (calDayOfMonth != day)
						return calDayOfMonth < day;
					break;
				case DOW_IN_MONTH_MODE: {
						// This computes the day of month of the day of type
						// "dayOfWeek" that lies in the same (sunday based) week as cal.
						calDayOfMonth += (dayOfWeek - calDayOfWeek);

						// Now we convert it to 7 based number (to get a one based offset
						// after dividing by 7).  If we count from the end of the
						// month, we get want a -7 based number counting the days from 
						// the end:
						if (day < 0)
							calDayOfMonth -= getDaysInMonth(calMonth, calYear) + 7;
						else
							calDayOfMonth += 6;

						//  day > 0                    day < 0
						//  S  M  T  W  T  F  S        S  M  T  W  T  F  S
						//     7  8  9 10 11 12         -36-35-34-33-32-31
						// 13 14 15 16 17 18 19      -30-29-28-27-26-25-24
						// 20 21 22 23 24 25 26      -23-22-21-20-19-18-17
						// 27 28 29 30 31 32 33      -16-15-14-13-12-11-10
						// 34 35 36                   -9 -8 -7
						// Now we calculate the day of week in month:
						int week = calDayOfMonth / 7;

						//  day > 0                    day < 0
						//  S  M  T  W  T  F  S        S  M  T  W  T  F  S
						//     1  1  1  1  1  1          -5 -5 -4 -4 -4 -4
						//  1  2  2  2  2  2  2       -4 -4 -4 -3 -3 -3 -3
						//  2  3  3  3  3  3  3       -3 -3 -3 -2 -2 -2 -2
						//  3  4  4  4  4  4  4       -2 -2 -2 -1 -1 -1 -1
						//  4  5  5                   -1 -1 -1
						if (week != day)
							return week < day;

						if (calDayOfWeek != dayOfWeek)
							return calDayOfWeek < dayOfWeek;

						// daylight savings starts/ends  on the given day.
						break;
					}
				case DOW_LE_DOM_MODE:
					// The greatest sunday before or equal December, 12
					// is the same as smallest sunday after or equal December, 6.
					day = Math.Abs(day) - 6;
					goto case DOW_GE_DOM_MODE;
				case DOW_GE_DOM_MODE:
					// Calculate the day of month of the day of type
					// "dayOfWeek" that lies before (or on) the given date.
					calDayOfMonth -= (calDayOfWeek < dayOfWeek ? 7 : 0) + calDayOfWeek
					- dayOfWeek;
					if (calDayOfMonth < day)
						return true;
					if (calDayOfWeek != dayOfWeek || calDayOfMonth >= day + 7)
						return false;

					// now we have the same day
					break;
			}

			// the millis decides:
			return (calMillis < millis);
		}
		#endregion

		#region Private Static Methods
		private static Hashtable timezones() {
			lock (typeof(TimeZone)) {
				if (timezones0 == null) {
					Hashtable timezones = new Hashtable();
					timezones0 = timezones;

					TimeZone tz;
					// Automatically generated by scripts/timezones.pl
					// XXX - Should we read this data from a file?
					tz = new TimeZone(-11000 * 3600, "MIT");
					timezones0.Add("MIT", tz);
					timezones0.Add("Pacific/Apia", tz);
					timezones0.Add("Pacific/Midway", tz);
					timezones0.Add("Pacific/Niue", tz);
					timezones0.Add("Pacific/Pago_Pago", tz);
					tz = new TimeZone(-10000 * 3600, "America/Adak",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("America/Adak", tz);
					tz = new TimeZone(-10000 * 3600, "HST");
					timezones0.Add("HST", tz);
					timezones0.Add("Pacific/Fakaofo", tz);
					timezones0.Add("Pacific/Honolulu", tz);
					timezones0.Add("Pacific/Johnston", tz);
					timezones0.Add("Pacific/Rarotonga", tz);
					timezones0.Add("Pacific/Tahiti", tz);
					tz = new TimeZone(-9500 * 3600, "Pacific/Marquesas");
					timezones0.Add("Pacific/Marquesas", tz);
					tz = new TimeZone
					  (-9000 * 3600, "AST",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("AST", tz);
					timezones0.Add("America/Anchorage", tz);
					timezones0.Add("America/Juneau", tz);
					timezones0.Add("America/Nome", tz);
					timezones0.Add("America/Yakutat", tz);
					tz = new TimeZone(-9000 * 3600, "Pacific/Gambier");
					timezones0.Add("Pacific/Gambier", tz);
					tz = new TimeZone
					  (-8000 * 3600, "PST",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("PST", tz);
					timezones0.Add("PST8PDT", tz);
					timezones0.Add("America/Dawson", tz);
					timezones0.Add("America/Los_Angeles", tz);
					timezones0.Add("America/Tijuana", tz);
					timezones0.Add("America/Vancouver", tz);
					timezones0.Add("America/Whitehorse", tz);
					timezones0.Add("US/Pacific-New", tz);
					tz = new TimeZone(-8000 * 3600, "Pacific/Pitcairn");
					timezones0.Add("Pacific/Pitcairn", tz);
					tz = new TimeZone
					  (-7000 * 3600, "MST",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("MST", tz);
					timezones0.Add("MST7MDT", tz);
					timezones0.Add("America/Boise", tz);
					timezones0.Add("America/Cambridge_Bay", tz);
					timezones0.Add("America/Chihuahua", tz);
					timezones0.Add("America/Denver", tz);
					timezones0.Add("America/Edmonton", tz);
					timezones0.Add("America/Inuvik", tz);
					timezones0.Add("America/Mazatlan", tz);
					timezones0.Add("America/Shiprock", tz);
					timezones0.Add("America/Yellowknife", tz);
					tz = new TimeZone(-7000 * 3600, "MST7");
					timezones0.Add("MST7", tz);
					timezones0.Add("PNT", tz);
					timezones0.Add("America/Dawson_Creek", tz);
					timezones0.Add("America/Hermosillo", tz);
					timezones0.Add("America/Phoenix", tz);
					tz = new TimeZone
					  (-6000 * 3600, "CST",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("CST", tz);
					timezones0.Add("CST6CDT", tz);
					timezones0.Add("America/Cancun", tz);
					timezones0.Add("America/Chicago", tz);
					timezones0.Add("America/Menominee", tz);
					timezones0.Add("America/Merida", tz);
					timezones0.Add("America/Mexico_City", tz);
					timezones0.Add("America/Monterrey", tz);
					timezones0.Add("America/North_Dakota/Center", tz);
					timezones0.Add("America/Rainy_River", tz);
					timezones0.Add("America/Rankin_Inlet", tz);
					tz = new TimeZone(-6000 * 3600, "America/Belize");
					timezones0.Add("America/Belize", tz);
					timezones0.Add("America/Costa_Rica", tz);
					timezones0.Add("America/El_Salvador", tz);
					timezones0.Add("America/Guatemala", tz);
					timezones0.Add("America/Managua", tz);
					timezones0.Add("America/Regina", tz);
					timezones0.Add("America/Swift_Current", tz);
					timezones0.Add("America/Tegucigalpa", tz);
					timezones0.Add("Pacific/Galapagos", tz);
					tz = new TimeZone
					  (-6000 * 3600, "America/Winnipeg",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("America/Winnipeg", tz);
					tz = new TimeZone
					  (-6000 * 3600, "Pacific/Easter",
					   OCTOBER, 2, SATURDAY, 23000 * 3600,
					   MARCH, 2, SATURDAY, 22000 * 3600);
					timezones0.Add("Pacific/Easter", tz);
					tz = new TimeZone
					  (-5000 * 3600, "America/Grand_Turk",
					   APRIL, 1, SUNDAY, 0 * 3600,
					   OCTOBER, -1, SUNDAY, 0 * 3600);
					timezones0.Add("America/Grand_Turk", tz);
					tz = new TimeZone
					  (-5000 * 3600, "America/Havana",
					   APRIL, 1, SUNDAY, 1000 * 3600,
					   OCTOBER, -1, SUNDAY, 1000 * 3600);
					timezones0.Add("America/Havana", tz);
					tz = new TimeZone(-5000 * 3600, "EST5");
					timezones0.Add("EST5", tz);
					timezones0.Add("IET", tz);
					timezones0.Add("America/Bogota", tz);
					timezones0.Add("America/Cayman", tz);
					timezones0.Add("America/Eirunepe", tz);
					timezones0.Add("America/Guayaquil", tz);
					timezones0.Add("America/Indiana/Indianapolis", tz);
					timezones0.Add("America/Indiana/Knox", tz);
					timezones0.Add("America/Indiana/Marengo", tz);
					timezones0.Add("America/Indiana/Vevay", tz);
					timezones0.Add("America/Indianapolis", tz);
					timezones0.Add("America/Jamaica", tz);
					timezones0.Add("America/Lima", tz);
					timezones0.Add("America/Panama", tz);
					timezones0.Add("America/Port-au-Prince", tz);
					timezones0.Add("America/Rio_Branco", tz);
					tz = new TimeZone
					  (-5000 * 3600, "EST",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("EST", tz);
					timezones0.Add("EST5EDT", tz);
					timezones0.Add("America/Detroit", tz);
					timezones0.Add("America/Iqaluit", tz);
					timezones0.Add("America/Kentucky/Louisville", tz);
					timezones0.Add("America/Kentucky/Monticello", tz);
					timezones0.Add("America/Louisville", tz);
					timezones0.Add("America/Montreal", tz);
					timezones0.Add("America/Nassau", tz);
					timezones0.Add("America/New_York", tz);
					timezones0.Add("America/Nipigon", tz);
					timezones0.Add("America/Pangnirtung", tz);
					timezones0.Add("America/Thunder_Bay", tz);
					timezones0.Add("America/Toronto", tz);
					tz = new TimeZone(-4000 * 3600, "PRT");
					timezones0.Add("PRT", tz);
					timezones0.Add("America/Anguilla", tz);
					timezones0.Add("America/Antigua", tz);
					timezones0.Add("America/Aruba", tz);
					timezones0.Add("America/Barbados", tz);
					timezones0.Add("America/Boa_Vista", tz);
					timezones0.Add("America/Caracas", tz);
					timezones0.Add("America/Curacao", tz);
					timezones0.Add("America/Dominica", tz);
					timezones0.Add("America/Grenada", tz);
					timezones0.Add("America/Guadeloupe", tz);
					timezones0.Add("America/Guyana", tz);
					timezones0.Add("America/La_Paz", tz);
					timezones0.Add("America/Manaus", tz);
					timezones0.Add("America/Martinique", tz);
					timezones0.Add("America/Montserrat", tz);
					timezones0.Add("America/Port_of_Spain", tz);
					timezones0.Add("America/Porto_Velho", tz);
					timezones0.Add("America/Puerto_Rico", tz);
					timezones0.Add("America/Santo_Domingo", tz);
					timezones0.Add("America/St_Kitts", tz);
					timezones0.Add("America/St_Lucia", tz);
					timezones0.Add("America/St_Thomas", tz);
					timezones0.Add("America/St_Vincent", tz);
					timezones0.Add("America/Tortola", tz);
					tz = new TimeZone
					  (-4000 * 3600, "America/Asuncion",
					   OCTOBER, 3, SUNDAY, 0 * 3600,
					   MARCH, 2, SUNDAY, 0 * 3600);
					timezones0.Add("America/Asuncion", tz);
					tz = new TimeZone
					  (-4000 * 3600, "America/Campo_Grande",
					   OCTOBER, 3, SUNDAY, 0 * 3600,
					   FEBRUARY, 3, SUNDAY, 0 * 3600);
					timezones0.Add("America/Campo_Grande", tz);
					timezones0.Add("America/Cuiaba", tz);
					tz = new TimeZone
					  (-4000 * 3600, "America/Goose_Bay",
					   APRIL, 1, SUNDAY, 60000,
					   OCTOBER, -1, SUNDAY, 60000);
					timezones0.Add("America/Goose_Bay", tz);
					tz = new TimeZone
					  (-4000 * 3600, "America/Santiago",
					   OCTOBER, 9, -SUNDAY, 1000 * 3600,
					   MARCH, 9, -SUNDAY, 0 * 3600);
					timezones0.Add("America/Santiago", tz);
					tz = new TimeZone
					  (-4000 * 3600, "America/Glace_Bay",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("America/Glace_Bay", tz);
					timezones0.Add("America/Halifax", tz);
					timezones0.Add("America/Thule", tz);
					timezones0.Add("Atlantic/Bermuda", tz);
					tz = new TimeZone
					  (-4000 * 3600, "Antarctica/Palmer",
					   OCTOBER, 9, -SUNDAY, 0 * 3600,
					   MARCH, 9, -SUNDAY, 0 * 3600);
					timezones0.Add("Antarctica/Palmer", tz);
					tz = new TimeZone
					  (-4000 * 3600, "Atlantic/Stanley",
					   SEPTEMBER, 1, SUNDAY, 2000 * 3600,
					   APRIL, 3, SUNDAY, 2000 * 3600);
					timezones0.Add("Atlantic/Stanley", tz);
					tz = new TimeZone
					  (-3500 * 3600, "CNT",
					   APRIL, 1, SUNDAY, 60000,
					   OCTOBER, -1, SUNDAY, 60000);
					timezones0.Add("CNT", tz);
					timezones0.Add("America/St_Johns", tz);
					tz = new TimeZone
					  (-3000 * 3600, "America/Godthab",
					   MARCH, 30, -SATURDAY, 23000 * 3600,
					   OCTOBER, 30, -SATURDAY, 23000 * 3600);
					timezones0.Add("America/Godthab", tz);
					tz = new TimeZone
					  (-3000 * 3600, "America/Miquelon",
					   APRIL, 1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("America/Miquelon", tz);
					tz = new TimeZone
					  (-3000 * 3600, "America/Sao_Paulo",
					   OCTOBER, 3, SUNDAY, 0 * 3600,
					   FEBRUARY, 3, SUNDAY, 0 * 3600);
					timezones0.Add("America/Sao_Paulo", tz);
					tz = new TimeZone(-3000 * 3600, "AGT");
					timezones0.Add("AGT", tz);
					timezones0.Add("America/Araguaina", tz);
					timezones0.Add("America/Argentina/Buenos_Aires", tz);
					timezones0.Add("America/Argentina/Catamarca", tz);
					timezones0.Add("America/Argentina/ComodRivadavia", tz);
					timezones0.Add("America/Argentina/Cordoba", tz);
					timezones0.Add("America/Argentina/Jujuy", tz);
					timezones0.Add("America/Argentina/La_Rioja", tz);
					timezones0.Add("America/Argentina/Mendoza", tz);
					timezones0.Add("America/Argentina/Rio_Gallegos", tz);
					timezones0.Add("America/Argentina/San_Juan", tz);
					timezones0.Add("America/Argentina/Tucuman", tz);
					timezones0.Add("America/Argentina/Ushuaia", tz);
					timezones0.Add("America/Bahia", tz);
					timezones0.Add("America/Belem", tz);
					timezones0.Add("America/Cayenne", tz);
					timezones0.Add("America/Fortaleza", tz);
					timezones0.Add("America/Maceio", tz);
					timezones0.Add("America/Montevideo", tz);
					timezones0.Add("America/Paramaribo", tz);
					timezones0.Add("America/Recife", tz);
					timezones0.Add("Antarctica/Rothera", tz);
					tz = new TimeZone(-2000 * 3600, "America/Noronha");
					timezones0.Add("America/Noronha", tz);
					timezones0.Add("Atlantic/South_Georgia", tz);
					tz = new TimeZone
					  (-1000 * 3600, "America/Scoresbysund",
					   MARCH, -1, SUNDAY, 1000 * 3600,
					   OCTOBER, -1, SUNDAY, 1000 * 3600);
					timezones0.Add("America/Scoresbysund", tz);
					timezones0.Add("Atlantic/Azores", tz);
					tz = new TimeZone(-1000 * 3600, "Atlantic/Cape_Verde");
					timezones0.Add("Atlantic/Cape_Verde", tz);
					tz = new TimeZone(0 * 3600, "GMT");
					timezones0.Add("GMT", tz);
					timezones0.Add("UTC", tz);
					timezones0.Add("Africa/Abidjan", tz);
					timezones0.Add("Africa/Accra", tz);
					timezones0.Add("Africa/Bamako", tz);
					timezones0.Add("Africa/Banjul", tz);
					timezones0.Add("Africa/Bissau", tz);
					timezones0.Add("Africa/Casablanca", tz);
					timezones0.Add("Africa/Conakry", tz);
					timezones0.Add("Africa/Dakar", tz);
					timezones0.Add("Africa/El_Aaiun", tz);
					timezones0.Add("Africa/Freetown", tz);
					timezones0.Add("Africa/Lome", tz);
					timezones0.Add("Africa/Monrovia", tz);
					timezones0.Add("Africa/Nouakchott", tz);
					timezones0.Add("Africa/Ouagadougou", tz);
					timezones0.Add("Africa/Sao_Tome", tz);
					timezones0.Add("Africa/Timbuktu", tz);
					timezones0.Add("America/Danmarkshavn", tz);
					timezones0.Add("Atlantic/Reykjavik", tz);
					timezones0.Add("Atlantic/St_Helena", tz);
					timezones0.Add("Europe/Belfast", tz);
					timezones0.Add("Europe/Dublin", tz);
					timezones0.Add("Europe/London", tz);
					tz = new TimeZone
					  (0 * 3600, "WET",
					   MARCH, -1, SUNDAY, 2000 * 3600,
					   OCTOBER, -1, SUNDAY, 2000 * 3600);
					timezones0.Add("WET", tz);
					timezones0.Add("Atlantic/Canary", tz);
					timezones0.Add("Atlantic/Faeroe", tz);
					timezones0.Add("Atlantic/Madeira", tz);
					timezones0.Add("Europe/Lisbon", tz);
					tz = new TimeZone(1000 * 3600, "Africa/Algiers");
					timezones0.Add("Africa/Algiers", tz);
					timezones0.Add("Africa/Bangui", tz);
					timezones0.Add("Africa/Brazzaville", tz);
					timezones0.Add("Africa/Douala", tz);
					timezones0.Add("Africa/Kinshasa", tz);
					timezones0.Add("Africa/Lagos", tz);
					timezones0.Add("Africa/Libreville", tz);
					timezones0.Add("Africa/Luanda", tz);
					timezones0.Add("Africa/Malabo", tz);
					timezones0.Add("Africa/Ndjamena", tz);
					timezones0.Add("Africa/Niamey", tz);
					timezones0.Add("Africa/Porto-Novo", tz);
					timezones0.Add("Africa/Tunis", tz);
					tz = new TimeZone
					  (1000 * 3600, "Africa/Windhoek",
					   SEPTEMBER, 1, SUNDAY, 2000 * 3600,
					   APRIL, 1, SUNDAY, 2000 * 3600);
					timezones0.Add("Africa/Windhoek", tz);
					tz = new TimeZone
					  (1000 * 3600, "CET",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("CET", tz);
					timezones0.Add("ECT", tz);
					timezones0.Add("MET", tz);
					timezones0.Add("Africa/Ceuta", tz);
					timezones0.Add("Arctic/Longyearbyen", tz);
					timezones0.Add("Atlantic/Jan_Mayen", tz);
					timezones0.Add("Europe/Amsterdam", tz);
					timezones0.Add("Europe/Andorra", tz);
					timezones0.Add("Europe/Belgrade", tz);
					timezones0.Add("Europe/Berlin", tz);
					timezones0.Add("Europe/Bratislava", tz);
					timezones0.Add("Europe/Brussels", tz);
					timezones0.Add("Europe/Budapest", tz);
					timezones0.Add("Europe/Copenhagen", tz);
					timezones0.Add("Europe/Gibraltar", tz);
					timezones0.Add("Europe/Ljubljana", tz);
					timezones0.Add("Europe/Luxembourg", tz);
					timezones0.Add("Europe/Madrid", tz);
					timezones0.Add("Europe/Malta", tz);
					timezones0.Add("Europe/Monaco", tz);
					timezones0.Add("Europe/Oslo", tz);
					timezones0.Add("Europe/Paris", tz);
					timezones0.Add("Europe/Prague", tz);
					timezones0.Add("Europe/Rome", tz);
					timezones0.Add("Europe/San_Marino", tz);
					timezones0.Add("Europe/Sarajevo", tz);
					timezones0.Add("Europe/Skopje", tz);
					timezones0.Add("Europe/Stockholm", tz);
					timezones0.Add("Europe/Tirane", tz);
					timezones0.Add("Europe/Vaduz", tz);
					timezones0.Add("Europe/Vatican", tz);
					timezones0.Add("Europe/Vienna", tz);
					timezones0.Add("Europe/Warsaw", tz);
					timezones0.Add("Europe/Zagreb", tz);
					timezones0.Add("Europe/Zurich", tz);
					tz = new TimeZone
					  (2000 * 3600, "ART",
					   APRIL, -1, FRIDAY, 1000 * 3600,
					   SEPTEMBER, -1, THURSDAY, 24000 * 3600);
					timezones0.Add("ART", tz);
					timezones0.Add("Africa/Cairo", tz);
					tz = new TimeZone(2000 * 3600, "CAT");
					timezones0.Add("CAT", tz);
					timezones0.Add("Africa/Blantyre", tz);
					timezones0.Add("Africa/Bujumbura", tz);
					timezones0.Add("Africa/Gaborone", tz);
					timezones0.Add("Africa/Harare", tz);
					timezones0.Add("Africa/Johannesburg", tz);
					timezones0.Add("Africa/Kigali", tz);
					timezones0.Add("Africa/Lubumbashi", tz);
					timezones0.Add("Africa/Lusaka", tz);
					timezones0.Add("Africa/Maputo", tz);
					timezones0.Add("Africa/Maseru", tz);
					timezones0.Add("Africa/Mbabane", tz);
					timezones0.Add("Africa/Tripoli", tz);
					timezones0.Add("Asia/Jerusalem", tz);
					tz = new TimeZone
					  (2000 * 3600, "Asia/Amman",
					   MARCH, -1, THURSDAY, 1000 * 3600,
					   SEPTEMBER, -1, THURSDAY, 1000 * 3600);
					timezones0.Add("Asia/Amman", tz);
					tz = new TimeZone
					  (2000 * 3600, "Asia/Beirut",
					   MARCH, -1, SUNDAY, 0 * 3600,
					   OCTOBER, -1, SUNDAY, 0 * 3600);
					timezones0.Add("Asia/Beirut", tz);
					tz = new TimeZone
					  (2000 * 3600, "Asia/Damascus",
					   APRIL, 1, 0, 0 * 3600,
					   OCTOBER, 1, 0, 0 * 3600);
					timezones0.Add("Asia/Damascus", tz);
					tz = new TimeZone
					  (2000 * 3600, "Asia/Gaza",
					   APRIL, 3, FRIDAY, 0 * 3600,
					   OCTOBER, 3, FRIDAY, 0 * 3600);
					timezones0.Add("Asia/Gaza", tz);
					tz = new TimeZone
					  (2000 * 3600, "EET",
					   MARCH, -1, SUNDAY, 4000 * 3600,
					   OCTOBER, -1, SUNDAY, 4000 * 3600);
					timezones0.Add("EET", tz);
					timezones0.Add("Asia/Istanbul", tz);
					timezones0.Add("Asia/Nicosia", tz);
					timezones0.Add("Europe/Athens", tz);
					timezones0.Add("Europe/Bucharest", tz);
					timezones0.Add("Europe/Chisinau", tz);
					timezones0.Add("Europe/Helsinki", tz);
					timezones0.Add("Europe/Istanbul", tz);
					timezones0.Add("Europe/Kiev", tz);
					timezones0.Add("Europe/Mariehamn", tz);
					timezones0.Add("Europe/Nicosia", tz);
					timezones0.Add("Europe/Riga", tz);
					timezones0.Add("Europe/Simferopol", tz);
					timezones0.Add("Europe/Sofia", tz);
					timezones0.Add("Europe/Tallinn", tz);
					timezones0.Add("Europe/Uzhgorod", tz);
					timezones0.Add("Europe/Vilnius", tz);
					timezones0.Add("Europe/Zaporozhye", tz);
					tz = new TimeZone
					  (2000 * 3600, "Europe/Kaliningrad",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Europe/Kaliningrad", tz);
					timezones0.Add("Europe/Minsk", tz);
					tz = new TimeZone
					  (3000 * 3600, "Asia/Baghdad",
					   APRIL, 1, 0, 4000 * 3600,
					   OCTOBER, 1, 0, 4000 * 3600);
					timezones0.Add("Asia/Baghdad", tz);
					tz = new TimeZone
					  (3000 * 3600, "Asia/Tbilisi",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Tbilisi", tz);
					timezones0.Add("Europe/Moscow", tz);
					tz = new TimeZone(3000 * 3600, "EAT");
					timezones0.Add("EAT", tz);
					timezones0.Add("Africa/Addis_Ababa", tz);
					timezones0.Add("Africa/Asmera", tz);
					timezones0.Add("Africa/Dar_es_Salaam", tz);
					timezones0.Add("Africa/Djibouti", tz);
					timezones0.Add("Africa/Kampala", tz);
					timezones0.Add("Africa/Khartoum", tz);
					timezones0.Add("Africa/Mogadishu", tz);
					timezones0.Add("Africa/Nairobi", tz);
					timezones0.Add("Antarctica/Syowa", tz);
					timezones0.Add("Asia/Aden", tz);
					timezones0.Add("Asia/Bahrain", tz);
					timezones0.Add("Asia/Kuwait", tz);
					timezones0.Add("Asia/Qatar", tz);
					timezones0.Add("Asia/Riyadh", tz);
					timezones0.Add("Indian/Antananarivo", tz);
					timezones0.Add("Indian/Comoro", tz);
					timezones0.Add("Indian/Mayotte", tz);
					tz = new TimeZone(3500 * 3600, "Asia/Tehran");
					timezones0.Add("Asia/Tehran", tz);
					tz = new TimeZone
					  (4000 * 3600, "Asia/Baku",
					   MARCH, -1, SUNDAY, 1000 * 3600,
					   OCTOBER, -1, SUNDAY, 1000 * 3600);
					timezones0.Add("Asia/Baku", tz);
					tz = new TimeZone
					  (4000 * 3600, "Asia/Yerevan",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Yerevan", tz);
					timezones0.Add("Europe/Samara", tz);
					tz = new TimeZone(4000 * 3600, "NET");
					timezones0.Add("NET", tz);
					timezones0.Add("Asia/Aqtau", tz);
					timezones0.Add("Asia/Dubai", tz);
					timezones0.Add("Asia/Muscat", tz);
					timezones0.Add("Asia/Oral", tz);
					timezones0.Add("Indian/Mahe", tz);
					timezones0.Add("Indian/Mauritius", tz);
					timezones0.Add("Indian/Reunion", tz);
					tz = new TimeZone(4500 * 3600, "Asia/Kabul");
					timezones0.Add("Asia/Kabul", tz);
					tz = new TimeZone
					  (5000 * 3600, "Asia/Bishkek",
					   MARCH, -1, SUNDAY, 2500 * 3600,
					   OCTOBER, -1, SUNDAY, 2500 * 3600);
					timezones0.Add("Asia/Bishkek", tz);
					tz = new TimeZone
					  (5000 * 3600, "Asia/Yekaterinburg",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Yekaterinburg", tz);
					tz = new TimeZone(5000 * 3600, "PLT");
					timezones0.Add("PLT", tz);
					timezones0.Add("Asia/Aqtobe", tz);
					timezones0.Add("Asia/Ashgabat", tz);
					timezones0.Add("Asia/Dushanbe", tz);
					timezones0.Add("Asia/Karachi", tz);
					timezones0.Add("Asia/Samarkand", tz);
					timezones0.Add("Asia/Tashkent", tz);
					timezones0.Add("Indian/Kerguelen", tz);
					timezones0.Add("Indian/Maldives", tz);
					tz = new TimeZone(5500 * 3600, "IST");
					timezones0.Add("IST", tz);
					timezones0.Add("Asia/Calcutta", tz);
					tz = new TimeZone(5750 * 3600, "Asia/Katmandu");
					timezones0.Add("Asia/Katmandu", tz);
					tz = new TimeZone(6000 * 3600, "BST");
					timezones0.Add("BST", tz);
					timezones0.Add("Antarctica/Mawson", tz);
					timezones0.Add("Antarctica/Vostok", tz);
					timezones0.Add("Asia/Almaty", tz);
					timezones0.Add("Asia/Colombo", tz);
					timezones0.Add("Asia/Dhaka", tz);
					timezones0.Add("Asia/Qyzylorda", tz);
					timezones0.Add("Asia/Thimphu", tz);
					timezones0.Add("Indian/Chagos", tz);
					tz = new TimeZone
					  (6000 * 3600, "Asia/Novosibirsk",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Novosibirsk", tz);
					timezones0.Add("Asia/Omsk", tz);
					tz = new TimeZone(6500 * 3600, "Asia/Rangoon");
					timezones0.Add("Asia/Rangoon", tz);
					timezones0.Add("Indian/Cocos", tz);
					tz = new TimeZone(7000 * 3600, "VST");
					timezones0.Add("VST", tz);
					timezones0.Add("Antarctica/Davis", tz);
					timezones0.Add("Asia/Bangkok", tz);
					timezones0.Add("Asia/Jakarta", tz);
					timezones0.Add("Asia/Phnom_Penh", tz);
					timezones0.Add("Asia/Pontianak", tz);
					timezones0.Add("Asia/Saigon", tz);
					timezones0.Add("Asia/Vientiane", tz);
					timezones0.Add("Indian/Christmas", tz);
					tz = new TimeZone
					  (7000 * 3600, "Asia/Hovd",
					   MARCH, -1, SATURDAY, 2000 * 3600,
					   SEPTEMBER, -1, SATURDAY, 2000 * 3600);
					timezones0.Add("Asia/Hovd", tz);
					tz = new TimeZone
					  (7000 * 3600, "Asia/Krasnoyarsk",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Krasnoyarsk", tz);
					tz = new TimeZone(8000 * 3600, "CTT");
					timezones0.Add("CTT", tz);
					timezones0.Add("Antarctica/Casey", tz);
					timezones0.Add("Asia/Brunei", tz);
					timezones0.Add("Asia/Chongqing", tz);
					timezones0.Add("Asia/Harbin", tz);
					timezones0.Add("Asia/Hong_Kong", tz);
					timezones0.Add("Asia/Kashgar", tz);
					timezones0.Add("Asia/Kuala_Lumpur", tz);
					timezones0.Add("Asia/Kuching", tz);
					timezones0.Add("Asia/Macau", tz);
					timezones0.Add("Asia/Makassar", tz);
					timezones0.Add("Asia/Manila", tz);
					timezones0.Add("Asia/Shanghai", tz);
					timezones0.Add("Asia/Singapore", tz);
					timezones0.Add("Asia/Taipei", tz);
					timezones0.Add("Asia/Urumqi", tz);
					timezones0.Add("Australia/Perth", tz);
					tz = new TimeZone
					  (8000 * 3600, "Asia/Irkutsk",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Irkutsk", tz);
					tz = new TimeZone
					  (8000 * 3600, "Asia/Ulaanbaatar",
					   MARCH, -1, SATURDAY, 2000 * 3600,
					   SEPTEMBER, -1, SATURDAY, 2000 * 3600);
					timezones0.Add("Asia/Ulaanbaatar", tz);
					tz = new TimeZone
					  (9000 * 3600, "Asia/Choibalsan",
					   MARCH, -1, SATURDAY, 2000 * 3600,
					   SEPTEMBER, -1, SATURDAY, 2000 * 3600);
					timezones0.Add("Asia/Choibalsan", tz);
					tz = new TimeZone(9000 * 3600, "JST");
					timezones0.Add("JST", tz);
					timezones0.Add("Asia/Dili", tz);
					timezones0.Add("Asia/Jayapura", tz);
					timezones0.Add("Asia/Pyongyang", tz);
					timezones0.Add("Asia/Seoul", tz);
					timezones0.Add("Asia/Tokyo", tz);
					timezones0.Add("Pacific/Palau", tz);
					tz = new TimeZone
					  (9000 * 3600, "Asia/Yakutsk",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Yakutsk", tz);
					tz = new TimeZone
					  (9500 * 3600, "Australia/Adelaide",
					   OCTOBER, -1, SUNDAY, 3000 * 3600,
					   MARCH, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Australia/Adelaide", tz);
					timezones0.Add("Australia/Broken_Hill", tz);
					tz = new TimeZone(9500 * 3600, "ACT");
					timezones0.Add("ACT", tz);
					timezones0.Add("Australia/Darwin", tz);
					tz = new TimeZone(10000 * 3600, "Antarctica/DumontDUrville");
					timezones0.Add("Antarctica/DumontDUrville", tz);
					timezones0.Add("Australia/Brisbane", tz);
					timezones0.Add("Australia/Lindeman", tz);
					timezones0.Add("Pacific/Guam", tz);
					timezones0.Add("Pacific/Port_Moresby", tz);
					timezones0.Add("Pacific/Saipan", tz);
					timezones0.Add("Pacific/Truk", tz);
					timezones0.Add("Pacific/Yap", tz);
					tz = new TimeZone
					  (10000 * 3600, "Asia/Sakhalin",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Sakhalin", tz);
					timezones0.Add("Asia/Vladivostok", tz);
					tz = new TimeZone
					  (10000 * 3600, "Australia/Hobart",
					   OCTOBER, 1, SUNDAY, 3000 * 3600,
					   MARCH, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Australia/Hobart", tz);
					tz = new TimeZone
					  (10000 * 3600, "AET",
					   OCTOBER, -1, SUNDAY, 3000 * 3600,
					   MARCH, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("AET", tz);
					timezones0.Add("Australia/Melbourne", tz);
					timezones0.Add("Australia/Sydney", tz);
					tz = new TimeZone
					  (10500 * 3600, "Australia/Lord_Howe",
					  OCTOBER, -1, SUNDAY, 2000 * 3600,
					  MARCH, -1, SUNDAY, 2000 * 3600, 500 * 3600);
					timezones0.Add("Australia/Lord_Howe", tz);
					tz = new TimeZone
					  (11000 * 3600, "Asia/Magadan",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Magadan", tz);
					tz = new TimeZone(11000 * 3600, "SST");
					timezones0.Add("SST", tz);
					timezones0.Add("Pacific/Efate", tz);
					timezones0.Add("Pacific/Guadalcanal", tz);
					timezones0.Add("Pacific/Kosrae", tz);
					timezones0.Add("Pacific/Noumea", tz);
					timezones0.Add("Pacific/Ponape", tz);
					tz = new TimeZone(11500 * 3600, "Pacific/Norfolk");
					timezones0.Add("Pacific/Norfolk", tz);
					tz = new TimeZone
					  (12000 * 3600, "NST",
					   OCTOBER, 1, SUNDAY, 3000 * 3600,
					   MARCH, 3, SUNDAY, 3000 * 3600);
					timezones0.Add("NST", tz);
					timezones0.Add("Antarctica/McMurdo", tz);
					timezones0.Add("Antarctica/South_Pole", tz);
					timezones0.Add("Pacific/Auckland", tz);
					tz = new TimeZone
					  (12000 * 3600, "Asia/Anadyr",
					   MARCH, -1, SUNDAY, 3000 * 3600,
					   OCTOBER, -1, SUNDAY, 3000 * 3600);
					timezones0.Add("Asia/Anadyr", tz);
					timezones0.Add("Asia/Kamchatka", tz);
					tz = new TimeZone(12000 * 3600, "Pacific/Fiji");
					timezones0.Add("Pacific/Fiji", tz);
					timezones0.Add("Pacific/Funafuti", tz);
					timezones0.Add("Pacific/Kwajalein", tz);
					timezones0.Add("Pacific/Majuro", tz);
					timezones0.Add("Pacific/Nauru", tz);
					timezones0.Add("Pacific/Tarawa", tz);
					timezones0.Add("Pacific/Wake", tz);
					timezones0.Add("Pacific/Wallis", tz);
					tz = new TimeZone
					  (12750 * 3600, "Pacific/Chatham",
					   OCTOBER, 1, SUNDAY, 3750 * 3600,
					   MARCH, 3, SUNDAY, 3750 * 3600);
					timezones0.Add("Pacific/Chatham", tz);
					tz = new TimeZone(13000 * 3600, "Pacific/Enderbury");
					timezones0.Add("Pacific/Enderbury", tz);
					timezones0.Add("Pacific/Tongatapu", tz);
					tz = new TimeZone(14000 * 3600, "Pacific/Kiritimati");
					timezones0.Add("Pacific/Kiritimati", tz);
				}
				return timezones0;
			}
		}
		#endregion

		#region Public Methods
		public override DaylightTime GetDaylightChanges(int year) {
			if (year < 1 || year > 9999)
				throw new ArgumentOutOfRangeException("year", year + " is not in a range between 1 and 9999.");

			//TODO:
			throw new NotImplementedException();
		}

		public override TimeSpan GetUtcOffset(DateTime time) {
			int daysInMonth = getDaysInMonth(time.Month, time.Year);
			if (time.Day < 1 || time.Day > daysInMonth)
				throw new ArgumentException("day out of range");
			if (time.DayOfWeek < DayOfWeek.Sunday ||
				time.DayOfWeek > DayOfWeek.Saturday)
				throw new ArgumentException("dayOfWeek out of range");
			if (time.Month < 0 || time.Month > 11)
				throw new ArgumentException("month out of range:" + time.Month);

			// This method is called by Calendar, so we mustn't use that class.
			int daylightSavings = 0;
			if (useDaylight && new GregorianCalendar().GetEra(time) == GregorianCalendar.ADEra &&
				time.Year >= startYear) {
				// This does only work for Gregorian calendars :-(
				// This is mainly because setStartYear doesn't take an era.
				bool afterStart = !isBefore(time.Year, time.Month, time.Day, (int)time.DayOfWeek + 1,
					time.Millisecond, startMode, startMonth, startDay, startDayOfWeek, startTime);
				bool beforeEnd = isBefore(time.Year, time.Month, time.Day, (int)time.DayOfWeek + 1,
								 time.Millisecond + dstSavings, endMode, endMonth, endDay,
								 endDayOfWeek, endTime);

				if (startMonth < endMonth)
					// use daylight savings, if the date is after the start of
					// savings, and before the end of savings.
					daylightSavings = afterStart && beforeEnd ? dstSavings : 0;
				else
					// use daylight savings, if the date is before the end of
					// savings, or after the start of savings.
					daylightSavings = beforeEnd || afterStart ? dstSavings : 0;
			}

			return new TimeSpan((rawOffset + daylightSavings) * TimeSpan.TicksPerMillisecond);
		}
		#endregion

		#region Public Static Methods
		public static TimeZone GetTimeZone(string ID) {
			// First check timezones hash
			TimeZone tz = (TimeZone)timezones()[ID];
			if (tz != null) {
				if (tz.StandardName.Equals(ID))
					return tz;

				tz = (TimeZone)tz.Clone();
				tz.id = ID;
				// We also save the alias, so that we return the same
				// object again if getTimeZone is called with the same
				// alias.
				timezones().Add(ID, tz);
				return tz;
			}

			// See if the ID is really a GMT offset form.
			// Note that GMT is in the table so we know it is different.
			if (ID.StartsWith("GMT")) {
				int pos = 3;
				int offset_direction = 1;

				if (ID[pos] == '-') {
					offset_direction = -1;
					pos++;
				} else if (ID[pos] == '+') {
					pos++;
				}

				try {
					int hour, minute;

					string offset_str = ID.Substring(pos);
					int idx = offset_str.IndexOf(":");
					if (idx != -1) {
						hour = Int32.Parse(offset_str.Substring(0, idx));
						minute = Int32.Parse(offset_str.Substring(idx + 1));
					} else {
						int offset_length = offset_str.Length;
						if (offset_length <= 2) {
							// Only hour
							hour = Int32.Parse(offset_str);
							minute = 0;
						} else {
							// hour and minute, not separated by colon
							hour = Int32.Parse(offset_str.Substring(0, offset_length - 2));
							minute = Int32.Parse(offset_str.Substring(offset_length - 2));
						}
					}

					return new TimeZone((hour * (60 * 60 * 1000) +
								   minute * (60 * 1000))
								  * offset_direction, ID);
				} catch (FormatException) {
				}
			}

			// Finally, return GMT per spec
			return GetTimeZone("GMT");
		}

		public object Clone() {
			return base.MemberwiseClone();
		}
		#endregion
	}
}