/**
 * 
 */
function btnPaint() {
	var btn = document.getElementById('btnPaint');
	var val = btn.innerText;
	console.debug('paint:'+val);
	send('/paint/', val);
}

function btnClear() {
	send('/datachannel/clear', 'sauber machen! :D');
}