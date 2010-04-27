//
// vCardParser.cs:
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

options {
	IGNORE_CASE = true;
	DEBUG_TOKEN_MANAGER = false;
	STATIC = false;
	DEBUG_PARSER = false;
}

PARSER_BEGIN(vCardParser)

namespace Deveel.Pim;

using System;
using System.Collections;
using System.IO;
using System.Text;

class vCardParser {
    public vCardParser(Stream input, string tz, string defaultCharset)
        : this(input) {
        if (tz != null)
            defaultTimeZone = TimeZone.GetTimeZone(tz);
        if (defaultCharset != null)
            this.defaultCharset = defaultCharset;
    }

    public vCardParser(TextReader input, string tz, string defaultCharset)
        : this(input) {
        if (tz != null)
            defaultTimeZone = TimeZone.GetTimeZone(tz);
        if (defaultCharset != null)
            this.defaultCharset = defaultCharset;
    }
    
	private static readonly string DefaultCharset = "UTF-8";
	
	private Contact contact;
	
	private int cellTel        = 0;
	private int cellHomeTel    = 0;
	private int cellWorkTel    = 0;
	private int voiceTel       = 0;
	private int voiceHomeTel   = 0;
	private int voiceWorkTel   = 0;
	private int fax            = 0;
	private int faxHome        = 0;
	private int faxWork        = 0;
	private int car            = 0;
	private int pager          = 0;
	private int primary        = 0;
	private int companyMain    = 0;
	
	// the email
	private int email          = 0;
	private int emailHome      = 0;
	private int emailWork      = 0;
	
	// the body
	private int note           = 0;

	// the web page
	private int webPage        = 0;
	private int webPageHome    = 0;
	private int webPageWork    = 0;
	
	// the job title
	private int title          = 0;


	// the default timezone to use if datetimes are not UTC
	// if null, no conversion is performed
	private TimeZone defaultTimeZone = null;
	
	// the default charset to use if some properties are encoded
	// but without the charset
    private string defaultCharset = DefaultCharset;


    // Sets the Parameters encoding, charset, language, value for a given property
    // fetching them from the given ParamList.
    // Notice that if the items are not set (i.e. are null) in the ParamList, they
    // will be set to null in the property too (this is to avoid inconsistency when
    // the same vCard property is encountered more than one time, and thus overwritten
    // in the Contact object model).
    private void SetParameters(Property property, ParamList plist) {
        if (plist != null) {
            property.Encoding = plist.Encoding;
            property.Charset = plist.Charset;
            property.Language = plist.Language;
            property.Value = plist.Value;

            property.XParams = plist.XParams;
        }
    }

    // Sets the Parameters encoding, charset, language, value and group for a given property
    // fetching them from the given ParamList and the group Token.
    private void SetParameters(Property property, ParamList plist, Token group) {
        if (!(group==null)) {
            property.Group = group.image;
        } else {
            property.Group = null;
        }
        SetParameters(property,plist);
    }

    // Unfolds a string (i.e. removes all the CRLF characters)
    private string Unfold (string str) {
		//ISSUE: is it right to use Environment.NewLine here? the iCal file
		//       could be exported to a different OS...
        int ind = str.IndexOf(Environment.NewLine);
        if (ind == -1)
            return UnfoldNewLine(str);
        
        string tmpString1 = str.Substring(0,ind);
        string tmpString2 = str.Substring(ind+2);
        return UnfoldNewLine(Unfold(tmpString1+tmpString2));
   }

    // Unfolds a string (i.e. removes all the line break characters).
    // This function is meant to ensure compatibility with vCard documents
    // that adhere loosely to the specification
    private string UnfoldNewLine (string str) {
        int ind = str.IndexOf("\n");
        if (ind == -1)
            return str;
            
        string tmpString1 = str.Substring(0,ind);
        string tmpString2 = str.Substring(ind+1);
        return UnfoldNewLine(tmpString1+tmpString2);
    }


    // Decode the given text according to the given encoding and charset
    private string Decode(string text, string encoding, string propertyCharset) {
        // If input charset is null then set it with default charset
        StringBuilder sb = new StringBuilder(text);
        if (propertyCharset == null)
            propertyCharset = defaultCharset; // we use the default charset
            
        if (encoding != null) {
            if (String.Compare("QUOTED-PRINTABLE", encoding, true) == 0) {
                try {
                    return QuotedPrintable.Decode(sb.ToString(), propertyCharset);
                } catch (DecoderFallbackException de) {
                    throw new ParseException(de.Message);
                } catch (ArgumentException ue) {
                    throw new ParseException(ue.Message);
                }
            }
        } else {
            try {
				return Encoding.GetEncoding(propertyCharset).GetString(Encoding.Default.GetBytes(sb.ToString()));
            } catch (Exception ue) {
                throw new ParseException(ue.Message);
            }
        }
        return sb.ToString();
    }

