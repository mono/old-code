/***************************************************************************
 *  Copyright 2005 Novell, Inc.
 *  Aaron Bockover <aaron@aaronbock.net>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using System.Text.RegularExpressions;

#if BUILT_TEST
public class Test
{
    public static void Main()
    {
        string [] rawLicenses = {
            "1995 Example Band. Licensed to the public under http://creativecommons.org/licenses/by/2.0/ verify at http://example.com/cclicenses.html",
            "1995 So and So Licensed to the public under http://something.org verify at http://something.org",
            "Copyright (C) 2005 Someone. licensed to the public under http://something.url",
            "2005 licensed to the public under http://blah.org",
            "Copyright (C) 2005 Not-CC.org"
        };
        
        foreach(string rawLicense in rawLicenses) {
            CreativeCommons lic = CreativeCommons.Parse(rawLicense);
            Console.WriteLine("---- LICENSE ----");
            Console.WriteLine("[{0}]\n", rawLicense);         
            Console.WriteLine("    Copyright Holder: " + lic.Copyright);
            Console.WriteLine("    License URL: " + lic.LicenseUrl);
            Console.WriteLine("    Verify URL: " + lic.VerifyUrl);
            Console.WriteLine("    Creative Commons: " + (lic.IsCreativeCommons ? "YES" : "NO"));
            Console.WriteLine("    Verifiable Creative Commons: " + (lic.IsVerifiableCreativeCommons ? "YES" : "NO"));
            Console.WriteLine("");
        }

        Console.WriteLine(new CreativeCommons("2005 Aaron Bockover", 
            "http://creativecommons.org/my-license",
            "http://aaronbock.net/my-verify"
        ));
    }
}
#endif

public class CreativeCommons
{
    private string license;
    private string copyright;
    private string licenseUrl = null;
    private string verifyUrl = null;

    public static CreativeCommons Parse(string rawLicense)
    {
        return new CreativeCommons(rawLicense);
    }

    public CreativeCommons(string copyright, string licenseUrl, 
        string verifyUrl)
    {
        this.copyright = copyright;
        this.licenseUrl = licenseUrl;
        this.verifyUrl = verifyUrl;
    }

    public CreativeCommons(string rawLicense)
    {
        string [] parts = Regex.Split(rawLicense, 
            @"licensed\ to\ the\ public\ under\ |verify\ at\ ",
            RegexOptions.IgnoreCase);
        
        license = rawLicense;
        
        if(parts.Length > 0) {
            copyright = parts[0].Trim();
            if(copyright.EndsWith(".")) {
                copyright = copyright.Substring(0, copyright.Length - 1);
            }

            if(parts.Length > 1) {
                licenseUrl = parts[1].Trim();

                if(parts.Length > 2) {
                    verifyUrl = parts[2].Trim();
                }
            }
        }
    }

    public string License {
        get {
            if(license != null) {
                return license;
            }

            string lic = String.Empty;
    
            if(copyright != null) {
                lic += copyright;

                if(licenseUrl != null) {
                    lic += " Licensed to the public under " + licenseUrl;
                    
                    if(verifyUrl != null) {
                        lic += " verify at " + verifyUrl;
                    }
                 }

                 return lic;
             }
             
             return null;
        }

        set {
            license = value;
        }
    }

    public string Copyright {
        get {
            return copyright;
        }

        set {
            copyright = value;
        }
    }

    public string LicenseUrl {
        get {
            return licenseUrl;
        }

        set {
            copyright = value;
        }
    }

    public string VerifyUrl {
        get {
            return verifyUrl;
        }

        set {
            verifyUrl = value;
        }
    }

    public bool IsCreativeCommons {
        get {
            return (LicenseUrl != null);
        }
    }

    public bool IsVerifiableCreativeCommons {
        get {
            return IsCreativeCommons && VerifyUrl != null;
        }
    }

    public override string ToString()
    {
        return License;
    }
}

