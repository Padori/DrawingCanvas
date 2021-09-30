mergeInto(LibraryManager.library, {

	ExportImage: function(appName,base64,imageName) {
		
		appName = Pointer_stringify(appName);
		base64 = Pointer_stringify(base64);
		imageName = Pointer_stringify(imageName);

		var printWindow = window.open("", "_blank");
		
		if(printWindow  == null)
		{
			window.alert('You have a popup blocker. To display site, please disable popup blocker');
			return;
		}

		printWindow.document.write('<html><head><title>'+appName+'</title>');
		printWindow.document.write('</head><body>');
		printWindow.document.write('<a id ="download" href="data:image/png;base64,'+base64+'" download="'+imageName+'.png"></a>');
		printWindow.document.write('<img id="screenshot" style="text-align:center" width="1024px" height="660px" src="data:image/png;base64,'+base64+'" />');
		printWindow.document.write('</body></html>');
				
		var script = document.createElement('script');
		script.innerHTML = 'document.getElementById("download").click()';
		printWindow.document.head.appendChild(script);
			
		setTimeout(function() {
			(function PrintDelay() {
				printWindow.print();
				printWindow.close();
			})();
		}, 1000);//print window after 1 second
	}

});