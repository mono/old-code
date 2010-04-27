//
// TimeUtils.cs:
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
using System.Globalization;
using System.Text;

namespace Deveel.Pim {
	class TimeUtils {
		public const String PATTERN_YYYYMMDD = "yyyyMMdd";
		public const int PATTERN_YYYYMMDD_LENGTH = 8;

		public const String PATTERN_YYYY_MM_DD = "yyyy-MM-dd";
		public const int PATTERN_YYYY_MM_DD_LENGTH = 10;

		public const String PATTERN_UTC = "yyyyMMdd'T'HHmmss'Z'";
		public const int PATTERN_UTC_LENGTH = 16;

		// UTC WOZ = UTC without 'Z'
		public const String PATTERN_UTC_WOZ = "yyyyMMdd'T'HHmmss";
		public const int PATTERN_UTC_WOZ_LENGTH = 15;

		// UTC WSEP = UTC with separator
		public const String PATTERN_UTC_WSEP = "yyyy-MM-dd'T'HH:mm:ss'Z'";
		public const int PATTERN_UTC_WSEP_LENGTH = 20;

		public const String PATTERN_LOCALTIME = "dd/MM/yyyy HH:mm:ss";
		public const int PATTERN_LOCALTIME_LENGTH = 19;

		// WOT = without time
		public const String PATTERN_LOCALTIME_WOT = "dd/MM/yyyy";
		public const int PATTERN_LOCALTIME_WOT_LENGTH = 10;


