<html>

    <script language="C#" runat="server">

        void Page_Load(Object Sender, EventArgs e) {

           if (!Page.IsPostBack) {
              NameLabel.Text = Request.QueryString["Name"];         
           }
        }

    </script>

    <body>

       <h3><font face="Verdana">Handling Page Navigation</font></h3>

       <p>

       This sample demonstrates how to receive a navigation request from another
       page, and extract the querystring argument within the Page_Load event.  

       <p>

       <hr>

      
       <form action="controls_NavigationTarget.aspx" runat=server>

          <font face="Verdana"> 

             Hi <asp:label id="NameLabel" runat=server/>!

          </font>

       </form>

    </body>

</html>