    public Contact vCard() {
        return vCard(new Contact());
    }
}

PARSER_END(vCardParser)

TOKEN : {
    // "\n" is accepted for compatibility's sake
    <WSLS : ("\r\n" | "\n" | <WS>)+ > |
    <CATEGORIES : "CATEGORIES"> |
    <VERSION : "VERSION"> |
    <TITLE : "TITLE"> |
    <NICKNAME : "NICKNAME"> |
    <EMAIL : "EMAIL"> |
    <FN : "FN"> |
    <ORG : "ORG"> |
    <BDAY : "BDAY"> |
    <PHOTO : "PHOTO"> |
    <ADR : "ADR"> |
    <UID : "UID"> |
    <LABEL : "LABEL"> |
    <ROLE : "ROLE"> |
    <TZ : "TZ"> |
    <LOGO : "LOGO"> |
    <NOTE : "NOTE"> |
    <URL : "URL"> |
    <N : "N"> |
    <REV : "REV"> |
    <TEL : "TEL"> |
    
    <VCBEGIN : "BEGIN" (<WS>)? ":" (<WS>)? "VCARD" ("\r\n" | "\n") > |
    <VCEND : "END" (<WS>)? ":" (<WS>)? "VCARD" > |
    <EXTENSION: ["X", "x"] "-" (~["\r","\n",":", ";"])+>
}

<*>
TOKEN : {
    // "\n" is accepted for compatibility's sake
    <EOL : "\r\n" | "\n" > : DEFAULT
}

<CONTENTSTATE>
TOKEN : {
    // soft EOL
    <SEOL : <EOL> <WS> >
}

<*>
TOKEN : {
    <COLON : ":" > : CONTENTSTATE
}

<DEFAULT, PARAMSTATE>
TOKEN :
{
    <SEMICOLON : ";" > : PARAMSTATE
}

<DEFAULT, PARAMSTATE>
TOKEN :
{
    <EQUAL : "=" >
}

// DEFINITION OF KNOWN TOKEN

<CONTENTSTATE>
TOKEN :
{
    <CONTENTSTRING : (~["\r","\n"])+ >
}

<*>
TOKEN :
{
    <WS : (" " | "\t")+ >
}

<PARAMSTATE>
TOKEN :
{
    <PARAMSTRING : (~[ ":" , "\r" , "\n" , ";" , " " , "\t" , "=" , "." , "[" , "]" ])+ >
}

<DEFAULT>
TOKEN :
{
    <IDENTIFIER : ["a"-"z", "A"-"Z"] (["-", "a"-"z", "A"-"Z", "0"-"9"])* >
}


Contact vCard(Contact vCard) : {
	contact = vCard;
}
{
	( <WSLS> )? <VCBEGIN> [<WS>] ( content() )+ <VCEND> ( <WSLS> )? <EOF>
    
    { return contact; }
}

void content() : {
	Token group=null;
}
{
	LOOKAHEAD(2)
	[ group = <IDENTIFIER> "." ] Property(group) |
    NotImplemented()
}

// All properties we do not implement, but are willing to parse.
// We simply ignore them as allowed by the standard.
void NotImplemented() : {
}
{
	<IDENTIFIER> Parameters() <COLON> Text() <EOL>
}

void Property(Token group) : {
}
{
	Version() | 
	Title(group) | 
	Name(group) | 
	Mail(group) | 
	Tel(group) | 
	FName(group) | 
	Organization(group) | 
	Address(group) | 
	Role(group) | 
	Url(group) | 
	Rev(group) | 
	Nickname(group) | 
	Birthday(group) | 
	Label(group) | 
	Timezone(group) | 
	Logo(group) | 
	Note(group) | 
	Uid(group) | 
	Photo(group) | 
	Extension(group) | 
	Categories(group)
}

void Categories(Token group) : {
	ParamList plist = null;
    string content = null;
}
{
    <CATEGORIES> plist = Parameters() <COLON> content = Text() <EOL>
    {
		string text = Unfold(content);
		text = Decode(text, plist.Encoding, plist.Charset);
		contact.Categories.PropertyValue = text;
		SetParameters(contact.Categories, plist, group);
	}
}

