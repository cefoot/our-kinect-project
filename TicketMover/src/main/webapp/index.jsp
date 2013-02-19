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
    <title>TicketMover</title>
</head>
<body>
    <h2>TicketMover</h2>
    <div id="status"></div>
    <button id="greeter">
        Send Hello to Server
    </button>
    <button id="ticketAdder">
        +
    </button>
</body>
</html>
