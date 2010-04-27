//
// ContactConverter.cs:
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
using System.Text;

namespace Deveel.Pim {
	/// <summary>
	/// This object is a converter from a Contact object model to a vCard string
	/// </summary>
	class ContactConverter : BaseConverter {
		#region .ctor
		public ContactConverter(TimeZone timezone, string charset)
			: base(timezone, charset) {
		}
		#endregion

		#region Fields
		private static int INITIAL_CAPACITY = 1024; // 1 K bytes
		#endregion

		#region Private Methods
		private StringBuilder ComposevCardComponent(string propertyValue, ArrayList properties, string field) {
			StringBuilder result = new StringBuilder(120);

			try {
				string group = GetGrouping(properties);
				if (group != null) {
					result.Append(group);
					result.Append(".");
				}

				result.Append(field);

				string encodingParam = GetEncoding(properties);
				if (encodingParam == null)
					encodingParam = ENCODING_QT;

				string charsetParam = GetCharset(properties);
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
						// We encode the value only if the charset isn't PLAIN_CHARSET
						//
						// At this level we have always an ENCODING (at least QP)
						propertyValue = Encode(propertyValue, encodingParam, charsetParam);

						// We set the ENCODING and the CHARSET only if:
						// 1. we are using the QP and the result doesn't contain any '='
						//    (the value doesn't contain chars to encode)
						// or
						// 2. we have a different encoding from QP
						//    (in this way we preserve the original property encoding)
						if (String.Compare(ENCODING_QT, encodingParam, true) == 0 &&
							propertyValue.IndexOf("=") != -1) {
							result.Append(";ENCODING=");
							result.Append(encodingParam);
							result.Append(";CHARSET=");
							result.Append(charsetParam);
						} else if (String.Compare(ENCODING_QT, encodingParam, true) != 0) {
							result.Append(";ENCODING=");
							result.Append(encodingParam);
							result.Append(";CHARSET=");
							result.Append(charsetParam);
						}
					}
				}

				string languageParam = GetLanguage(properties);
				if (languageParam != null) {
					result.Append(";LANGUAGE=");
					result.Append(languageParam);
				}

				string valueParam = GetValue(properties);
				if (valueParam != null) {
					result.Append(";VALUE=");
					result.Append(valueParam);
				}

				string typeParam = GetType(properties);
				if (typeParam != null) {
					result.Append(";TYPE=");
					result.Append(typeParam);
				}

				result.Append(GetXParams(properties));

				result.Append(":");
				result.Append(propertyValue);
				result.Append("\r\n");
			} catch (Exception e) {
				throw new ConverterException("Error to compose vCard component ", e);
			}