void Extension(Token group) : {
	ParamList plist = null;
	string content = null;
	Token xtagName = null;
}
{
	xtagName = <EXTENSION> plist=Parameters() <COLON> content=Text() <EOL>
	{
		XTag tmpxTag = new XTag();
		string text = Unfold(content);
		text = Decode(text,plist.Encoding, plist.Charset);
		tmpxTag.Tag.PropertyValue = text;
		SetParameters(tmpxTag.Tag, plist, group);
		
		tmpxTag.TagValue = xtagName.image;
		contact.AddXTag(tmpxTag);
    }
}

void Version() : {
	Token ver;
}
{
	<VERSION> Parameters() <COLON> ver=<CONTENTSTRING> <EOL>
    {
		if (!(ver.image.Equals("2.1")) && 
			!(ver.image.Equals("3.0")))
			throw new ParseException("Encountered a vCard version other than 2.1 or 3.0 (" + ver.image + ")");
	}
}

void Title(Token group) : {
	ParamList plist = null;
	string content  = null;
}
{
	<TITLE> plist=Parameters() <COLON> content=Text() <EOL>
	{
		string text=Unfold(content);
		text = Decode(text,plist.Encoding, plist.Charset);
		Title tmptitle = new Title(text);
		SetParameters(tmptitle, plist, group);
		
		if (title == 0) {
			contact.BusinessDetail.Titles = new ArrayList();
			tmptitle.TitleType = "JobTitle";
		} else {
			tmptitle.TitleType = "JobTitle" + (title + 1);
		}
		
		contact.BusinessDetail.AddTitle(tmptitle);
		title++;
	}
}

void Mail(Token group) : {
    ParamList plist = null;
    string content  = null;
}
{
    <EMAIL> plist=Parameters() <COLON> content=Text() <EOL>
    {
        // NOTE: The email in outlook are email1address, email2address, email3address.
        // So the first INTERNET mail is email1address and the other INTERNET are labeled as OtherEmail2address, otherEmail3Address...
        // The first INTERNET;HOME email is Email2Address, and the other are HomeEmail2Address....
        // The first INTERNET;WORK email is Email3Address, and the other are BusinessEmail2Address....
        // If there is not specify the email's type then consider the email like
        // Email1Address and the other are OtherEmail2Address...
        if (plist.Count == 0 ||
           (plist.Count == 1 && plist.ContainsKey("INTERNET")) ||
           (plist.ContainsKey("PREF") && plist.ContainsKey("INTERNET"))) {
            string text = Unfold(content);
            text = Decode(text, plist.Encoding, plist.Charset);
            Email tmpmail = new Email(text);
            SetParameters(tmpmail,plist,group);
            if (email == 0) {
                //if (emailHome == 0) {
                //    contact.PersonalDetail.Emails = new ArrayList();
                //}
                tmpmail.EmailType = "Email1Address";
            } else {
                tmpmail.EmailType = "OtherEmail" + (email + 1) + "Address";
            }
            contact.PersonalDetail.Emails.Add(tmpmail);
            email++;
        } else if (plist.ContainsKey("HOME")) {
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            Email tmpmail = new Email(text);
            SetParameters(tmpmail,plist,group);
            if (emailHome == 0) {
                //if (email == 0) {
                //    contact.PersonalDetail.Emails = new ArrayList();
                //}
                tmpmail.EmailType = "Email2Address";
            } else {
                tmpmail.EmailType = "HomeEmail" + (emailHome + 1) + "Address";
            }
            contact.PersonalDetail.Emails.Add(tmpmail);
            emailHome++;
        } else if (plist.ContainsKey("WORK")) {
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            Email tmpmail = new Email(text);
            SetParameters(tmpmail,plist,group);
            if (emailWork == 0) {
                //contact.BusinessDetail.Emails = new ArrayList();
                tmpmail.EmailType = "Email3Address";
            } else {
                tmpmail.EmailType = "BusinessEmail" + (emailWork + 1) + "Address";
            }
            contact.BusinessDetail.Emails.Add(tmpmail);
            emailWork++;
        } else {
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            Email tmpmail = new Email(text);
            SetParameters(tmpmail,plist,group);
            if (email == 0) {
                //if (emailHome == 0) {
                //    contact.PersonalDetail.Emails = new ArrayList();
                //}
                tmpmail.EmailType = "Email1Address";
            } else {
                tmpmail.EmailType = "OtherEmail" + (email + 1) + "Address";
            }
            contact.PersonalDetail.Emails.Add(tmpmail);
            email++;
        
        }
    }
}

