<html>

    <script language="C#" runat="server">

        void EnterBtn_Click(Object Src, EventArgs E) {

            // Navigate to a new page (passing name as a querystring argument) if
            // user has entered a valid name value in the <asp:textbox>

            if (Name.Text != "") {
               Response.Redirect("controls_navigationtarget.aspx?name=" + System.Web.HttpUtility.UrlEncode(Name.Text));
            }
            else {
               Message.Text = "Hey! Please enter your name in the textbox!";
            }
        }

    </script>

    <body>

       <h3><font face="Verdana">Performing Page Navigation (Scenario 2)</font></h3>

       <p>

       This sample demonstrates how to navigate to a new page from within a &lt;asp:button&gt; click event,
       passing a &lt;asp:textbox&gt; value as a querystring argument (validating first that the a legal
       textbox value has been specified).

       <p>

       <hr>

       <form action="controls6.aspx" runat="server">

          <font face="Verdana">

             Please enter your name: <asp:textbox id="Name" runat=server/>
                                     <asp:button text="Enter" Onclick="EnterBtn_Click" runat=server/>

             <p>

             <asp:label id="Message" forecolor="red" font-bold="true" runat=server/>

          </font>

       </form>

    </body>

</html>

