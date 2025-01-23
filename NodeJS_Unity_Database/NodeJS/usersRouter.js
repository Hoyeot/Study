const express = require('express');
const userDBC = require('./database');
const router = express.Router();

router.get('/getUsers', async (req, res) =>{
    let res_get_users = {
        status_code : 500,
        users : []
    };

    try {
        const rows = await userDBC.getUsers();
        res_get_users.status_code = 200;
        if(rows.length > 0)
        {
            rows.forEach((user) =>{
                res_get_users.users.push({
                    userId : user.user_id,
                    userPassword : user.user_pw,
                    userName : user.user_name,
                    userNickName : user.user_nickname,
                    userHighPoint : user.high_point
                });
            });
        }
        else{
            console.log('없음');
        }
    }
    catch(error)
    {
        console.log(error.message);
    }
    finally
    {
        var result = '';

        for (var i = 0; i < res_get_users.users.length; i++)
        {
            result += "user_id : " + res_get_users.users[i].userId + "<br>";
            result += "user_pw : " + res_get_users.users[i].userPassword + "<br>";
            result += "user_name : " + res_get_users.users[i].userName + "<br>";
            result += "user_nickname : " + res_get_users.users[i].userNickName + "<br>";
            result +=  "high_point : " + res_get_users.users[i].userHighPoint + "<br>";
            result += "<br>";
        }
        res.send(result);
    }
});

router.post('/insertUser', async(req, res) => {
    console.log("받은 데이터 : " + req.body);
    const values = [req.body.user_id, req.body.user_pw, req.body.user_name, req.body.user_nickname];
    const res_signup = {
        status_code : 500
    };

    try {
        const {user_id, user_pw, user_name, user_nickname} = req.body;
        const rows = await userDBC.insertUser([user_id, user_pw, user_name, user_nickname]);

        if (rows.affectedRows > 0)
        {
            res_signup_status_code = 200;
        }
        else{
            res_signup.status_code = 201;
        }
    }
    catch(err){
        console.log(err.message);
    }
    finally{
        res.json(res_signup);
    }
});

router.post('/deleteUser', async(req, res) =>{
    const res_delete_user = {
        status_code : 500
    };

    try{
        const {user_id} = req.body;
        const rows = await userDBC.deleteUser([user_id]);

        if (rows.affectedRows > 0)
        {
            res_delete_user.status_code = 200;
        }
        else{
            res_delete_user.status_code = 201;
        }
    }
    catch(err){
        console.log(err.message);
    }
    finally{
        res.json(res_delete_user);
    }
});

router.post('/updateUser', async(req, res) => {
    const res_update_user = {
        status_code : 500
    };

    try{
        const {user_id, high_point} = req.body;
    }
    catch(err){

    }
    finally{

    }
})

module.exports = router;