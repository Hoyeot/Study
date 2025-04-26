const mysql = require('mysql2');
const crypto = require('crypto');

const pool = mysql.createPool({
    host: 'localhost',
    port: '3306',
    user: 'root',
    password: 'root',
    database: 'study_db'
});

const getUsers = async (user_id, user_pw) => {
    const promisePool = pool.promise();
    
    // salt 조회
    const [userData] = await promisePool.query(
        'SELECT salt FROM user_mst WHERE user_id = ?',
        [user_id]
    );
    
    if (userData.length === 0) {
        return [];
    }
    
    const salt = userData[0].salt;
    
    // 입력된 비밀번호 + salt 해시
    const hashedInputPassword = crypto.createHash('sha256')
                                    .update(user_pw + salt)
                                    .digest('hex');
    
    // 해시된 비밀번호로 사용자 조회
    const [rows] = await promisePool.query(
        'SELECT * FROM user_mst WHERE user_id = ? AND user_pw = ?',
        [user_id, hashedInputPassword]
    );
    
    console.log(rows);
    return rows;
};

const getUserSalt = async (user_id) => {
    const promisePool = pool.promise();
    const [rows] = await promisePool.query(
        'SELECT salt FROM user_mst WHERE user_id = ?',
        [user_id]
    );
    return rows[0] || null;
};

const chkUsers = async(user_id) => {
    const promisePool = pool.promise();
    const [rows] = await promisePool.query('SELECT * FROM user_mst WHERE user_id = ?;', [user_id]);
    console.log(rows);
    return rows;
};

const insertUser = async (user_id, user_pw, salt) => {
    const promisePool = pool.promise();
    const hashedPassword = crypto.createHash('sha256')
                                .update(user_pw + salt)
                                .digest('hex');
    
    const [rows] = await promisePool.query(
        'INSERT INTO user_mst (user_id, user_pw, salt, useflag) VALUES (?, ?, ?, 0)',
        [user_id, hashedPassword, salt]
    );
    return rows;
}


const updateUser = async (useFlag, user_id) => {
    const promisePool = pool.promise();
    const [rows] = await promisePool.query('UPDATE user_mst set useflag = ? where user_id = ?;', [useFlag, user_id]);
    return rows;
}

module.exports = {
    getUsers, chkUsers, insertUser, updateUser, getUserSalt
};