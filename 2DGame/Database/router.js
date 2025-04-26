const express = require('express');
const crypto = require('crypto');
const userDBC = require('./database');
const router = express.Router();

function generateSalt() {
    return crypto.randomBytes(16).toString('hex');
}

// 솔트 + 비밀번호 해시 함수
function sha256WithSalt(password, salt) {
    return crypto.createHash('sha256')
                .update(password + salt) // 비밀번호 + 솔트 조합
                .digest('hex');
}

router.post('/getUsers', async (req, res) => {
    let res_get_users = {
        status_code: 500,
        userId: null,
        useFlag: null,
        users: []
    };

    try {
        const { user_id, user_pw } = req.body;
        
        // 1. DB에서 salt 조회
        const userData = await userDBC.getUserSalt(user_id);
        if (!userData) {
            res_get_users.status_code = 404;
            return res.json(res_get_users);
        }

        // 2. 비밀번호 검증
        const hashedInputPassword = crypto.createHash('sha256')
                                        .update(user_pw + userData.salt)
                                        .digest('hex');

        const rows = await userDBC.getUsers(user_id, hashedInputPassword);
        console.log("DB query result:", rows); // DB 결과 로그
        
        if (rows.length > 0) {
            res_get_users.status_code = 200;
            res_get_users.userId = user_id;
            res_get_users.useFlag = rows[0].useflag;
            res_get_users.users = rows.map(user => ({
                userId: user.user_id,
                userPassword: '***',
                useFlag: user.useflag
            }));
        } else {
            res_get_users.status_code = 401; // 비밀번호 불일치
        }
    } catch (error) {
        console.error('Error:', error);
    } finally {
        res.json(res_get_users);
    }
});

router.post('/chkUsers', async (req, res) =>{
    let res_get_users = {
        status_code : 500,
        userId : null,
        useFlag : null,
        users : []
    };
    console.log("받은 데이터 : " + JSON.stringify(req.body));
    const { user_id } = req.body;

    try {
        const rows = await userDBC.chkUsers(user_id);
        res_get_users.status_code = 200;
        res_get_users.userId = user_id;
        if(rows.length > 0)
        {
            rows.forEach((user) =>{
                res_get_users.users.push({
                    userId : user.user_id,
                    useFlag : user.useflag
                });
                res_get_users.useFlag = user.useflag;
            });
        }
        else{
            res_get_users.status_code = 999;
            console.log('없음');
        }
    }
    catch(error)
    {
        console.log(error.message);
    }
    finally
    {
        res.json(res_get_users);
    }
});

router.post('/insertUser', async(req, res) => {
    console.log("받은 데이터 : " + req.body);
    const res_signup = {
        status_code : 500
    };

    try {
        const {user_id, user_pw} = req.body;
        const salt = generateSalt(); // 솔트 생성
        const hashedPassword = sha256WithSalt(user_pw, salt); // 해시 처리
        const rows = await userDBC.insertUser(user_id, hashedPassword, salt);

        if (rows.affectedRows > 0)
        {
            res_signup.status_code = 200;
        }
        else{
            res_signup.status_code = 999;
        }
    }
    catch(err){
        res_signup.status_code = 888;
        console.log(err.message);
    }
    finally{
        res.json(res_signup);
    }
});

router.post('/updateUser', async(req, res) => {
    console.log("받은 데이터 : " + req.body);
    const res_signup = {
        status_code : 500
    };

    try {
        const {useFlag, user_id} = req.body;
        const rows = await userDBC.updateUser(useFlag, user_id);

        if (rows.affectedRows > 0)
        {
            res_signup.status_code = 200;
        }
        else{
            res_signup.status_code = 999;
        }
    }
    catch(err){
        res_signup.status_code = 888;
        console.log(err.message);
    }
    finally{
        res.json(res_signup);
    }
});

module.exports = router;