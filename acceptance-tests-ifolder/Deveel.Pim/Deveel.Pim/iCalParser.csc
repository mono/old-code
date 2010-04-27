//
// iCalParser.cs:
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

PARSER_BEGIN(iCalParser)

namespace Deveel.Pim;

using System;
using System.IO;
using System.Text;

class iCalParser {
    public iCalParser(Stream input, string tz, string defaultCharset)
        : this(input) {
        if (tz != null) 
            defaultTimeZone = TimeZone.GetTimeZone(tz);
        if (defaultCharset != null)
            this.defaultCharset = defaultCharset;
    }
    
    public iCalParser(TextReader input, string tz, string defaultCharset)
        : this(input) {
        if (tz != null) 
            defaultTimeZone = TimeZone.GetTimeZone(tz);
        if (defaultCharset != null)
            this.defaultCharset = defaultCharset;
    }
    
    private static readonly string DEFAULT_CHARSET = Encoding.Default.EncodingName;
	
    // the default timezone to use if datetimes are not UTC
    // if null, no conversion is performed
    private TimeZone defaultTimeZone = null;

    // the default charset to use if some properties are encoded
    // but without the charset
    private String defaultCharset = DEFAULT_CHARSET;

    private Calendar calendar;
    private string dtalarm = null; // Date to start reminder

