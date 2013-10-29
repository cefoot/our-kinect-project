<!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
    <script data-dojo-config="async: true, tlmSiblingOfDojo: true, deps: ['application.js']"
            src="${pageContext.request.contextPath}/dojo/dojo.js.uncompressed.js"></script>
    <script type="text/javascript">
        var config = {
            contextPath: '${pageContext.request.contextPath}'
        };
    </script>
    <title>SocketBtn</title>
</head>
<body>
    <h2>SocketBtn</h2>
    <div id="status"></div>
    <button id="btn" style="height: 100px;width: 100px">
        Click to Paint
    </button>
</body>
</html>
