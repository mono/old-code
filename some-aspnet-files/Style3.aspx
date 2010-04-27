<html>

<script language="C#" runat="server">

    void Page_Load(Object Src, EventArgs E )  {

        Message.InnerHtml += "<h5>Accessing Styles...</h5>";

        Message.InnerHtml += "The color of the span is: " + MySpan.Style["color"] + "<br>";
        Message.InnerHtml += "The width of the textbox is: " + MyText.Style["width"] + "<p>";

        Message.InnerHtml += "MySelect's style collection is: <br>";

        IEnumerator keys = MySelect.Style.Keys.GetEnumerator();

        while (keys.MoveNext()) {

            String key = (String)keys.Current;
            Message.InnerHtml += "<img src='/quickstart/images/bullet.gif'>&nbsp;&nbsp;";
            Message.InnerHtml += key + "=" + MySelect.Style[key] + "<br>";
        }
    }

    void Submit_Click(Object Src, EventArgs E )  {

        Message.InnerHtml += "<h5>Modifying Styles...</h5>";

        MySpan.Style["color"] = ColorSelect.Value;
        MyText.Style["width"] = "600";

        Message.InnerHtml += "The color of the span is: " + MySpan.Style["color"] + "<br>";
        Message.InnerHtml += "The width of the textbox is: " + MyText.Style["width"];
    }

</script>

<body>

  <form runat="server">

      <h3><font face="verdana">Programmatically Accessing Styles</font></h3>

      <div style="font: 8pt verdana;background-color:cccccc;border-color:black;border-width:1;border-style:solid;padding:1,10,25,10">
          <span id="Message" EnableViewState="false" runat="server"/>
          <p>
          Select a color for the span: <p>
          <select id="ColorSelect" style="font: 11pt verdana;font-weight:700;" runat="server">
            <option selected="selected">red</option>
            <option>green</option>
            <option>blue</option>
          </select>
          <input type="submit" runat="server" Value="Change Style" OnServerClick="Submit_Click">
      </div>

      <p><font face="verdana"><h4>Styled Span</h4></font><p>

      <span id="MySpan" style="font: 12pt verdana; color:orange;font-weight:700" runat="server">
          This is some literal text inside a styled span control
      </span>

      <p><font face="verdana"><h4>Styled Button</h4></font><p>

      <button id="MyButton" style="font: 8pt verdana;background-color:lightgreen;border-color:black;width:100" runat="server">Click me!</button>

      <p><font face="verdana"><h4>Styled Text Input</h4></font><p>

      Enter some text: <p>
      <input id="MyText" type="text" value="One, Two, Three" style="font: 14pt verdana;background-color:yellow;border-style:dashed;border-color:red;width:300;" runat="server"/>

      <p><font face="verdana"><h4>Styled Select Input</h4></font><p>

      Select an item: <p>
      <select id="MySelect" style="font: 14pt verdana;background-color:lightblue;color:purple;" runat="server">
        <option>Item 1</option>
        <option>Item 2</option>
        <option>Item 3</option>
      </select>

      <p><font face="verdana"><h4>Styled Radio Buttons</h4></font><p>

      Select an option: <p>
      <span style="font: 16 pt verdana;font-weight:300">
          <input id="MyRadio1" type="radio" name="Mode" checked style="width:50;background-color:red;zoom:200%" runat="server"/>Option 1<br>
          <input id="MyRadio2" type="radio" name="Mode" style="width:50;background-color:red;zoom:200%" runat="server"/>Option 2<br>
          <input id="MyRadio3" type="radio" name="Mode" style="width:50;background-color:red;zoom:200%" runat="server"/>Option 3
      </span>

    </form>

</body>
</html>

