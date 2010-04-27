//
// CalendarConverter.cs:
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
using System.Text;

namespace Deveel.Pim {
	class CalendarConverter : BaseConverter {
		#region .ctor
		public CalendarConverter(TimeZone timezone, string charset)
			: base(timezone, charset) {
		}
		#endregion
		
		#region Fields
		private const string BEGIN_VCALEN = "BEGIN:VCALENDAR\r\n";
		private const string BEGIN_VEVENT = "BEGIN:VEVENT\r\n";
		private const string END_VEVENT = "END:VEVENT\r\n";
		private const string END_VCALEN = "END:VCALENDAR\r\n";

		private Calendar calendar = null;
		#endregion

		#region Private Methods
		private StringBuilder ComposeiCalTextComponent(Property property, String field) {
			StringBuilder result = new StringBuilder(240); // Estimate 240 is needed
			result.Append(field);
			string propertyValue = property.Value;

			try {
				//
				// Encode value as QUOTED-PRINTABLE and set encodingParam at the
				// default value (at the moment we handle only ENCODING=QUOTED-PRINTABLE)
				//
				String encodingParam = ENCODING_QT;
				String charsetParam = property.Charset;
				if (charsetParam == null) {
					if (Charset == null) {
						charsetParam = CHARSET_UTF8;
					} else {
						charsetParam = Charset;
					}
				}

				if (propertyValue == null) {
					propertyValue = "";
				} else {
					if (String.Compare(PLAIN_CHARSET, charsetParam, true) != 0) {
						//
						// We encode the value only if the charset isn't PLAIN_CHARSET
						//
						propertyValue = Encode(propertyValue, encodingParam,
											   charsetParam);

						if (propertyValue.IndexOf("=") != -1) {
							result.Append(";ENCODING=").Append(encodingParam);
							result.Append(";CHARSET=").Append(charsetParam);
						}
					}
				}
				
				string altrepParam = property.AlternateText;
				if (altrepParam != null) {
					result.Append(";ALTREP=");
					result.Append(altrepParam);
				}
				
				string languageParam = property.Language;
				if (languageParam != null) {
					result.Append(";LANGUAGE=");
					result.Append(languageParam);
				}
				
				string cnParam = property.CommonName;
				if (cnParam != null) {
					result.Append(";CN=");
					result.Append(cnParam);
				}
				
				string cuttypeParam = property.CalendarUserType;
				if (cuttypeParam != null) {
					result.Append(";CUTYPE=");
					result.Append(cuttypeParam);
				}
				
				string delegatedFromParam = property.DelegatedFrom;
				if (delegatedFromParam != null) {
					result.Append(";DELEGATED-FROM=");
					result.Append(delegatedFromParam);
				}
				
				string delegatedToParam = property.DelegatedTo;
				if (delegatedToParam != null) {
					result.Append(";DELEGATED-TO=");
					result.Append(delegatedToParam);
				}
				
				string dirParam = property.Directory;
				if (dirParam != null) {
					result.Append(";DIR=");
					result.Append(dirParam);
				}
				
				string memberParam = property.Member;
				if (memberParam != null) {
					result.Append(";MEMBER=");
					result.Append(memberParam);
				}
				
				string partstatParam = property.PartecipationStatus;
				if (partstatParam != null) {
					result.Append(";PARTSTAT=");
					result.Append(partstatParam);
				}
				
				string relatedParam = property.Related;
				if (relatedParam != null) {
					result.Append(";RELATED=");
					result.Append(relatedParam);
				}
				
				string sentbyParam = property.SentBy;
				if (sentbyParam != null) {
					result.Append(";SENT-BY=\"");
					result.Append(sentbyParam).Append("\"");
				}
				
				string valueParam = property.Value;
				if (valueParam != null) {
					result.Append(";VALUE=");
					result.Append(valueParam);
				}

				result.Append(GetXParams(property));

				result.Append(":");
				result.Append(propertyValue);
				result.Append("\r\n");
			} catch (Exception e) {
				throw new ConverterException("Error to compose iCalendar component ", e);
			}
			
			return result;
		}

		 // X-PROP:
		private StringBuilder ComposeFieldXTag(XTagCollection xTags) {
			StringBuilder output = new StringBuilder();

			if (xTags == null || xTags.Count == 0)
				return output;

			Property xtag = null;
			string value;

			int size = xTags.Count;
			for (int i = 0; i < size; i++) {

				XTag xtagObj = xTags[i];

				xtag = xtagObj.Tag;
				value = xtag.Value;

				output.Append(ComposeiCalTextComponent(xtag, xtagObj.TagValue));
			}
			return output;
		}