		// Set a string date from UTC format (yyyyMMdd'T'HHmmss'Z') into
		// a format dd/MM/yyyy HH:mm:ss according to default local timezone.
		public static string UTCToLocalTime(string UTCFormat) {
			string actualTime = UTCFormat;
			try {
				DateTime date = DateTime.ParseExact(UTCFormat, PATTERN_UTC, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
				actualTime = date.ToString(PATTERN_LOCALTIME);

			} catch (Exception) {
			}

			return actualTime;
		}

		// Set a string date from dd/MM/yyyy HH:mm:ss according to default local
		// timezone into a UTC date pattern yyyyMMdd'T'HHmmss'Z'
		public static String localTimeToUTC(String actualTime) {
			String UTCFormat = actualTime;
			try {
				string format = PATTERN_LOCALTIME;

				if (actualTime.Length <= 10)
					format = PATTERN_LOCALTIME_WOT;

				DateTime date = DateTime.ParseExact(actualTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
				UTCFormat = date.ToUniversalTime().ToString(PATTERN_UTC);
			} catch (Exception) {
			}

			return UTCFormat;
		}

		public static string convertLocalDateToUTC(string sDate, TimeZone timezone) {
			if (sDate == null || sDate.Length == 0 || isInAllDayFormat(sDate))
				return sDate;

			if (sDate.IndexOf('Z') != -1) {
				//
				// No conversion is required
				//
				return sDate;
			}

			DateTime date = DateTime.ParseExact(sDate, PATTERN_UTC_WOZ, CultureInfo.InvariantCulture);
			if (timezone != null) {
				date.Add(timezone.GetUtcOffset(date));
			}

			//CHECK: this has to be check...
			return date.ToString(PATTERN_UTC);
		}

		public static String convertUTCDateToLocal(String sDate, TimeZone timezone) {
			if (sDate == null || sDate.Length == 0 || isInAllDayFormat(sDate))
				return sDate;

			if (timezone == null)
				return sDate;

			if (!sDate.EndsWith("Z"))
				return sDate;

			DateTime date = DateTime.ParseExact(sDate, PATTERN_UTC, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
			date.Add(timezone.GetUtcOffset(date));

			return date.ToString(PATTERN_UTC);
		}

		public static String NormalizeToISO8601(String sDate, TimeZone tz) {
			if (sDate == null || sDate.Length == 0)
				return sDate;

			if (tz != null) {
				//
				// Try to apply the timezone
				//
				DateTime date;
				try {
					date = DateTime.ParseExact(sDate, PATTERN_UTC, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
					if (!sDate.EndsWith("000000Z")) {
						sDate = date.Add(tz.GetUtcOffset(date)).ToString(PATTERN_UTC);
					}
				} catch (FormatException) {
					//
					// Ignore this error. The date isn't in this format.
					//
					// Try with yyyy-MM-dd'T'HH:mm:ss'Z'
					try {
						date = DateTime.ParseExact(sDate, PATTERN_UTC_WSEP, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
						if (!sDate.EndsWith("00:00:00Z")) {
							sDate = date.Add(tz.GetUtcOffset(date)).ToString(PATTERN_UTC_WSEP);
						}
					} catch (Exception) {
						//
						// Ignore this error. The date isn't in this format.
						//
					}
				}
			}

			int year = -1;
			int month = -1;
			int day = -1;
			String tmp = null;
			int last = 0;

			//
			// The first four digits are the year
			//
			tmp = sDate.Substring(0, 4);
			year = Int32.Parse(tmp);

			//
			// Read the month
			//
			char c = sDate[4];
			if (c == '/' || c == '-') {
				tmp = sDate.Substring(5, 2);
				last = 7;
			} else {
				tmp = sDate.Substring(4, 2);
				last = 6;
			}

			month = Int32.Parse(tmp);

			//
			// Read the day
			//
			c = sDate[last];
			if (c == '/' || c == '-') {
				tmp = sDate.Substring(last + 1, 2);
			} else {
				tmp = sDate.Substring(last, 1);
			}
			day = Int32.Parse(tmp);

			StringBuilder isoDate = new StringBuilder(10);
			isoDate.Append(year);
			isoDate.Append("-");

			if (month < 10) {
				isoDate.Append("0");
			}
			isoDate.Append(month);
			isoDate.Append("-");

			if (day < 10) {
				isoDate.Append("0");
			}
			isoDate.Append(day);
			return isoDate.ToString();
		}

		public static int getMinutes(String iso8601Duration) {
			if (iso8601Duration == null ||
				iso8601Duration.Length == 0 ||
				String.Compare(iso8601Duration, "null", true) == 0) {
				return -1;
			}

			return (int)TimeSpan.Parse(iso8601Duration).TotalMinutes;
		}

		public static String getIso8601Duration(String minutes) {
			if (minutes == null || minutes.Length == 0)
				return minutes;

			int min = Int32.Parse(minutes);

			if (min == -1)
				return null;

			long mills = min * 60L * 1000L;
			TimeSpan d = new TimeSpan(TimeSpan.TicksPerMillisecond * mills);
			return d.ToString();
		}

		public static String getDTEnd(String dtStart, String duration, String dtEnd) {
			DateTime dateStart;

			if (duration == null || duration.Length == 0 ||
				String.Compare(duration, "null", true) == 0 ||
				dtStart == null || dtStart.Length == 0 ||
				String.Compare(dtStart, "null", true) == 0) {
				return dtEnd;
			}

			string format = getDateFormat(dtStart);

			try {
				dateStart = DateTime.ParseExact(dtStart, format, CultureInfo.InvariantCulture);
			} catch (FormatException) {
				//
				// If we are unable to parse dtStart return dtEnd unchanged
				//
				return dtEnd;
			}

			int minutes = getMinutes(duration);

			DateTime dateEnd = dateStart.AddMinutes(minutes);

			return dateEnd.ToString(format);
		}

		public static int getAlarmMinutes(String dtStart, String dtAlarm) {
			DateTime dateStart;
			DateTime dateAlarm;

			if (dtStart == null || dtStart.Length == 0 ||
				dtAlarm == null || dtAlarm.Length == 0)
				return 0;

			try {
				dateStart = DateTime.ParseExact(dtStart, getDateFormat(dtStart), CultureInfo.InvariantCulture);
				dateAlarm = DateTime.ParseExact(dtAlarm, getDateFormat(dtAlarm), CultureInfo.InvariantCulture);
			} catch (FormatException) {
				//
				// If we are unable to parse dtStart or dtAlarm return 0
				//
				return 0;
			}

			TimeSpan span = dateStart - dateAlarm;
			//CHECK: the return value has to be checked...
			return span.Minutes;
		}

		public static int getAlarmInterval(String interval) {
			if (interval.Length == 0)
				return 0;

			try {
				TimeSpan.Parse(interval);
			} catch (Exception) {
				return Int32.Parse(interval);
			}
			return getMinutes(interval);
		}

		public static bool isInAllDayFormat(String date) {
			string pattern = getDateFormat(date);
			if (pattern == null)
				return false;

			if (pattern.Equals(PATTERN_YYYYMMDD) ||
				pattern.Equals(PATTERN_YYYY_MM_DD))
				return true;

			return false;
		}

		public static String convertDateFromInDayFormat(String stringDate, String hhmmss) {
			if (stringDate == null || stringDate.Length == 0)
				return "";

			StringBuilder sb = null;
			try {
				DateTime date = DateTime.ParseExact(stringDate, PATTERN_YYYY_MM_DD, CultureInfo.InvariantCulture);
				sb = new StringBuilder(date.ToString(PATTERN_YYYYMMDD));
				sb.Append('T');
				sb.Append(hhmmss);
				return sb.ToString();

			} catch (Exception) {
			}

			return "";
		}


		public static String convertDateFromInDayFormat(atring stringDate, string hhmmss, bool inUtc) {
			if (stringDate == null || stringDate.Length == 0)
				return "";

			StringBuilder sb = null;
			try {
				DateTime date = DateTime.ParseExact(stringDate, PATTERN_YYYYMMDD, CultureInfo.InvariantCulture);
				sb = new StringBuilder(date.ToString(PATTERN_YYYYMMDD));
				sb.Append('T');
				sb.Append(hhmmss);
				if (inUtc) {
					sb.Append('Z');
				}
				return sb.ToString();

			} catch (Exception) {
			}

			return "";
		}

		public static String convertDateFromTo(String stringDate, String patternToUse) {
			if (stringDate == null || stringDate.Length == 0)
				return "";

			try {
				string pattern = getDateFormat(stringDate);
				DateTime date = DateTime.ParseExact(stringDate, pattern, CultureInfo.InvariantCulture);
				return date.ToString(patternToUse);

			} catch (Exception) {
			}

			return "";
		}


		public static String getDateFormat(String date) {
			String[] patterns = new String[] {
                            PATTERN_UTC,
                            PATTERN_UTC_WOZ,
                            PATTERN_UTC_WSEP,
                            PATTERN_YYYY_MM_DD,
                            PATTERN_YYYYMMDD,
                            PATTERN_LOCALTIME,
                            PATTERN_LOCALTIME_WOT
        };

			int[] patternsLength = new int[] {
                               PATTERN_UTC_LENGTH,
                               PATTERN_UTC_WOZ_LENGTH,
                               PATTERN_UTC_WSEP_LENGTH,
                               PATTERN_YYYY_MM_DD_LENGTH,
                               PATTERN_YYYYMMDD_LENGTH,
                               PATTERN_LOCALTIME_LENGTH,
                               PATTERN_LOCALTIME_WOT_LENGTH
        };

			if (date == null || date.Length == 0)
				return null;

			int s = patterns.Length;
			DateTime d;
			for (int i = 0; i < s; i++) {
				try {
					d = DateTime.ParseExact(date, patterns[i], CultureInfo.InvariantCulture);

					if (date.Length == patternsLength[i])
						return patterns[i];
				} catch (FormatException) {
					continue;
				}
			}
			return null;
		}

		public static bool isAllDayEvent(String dateStart, String dateEnd) {
			if (dateStart == null || dateEnd == null) {
				return false;
			}

			if (dateStart.EndsWith("T000000Z")) {
				if (dateEnd.EndsWith("T235959Z") ||
					dateEnd.EndsWith("T235900Z") ||
					dateEnd.EndsWith("T240000Z")) {
					// It is an all day event.
					return true;
				}
			}
			return false;
		}
	}
}