void Url(Token group) : {
    ParamList plist = null;
    string content  = null;
}
{
    <URL> plist=Parameters() <COLON> content=Text() <EOL>
    {
        if (!plist.ContainsKey("HOME") && !plist.ContainsKey("WORK")) {
            WebPage tmppage = new WebPage();
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            tmppage.PropertyValue = text;
            SetParameters(tmppage,plist,group);

            if (webPage == 0) {
                //if (webPageHome == 0) {
                //    contact.PersonalDetail.WebPages = new ArrayList();
                //}
                tmppage.WebPageType = "WebPage";
            } else {
                tmppage.WebPageType = "WebPage" + (webPage + 1);
            }
            contact.PersonalDetail.WebPages.Add(tmppage);
            webPage++;
        }

        if (plist.ContainsKey("HOME")) {
            WebPage tmppage = new WebPage();
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            tmppage.PropertyValue = text;
            SetParameters(tmppage,plist,group);

            if (webPageHome == 0) {
                //if (webPage == 0) {
                //    contact.PersonalDetail.WebPages = new ArrayList();
                //}
                tmppage.WebPageType = "HomeWebPage";
            } else {
                tmppage.WebPageType = "Home" + (webPageHome + 1) + "WebPage";
            }
            contact.PersonalDetail.WebPages.Add(tmppage);
            webPageHome++;
        }
            
        if (plist.ContainsKey("WORK")) {
            WebPage tmppage = new WebPage();
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            tmppage.PropertyValue = text;
            SetParameters(tmppage,plist,group);

            if (webPageWork == 0) {
                //contact.BusinessDetail.WebPages = new ArrayList();
                tmppage.WebPageType = "BusinessWebPage";
            } else {
                tmppage.WebPageType = "Business" + (webPageWork + 1) + "WebPage";
            }
            contact.BusinessDetail.WebPages.Add(tmppage);
            webPageWork++;
        }
    }
}

