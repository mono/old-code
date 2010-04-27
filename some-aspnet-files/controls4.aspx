<html>

    <script language="C#" runat="server">

        void AddBtn_Click(Object Src, EventArgs E) {

            if (AvailableFonts.SelectedIndex != -1) {
               InstalledFonts.Items.Add(new ListItem(AvailableFonts.SelectedItem.Value));
               AvailableFonts.Items.Remove(AvailableFonts.SelectedItem.Value);
            }
        }

        void AddAllBtn_Click(Object Src, EventArgs E) {

            while (AvailableFonts.Items.Count != 0) {

               InstalledFonts.Items.Add(new ListItem(AvailableFonts.Items[0].Value));
               AvailableFonts.Items.Remove(AvailableFonts.Items[0].Value);
            }
        }

        void RemoveBtn_Click(Object Src, EventArgs E) {

            if (InstalledFonts.SelectedIndex != -1) {

               AvailableFonts.Items.Add(new ListItem(InstalledFonts.SelectedItem.Value));
               InstalledFonts.Items.Remove(InstalledFonts.SelectedItem.Value);
            }
        }

        void RemoveAllBtn_Click(Object Src, EventArgs E) {

            while (InstalledFonts.Items.Count != 0) {

               AvailableFonts.Items.Add(new ListItem(InstalledFonts.Items[0].Value));
               InstalledFonts.Items.Remove(InstalledFonts.Items[0].Value);
            }
        }

    </script>

    <body>

       <h3><font face="Verdana">Handling Multiple Control Action Events</font></h3>

       <p>

       This sample demonstrates how to handle multiple control action events raised from 
       different &lt;asp:button&gt; controls.

       <p>

       <hr>

       <form action="controls4.aspx" runat=server>

          <table>
             <tr>
               <td>
                 Available Fonts 
               </td>
               <td>
                 <!-- Filler -->
               </td>
               <td>
                 Installed Fonts 
               </td>
             </tr> 
             <tr>
               <td>
                 <asp:listbox id="AvailableFonts" width="100px" runat=server>
                    <asp:listitem>Roman</asp:listitem>
                    <asp:listitem>Arial Black</asp:listitem>
                    <asp:listitem>Garamond</asp:listitem>
                    <asp:listitem>Somona</asp:listitem>
                    <asp:listitem>Symbol</asp:listitem>
                 </asp:listbox>
               </td>
               <td>
                 <!-- Filler -->
               </td>
               <td>
                 <asp:listbox id="InstalledFonts" width="100px" runat=server>
                    <asp:listitem>Times</asp:listitem>
                    <asp:listitem>Helvetica</asp:listitem>
                    <asp:listitem>Arial</asp:listitem>
                 </asp:listbox>
               </td>
             </tr> 
             <tr>
               <td>
                 <!-- Filler -->
               </td>
               <td>
                 <asp:button text="<<" OnClick="RemoveAllBtn_Click" runat=server/>
                 <asp:button text="<" OnClick="RemoveBtn_Click" runat=server/> 
                 <asp:button text=">" OnClick="AddBtn_Click" runat=server/>
                 <asp:button text=">>" OnClick="AddAllBtn_Click" runat=server/> 
               </td>
               <td>
                 <!-- Filler -->
               </td>
             </tr>
          </table>

       </form>

    </body>

</html>

