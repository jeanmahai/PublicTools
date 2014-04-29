/*
* 基于jquery的验证框架
*
* */
(function(){
	var key="__validator__";
	var ruleFilter="[data-validator]";
	var ruleAttr="data-validator";
	var errorAttr="data-error";
	
	function getVal(tag){
		if(tag.is("input")) return $.trim(tag.val());
		return "";
	}
	
	function validate(tag){
		var val=getVal(tag);
		var cfg=$.getValidator();
		if(!cfg) cfg={img:"",rules:{}};
		var rule=tag.attr(ruleAttr);
		if((/^@(.*)$/).test(rule)){
			var name=RegExp.$1;
			if(cfg.rules[name]){
				return cfg.rules[name](val);
			}
		}
		else{
			var reg=new RegExp(rule);
			return reg.test(val);
		}
	}
	
	function mouseFocus(){
		
	}
	
	function mouseBlur(){
		var result=validate($(this));
		if(false===result){
			showError($(this));
		}
		else{
			hideError($(this));
		}
	}
	
	function showError(tag){
		if(tag.data("__error__")) return;
		var msg=tag.attr(errorAttr);
		var error=$("<div></div>");
		error.text(msg);
		error.appendTo(tag.parent());
		tag.data("__error__",error);
	}
	function hideError(tag){
		var error=tag.data("__error__");
		if(!error) return;
		error.remove();
		tag.removeData("__error__");
	}
	
	if(!$.setValidator)
		/*
			cfg={
				img:"",
				rules:{
					require:function(value){},
					int:function(value){}
				}
			}
		*/
		$.setValidator=function(cfg){
			var config;
			config=$(window).data(key);
			if(!config) config={};
			$.extend(config,cfg);
			$(window).data(key,config);
		};
	
	if(!$.getValidator)
		$.getValidator=function(){
			return $(window).data(key);
		};
	
	$.fn.extend({
		validate:function(){
			var result=true;
			$(this).find(ruleFilter).each(function(){
				result=(result && validate($(this)));
				if(false===result) showError($(this));
				else hideError($(this));
			});
			return result;
		}
		,addMouseListener:function(){
			$(this).find(ruleFilter)
			       //.bind("focus",mouseFocus)
				   .bind("blur",mouseBlur);
		}
	});
})();
