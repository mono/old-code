<html>

    <script language="C#" runat="server">

        void EnterBtn_Click(Object Src, EventArgs E) {
            Message.Text = "Hi " + Name.Text + ", welcome to ASP.NET!";
        }

    </script>

    <body>

       <h3><font face="Verdana">Handling Control Action Events</font></h3>

       <p>

       This sample demonstrates how to access a &lt;asp:textbox&gt; server control within the "Click" 
       event of a &lt;asp:button&gt;, and use its content to modify the text of a &lt;asp:label&gt;.

       <p>

       <hr>

       <form action="controls3.aspx" runat=server>

          <font face="Verdana"> 

             Please enter your name: <asp:textbox id="Name" runat=server/> 
                                     <asp:button text="Enter" Onclick="EnterBtn_Click" runat=server/>

             <p>

             <asp:label id="Message"  runat=server/>

          </font>

       </form>

    </body>

</html>

