<script language="JavaScript" type="text/javascript">
<!--
	if (window.navigator.appName == "Microsoft Internet Explorer") {
		document.write('<style type="text/css" media="all">@import url(/inc/ie.css);<\/style>');
	}
// -->
</script>

<script language="JavaScript1.2" type="text/javascript">
<!--
function openExternal(url,w,h,s,n){
	var attr=(typeof w!='undefined'&&w!='noW'&&typeof h!='undefined'&&h!='noH')?'width='+w+',height='+h+',resizable':'';
	attr=(typeof s!='undefined'&&s!='noscroll')?attr+',scrollbars':attr;
	var winExt=(typeof n=='undefined')?window.open(url,'wwwexternal',attr):window.open(url,n,attr);
	if(winExt.blur)winExt.focus();
};
//-->
</script>

<script language="JavaScript" type="text/javascript">

// Coded by Travis Beckham - http://www.squidfingers.com | http://www.podlob.com
// If want to use this code, feel free to do so, but please leave this message intact.
// --- version date: 06/02/03 ---

// Node Functions
<!--
if(!window.Node){
	var Node = {ELEMENT_NODE : 1, TEXT_NODE : 3};
}
function checkNode(node, filter){
	return (filter == null || node.nodeType == Node[filter] || node.nodeName.toUpperCase() == filter.toUpperCase());
}
function getChildren(node, filter){
	var result = new Array();
	var children = node.childNodes;
	for(var i = 0; i < children.length; i++){
		if(checkNode(children[i], filter)) result[result.length] = children[i];
	}
	return result;
}
function getChildrenByElement(node){
	return getChildren(node, "ELEMENT_NODE");
}
function getChildrenBeforeIndex(node, index) {
	var result = new Array();
	var children = node.childNodes;
	for(var i = 0; i < index; i++){
		result[result.length] = children[i];
	}
	return result;
}
function getFirstChild(node, filter){
	var child;
	var children = node.childNodes;
	for(var i = 0; i < children.length; i++){
		child = children[i];
		if(checkNode(child, filter)) return child;
	}
	return null;
}
function getIndexOfFirstChild(node, filter){
	var child;
	var children = node.childNodes;
	for(var i = 0; i < children.length; i++){
		child = children[i];
		if(checkNode(child, filter)) return i;
	}
	return null;
}
function getFirstChildByText(node){
	return getFirstChild(node, "TEXT_NODE");
}
function getFirstChildByBold(node) {
	return getFirstChild(node, "B");
}
function getFirstChildByOL(node) {
	return getFirstChild(node, "OL");
}
function getNextSibling(node, filter){
	for(var sibling = node.nextSibling; sibling != null; sibling = sibling.nextSibling){
		if(checkNode(sibling, filter)) return sibling;
	}
	return null;
}
function getNextSiblingByElement(node){
	return getNextSibling(node, "ELEMENT_NODE");
}

function getIndexOfFirstChildByOL(node) {
	return getIndexOfFirstChild(node, "OL");
}

// ||||||||||||||||||||||||||||||||||||||||||||||||||
// Menu Functions & Properties

var activeMenu = null;

function showMenu(){
	if(activeMenu){
		activeMenu.className = "";
		getNextSiblingByElement(activeMenu).style.display = "none";
	}
	if(this == activeMenu){
		activeMenu = null;
	}else{
		this.className = "active";
		getNextSiblingByElement(this).style.display = "block";
		activeMenu = this;
	}
	return false;
}
function initMenu(){
	var menus, menu, text, a, i;
	menus = getChildrenByElement(document.getElementById("menu"));
	for(i = 0; i < menus.length; i++){
		menu = menus[i];   // "menu" is an li element
		//text = getFirstChildByText(menu);   // assumes there is a text child.  The highlighter puts in Bold children
		submenu = getFirstChildByOL(menu);
		if (submenu != null)                             // if there's a submenu
		{
			a = document.createElement("a");

			submenuIndex = getIndexOfFirstChildByOL(menu);        // get all the children before the submenu (text, bold, etc.)
			linkkids = getChildrenBeforeIndex(menu, submenuIndex);
			if (linkkids != null)
			{
				for(var j = 0; j < linkkids.length; j++){
					a.appendChild(linkkids[j].cloneNode(true));   // append them to the anchor
				}
				for(var k = 0; k < linkkids.length; k++){
					menu.removeChild(linkkids[k]);                // remove them from menu
				}
			}
			menu.insertBefore(a, submenu);                        // put the anchor in front of the submenu

			a.href = "#";
			a.onclick = showMenu;
			if (typeof pageName != 'undefined'){
//alert(a.firstChild.nodeValue);
				if (a.firstChild.nodeValue.substring(0,pageName.length) == pageName) { a.onclick(); }
			}
			a.onfocus = function(){this.blur()};
		}
	}
}

// ||||||||||||||||||||||||||||||||||||||||||||||||||

if(document.createElement) window.onload = initMenu;
//-->
</script>