    private void SetParameters(Property property, ParamList plist) {
        if (plist != null) {
            property.AlternateText = plist.Altrep;
            property.CommonName = plist.Cn;
            property.CalendarUserType = plist.Cutype;
            property.DelegatedFrom = plist.DelegatedFrom;
            property.DelegatedTo = plist.DelegatedTo;
            property.Directory = plist.Dir;
            property.Encoding = plist.Encoding;
            property.Language = plist.Language;
            property.Member = plist.Member;
            property.PartecipationStatus = plist.Partstat;
            property.Related = plist.Related;
            property.SentBy = plist.Sentby;
            property.Value = plist.Value;
            property.XParams = plist.XParams;
        }
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
    // This function is meant to ensure compatibility with iCalendar documents
    // that adhere loosely to the specification
    private string UnfoldNewLine (string str) {
        int ind = str.IndexOf("\n");
        if (ind == -1)
            return str;
            
        string tmpString1 = str.Substring(0,ind);
        string tmpString2 = str.Substring(ind+1);
        return UnfoldNewLine(tmpString1+tmpString2);
    }

    // Datetimes are supposed to be in UTC as specified by the 'Z' at the tail
    // of the date text. However, if datetimes is not in UTC (some devices do
    // not send UTC datetimes), we consider the datetime as in defaultTimeZone
    // and convert it to UTC (and the right format).
    private string FixTimeZone(String t) {
        if (t == null || t.Length == 0)
            return "";

        if (t.EndsWith("Z"))
            // This is already in UTC!!!
            return t;

        //
        // The date is not UTC. Shall we convert it to a local date? If not,
        // we just make it UTC adding a tailing 'Z'
        //

        if (defaultTimeZone == null)
            return t + 'Z';

        try {
            return TimeUtils.convertLocalDateToUTC(t, defaultTimeZone);
        } catch (Exception e) {
            return t;
        }
    }

    // This equal to the grammar parsing rule, but javacc is not able
    // to use equal names rules with different arguments as in java.
    public Calendar iCal() {
        return iCal(new Calendar());
    }

    //
    // Fixed properties like start date, end date, duration and so on.
    // Here we can compute for example the interval of reminder
    //
    private void FixProperties() {
        string dateStart = calendar.Event.DateStart.StringValue;
        string dateEnd = calendar.Event.DateEnd.StringValue;
        string duration = calendar.Event.Duration.StringValue;

        // Compute End Date by Start Date and Duration
        dateEnd = TimeUtils.getDTEnd(dateStart, duration, dateEnd);
        calendar.Event.DateEnd.PropertyValue = dateEnd;
        
        // If the event is an all day check if there is the end date:
        // 1) if end date is null then set it with start date value.
        // 2) if end date is not into yyyy-MM-dd or yyyyMMdd format then
        //    normalize it in yyyy-MM-dd format.
        bool startAllDay = TimeUtils.isInAllDayFormat(dateStart);
        bool endAllDay = TimeUtils.isInAllDayFormat(dateEnd);

        if (startAllDay) {
            if (dateEnd == null) {
                dateEnd = dateStart;
            } else {
                if (!endAllDay) {
                    dateEnd = TimeUtils.convertDateFromTo(dateEnd, TimeUtils.PATTERN_YYYY_MM_DD);
                }
            }
        }
        
        // We have to check if the dates are not in the DayFormat but are however
        // relative to an all day event.
        if (!calendar.Event.IsAllDay) {
            bool isAllDayEvent = false;
            
            try {
                // Before to check the dates, we have to convert them in local format
                // in order to have 00:00:00 as time for the middle night 
                string tmpDateStart = TimeUtils.convertUTCDateToLocal(dateStart, defaultTimeZone);
                string tmpDateEnd = TimeUtils.convertUTCDateToLocal(dateEnd, defaultTimeZone);

                isAllDayEvent = TimeUtils.isAllDayEvent(tmpDateStart, tmpDateEnd);
                
                if (isAllDayEvent) {
                    // Convert the dates in DayFormat
                    dateStart = TimeUtils.convertDateFromTo(tmpDateStart, TimeUtils.PATTERN_YYYY_MM_DD);
                    dateEnd = TimeUtils.convertDateFromTo(tmpDateEnd, TimeUtils.PATTERN_YYYY_MM_DD);
                    
                    calendar.Event.DateStart.PropertyValue = dateStart;
                    calendar.Event.DateEnd.PropertyValue = dateEnd;

                    calendar.Event.IsAllDay = true;
                }
            } catch (Exception e) {
                // We ignore this exception. The event isn't an all day event
            }
        }

        // Calculate minutes before to start reminder alarm
        if (dtalarm != null)
            calendar.Event.Reminder.Minutes = TimeUtils.getAlarmMinutes(dateStart, dtalarm);
    }
}

PARSER_END(iCalParser)

TOKEN : {
    // "\n" is accepted for compatibility's sake
    <WSLS : ("\r\n" | "\n" | <WS>)+ > |
    <VCAL_BEGIN : "BEGIN" <COLON> "VCALENDAR"> |
    <VCAL_END : "END" <COLON> "VCALENDAR"> |
    <VEVENT_BEGIN : "BEGIN" <COLON> "VEVENT"> |
    <VEVENT_END : "END" <COLON> "VEVENT"> |
    <CALSCALE : "CALSCALE"> |
    <METHOD : "METHOD"> |
    <PRODID : "PRODID"> |
    <VERSION : "VERSION"> |
    <CATEGORIES : "CATEGORIES"> |
    <CLASS : "CLASS"> |
    <CREATED : "CREATED"> |
    <DCREATED : "DCREATED"> |
    <DESCRIPTION : "DESCRIPTION"> |
    <DTSTART : "DTSTART"> |
    <GEO : "GEO"> |
    <LASTMODIFIED : "LAST-MODIFIED"> |
    <LOCATION : "LOCATION"> |
    <ORGANIZER : "ORGANIZER"> |
    <PRIORITY : "PRIORITY"> |
    <DTSTAMP : "DTSTAMP"> |
    <SEQUENCE : "SEQUENCE"> |
    <STATUS : "STATUS"> |
    <SUMMARY : "SUMMARY"> |
    <TRANSP : "TRANSP"> |
    <UID : "UID"> |
    <URL : "URL"> |
    <DTEND : "DTEND"> |
    <DURATION : "DURATION"> |
    <CONTACT : "CONTACT"> |
    <RRULE : "RRULE"> |
    
    <DALARM : "DALARM"> |
    <AALARM : "AALARM"> |
    <PALARM : "PALARM"> |

	// General tokens    
    <EXTENSION: ["X", "x"] "-" (~["\r","\n",":",";"])+>
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
    <DOUBLE_QUOTE : "\"">
}

<DEFAULT, PARAMSTATE>
TOKEN : {
    <EQUAL: "=">
}

<*>
TOKEN : {
    <COLON : ":" > : CONTENTSTATE
}

<DEFAULT, PARAMSTATE>
TOKEN : {
    <SEMICOLON : ";" > : PARAMSTATE
}


<CONTENTSTATE>
TOKEN : {
    <CONTENTSTRING : (~["\r","\n"])+ >
}

<*>
TOKEN : {
    // Linear White Space
    <LWS : (" " | "\t") >
}

<*>
TOKEN : {
    // White Space
    <WS : (<LWS>)+ >
}

<PARAMSTATE>
TOKEN : {
    <PARAMSTRING : (~[ ":" , "\r" , "\n" , ";" , " " , "\t" , "=" , "." , "[" , "]" ] | (["\""](~["\r","\n"])*["\""]) )+ >
}

<DEFAULT>
TOKEN : {
    <IDENTIFIER: ["a"-"z", "A"-"Z"] (["-", "a"-"z", "A"-"Z", "0"-"9"])* >
}


Calendar iCal(Calendar iCalendar) : {
    calendar = iCalendar;
}
{
    ( <WSLS> )? <VCAL_BEGIN> ( <WSLS> )? ( content() )+ <VCAL_END> (<WSLS>)?
    <EOF> {
        FixProperties();
        return calendar;
    }
}

void content() : {
}
{
    calscale() |
    method() |
    prodid() |
    version() |
    VEvent() |
    extensionCal() |
    notImplemented()
}

void calscale() : {
    string calscale;
    ParamList plist = null;
}
{
    <CALSCALE> plist = Parameters() <COLON> calscale = text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.CalendarScale.PropertyValue = calscale;
        SetParameters(calendar.CalendarScale, plist);
    }
}

