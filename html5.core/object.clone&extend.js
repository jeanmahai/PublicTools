if(!Object.clone){
	Object.clone=function(obj){		
		
		var result;
		if(obj instanceof Array){
			result=new Array();
			var i=0;
			var len=obj.length;
			for(;i<len;i++){
				result.push(Object.clone(obj[i]));
			}
			return result;
		}
		if(obj instanceof Object){
			result=new Object();
			for(var p in obj){
				result[p]=Object.clone(obj[p]);
			}
			return result;
		}
		if(typeof(obj)==="string") return new String(obj).valueOf();
		if(typeof(obj)==="number") return new Number(obj).valueOf();
		if(typeof(obj)==="boolean") return new Boolean(obj).valueOf();
		if(typeof(obj)==="function") return obj;
		return obj.valueOf();
	};
}
if(!Object.extend){
	Object.extend=function(o1,o2){
		for(var p in o1){
			o2[p]=Object.clone(o1[p]);
		}
		return o2;
	};
}



