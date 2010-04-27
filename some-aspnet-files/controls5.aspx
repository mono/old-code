<html>

    <script language="C#" runat="server">

        void Page_Load(Object Src, EventArgs E) {

           Random randomGenerator = new Random(DateTime.Now.Millisecond);

           int randomNum = randomGenerator.Next(0, 3);

           switch(randomNum) {

              case 0:
                Name.Text = "Scott";
                break;

              case 1:
                Name.Text = "Fred";
                break;

              case 2:
                Name.Text = "Adam";
                break;
           }

           AnchorLink.NavigateUrl = "controls_navigationtarget.aspx?name=" + System.Web.HttpUtility.UrlEncode(Name.Text);
        }

    </script>

    <body>

       <h3><font face="Verdana">Performing Page Navigation (Scenario 1)</font></h3>

       <p>

       This sample demonstrates how to generate a HTML Anchor tag that will cause the client to
       navigate to a new page when he/she clicks it within the browser.

       <p>

       <hr>

       <p>

       <asp:hyperlink id="AnchorLink" font-size=24 runat=server>
          Hi <asp:label id="Name" runat=server/> please click this link!
       </asp:hyperlink>

    </body>

</html>