void method() : {
    string method;
    ParamList plist = null;
}
{
    <METHOD> plist = Parameters() <COLON> method = text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Method.PropertyValue = method;
        SetParameters(calendar.Method, plist);
    }
}

void prodid() :
{
    string prodid;
    ParamList plist = null;
}
{
    <PRODID> plist = Parameters() <COLON> prodid = text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.ProductId.PropertyValue = prodid;
        SetParameters(calendar.ProductId, plist);
    }
}

void version() :
{
    string version;
    ParamList plist = null;
}
{
    <VERSION> plist = Parameters() <COLON> version = text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Version.PropertyValue = version;
        SetParameters(calendar.Version, plist);
    }
}

void VEvent() : {
}
{
    <VEVENT_BEGIN><WSLS> (contentEvent())* (<WSLS>)? <VEVENT_END><WSLS>
}

void extensionCal() : {
    string content;
    ParamList plist  = null;
    FieldsList flist = new FieldsList();
    Token xtagName   = null;
}
{
    xtagName = <EXTENSION> plist = Parameters() <COLON> content=text(plist.Encoding, plist.Charset) <EOL>
    {
        flist.AddStringValue(content);
        
        XTag tmpxTag = new XTag();
        tmpxTag.Tag.PropertyValue = flist.Element;
        SetParameters(tmpxTag.Tag,plist);
        tmpxTag.TagValue = xtagName.image;
        calendar.XTags.Add(tmpxTag);
    }
}

void contentEvent() : {
}
{
    categories() |
    classEvent() |
    created() |
    dcreated() |
    description() |
    dtstart() |
    geo() |
    lastmodified() |
    location() |
    organizer() |
    priority() |
    dtstamp() |
    sequence() |
    status() |
    summary() |
    transp() |
    uid() |
    url() |
    dtend() |
    duration() |
    contact() |
    rrule() |
    extensionEvent() |
    dalarm() |
    aalarm() |
    palarm() |
    notImplemented()
}

