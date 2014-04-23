if (!String.format) {
    String.format = function (str) {
		var args=arguments;
        return str.replace(new RegExp("\\{([0-9]+)\\}","gi"),function(){
			var index=parseInt(arguments[1]);
			return args[index+1];
		});
    };
}
