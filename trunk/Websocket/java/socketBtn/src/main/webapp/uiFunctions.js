/**
 * 
 */
function btnPaint() {
	var btn = document.getElementById('btnPaint');
	var val = btn.innerText;
	send('/paint/', val);
}

function btnClear() {
	send('/datachannel/clear', 'sauber machen! :D');
}