void categories() :
{
    string categories;
    ParamList plist = null;
}
{
    <CATEGORIES> plist=Parameters() <COLON> categories = text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Categories.PropertyValue = categories;
        SetParameters(calendar.Event.Categories,plist);
    }
}

void classEvent() : {
    string classEvent;
    ParamList plist = null;
}
{
    <CLASS> plist = Parameters() <COLON> classEvent = text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.ClassEvent.PropertyValue = classEvent;
        SetParameters(calendar.Event.ClassEvent,plist);
    }
}

void created() : {
    Token tag = null;
    string created;
    ParamList plist = null;
}
{
    tag=<CREATED> plist=Parameters() <COLON> created=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Created.Tag = tag.image;
        calendar.Event.Created.PropertyValue = created;
        SetParameters(calendar.Event.Created,plist);
    }
}

void dcreated() : {
    Token tag = null;
    string dcreated;
    ParamList plist = null;
}
{
    tag=<DCREATED> plist=Parameters() <COLON> dcreated=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Created.Tag = tag.image;
        calendar.Event.Created.PropertyValue = FixTimeZone(dcreated);
        SetParameters(calendar.Event.Created,plist);
    }
}

void description() : {
    string description;
    ParamList plist=null;
}
{
    <DESCRIPTION> plist=Parameters() <COLON> description=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Description.PropertyValue = description;
        SetParameters(calendar.Event.Description,plist);
    }
}

void dtstart() : {
    string dtstart;
    ParamList plist = null;
}
{
    <DTSTART> plist=Parameters() <COLON> dtstart=text(plist.Encoding, plist.Charset) <EOL>
    {
        //
        // Check if the event is an AllDay event
        //
        if (TimeUtils.isInAllDayFormat(dtstart)) {
            dtstart = TimeUtils.convertDateFromTo(dtstart, TimeUtils.PATTERN_YYYY_MM_DD);
            calendar.Event.DateStart.PropertyValue = dtstart;
            calendar.Event.IsAllDay = true;
        } else {
            calendar.Event.IsAllDay = false;
            calendar.Event.DateStart.PropertyValue = FixTimeZone(dtstart);
        }
        SetParameters(calendar.Event.DateStart,plist);
    }
}

void geo() : {
    string geo;
    ParamList  plist = null;
    FieldsList flist = new FieldsList();
}
{
    <GEO> plist=Parameters() <COLON> geo=text(plist.Encoding, plist.Charset) <EOL>
    {
        flist.AddValue(geo);

        int pos = 0;

        // Latitude
        pos = 0;
        if (flist.Count > pos) {
            calendar.Event.Latitude.PropertyValue = flist[pos];
            SetParameters(calendar.Event.Latitude,plist);
        }

        // Longitude
        pos = 1;
        if (flist.Count > pos) {
            calendar.Event.Longitude.PropertyValue = flist[pos];
            SetParameters(calendar.Event.Longitude,plist);
        }
    }
}

void lastmodified() : {
    string lm;
    ParamList  plist = null;
}
{
    <LASTMODIFIED> plist=Parameters() <COLON> lm=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.LastModified.PropertyValue = FixTimeZone(lm);
        SetParameters(calendar.Event.LastModified,plist);
    }
}

void location() : {
    string location;
    ParamList plist;
}
{
    <LOCATION> plist=Parameters() <COLON> location=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Location.PropertyValue = location;
        SetParameters(calendar.Event.Location,plist);
    }
}

void organizer() : {
    string organizer;
    ParamList plist;
}
{
    <ORGANIZER> plist=Parameters() <COLON> organizer=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Organizer.PropertyValue = organizer;
        SetParameters(calendar.Event.Organizer,plist);
    }
}

void priority() : {
    string priority;
    ParamList plist;
}
{
    <PRIORITY> plist=Parameters() <COLON> priority=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Priority.PropertyValue = priority;
        SetParameters(calendar.Event.Priority,plist);
    }
}