			return result;
		}

		private StringBuilder ComposeFieldCategories(Property categories) {
			if (categories.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(categories);

			return ComposevCardComponent(categories.Value, properties, "CATEGORIES");
		}

		private StringBuilder ComposeFieldName(Name name) {
			if (name.LastName.Value == null &&
				name.FirstName.Value == null &&
				name.MiddleName.Value == null &&
				name.Salutation.Value == null &&
				name.Suffix.Value == null) {
				return new StringBuilder(0);
			}

			//TODO: optimize this to avoid inserting ';' character too many times...

			StringBuilder output = new StringBuilder(120); // Estimate 120 as needed
			ArrayList properties = new ArrayList();

			if (name.LastName.Value != null) {
				output.Append(name.LastName.Value);
				properties.Add(name.LastName);
			}

			output.Append(";");

			if (name.FirstName.Value != null) {
				output.Append(name.FirstName.Value);
				properties.Add(name.FirstName);
			}

			output.Append(";");

			if (name.MiddleName.Value != null) {
				output.Append(name.MiddleName.Value);
				properties.Add(name.MiddleName);
			}

			output.Append(";");

			if (name.Salutation.Value != null) {
				output.Append(name.Salutation.Value);
				properties.Add(name.Salutation);
			}

			output.Append(";");

			if (name.Suffix.Value != null) {
				output.Append(name.Suffix.Value);
				properties.Add(name.Suffix);
			}

			return ComposevCardComponent(output.ToString(), properties, "N");
		}

		private StringBuilder ComposeFieldFormalName(Property displayName) {
			if (displayName.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(displayName);

			return ComposevCardComponent(displayName.Value, properties, "FN");
		}

		private StringBuilder ComposeFieldNickname(Property nickname) {
			if (nickname.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(nickname);

			return ComposevCardComponent(nickname.Value, properties, "NICKNAME");
		}

		private StringBuilder ComposeFieldAddress(Address address, String type) {
			if ((address == null) ||
				(address.PostOfficeAddress.Value == null &&
				 address.RoomNumber.Value == null &&
				 address.Street.Value == null &&
				 address.City.Value == null &&
				 address.State.Value == null &&
				 address.PostalCode.Value == null &&
				 address.Country.Value == null &&
				 address.ExtendedAddress.Value == null)) {
				return new StringBuilder(0);
			}

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			//TODO: optimize this to avoid inserting ';' character too many times...

			if (address.PostOfficeAddress.Value != null) {
				output.Append(address.PostOfficeAddress.Value);
				properties.Add(address.PostOfficeAddress);
			}

			output.Append(";");
			if (address.ExtendedAddress.Value != null) {
				output.Append(address.ExtendedAddress.Value);
				properties.Add(address.ExtendedAddress);
			}

			output.Append(";");
			if (address.Street.Value != null) {
				output.Append(address.Street.Value);
				properties.Add(address.Street);
			}

			output.Append(";");
			if (address.City.Value != null) {
				output.Append(address.City.Value);
				properties.Add(address.City);
			}

			output.Append(";");
			if (address.State.Value != null) {
				output.Append(address.State.Value);
				properties.Add(address.State);
			}

			output.Append(";");
			if (address.PostalCode.Value != null) {
				output.Append(address.PostalCode.Value);
				properties.Add(address.PostalCode);
			}

			output.Append(";");
			if (address.Country.Value != null) {
				output.Append(address.Country.Value);
				properties.Add(address.Country);
			}

			if (type.Equals("HOME")) {
				return ComposevCardComponent(output.ToString(), properties, "ADR;HOME");
			} else if (type.Equals("OTHER")) {
				return ComposevCardComponent(output.ToString(), properties, "ADR");
			} else if (type.Equals("WORK")) {
				return ComposevCardComponent(output.ToString(), properties, "ADR;WORK");
			}

			return new StringBuilder(0);
		}

		private StringBuilder ComposeFieldPhoto(Property photo) {
			if (photo.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(photo);

			return ComposevCardComponent(photo.Value, properties, "PHOTO");
		}

		private string ComposeFieldBirthday(String birthday) {
			if (birthday == null)
				return String.Empty;

			try {
				birthday = TimeUtils.NormalizeToISO8601(birthday, TimeZone);
			} catch (Exception ex) {
				throw new ConverterException("Error parsing birthday", ex);
			}

			return ("BDAY:" + birthday + "\r\n");
		}

		private string ComposeFieldTelephone(PropertyCollection phones) {
			if (phones == null || phones.Count == 0)
				return String.Empty;

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			int size = phones.Count;
			for (int i = 0; i < size; i++) {
				Phone telephone = (Phone)phones[i];
				string phoneType = ComposePhoneType(telephone.PhoneType);

				properties.Clear();
				properties.Insert(0, telephone);

				output.Append(ComposevCardComponent(telephone.Value, properties, "TEL" + phoneType));
			}

			return output.ToString();
		}

		private string ComposePhoneType(string type) {
			if (type == null)
				return String.Empty;

			// Mobile phone
			if (type.Equals("MobileTelephoneNumber")) {
				return ";CELL";
			} else if (type.Equals("MobileHomeTelephoneNumber")) {
				return ";CELL;HOME";
			} else if (type.Equals("MobileBusinessTelephoneNumber")) {
				return ";CELL;WORK";
			}

			// Voice
			if (type.Equals("OtherTelephoneNumber")) {
				return ";VOICE";
			} else if (type.Equals("HomeTelephoneNumber")) {
				return ";VOICE;HOME";
			} else if (type.Equals("BusinessTelephoneNumber")) {
				return ";VOICE;WORK";
			}

			// FAX
			if (type.Equals("OtherFaxNumber")) {
				return ";FAX";
			} else if (type.Equals("HomeFaxNumber")) {
				return ";FAX;HOME";
			} else if (type.Equals("BusinessFaxNumber")) {
				return ";FAX;WORK";
			}

			// Pager
			if (type.Equals("PagerNumber")) {
				return ";PAGER";
			}

			for (int j = 2; j <= 10; j++) {
				// Mobile phone
				if (type.Equals("Mobile" + j + "TelephoneNumber")) {
					return ";CELL";
				} else if (type.Equals("MobileHome" + j + "TelephoneNumber")) {
					return ";CELL;HOME";
				} else if (type.Equals("MobileBusiness" + j + "TelephoneNumber")) {
					return (";CELL;WORK");
				}

				// Voice
				if (type.Equals("Other" + j + "TelephoneNumber")) {
					return ";VOICE";
				} else if (type.Equals("Home" + j + "TelephoneNumber")) {
					return ";VOICE;HOME";
				} else if (type.Equals("Business" + j + "TelephoneNumber")) {
					return ";VOICE;WORK";
				}

				// Fax
				if (type.Equals("Other" + j + "FaxNumber")) {
					return ";FAX";
				} else if (type.Equals("Home" + j + "FaxNumber")) {
					return ";FAX;HOME";
				} else if (type.Equals("Business" + j + "FaxNumber")) {
					return ";FAX;WORK";
				}

				// Pager
				if (type.Equals("PagerNumber" + j)) {
					return (";PAGER");
				}
			}

			// Others
			if (type.Equals("CarTelephoneNumber")) {
				return ";CAR;VOICE";
			} else if (type.Equals("CompanyMainTelephoneNumber")) {
				return ";WORK;PREF";
			} else if (type.Equals("PrimaryTelephoneNumber")) {
				return ";PREF;VOICE";
			}

			return String.Empty;
		}

		private string ComposeFieldEmail(PropertyCollection emails) {
			if (emails == null || emails.Count == 0)
				return String.Empty;

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			int size = emails.Count;
			for (int i = 0; i < size; i++) {

				Email email = (Email)emails[i];
				string emailType = ComposeEmailType(email.EmailType);

				properties.Clear();
				properties.Add(email);

				output.Append(ComposevCardComponent(email.Value, properties, "EMAIL" + emailType));
			}

			return output.ToString();
		}

		private string ComposeEmailType(String type) {
			if (type == null)
				return String.Empty;

			if (type.Equals("Email1Address")) {
				return ";INTERNET";
			} else if (type.Equals("Email2Address")) {
				return ";INTERNET;HOME";
			} else if (type.Equals("Email3Address")) {
				return ";INTERNET;WORK";
			}

			for (int j = 2; j <= 10; j++) {
				if (type.Equals("Other" + j + "EmailAddress")) {
					return ";INTERNET";
				} else if (type.Equals("HomeEmail" + j + "Address")) {
					return ";INTERNET;HOME";
				} else if (type.Equals("BusinessEmail" + j + "Address")) {
					return ";INTERNET;WORK";
				}
			}

			return String.Empty;
		}

		private string ComposeFieldWebPage(PropertyCollection webpages) {
			if (webpages == null || webpages.Count == 0)
				return String.Empty;

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			int size = webpages.Count;
			for (int i = 0; i < size; i++) {

				WebPage address = (WebPage)webpages[i];
				string webpageType = ComposeWebPageType(address.WebPageType);

				properties.Insert(0, address);

				output.Append(ComposevCardComponent(address.Value, properties, webpageType));
			}

			return output.ToString();
		}

		private string ComposeWebPageType(String type) {
			if (type == null) {
				return String.Empty;
			} else if (type.Equals("WebPage")) {
				return "URL";
			} else if (type.Equals("HomeWebPage")) {
				return "URL;HOME";
			} else if (type.Equals("BusinessWebPage")) {
				return "URL;WORK";
			}

			for (int j = 2; j <= 10; j++) {
				if (type.Equals("WebPage" + j)) {
					return "URL";
				} else if (type.Equals("Home" + j + "WebPage")) {
					return "URL;HOME";
				} else if (type.Equals("Business" + j + "WebPage")) {
					return "URL;WORK";
				}
			}

			return String.Empty;
		}

		private StringBuilder ComposeFieldPersonalLabel(Property label) {
			if (label.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(label);

			return ComposevCardComponent(label.Value, properties, "LABEL;HOME");
		}

		private StringBuilder ComposeFieldOtherLabel(Property label) {
			if (label.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(label);

			return ComposevCardComponent(label.Value, properties, "LABEL;OTHER");
		}

		private StringBuilder ComposeFieldBusinessLabel(Property label) {
			if (label.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(label);

			return ComposevCardComponent(label.Value, properties, "LABEL;WORK");
		}

		private StringBuilder ComposeFieldRole(Property role) {
			if (role.Value == null)
				return new StringBuilder(0);

			ArrayList properties = new ArrayList();
			properties.Add(role);

			return ComposevCardComponent(role.Value, properties, "ROLE");
		}

		private string ComposeFieldTitle(ArrayList titles) {
			if (titles == null || titles.Count == 0)
				return String.Empty;

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			int size = titles.Count;
			for (int i = 0; i < size; i++) {
				Title title = (Title)titles[i];
				properties.Insert(0, title);

				output.Append(ComposevCardComponent(title.Value, properties, "TITLE"));
			}

			return output.ToString();
		}

		private StringBuilder ComposeFieldOrg(Property company, Property department) {
			if (company.Value == null && department.Value == null)
				return new StringBuilder(0);

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			if (company.Value != null) {
				output.Append(company.Value);
				properties.Add(company);
			}

			output.Append(";");

			if (department.Value != null) {
				output.Append(department.Value);
				properties.Add(department);
			}

			return ComposevCardComponent(output.ToString(), properties, "ORG");
		}

		private string ComposeFieldXTag(ArrayList xTags) {
			if (xTags == null || xTags.Count == 0)
				return String.Empty;

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			int size = xTags.Count;
			for (int i = 0; i < size; i++) {

				XTag xtagObj = (XTag)xTags[i];

				Property xtag = xtagObj.Tag;
				string value = xtag.Value;

				properties.Clear();
				properties.Add(xtag);

				output.Append(ComposevCardComponent(xtag.Value, properties, xtagObj.TagValue));
			}

			return output.ToString();
		}

		private string ComposeFieldNote(ArrayList notes) {
			if (notes == null || notes.Count == 0)
				return "";

			StringBuilder output = new StringBuilder();
			ArrayList properties = new ArrayList();

			int size = notes.Count;
			for (int i = 0; i < size; i++) {

				Note note = (Note)notes[i];
				properties.Insert(0, note);

				output.Append(ComposevCardComponent(note.Value, properties, "NOTE"));
			}

			return output.ToString();
		}

		private string composeFieldUid(String uid) {
			if (uid == null)
				return String.Empty;

			return "UID:" + uid + "\r\n";
		}

		private string ComposeFieldTimezone(string tz) {
			if (tz == null)
				return String.Empty;

			return "TZ:" + tz + "\r\n";
		}

		private String ComposeFieldRevision(String revision) {
			if (revision == null)
				return String.Empty;

			return "REV:" + revision + "\r\n";
		}
		#endregion

		#region Public Methods
		public override string Convert(object obj) {
			Contact contact = (Contact)obj;
			StringBuilder output = new StringBuilder(INITIAL_CAPACITY);
			output.Append("BEGIN:VCARD\r\nVERSION:2.1\r\n");

			if (contact.Name != null) {
				output.Append(ComposeFieldName(contact.Name));
				output.Append(ComposeFieldFormalName(contact.Name.DisplayName));
				output.Append(ComposeFieldNickname(contact.Name.Nickname));
			}
			if (contact.PersonalDetail != null) {
				output.Append(ComposeFieldAddress(contact.PersonalDetail.Address, "HOME"));
				output.Append(ComposeFieldAddress(contact.PersonalDetail.OtherAddress, "OTHER"));
				output.Append(ComposeFieldBirthday(contact.PersonalDetail.Birthday));
				output.Append(ComposeFieldPersonalLabel(contact.PersonalDetail.Address.Label));
				output.Append(ComposeFieldOtherLabel(contact.PersonalDetail.OtherAddress.Label));
				if (contact.PersonalDetail != null) {
					output.Append(ComposeFieldTelephone(contact.PersonalDetail.Phones));
					output.Append(ComposeFieldEmail(contact.PersonalDetail.Emails));
					output.Append(ComposeFieldWebPage(contact.PersonalDetail.WebPages));
				}
			}
			if (contact.BusinessDetail != null) {
				output.Append(ComposeFieldAddress(contact.BusinessDetail.Address, "WORK"));
				output.Append(ComposeFieldRole(contact.BusinessDetail.Role));
				output.Append(ComposeFieldTitle(contact.BusinessDetail.Titles));
				output.Append(ComposeFieldOrg(contact.BusinessDetail.Company, contact.BusinessDetail.Department));
				output.Append(ComposeFieldBusinessLabel(contact.BusinessDetail.Address.Label));
				if (contact.BusinessDetail != null) {
					output.Append(ComposeFieldTelephone(contact.BusinessDetail.Phones));
					output.Append(ComposeFieldEmail(contact.BusinessDetail.Emails));
					output.Append(ComposeFieldWebPage(contact.BusinessDetail.WebPages));
				}
			}
			output.Append(ComposeFieldNote(contact.Notes));
			output.Append(ComposeFieldXTag(contact.XTags));
			output.Append(ComposeFieldRevision(contact.Revision));
			output.Append(ComposeFieldCategories(contact.Categories));
			output.Append(ComposeFieldPhoto(contact.PersonalDetail.Photo));
			output.Append(composeFieldUid(contact.UID));


			output.Append("END:VCARD\r\n");
			return output.ToString();
		}
		#endregion
	}
}