>地址:https://github.com/rogerwang/node-webkit

>发布工具:https://github.com/geo8bit/nodebob

>安装npm
1.cd到自己的软件目录
2.使用npm install XXX进行安装
3.npm安装完成可以使用了.
注意:最好先安装好第三方的nodejs包,在使用,因为我把app写好之后准备打包的时候才安装的npm,报错装不起,不知道是不是package.json的冲突造成的.

>node-webkit打包
1.下载nodebob
2.把自己的app文件全部copy到nodebob>app下
3.运行build.bat
4.会生成一个release的文件夹,这里面就是打包好的App