void dtstamp() : {
    string dtstamp;
    ParamList plist;
}
{
    <DTSTAMP> plist=Parameters() <COLON> dtstamp=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.DateStamp.PropertyValue = FixTimeZone(dtstamp);
        SetParameters(calendar.Event.DateStamp,plist);
    }
}

void sequence() : {
    string sequence;
    ParamList plist;
}
{
    <SEQUENCE> plist=Parameters() <COLON> sequence=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Sequence.PropertyValue = sequence;
        SetParameters(calendar.Event.Sequence,plist);
    }
}

void status() : {
    string status;
    ParamList plist;
}
{
    <STATUS> plist=Parameters() <COLON> status=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Status.PropertyValue = status;
        SetParameters(calendar.Event.Status,plist);
    }
}

void summary() : {
    string summary;
    ParamList plist;
}
{
    <SUMMARY> plist=Parameters() <COLON> summary=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Summary.PropertyValue = summary;
        SetParameters(calendar.Event.Summary,plist);
    }
}

void transp() : {
    string transp;
    ParamList plist;
}
{
    <TRANSP> plist=Parameters() <COLON> transp=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Transport.PropertyValue = transp;
        SetParameters(calendar.Event.Transport,plist);
    }
}

void uid() : {
    string uid;
    ParamList plist;
}
{
    <UID> plist=Parameters() <COLON> uid=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.UID.PropertyValue = uid;
        SetParameters(calendar.Event.UID,plist);
    }
}

void url() : {
    string url;
    ParamList plist;
}
{
    <URL> plist=Parameters() <COLON> url=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Url.PropertyValue = url;
        SetParameters(calendar.Event.Url,plist);
    }
}


void dtend() : {
    string dtend;
    ParamList plist;
}
{
    <DTEND> plist=Parameters() <COLON> dtend=text(plist.Encoding, plist.Charset) <EOL>
    {
        //
        // Check if the event is an AllDay event
        //
        if (TimeUtils.isInAllDayFormat(dtend)) {
            dtend = TimeUtils.convertDateFromTo(dtend, TimeUtils.PATTERN_YYYY_MM_DD);
            calendar.Event.IsAllDay = true;
            calendar.Event.DateEnd.PropertyValue = dtend;
        } else {
            calendar.Event.IsAllDay = false;
            calendar.Event.DateEnd.PropertyValue = FixTimeZone(dtend);
        }
        SetParameters(calendar.Event.DateEnd,plist);
    }
}


void duration() : {
    string duration;
    ParamList plist;
}
{
    <DURATION>  plist=Parameters() <COLON> duration=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Duration.PropertyValue = duration;
        SetParameters(calendar.Event.Duration,plist);
    }
}

void contact() : {
    string contact;
    ParamList plist;
}
{
    <CONTACT> plist=Parameters() <COLON> contact=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Contact.PropertyValue = contact;
        SetParameters(calendar.Event.Contact,plist);
    }
}

void rrule() : {
   string rrule;
   ParamList plist;
}
{
    <RRULE> plist=Parameters() <COLON> rrule=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.Rrule.PropertyValue = rrule;
        SetParameters(calendar.Event.Rrule,plist);
    }
}

void extensionEvent() : {
    string content;
    ParamList plist  = null;
    FieldsList flist = new FieldsList();
    Token xtagName   = null;
}
{
    xtagName=<EXTENSION> plist=Parameters() <COLON> content=text(plist.Encoding, plist.Charset) <EOL>
    {
        flist.AddStringValue(content);
        XTag tmpxTag = new XTag();
        tmpxTag.Tag.PropertyValue = flist.Element;
        SetParameters(tmpxTag.Tag,plist);
        tmpxTag.TagValue = xtagName.image;
        calendar.Event.XTags.Add(tmpxTag);
    }
}

