/*
	发布工具,从pendingchange中复制出需要发布的文件信息,粘贴到textarea中,设置好target path,即要复制到那个目录,match
	是匹配那些目录进行复制,点OK即复制这些文件.文件列表的格式就是从pendingchange中复制出来的格式,不需要修改.
	
*/


var PATH=require("path");
var FS=require("fs");
var PROCESS=require("process");
//var GUI=require("nw.gui");

//GUI.Window.get().showDevTools();

PROCESS.on("uncaughtException",function(err){
	showMessage("发生错误了:"+err);
});

function getPathsArr(strs){
	var lines=strs.split("\n");
	var arrPaths=[],
		arrLineParts,
		i,
		len=lines.length;
	for(i=0;i<len;i+=1){
		arrLineParts=lines[i].split("\t");
		if(arrLineParts.length!==3) continue;
		arrPaths.push(arrLineParts[2]+"\\"+arrLineParts[0]);
	}
	return arrPaths;
}

function takeFromArray(arr,count){
	var i,result=[];
	for(i=0;i<count;i+=1){
		result.push(arr[i]);
	}
	return result;
}

function checkDir(arrDir,index,callback){
	if(index>=arrDir.length) {
        callback();
        return;
    }
	var path=takeFromArray(arrDir,index+1).join("\\");
	FS.exists(path,function(ext){
		if(ext){
			if(index===(arrDir.length-1)) callback();
			else checkDir(arrDir,index+1,callback);
		}
		else{
			FS.mkdir(path);
			showMessage("创建目录成功:"+path);
			checkDir(arrDir,index+1,callback);
		}
	});
}

function copyTo(path,targetPath,match){
	//var targetPath=PATH.join(targetPath);
	
	var matchPath=path.split(match)[1];
	if(!matchPath) {
		showMessage("no matched path");
		return;
	}
	targetPath=PATH.join(targetPath,matchPath);
	var parts=PATH.dirname(targetPath).split(PATH.sep);
	checkDir(parts,0,function(){
		FS.exists(targetPath,function(ext){
			if(ext) {
				FS.unlink(targetPath,function(){
					FS.createReadStream(path).pipe(FS.createWriteStream(targetPath));
					showMessage("复制文件成功:"+targetPath);
				})
			}
			else{
				FS.createReadStream(path).pipe(FS.createWriteStream(targetPath));
				showMessage("复制文件成功:"+targetPath);
			}
		});
	});
}

function showMessage(str){
	document.getElementById("divMsg").innerHTML+=str+"<br/>";
	//console.info(str);
}

function getCurrentPath(){
	return PROCESS.execPath;
}

function logSomething(content){
	FS.realpath(".",function(err,resolvedPath){
		if(err) console.error(err);
		consoel.info("current path > "+resolvedPath);
		var logFilePath=PATH.join(resolvedPath,"logs",Date.now()+".txt");
		FS.exists(logFilePath,function(ext){
			if(!ext){
				
			}
		});
	});
	//判断日志文件是否存在
    var curPath=getCurrentPath();
    var curDir=PATH.dirname(curPath);
    var logDir=PATH.join(curDir,"logs");
    FS.exists(logDir,function(ext){
        if(!ext) FS.mkdir(logDir);
    });
	//如果不存在,则创建
	//append content
}

function ok(){
	var dom=document.getElementById("txtPaths");
	var val=dom.value;
	var arrPaths=getPathsArr(val);
	var i,
		len=arrPaths.length,
		targetPath=PATH.normalize(document.getElementById("txtTargetPath").value),
		match=document.getElementById("txtMatch").value;
	for(i=0;i<len;i+=1){
		copyTo(arrPaths[i],targetPath,match);
	}
}

//getCurrentPath();
console.info(PROCESS.execPath);
		
