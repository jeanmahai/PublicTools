(function(){
	if(!window["N"]) window["N"]={};
	var n=window["N"];
	
	var views=[];
	function getNewView(){
		return views[views.length-1];
	}
	
	function View(){
		this.section=[];
		this.style=[];
		this.script=[];
		this.html="";
		this.context=null;
		this.url="";
		this.realUrl="";
	}
	function Style(){
		this.name="";
		this.text="";
	}
	function Script(){
		this.name="";
		this.text="";
	}
	function Section(){
		View.call(this);
		delete this.url;
	}
	
	function getRealPageUrl(){
		var index=window.location.href.indexOf("#");
		return window.location.href.substring(index+1);
	}
	
	function extend(o1,o2){
		for(var p in o1) o2[p]=o1[p];
		return o2;
	}
	
	function analysisHtml(html){
		//section
		var regTxt="<!--@section src=(.*)-->";
		var _view=getNewView();
		var matches=html.match(new RegExp(regTxt,"gi"));
		console.info("section match:",matches.length);
		matches.each(function(){
			var _section=new Section();
			_section.realUrl= new RegExp(regTxt,"gi").exec(this.valueOf())[1];
			_view.section.push(_section);
		});
		//style
		regTxt="<!--@style src=(.*)-->";
		matches=html.match(new RegExp(regTxt,"gi"));
		console.info("style match:",matches.length);
		matches.each(function(){
			var _style=new Style();
			_style.name= new RegExp(regTxt,"gi").exec(this.valueOf())[1];
			_view.style.push(_style);
		});
		//script
		regTxt="<!--@script src=(.*)-->";
		matches=html.match(new RegExp(regTxt,"gi"));
		console.info("script match:",matches.length);
		matches.each(function(){
			var _script=new Script();
			_script.name= new RegExp(regTxt,"gi").exec(this.valueOf())[1];
			_view.script.push(_script);
		});
		//context
		regTxt="<!--@context .* -->";
		matches=html.match(new RegExp(regTxt,"gi"));
		console.info("script match:",matches.length);
		matches.each(function(){
			console.info("context:"+this.valueOf());
		});
	}
	
	function popstate(){
		window.removeEventListener("popstate",popstate);
		console.info("event:popstate");
		console.info(String.format("real page url:{0}",getRealPageUrl()));
		
		var _view=extend({
			url:window.location.href,
			realUrl:getRealPageUrl()
		},new View());
		views.push(_view);
		
		console.info("loading page");
		$.get(_view.realUrl,function(html){
			analysisHtml(html);
			console.info(views);
		});
		window.addEventListener("popstate",popstate);
	}
	
	popstate();
})();