// ------ Not Standard tag-------
void aalarm() : {
    string aalarm;
    ParamList plist;
    FieldsList flist = new FieldsList();
}
{
    <AALARM> plist=Parameters() <COLON> aalarm=text(plist.Encoding, plist.Charset) <EOL>
    {
        flist.AddValue(aalarm);
        if (calendar.Event.Reminder == null)
            calendar.Event.Reminder = new Reminder();

        calendar.Event.Reminder.IsActive = true;

        int pos;

        // Minutes before start reminder
        pos = 0;
        if (flist.Count > pos)
            dtalarm = FixTimeZone(Unfold(flist[pos]));

        // Interval in which the reminder has to be repeated
        pos = 1;
        if (flist.Count > pos)
            calendar.Event.Reminder.Interval = TimeUtils.getAlarmInterval(Unfold(flist[pos]));

        // The number of times that the reminder has to be repeated
        // if the value in VCALENDAR is "" the default value in Remainder is 0
        pos = 2;        
        if (flist.Count > pos) {
            string tmpRepeatNo = Unfold(flist[pos]);
            
            if (tmpRepeatNo.Length != 0)
                calendar.Event.Reminder.RepeatCount = Int32.Parse(tmpRepeatNo);
        }

        // The sound file to be played when the reminder is executed.
        pos = 3;
        if (flist.Count > pos) {
            calendar.Event.Reminder.SoundFile = Unfold(flist[pos]);
            
            // Set the type of reminder. 8 = olSound plays the sound file
            calendar.Event.Reminder.Options = 8;
        }

        SetParameters(calendar.Event.Reminder,plist);
    }
}

void dalarm() : {
    string dalarm;
    ParamList plist;
}
{
    <DALARM> plist=Parameters() <COLON> dalarm=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.DisplayAlarm.PropertyValue = dalarm;
        SetParameters(calendar.Event.DisplayAlarm,plist);
    }
}

void palarm() : {
    string palarm;
    ParamList plist;
}
{
    <PALARM> plist=Parameters() <COLON> palarm=text(plist.Encoding, plist.Charset) <EOL>
    {
        calendar.Event.ProcedureAlarm.PropertyValue = palarm;
        SetParameters(calendar.Event.ProcedureAlarm,plist);
    }
}

// All properties we do not implement, but are willing to parse.
// We simply ignore them as allowed by the standard.
void notImplemented() : {
    Token identifier;
}
{
    identifier=<IDENTIFIER> Parameters() <COLON> text(null, null) <EOL>
}

// --------------------------- SERVICE BNF EXPANSIONS -----------------------------------

ParamList Parameters() : {
    ParamList paramList = new ParamList();
    Token paramName = null, paramValue = null;
}
{
    (
        <SEMICOLON> paramName=<PARAMSTRING> ( <EQUAL> paramValue = <PARAMSTRING> )?
        {
            paramList.Add(paramName.image, (paramValue == null) ? null : paramValue.image);
        }
    )*
    { return paramList; }
}

string text(string encoding, string propertyCharset) :
{
    Token t = null;
    StringBuilder sb = new StringBuilder();
}
{
    ( ( <SEOL> )? t = <CONTENTSTRING> { sb.Append(t.image); } )*
    {
        // If input charset is null then set it with default charset
        if (propertyCharset == null) {
            propertyCharset = defaultCharset; // we use the default charset
        }
        if (encoding != null) {
            if (encoding.Equals("QUOTED-PRINTABLE")) {
                try {
				    //ISSUE: check the QuotedPrintable implementation: not sure
				    //       it returns correct value.
                    return QuotedPrintable.Decode(sb.ToString(), propertyCharset);
                } catch (DecoderFallbackException de) {
                    throw new ParseException(de.Message);
                } catch (ArgumentException ue) {
                    throw new ParseException(ue.Message);
                }
            }
        } else {
            try {
                //ISSUE: check this...
                return Encoding.GetEncoding(propertyCharset).GetString(Encoding.Default.GetBytes(sb.ToString()));
            } catch (ArgumentException ue) {
                throw new ParseException(ue.Message);
            }
        }
        return sb.ToString();
    }
}