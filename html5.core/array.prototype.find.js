if (!Array.prototype.find) {
    /*
    删除,
    
    {function},function返回true表示需要删除,否则不删除
    {deep},指定是否删除之后是否继续循环,true:是,false:否
    */
    Array.prototype.find = function (fn) {
        var result=[];
		if (fn && typeof(fn)=="function") {
			var len=this.length;
            for (var i = 0; i < len; i++) {
                if (fn(this[i]) == true) {
					result.push(this[i]);
                }
            }
        }
		return result;
    };
}