void Tel(Token group) :
{
    ParamList plist = null;
    string content  = null;
}
{
    <TEL> plist=Parameters() <COLON> content=Text() <EOL>
    {
        content=Decode(content,plist.Encoding, plist.Charset);
        if (plist.ContainsKey("WORK")) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone, plist, group);
            // Check if it is the very first for a business detail.
            //if ((cellWorkTel == 0) && (voiceWorkTel == 0) &&
            //    (faxWork == 0) && (pager == 0) && (primary == 0) &&
            //    (companyMain == 0)) {
            //    contact.BusinessDetail.Phones = new ArrayList();
            //}

            if (plist.ContainsKey("CELL")) {
                if (cellWorkTel == 0) {
                    tmphone.PhoneType = "MobileBusinessTelephoneNumber";
                } else {
                    tmphone.PhoneType = "MobileBusiness" + (cellWorkTel + 1) + "TelephoneNumber";
                }
                contact.BusinessDetail.Phones.Add(tmphone);
                cellWorkTel++;
            }

            if (plist.ContainsKey("VOICE") || (plist.Count == 1)) {
                if (voiceWorkTel == 0) {
                    tmphone.PhoneType = "BusinessTelephoneNumber";
                } else {
                    tmphone.PhoneType = "Business" + (voiceWorkTel + 1) + "TelephoneNumber";
                }
                contact.BusinessDetail.Phones.Add(tmphone);
                voiceWorkTel++;
            }
            if (plist.ContainsKey("FAX")) {
                if (faxWork == 0) {
                    tmphone.PhoneType = "BusinessFaxNumber";
                } else {
                    tmphone.PhoneType = "Business" + (faxWork + 1) + "FaxNumber";
                }
                contact.BusinessDetail.Phones.Add(tmphone);
                faxWork++;
            }
            // suppose that can exists only one voice work telephone pref.
            if (plist.ContainsKey("PREF")) {
                tmphone.PhoneType = "CompanyMainTelephoneNumber";
                contact.BusinessDetail.Phones.Add(tmphone);
                companyMain++;
            }
        } else if ((plist.ContainsKey("CELL") && plist.Count == 1) ||
            (plist.ContainsKey("CELL") && plist.ContainsKey("VOICE"))) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone, plist, group);

            if (cellTel == 0) {
                //if ((cellHomeTel == 0) && (voiceTel == 0) && (voiceHomeTel == 0) && (fax == 0) && (faxHome == 0) && (car == 0)) {
                //    contact.PersonalDetail.Phones = new ArrayList();
                //}
                tmphone.PhoneType = "MobileTelephoneNumber";
            } else {
                tmphone.PhoneType = "Mobile" + (cellTel + 1) + "TelephoneNumber";
            }
            contact.PersonalDetail.Phones.Add(tmphone);
            cellTel++;
        } else if (plist.ContainsKey("HOME") && plist.ContainsKey("CELL")) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone, plist, group);
            if (cellHomeTel == 0) {
                //if ((cellTel == 0) && (voiceTel == 0) && (voiceHomeTel == 0) && (fax == 0) && (faxHome == 0) && (car == 0)) {
                //    contact.PersonalDetail.Phones = new ArrayList();
                //}
                tmphone.PhoneType = "MobileHomeTelephoneNumber";
            } else {
                tmphone.PhoneType = "MobileHome" + (cellHomeTel + 1) + "TelephoneNumber";
            }
            contact.PersonalDetail.Phones.Add(tmphone);
            cellHomeTel++;
        } else if (plist.Count == 1 && plist.ContainsKey("VOICE")) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone,plist,group);
            if (voiceTel == 0) {
                //if ((cellTel == 0) && (cellHomeTel == 0) && (voiceHomeTel == 0) && (fax == 0) && (faxHome == 0) && (car == 0)) {
                //    contact.PersonalDetail.Phones = new ArrayList();
                //}
                tmphone.PhoneType = "OtherTelephoneNumber";
            } else {
                tmphone.PhoneType = "Other" + (voiceTel + 1) + "TelephoneNumber";
            }
            contact.PersonalDetail.Phones.Add(tmphone);
            voiceTel++;
        } else if ((plist.ContainsKey("VOICE") && plist.ContainsKey("HOME"))  ||
            (plist.Count == 1 && plist.ContainsKey("HOME"))  )       {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone, plist, group);
            if (voiceHomeTel == 0) {
                //if ((cellTel == 0) && (cellHomeTel == 0) && (voiceTel == 0) && (fax == 0) && (faxHome == 0) && (car == 0)) {
                //    contact.PersonalDetail.Phones = new ArrayList();
                //}
                tmphone.PhoneType = "HomeTelephoneNumber";
            } else {
                tmphone.PhoneType = "Home" + (voiceHomeTel + 1) + "TelephoneNumber";
            }
            contact.PersonalDetail.Phones.Add(tmphone);
            voiceHomeTel++;
        } else if (plist.Count == 1 && plist.ContainsKey("FAX")) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone, plist, group);
            if (fax == 0) {
                //if ((cellTel == 0) && (cellHomeTel == 0) && (voiceTel == 0) && (voiceHomeTel == 0) && (faxHome == 0) && (car == 0)) {
                //    contact.PersonalDetail.Phones = new ArrayList();
                //}
                tmphone.PhoneType = "OtherFaxNumber";
            } else {
                tmphone.PhoneType = "Other" + (fax + 1) + "FaxNumber";
            }
            contact.PersonalDetail.Phones.Add(tmphone);
            fax++;
        } else if (plist.ContainsKey("HOME") && plist.ContainsKey("FAX")) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone,plist,group);
            if (faxHome == 0) {
                //if ((cellTel == 0) && (cellHomeTel == 0) && (voiceTel == 0) && (voiceHomeTel == 0) && (fax == 0) && (car == 0)) {
                //    contact.PersonalDetail.Phones = new ArrayList();
                //}
                tmphone.PhoneType = "HomeFaxNumber";
            } else {
                tmphone.PhoneType = "Home" + (faxHome + 1) + "FaxNumber";
            }
            contact.PersonalDetail.Phones.Add(tmphone);
            faxHome++;
        } else if (plist.ContainsKey("CAR")) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone,plist,group);
            tmphone.PhoneType = "CarTelephoneNumber";
            //if ((car == 0) && (cellTel == 0) && (cellHomeTel == 0) && (voiceTel == 0) && (voiceHomeTel == 0) && (fax == 0) && (faxHome == 0)) {
            //    contact.PersonalDetail.Phones = new ArrayList();
            //}
            contact.PersonalDetail.Phones.Add(tmphone);
            car++;
        } else if (plist.ContainsKey("PAGER")) {
            Phone tmphone = new Phone(content);
            SetParameters(tmphone,plist,group);
            if (pager == 0) {
                //if ((cellWorkTel == 0) && (voiceWorkTel == 0) && (faxWork == 0) && (primary == 0) && (companyMain == 0)) {
                //    contact.BusinessDetail.Phones = new ArrayList();
                //}
                tmphone.PhoneType = "PagerNumber";
            } else {
                tmphone.PhoneType = "PagerNumber" + (pager + 1);
            }
            contact.BusinessDetail.Phones.Add(tmphone);
            pager++;
        } else if ((plist.ContainsKey("PREF") && plist.ContainsKey("VOICE")) ||
            (plist.ContainsKey("PREF") && plist.Count == 1)) {

            // suppose that can exists only one voice telephone pref.
            Phone tmphone = new Phone(content);
            SetParameters(tmphone,plist,group);
            //if ((primary == 0) && (cellWorkTel == 0) && (voiceWorkTel == 0) && (faxWork == 0) && (pager == 0) && (companyMain == 0)) {
            //    contact.BusinessDetail.Phones = new ArrayList();
            //}
            tmphone.PhoneType = "PrimaryTelephoneNumber";
            contact.BusinessDetail.Phones.Add(tmphone);
            primary++;
        }
    }
}

