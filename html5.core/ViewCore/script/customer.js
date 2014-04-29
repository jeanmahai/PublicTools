/**
 * Created by jm96 on 14-4-28.
 */
(function(){
    if(!window["customer"]) window["customer"]={
        login:function(name,psw){
            var deferred= Q.defer();
            if(name=="jean" && psw=="1") deferred.resolve(name);
            else deferred.reject(new Error("user not exists or password is wrong"));
            return deferred.promise;
        }
    };
})();