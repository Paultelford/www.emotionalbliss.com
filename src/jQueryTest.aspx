<%@ Page Language="VB" AutoEventWireup="false" CodeFile="jQueryTest.aspx.vb" Inherits="jQueryTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>jQuery Test Page</title>
    <script type="text/javascript" src="/jquery.js"></script>
    <script language="javascript" type="text/javascript">
        $(document).ready(function(){
            $("a.bag").hide("quick");
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <a href="homeIntro.aspx" class="bag">EB Home</a>
        <a href="homeIntro.aspx" >EB Home</a>
    </div>
    </form>
</body>
</html>