void FName(Token group) :
{
    ParamList plist     = null;
    string content      = null;
}
{
    <FN>  plist=Parameters() <COLON> content=Text() <EOL>
    {
        content=Decode(content,plist.Encoding, plist.Charset);
        contact.Name.DisplayName.PropertyValue = content;
        SetParameters(contact.Name.DisplayName, plist, group);
    }
}

void Role(Token group) :
{
    ParamList plist     = null;
    string content      = null;
}
{
    <ROLE> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        contact.BusinessDetail.Role.PropertyValue = text;
        SetParameters(contact.BusinessDetail.Role, plist, group);
    }
}

void Rev(Token group) : {
    ParamList plist     = null;
    string content      = null;
}
{
    <REV> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        contact.Revision = text;
    }
}

void Nickname(Token group) : {
    ParamList plist = null;
    string content = null;
}
{
    <NICKNAME> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        contact.Name.Nickname.PropertyValue = text;
        SetParameters(contact.Name.Nickname,plist,group);
    }
}

void Organization(Token group) : {
    ParamList plist = null;
    string content = null;
    FieldsList flist = new FieldsList();
    string encoding = null;
}
{
    <ORG> plist=Parameters() <COLON> content=Text() <EOL>
    {
        flist.AddValue(content);

        int pos;  // Position in tlist (i.e. position of the current value field)

        // Organization Name
        pos = 0;
        if (flist.Count > pos) {
            string text=Unfold(flist[pos]);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.BusinessDetail.Company.PropertyValue = text;
            SetParameters(contact.BusinessDetail.Company, plist, group);
        }

        // Organizational Unit
        pos = 1;
        if (flist.Count > pos) {
            string text=Unfold(flist[pos]);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.BusinessDetail.Department.PropertyValue = text;
            SetParameters(contact.BusinessDetail.Department, plist, group);
        }
    }
}

