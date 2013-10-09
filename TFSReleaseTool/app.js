var path=require("path");
var fs=require("fs");

//var nor=path.normalize("d:\\jean.h.ma\\downloads\\github\\.");
//fs.exists(nor,function(exists){
//    console.info(exists);
//});
//
//fs.createReadStream("D:\\OY\\OYSD\\03_Code\\SourceCode\\Front\\02_Portal\\WebUI\\Order\\OrderDetail.aspx")
//    .pipe(fs.createWriteStream("C:\\Users\\jm96\\Desktop\\111.aspx"));

function getPathsStr(){
    var dom=document.getElementById("txtPaths");
    alert(dom.value);
    return dom.value;
}

function getAllPath(str){
    showMessage(str);
    var paths=[];
    var strs=str.split("\n");
    var len=strs.length;
    var i,fileName,fileFolder,fileParts,fileFullPath;

    for(i=0;i<len;i+=1){
        fileParts=strs[i].split("\t");
        fileName=fileParts[0];
        fileFolder=fileParts[2];
        fileFullPath=path.join(fileFolder,fileName);
        showMessage(fileFullPath);
        paths.push(fileFullPath);
    }
    return paths;
}

function showMessage(str){
    document.getElementById("divMsg").innerHTML+=str;
}

function showPaths(){
    var strPath=getPathsStr();
    var arrPath=getAllPath(strPath);
    showMessage(arrPath.join("<br/>"));
}