		// Added X-Param to the input list of the property parameters
		// The buffer iterates throguh the parameters and adds the
		// start parameter char ';' and then the parameter.
		// Avoids the add the starting ';' by the caller and delete
		// the trailing ';' here.
		private void AddXParams(StringBuilder paramList, Property prop) {
			if (prop.XParams != null && prop.XParams.Count > 0) {
				foreach (DictionaryEntry entry in prop.XParams) {
					paramList.Append(";");
					paramList.Append(entry.Key); // the tag
					paramList.Append("=\"");
					paramList.Append(entry.Value);
					paramList.Append("\"");
				}				
			}
		}

		private StringBuilder ComposeFieldCalscale(Property calscale) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (calscale.Value != null) {
				result.Append("CALSCALE");
				AddXParams(result, calscale);
				result.Append(':');
				result.Append(calscale.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldMethod(Property method) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (method.Value != null) {
				result.Append("METHOD");
				AddXParams(result, method);
				result.Append(':');
				result.Append(method.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldProdId(Property prodid) {
			if (prodid.Value == null)
				return new StringBuilder(0);
			
			return ComposeiCalTextComponent(prodid, "PRODID");
		}

		private StringBuilder ComposeFieldVersion(Property version) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (version.Value != null) {
				result.Append("VERSION");
				AddXParams(result, version);
				result.Append(":");
				result.Append(version.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldCategories(Property categories) {
			if (categories.Value == null)
				return new StringBuilder(0);
			
			return ComposeiCalTextComponent(categories, "CATEGORIES");
		}

		private StringBuilder ComposeFieldClass(Property classEvent) {
			if (classEvent.Value == null)
				return  new StringBuilder(0);
			
			return ComposeiCalTextComponent(classEvent, "CLASS");
		}

		private StringBuilder ComposeFieldDescription(Property description) {
			if (description.Value != null)
				return new StringBuilder(0);
			
			return ComposeiCalTextComponent(description, "DESCRIPTION");
		}

		private StringBuilder ComposeFieldGeo(Property latitude, Property longitude) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed

			string outLat = latitude.Value;
			string outLon = longitude.Value;

			if (outLat != null && outLon != null) {
				result.Append("GEO");
				AddXParams(result, latitude);
				AddXParams(result, longitude);
				result.Append(":");
				result.Append(outLat);
				result.Append(";");
				result.Append(outLon);
				result.Append("\r\n");
			}
			
			return result;
		}

		private StringBuilder ComposeFieldLocation(Property location) {
			if (location.Value != null)
				return new StringBuilder(0);
			
			return ComposeiCalTextComponent(location, "LOCATION");
		}

		private StringBuilder ComposeFieldPriority(Property priority) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (priority.Value != null) {
				result.Append("PRIORITY");
				AddXParams(result, priority);
				result.Append(":");
				result.Append(priority.Value);
				result.Append("\r\n");
			}
			
			return result;
		}

		private StringBuilder ComposeFieldStatus(Property status) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (status.Value != null) {
				result.Append("STATUS");
				AddXParams(result, status);
				result.Append(":");
				result.Append(status.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldSummary(Property summary) {
			if (summary.Value != null)
				return new StringBuilder(0);
			
			return ComposeiCalTextComponent(summary, "SUMMARY");
		}

		private StringBuilder ComposeFieldDtEnd(Property dtEnd) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (dtEnd.Value != null) {
				result.Append("DTEND");

				string valueParam = dtEnd.Value;
				if (valueParam != null) {
					result.Append(";VALUE=");
					result.Append(valueParam);
				}
				
				AddXParams(result, dtEnd);

				string dtEndVal = dtEnd.Value;

				if (TimeUtils.isInAllDayFormat(dtEndVal))
					dtEndVal = TimeUtils.convertDateFromInDayFormat(dtEndVal, "235900");
				
				dtEndVal = HandleLocalDateConversion(dtEndVal, TimeZone);
				result.Append(":");
				result.Append(dtEndVal);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldDtStart(Property dtStart) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (dtStart.Value != null) {
				result.Append("DTSTART");

				String valueParam = dtStart.Value;
				if (valueParam != null) {
					result.Append(";VALUE=");
					result.Append(valueParam);
				}
				
				AddXParams(result, dtStart);

				string dtStartVal = dtStart.Value;

				if (TimeUtils.isInAllDayFormat(dtStartVal))
					dtStartVal = TimeUtils.convertDateFromInDayFormat(dtStartVal, "000000");
				
				dtStartVal = HandleLocalDateConversion(dtStartVal, TimeZone);
				result.Append(":");
				result.Append(dtStartVal);
				result.Append("\r\n");
			}
			return result;
		}


		private StringBuilder ComposeFieldTransp(Property transp) {
			if (transp.Value == null)
				return new StringBuilder(0);
			
			return ComposeiCalTextComponent(transp, "TRANSP");
		}

		private StringBuilder ComposeFieldOrganizer(Property org) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (org.Value != null) {
				result.Append("ORGANIZER");

				//building a properties string
				string cnParam = org.CommonName;
				string dirParam = org.Directory;
				string sentbyParam = org.SentBy;
				string languageParam = org.Language;
				
				if (cnParam != null) {
					result.Append(";CN=");
					result.Append(cnParam);
				}
				
				if (dirParam != null) {
					result.Append(";DIR=");
					result.Append(dirParam);
				}
				
				if (sentbyParam != null) {
					result.Append(";SENT-BY=\"");
					result.Append(sentbyParam);
					result.Append("\"");
				}
				
				if (languageParam != null) {
					result.Append(";LANGUAGE=");
					result.Append(languageParam);
				}
				
				AddXParams(result, org);
				result.Append(":");
				result.Append(org.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldUrl(Property url) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (url.Value != null) {
				result.Append("URL");
				AddXParams(result, url);
				result.Append(":");
				result.Append(url.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldUid(Property uid) {
			if (uid.Value != null)
				return new StringBuilder(0);
			
			return ComposeiCalTextComponent(uid, "UID");
		}

		private StringBuilder ComposeFieldRrule(Property rrule) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (rrule.Value != null) {
				result.Append("RRULE");
				AddXParams(result, rrule);
				result.Append(":");
				result.Append(rrule.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldContact(Property contact) {
			if (contact.Value != null)
				return new StringBuilder(0);
				
			return ComposeiCalTextComponent(contact, "CONTACT");
		}

		private StringBuilder composeFieldCreated(Property created) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (created.Value != null) {
				result.Append(created.Tag);
				AddXParams(result, created);

				string createdVal = created.Value;
				createdVal = HandleLocalDateConversion(createdVal, TimeZone);

				result.Append(":");
				result.Append(createdVal);
				result.Append("\r\n");
			}
			
			return result;
		}

		private StringBuilder ComposeFieldDtstamp(Property dtStamp) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (dtStamp.Value != null) {
				result.Append("DTSTAMP");
				AddXParams(result, dtStamp);

				String dtStampVal = dtStamp.Value;
				dtStampVal = HandleLocalDateConversion(dtStampVal, TimeZone);

				result.Append(":");
				result.Append(dtStampVal);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldLastModified(Property lastModified) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (lastModified.Value != null) {
				result.Append("LAST-MODIFIED");
				AddXParams(result, lastModified);
				result.Append(":");
				
				string lastModifiedVal = lastModified.Value;
				lastModifiedVal = HandleLocalDateConversion(lastModifiedVal, TimeZone);
				result.Append(lastModifiedVal);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldSequence(Property sequence) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (sequence.Value != null) {
				result.Append("SEQUENCE");
				AddXParams(result, sequence);
				result.Append(":");
				result.Append(sequence.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldDAlarm(Property dalarm) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (dalarm.Value != null) {
				result.Append("DALARM");
				AddXParams(result, dalarm);
				result.Append(":");
				result.Append(dalarm.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldPAlarm(Property palarm) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			if (palarm.Value != null) {
				result.Append("PALARM");
				AddXParams(result, palarm);
				result.Append(":");
				result.Append(palarm.Value);
				result.Append("\r\n");
			}
			return result;
		}

		private StringBuilder ComposeFieldReminder(Property dtStart, Reminder reminder) {
			StringBuilder result = new StringBuilder(60); // Estimate 60 is needed
			result.Append("AALARM");

			string typeParam = reminder.PropertyType;
			if (typeParam != null) {
				result.Append(";TYPE=");
				result.Append(typeParam);
			}
			
			string valueParam = reminder.Value;
			if (valueParam != null) {
				result.Append(";VALUE=");
				result.Append(valueParam);
			}
			
			AddXParams(result, reminder);

			DateTime dateStart;

			string dtStartVal = dtStart.Value;
			dtStartVal = HandleLocalDateConversion(dtStartVal, TimeZone);

			try {
				dateStart = DateTime.ParseExact(dtStartVal, TimeUtils.getDateFormat(dtStartVal), CultureInfo.InvariantCulture);
			} catch (FormatException) {
				//is not possible
				//TODO: what to do now?
				dateStart = DateTime.Now;
			}

			DateTime dtAlarm = dateStart.AddMinutes(-reminder.Minutes);
			String dtAlarmVal = dtAlarm.ToString("yyyyMMdd'T'HHmmss'Z'");

			result.Append(":");
			result.Append(dtAlarmVal);
			result.Append(";");
			
			if (reminder.Interval != 0)
				result.Append(TimeUtils.getIso8601Duration(reminder.Interval.ToString()));
			
			result.Append(";");
			result.Append(reminder.RepeatCount);

			result.Append(";");
			if (reminder.SoundFile != null)
				result.Append(reminder.SoundFile);
			
			result.Append("\r\n");

			return result;
		}
		#endregion

		#region Public Methods
		public override string Convert(Object obj) {
			calendar = (Calendar)obj;

			StringBuilder output = new StringBuilder(BEGIN_VCALEN);

			if (calendar.CalendarScale != null) {
				output.Append(ComposeFieldCalscale(calendar.CalendarScale));
			}
			if (calendar.Method != null) {
				output.Append(ComposeFieldMethod(calendar.Method));
			}
			if (calendar.ProductId != null) {
				output.Append(ComposeFieldProdId(calendar.ProductId));
			}
			if (calendar.Version != null) {
				output.Append(ComposeFieldVersion(calendar.Version));
			}
			// X-PROP
			output.Append(ComposeFieldXTag(calendar.XTags));

			//
			// VEVENT
			//
			if (calendar.Event != null) {
				output.Append(BEGIN_VEVENT);

				if (calendar.Event.Categories != null) {
					output.Append(ComposeFieldCategories(calendar.Event.Categories));
				}
				if (calendar.Event.ClassEvent != null) {
					output.Append(ComposeFieldClass(calendar.Event.ClassEvent));
				}
				if (calendar.Event.Description != null) {
					output.Append(ComposeFieldDescription(calendar.Event.Description));
				}
				if (calendar.Event.Latitude != null ||
					calendar.Event.Longitude != null) {
					output.Append(ComposeFieldGeo(calendar.Event.Latitude,
											calendar.Event.Longitude
										   ));
				}
				if (calendar.Event.Location != null) {
					output.Append(ComposeFieldLocation(calendar.Event.Location));
				}
				if (calendar.Event.Priority != null) {
					output.Append(ComposeFieldPriority(calendar.Event.Priority));
				}
				if (calendar.Event.Status != null) {
					output.Append(ComposeFieldStatus(calendar.Event.Status));
				}
				if (calendar.Event.Summary != null) {
					output.Append(ComposeFieldSummary(calendar.Event.Summary));
				}
				if (calendar.Event.DateEnd != null) {
					output.Append(ComposeFieldDtEnd(
							calendar.Event.DateEnd));
				}
				if (calendar.Event.DateStart != null) {
					output.Append(ComposeFieldDtStart(
							calendar.Event.DateStart));
				}

				//
				// NOTE: We decided not to store the duration but only Start and End
				//

				if (calendar.Event.Transport != null) {
					output.Append(ComposeFieldTransp(calendar.Event.Transport));
				}
				if (calendar.Event.Organizer != null) {
					output.Append(ComposeFieldOrganizer(calendar.Event.Organizer));
				}
				if (calendar.Event.Url != null) {
					output.Append(ComposeFieldUrl(calendar.Event.Url));
				}
				if (calendar.Event.UID != null) {
					output.Append(ComposeFieldUid(calendar.Event.UID));
				}
				if (calendar.Event.Rrule != null) {
					output.Append(ComposeFieldRrule(calendar.Event.Rrule));
				}
				if (calendar.Event.Created != null) {
					output.Append(composeFieldCreated(
							calendar.Event.Created));
				}
				if (calendar.Event.DateStamp != null) {
					output.Append(ComposeFieldDtstamp(
							calendar.Event.DateStamp));
				}
				if (calendar.Event.LastModified != null) {
					output.Append(ComposeFieldLastModified(
						calendar.Event.LastModified));
				}
				if (calendar.Event.Sequence != null) {
					output.Append(ComposeFieldSequence(calendar.Event.Sequence));
				}
				if (calendar.Event.Contact != null) {
					output.Append(this.ComposeFieldContact(calendar.Event.Contact));
				}
				if (calendar.Event.DisplayAlarm != null) {
					output.Append(ComposeFieldDAlarm(calendar.Event.DisplayAlarm));
				}
				if (calendar.Event.ProcedureAlarm != null) {
					output.Append(ComposeFieldPAlarm(calendar.Event.ProcedureAlarm));
				}
				if (calendar.Event.Reminder != null &&
					calendar.Event.Reminder.IsActive) {
					output.Append(
						ComposeFieldReminder(calendar.Event.DateStart,
											 calendar.Event.Reminder
						)
					);
				}

				// X-PROP
				XTagCollection eventXTag = calendar.Event.XTags;
				output.Append(ComposeFieldXTag(eventXTag));

				output.Append(END_VEVENT);
			}
			output.Append(END_VCALEN);

			return output.ToString();
		}
		#endregion
	}
}