void Address(Token group) : {
    ParamList plist = null;
    string content = null;
    FieldsList flist = new FieldsList();
}
{
    <ADR> plist=Parameters() <COLON> content=Text() <EOL>
    {
        flist.AddValue(content);

        int pos;  // Position in tlist (i.e. position of the current value field)

        if (plist.ContainsKey("WORK")) {
        // Businnes Address

            //Post Office Address
            pos = 0;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.BusinessDetail.Address.PostOfficeAddress.PropertyValue = text;
                SetParameters(contact.BusinessDetail.Address.PostOfficeAddress, plist, group);
            }
            // Extended Address
            pos = 1;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.BusinessDetail.Address.ExtendedAddress.PropertyValue = text;
                SetParameters(contact.BusinessDetail.Address.ExtendedAddress, plist, group);
            }
            // Street
            pos = 2;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.BusinessDetail.Address.Street.PropertyValue = text;
                SetParameters(contact.BusinessDetail.Address.Street, plist, group);
            }
            // Locality
            pos = 3;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.BusinessDetail.Address.City.PropertyValue = text;
                SetParameters(contact.BusinessDetail.Address.City, plist, group);
            }
            // Region
            pos = 4;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.BusinessDetail.Address.State.PropertyValue = text;
                SetParameters(contact.BusinessDetail.Address.State, plist, group);
            }
            // Postal Code
            pos = 5;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.BusinessDetail.Address.PostalCode.PropertyValue = text;
                SetParameters(contact.BusinessDetail.Address.PostalCode, plist, group);
            }
            // Country
            pos = 6;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.BusinessDetail.Address.Country.PropertyValue = text;
                SetParameters(contact.BusinessDetail.Address.Country, plist, group);
            }
        }
        if (plist.ContainsKey("HOME")) {
        // Home Address
            //Post Office Address
            pos = 0;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.Address.PostOfficeAddress.PropertyValue = text;
                SetParameters(contact.PersonalDetail.Address.PostOfficeAddress, plist, group);
            }
            // Extended Address
            pos = 1;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.Address.ExtendedAddress.PropertyValue = text;
                SetParameters(contact.PersonalDetail.Address.ExtendedAddress, plist, group);
            }
            // Street
            pos = 2;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.Address.Street.PropertyValue = text;
                SetParameters(contact.PersonalDetail.Address.Street, plist, group);
            }
            // Locality
            pos = 3;
            if (flist.Count > pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.Address.City.PropertyValue = text;
                SetParameters(contact.PersonalDetail.Address.City, plist, group);
            }
            // Region
            pos = 4;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.Address.State.PropertyValue = text;
                SetParameters(contact.PersonalDetail.Address.State, plist, group);
            }
            // Postal Code
            pos = 5;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.Address.PostalCode.PropertyValue = text;
                SetParameters(contact.PersonalDetail.Address.PostalCode, plist, group);
            }
            // Country
            pos = 6;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.Address.Country.PropertyValue = text;
                SetParameters(contact.PersonalDetail.Address.Country, plist, group);
            }
        }
        // other address
        if (!plist.ContainsKey("HOME") && !plist.ContainsKey("WORK")) {
        // Other Address
            //Post Office Address
            pos = 0;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.OtherAddress.PostOfficeAddress.PropertyValue = text;
                SetParameters(contact.PersonalDetail.OtherAddress.PostOfficeAddress, plist, group);
            }
            // Extended Address
            pos = 1;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.OtherAddress.ExtendedAddress.PropertyValue = text;
                SetParameters(contact.PersonalDetail.OtherAddress.ExtendedAddress, plist, group);
            }
            // Street
            pos = 2;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.OtherAddress.Street.PropertyValue = text;
                SetParameters(contact.PersonalDetail.OtherAddress.Street, plist, group);
            }
            // Locality
            pos = 3;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.OtherAddress.City.PropertyValue = text;
                SetParameters(contact.PersonalDetail.OtherAddress.City, plist, group);
            }
            // Region
            pos = 4;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.OtherAddress.State.PropertyValue = text;
                SetParameters(contact.PersonalDetail.OtherAddress.State, plist, group);
            }
            // Postal Code
            pos = 5;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.OtherAddress.PostalCode.PropertyValue = text;
                SetParameters(contact.PersonalDetail.OtherAddress.PostalCode, plist, group);
            }
            // Country
            pos = 6;
            if (flist.Count>pos) {
                string text=Unfold(flist[pos]);
                text=Decode(text,plist.Encoding, plist.Charset);
                contact.PersonalDetail.OtherAddress.Country.PropertyValue = text;
                SetParameters(contact.PersonalDetail.OtherAddress.Country, plist, group);
            }
        }
    }
}

void Birthday(Token group) : {
    ParamList plist = null;
    string content = null;
}
{
    <BDAY> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        string birthday = text;
        
        try {
            birthday = TimeUtils.NormalizeToISO8601(birthday, defaultTimeZone);
            contact.PersonalDetail.Birthday = birthday;
        } catch (Exception e) {
            // If the birthday isn't in a valid format (see TimeUtils.NormalizeToISO8601), 
            // ignore it
        }
    }
}

void Label(Token group) : {
    ParamList plist = null;
    string content = null;
}
{
    <LABEL> plist=Parameters() <COLON> content=Text() <EOL>
    {
        if (plist.ContainsKey("WORK")) {
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.BusinessDetail.Address.Label.PropertyValue = text;
            SetParameters(contact.BusinessDetail.Address.Label, plist, group);
        }
        if (plist.ContainsKey("HOME")) {
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.PersonalDetail.Address.Label.PropertyValue = text;
            SetParameters(contact.PersonalDetail.Address.Label, plist, group);
        }
        if (!plist.ContainsKey("HOME") && !plist.ContainsKey("WORK")) {
            string text=Unfold(content);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.PersonalDetail.OtherAddress.Label.PropertyValue = text;
            SetParameters(contact.PersonalDetail.OtherAddress.Label, plist, group);
        }
    }
}

