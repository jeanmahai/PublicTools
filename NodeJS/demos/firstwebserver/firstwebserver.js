var http=require("http");

http.createServer(function(req,res){
	
	res.writeHead(200,{"Content-type":"text/plain"});
	
	res.write("it is my first web server");
	
	res.end()
	
}).listen(8081);

console.info("server starting.port:8081");