void Timezone(Token group) : {
    ParamList plist = null;
    string content = null;
}
{
    <TZ> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        contact.TimeZone = text;
    }
}

void Logo(Token group) : {
    ParamList plist = null;
    string content = null;
}
{
    <LOGO> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        contact.BusinessDetail.Logo.PropertyValue = text;
        SetParameters(contact.BusinessDetail.Logo, plist, group);
    }
}

void Note(Token group) : {
    ParamList plist = null;
    string content = null;
}
{
    <NOTE> plist=Parameters() <COLON> content=Text() <EOL>
    {
        Note tmpnote = new Note();
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        tmpnote.PropertyValue = text;
        SetParameters(tmpnote, plist, group);

        note++;
        if (note == 1) {
            contact.Notes = new ArrayList();
            tmpnote.NoteType = "Body";
        } else {
            tmpnote.NoteType = "Body" + note;
        }
        contact.AddNote(tmpnote);
    }
}

void Uid(Token group) : {
    ParamList plist = null;
    string content = null;
}
{
    <UID> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        contact.UID = text;
    }
}

void Photo(Token group) : {
    ParamList plist = null;
    string content = null;
    FieldsList flist = new FieldsList();
}
{
    <PHOTO> plist=Parameters() <COLON> content=Text() <EOL>
    {
        string text=Unfold(content);
        text=Decode(text,plist.Encoding, plist.Charset);
        contact.PersonalDetail.Photo.PropertyValue = text;
        SetParameters(contact.PersonalDetail.Photo, plist, group);
    }
}

void Name(Token group) : {
    ParamList plist = null;
    string content = null;
    FieldsList flist = new FieldsList();
}
{
    <N> plist = Parameters() <COLON> content =Text() <EOL>
    {
        flist.AddValue(content);

        int pos;  // Position in tlist (i.e. position of the current value field)

        // Last name
        pos=0;
        if (flist.Count > pos) {
            string text=Unfold(flist[pos]);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.Name.LastName.PropertyValue = text;
            SetParameters(contact.Name.LastName, plist, group);
        }
        // First name
        pos=1;
        if (flist.Count > pos) {
            string text=Unfold(flist[pos]);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.Name.FirstName.PropertyValue = text;
            SetParameters(contact.Name.FirstName, plist, group);
        }
        // Middle name
        pos=2;
        if (flist.Count > pos) {
            string text=Unfold(flist[pos]);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.Name.MiddleName.PropertyValue = text;
            SetParameters(contact.Name.MiddleName, plist, group);
        }
        // Prefix
        pos=3;
        if (flist.Count > pos) {
            string text=Unfold(flist[pos]);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.Name.Salutation.PropertyValue = text;
            SetParameters(contact.Name.Salutation, plist, group);
        }
        // Suffix
        pos=4;
        if (flist.Count > pos) {
            string text=Unfold(flist[pos]);
            text=Decode(text,plist.Encoding, plist.Charset);
            contact.Name.Suffix.PropertyValue = text;
            SetParameters(contact.Name.Suffix, plist, group);
        }
    }
}

// Parses property Parameters and returns a ParamList with the generated Tokens.
// Example: in TEL;WORK;VOICE;CHARSET=ISO-8859-8:+1-800-555-1234 will parse the
// ";WORK;VOICE;CHARSET=ISO-8859-8:" part and return a list containing "WORK" and
// "VOICE" as types, and "ISO-8859-8" as charset.
ParamList Parameters() : {
    ParamList paramList = new ParamList();
    Token paramName = null, paramValue = null;
}
{
	(
		<SEMICOLON> paramName = <PARAMSTRING> ( <EQUAL> paramValue = <PARAMSTRING> )?
		{
			paramList.Add(paramName.image, (paramValue == null) ? null : paramValue.image);
		}
	)*
	{ return paramList; }
}


// Parses the fields of a property value and returns a StringBuffer with text after ":".
//The single element comma separated are retrieved by function in FieldsList class
string Text() : {
    Token t = null;
    StringBuilder sb = new StringBuilder();
}
{
    ( ( <SEOL> )? t = <CONTENTSTRING> { sb.Append(t.image); } )*
    { return sb.ToString(); }
}